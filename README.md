jellyfin-danmaku
jellyfin 弹幕插件


原来的/Izumiko/jellyfin-danmaku已经一年没有更新了，最近也不怎么好使，发现这位大佬l429609201/dd-danmaku的还有在更，就魔改了一下让jellyfin也能用上。代码都是AI跑的，本人并不会代码，凑合用。 还是有挺多BUG的，有空和token再修吧。弹弹play本人没有api，申请没通过，所以这个就建议大家自行解决吧。推荐个大佬的项目https://github.com/l429609201/misaka_danmu_server

<img width="2970" height="1638" alt="image" src="https://github.com/user-attachments/assets/478ae8ca-1c22-4948-ae06-8d54b150d3bd" /><img width="2329" height="1579" alt="image" src="https://github.com/user-attachments/assets/46b2b2a3-4663-4012-b5f4-68f574c8a5f6" />





使用方法

**脚本地址 (二选一):**

修改文件 /usr/share/jellyfin/web/index.html (Default)

或 /jellyfin/jellyfin-web/index.html (Official Docker)

在</body>前添加如下标签

<完整版>
<script src="https://cdn.jsdelivr.net/gh/505653375/dd-danmaku@main/ede.js" charset="utf-8"></script>



<本地版 (需自行下载脚本文件)>
<script src="ede.js" charset="utf-8"></script>



其他方法可以自行研究


参考项目，感谢下面这些大佬的付出
 - [l429609201/dd-danmaku](https://github.com/l429609201/dd-danmaku)
 - [Izumiko/jellyfin-danmaku](https://github.com/Izumiko/jellyfin-danmaku)
 - [pipi20xx/dd-danmaku](https://github.com/pipi20xx/dd-danmaku)
 - [chen3861229/dd-danmaku](https://github.com/chen3861229/dd-danmaku)


## 常见问题

如果遇到弹幕加载失败或匹配错误，请优先尝试以下操作：
1.  检查网络连接和 API 设置。
2.  使用“手动匹配”功能，输入正确的番剧名称进行搜索。
3.  点击“**清除本地匹配缓存**”按钮，然后刷新页面或重新进入播放，让插件重新匹配。

如果问题依旧，欢迎提交 Issue。
