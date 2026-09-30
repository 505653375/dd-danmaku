const prefix = '/dd-danmaku'

function clientContext() {
  let client = window.ApiClient
  if (!client) {
    try { client = window.parent?.ApiClient } catch { /* cross-origin */ }
  }
  const token = client?.accessToken?.()
  if (!token) throw new Error('未获取到 Jellyfin 登录会话，请从已登录的 Jellyfin 管理页面打开')
  const base = new URL(client.serverAddress(), window.location.href)
  if (base.origin !== window.location.origin) throw new Error('管理 API 必须与 Jellyfin 会话同源')
  const userId = client?.getCurrentUserId?.()
  return { token, userId, base: base.href.replace(/\/$/, '') }
}

function currentUserId() {
  const { userId } = clientContext()
  if (!userId) throw new Error('未获取到当前 Jellyfin 用户标识')
  return userId
}

async function request(path, options = {}, compatibility = false) {
  const { token, base } = clientContext()
  const response = await fetch(`${base}${compatibility ? '' : prefix}${path}`, {
    ...options,
    credentials: 'same-origin', cache: 'no-store', redirect: 'error',
    headers: { 'Content-Type': 'application/json', ...options.headers, 'Authorization': 'MediaBrowser Token="' + token + '"' },
  })
  const body = await response.json().catch(() => null)
  if (!response.ok || body?.success === false || body?.Success === false) {
    const error = new Error(body?.message || body?.Message || `请求失败（${response.status}）`)
    error.status = response.status
    error.code = body?.errorCode || body?.ErrorCode || ''
    throw error
  }
  if (body === null || typeof body !== 'object') {
    throw new Error(`接口 ${path} 返回空值或非 JSON 数据（HTTP ${response.status}），请检查 DLL 是否更新并重启 Jellyfin，以及反向代理路由`)
  }
  if (compatibility) return body
  const data = 'data' in body ? body.data : 'Data' in body ? body.Data : body
  if (data?.secretEncoding === 'xor-utf8-base64-v1') {
    try {
      const bytes = Uint8Array.from(atob(data.payload), c => c.charCodeAt(0))
      const key = new TextEncoder().encode(token)
      for (let i = 0; i < bytes.length; i++) bytes[i] ^= key[i % key.length]
      return JSON.parse(new TextDecoder('utf-8', { fatal: true }).decode(bytes))
    } catch { throw new Error('敏感配置解码失败，请重新登录并读取；不要保存当前表单') }
  }
  return data
}

