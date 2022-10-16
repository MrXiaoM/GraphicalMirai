# LoginSolver

这是 GraphicalMirai 的滑块验证解决器，使用 [WebView2](https://learn.microsoft.com/zh-cn/microsoft-edge/webview2/) 抓包返回 ticket。

由于不是所有人都需要进行滑块验证，故将其分为单独的工具。

## 编译后存放位置

应将编译后的文件放置到 `../tools/LoginSolver/` 以供 GraphicalMirai 调用。

## 发布

LoginSolver 将会随 GraphicalMirai 单独发布和集成发布，不管 LoginSolver 在该版本有没有更新，版本号始终与 GraphicalMirai 同步。

## 用法

由于该工具并不是为人类而设计，故操作较为复杂。
将你获取到的滑块验证链接使用 base64 进行编码，然后执行以下命令行来进行滑块验证

```
LoginSolve.exe --url=编码后的链接
```

完成后，将在控制台输出 ticket 并退出
```
ticket: tTICKET_CONTENT*
```

你可以使用 `--user-agent=` 来指定浏览器 UA，同样的，需要使用 base64 编码。
