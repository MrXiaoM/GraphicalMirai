<img align="right" width="128" height="128" src="logo.png"/>

# GraphicalMirai

[mirai-console](https://github.com/mamoe/mirai) 的图形界面下载器/启动器/插件中心。

未完成，正在编写中。

## 当前进度

- [x] 下载 mirai
- [x] 启动 mirai
- [x] 控制台着色
- [ ] ~~调用 JNI 实现与 mirai 通信，打通实现其他功能的道路 (实在不行的话就用 MAH 算了)~~
- [ ] 使用 Socket 实现 GraphicalMirai 与 mirai 通信，不强制安装 MAH 但强制安装 GraphicalMirai 通信桥
- [ ] 登录UI
- [ ] 管理自动登录
- [ ] 辅助处理滑块验证
- [ ] 多用户聊天窗口
- [ ] 管理本地插件
- [x] 获取论坛上的插件
- [ ] 下载论坛上的插件
- [ ] 获取 mirai-repo 上的插件
- [ ] 下载 mirai-repo 上的插件
- [ ] 包管理器

## OS 支持

我们为 Windows 7 及以上的操作系统提供兼容支持，对于其他非 Windows 操作系统，GraphicalMirai 将提供一个「导出」按钮将当前 mirai 打包，在相应系统执行预设的脚本即可启动 mirai。

## 使用字体

详见 [font/README.md](font)

## 图像解码器

由于论坛的图片均使用了 webp 压缩，若想在插件中心可查看用户头像，请到

https://storage.googleapis.com/downloads.webmproject.org/releases/webp/index.html

下载 [WebpCodecSetup.exe](https://storage.googleapis.com/downloads.webmproject.org/releases/webp/WebpCodecSetup.exe) 并安装即可