export const api = {
  githubSettings: () => request('/api/config/github'),
  saveGithubSettings: data => request('/api/config/github', { method: 'PUT', body: JSON.stringify(data) }),
  checkUpdate: () => request('/api/updates/check'),
  parameterFiles: () => request('/api/parameter-files'),
  parameterFile: (id) => request(`/api/parameter-files/${encodeURIComponent(id)}`),
  saveParameterFile: (id, data) => request(`/api/parameter-files/${encodeURIComponent(id)}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteParameterFile: (id) => request(`/api/parameter-files/${encodeURIComponent(id)}`, { method: 'DELETE' }),
  copyParameterFile: (id, data) => request(`/api/parameter-files/${encodeURIComponent(id)}/copy`, { method: 'POST', body: JSON.stringify(data) }),
  convertParameterFile: (id) => request(`/api/parameter-files/${encodeURIComponent(id)}/convert`, { method: 'POST' }),
  dashboard: () => request('/api/overview'),
  startScan: () => request('/api/library-scan/run', { method: 'POST' }),
  cancelScan: () => request('/api/library-scan/run', { method: 'DELETE' }),
  saveScanMode: (deep) => request('/api/library-scan/mode', { method: 'PUT', body: JSON.stringify({ deep }) }),
  scanScope: () => request('/api/library-scan/scope'),
  saveScanScope: libraryIds => request('/api/library-scan/scope', { method: 'PUT', body: JSON.stringify({ libraryIds }) }),
  aiSettings: () => request('/api/config/ai'),
  aiModels: data => request('/api/config/ai/models', { method: 'POST', body: JSON.stringify(data) }),
  saveAiSettings: (data) => request('/api/config/ai', { method: 'PUT', body: JSON.stringify(data) }),
  capabilities: () => request('/api/capabilities'),
  playbackSettings: () => request('/api/config/playback'),
  savePlaybackSettings: data => request('/api/config/playback', { method: 'PUT', body: JSON.stringify(data) }),
  config: () => request('/api/config'),
  users: () => request('/api/users'),
  saveConfig: (data) => request('/api/config', { method: 'PUT', body: JSON.stringify(data) }),
  parameters: (filters = {}) => {
    const query = new URLSearchParams(Object.entries({ Namespace: filters.namespace, Key: filters.key, Keyword: filters.keyword }).filter(([, value]) => value != null && value !== ''))
    return request(`/api/ParameterPersistence/Query${query.size ? `?${query}` : ''}`, {}, true)
  },
  saveParameter: (data) => request('/api/ParameterPersistence/Create', { method: 'POST', body: JSON.stringify({ userid: currentUserId(), ...data }) }, true),
  updateParameter: (data) => request('/api/ParameterPersistence/Update', { method: 'POST', body: JSON.stringify({ userid: currentUserId(), ...data }) }, true),
  deleteParameter: (data) => request('/api/ParameterPersistence/Delete', { method: 'POST', body: JSON.stringify({ userid: currentUserId(), ...data }) }, true),
  records: (page = 1, pageSize = 20, signal, filters = {}) => request(`/api/records?${new URLSearchParams({ page, pageSize, ...filters })}`, { signal }),
  recordDetail: (id, signal) => request(`/api/records/detail?recordId=${encodeURIComponent(id)}`, { signal }),
  verifyRecord: id => request(`/api/records/verify?recordId=${encodeURIComponent(id)}`, { method: 'POST' }),
  removeRecord: (id, deleteFile) => request(`/api/records/remove?recordId=${encodeURIComponent(id)}&deleteFile=${deleteFile}`, { method: 'DELETE' }),
  mediaLink: id => {
    const { base } = clientContext()
    return `${base}/web/index.html#!/item?id=${encodeURIComponent(id)}`
  },
  downloadRecord: async id => {
    const { base, token } = clientContext()
    const response = await fetch(`${base}${prefix}/api/records/download?recordId=${encodeURIComponent(id)}`, {
      headers: { 'Authorization': 'MediaBrowser Token="' + token + '"' }, credentials: 'same-origin', cache: 'no-store', redirect: 'error',
    })
    if (!response.ok || !response.headers.get('content-type')?.includes('application/xml')) {
      const body = await response.json().catch(() => null)
      throw new Error(body?.message || `下载失败（${response.status}）`)
    }
    const url = URL.createObjectURL(await response.blob())
    const link = document.createElement('a')
    link.href = url; link.download = 'danmaku.xml'; link.click()
    setTimeout(() => URL.revokeObjectURL(url), 1000)
  },
  queryPlayback: (itemId, signal, source) => request(`/api/playback/${encodeURIComponent(itemId)}${source ? `?source=${encodeURIComponent(source)}` : ''}`, { signal }),
  resolveMatch: (data, signal) => request('/api/matches/resolve', { method: 'POST', body: JSON.stringify(data), signal }),
  frontendDefaults: (userId = '', signal) => request(`/api/config/frontend-defaults${userId ? `/users/${encodeURIComponent(userId)}` : ''}`, { signal }),
  saveFrontendDefaults: (userId, data) => request(`/api/config/frontend-defaults${userId ? `/users/${encodeURIComponent(userId)}` : ''}`, { method: 'PUT', body: JSON.stringify(data) }),
  resetFrontendDefaults: (userId) => request(`/api/config/frontend-defaults/users/${encodeURIComponent(userId)}`, { method: 'DELETE' }),
}
