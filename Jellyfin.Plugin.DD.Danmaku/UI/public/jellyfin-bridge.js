(function () {
    'use strict';
    return function (view) {
        var frame = view.querySelector('.dd-admin-frame');
        var error = view.querySelector('.dd-error');
        view.addEventListener('viewshow', function () {
            error.hidden = true;
            try {
                var client = window.ApiClient;
                if (!client || !client.accessToken()) throw new Error('请先登录 Jellyfin 管理员账号');
                var base = client.serverAddress().replace(/\/$/, '');
                var url = new URL(base + '/dd-danmaku/admin/index.html', window.location.href);
                if (url.origin !== window.location.origin) throw new Error('管理页面必须与当前 Jellyfin 页面同源');
                frame.src = url.href;
            } catch (e) {
                error.textContent = e.message || '无法打开弹幕管理页面';
                error.hidden = false;
            }
        });
        view.addEventListener('viewbeforehide', function () { frame.src = 'about:blank'; });
    };
})();
