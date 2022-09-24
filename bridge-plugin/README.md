# GraphicalMiraiBridge

GraphicalMirai 通信桥是 GraphicalMirai 与 mirai 间通信以实现多聊天窗口显示等功能的桥梁。

通信桥 `.jar` 插件会内嵌到 GraphicalMirai 内，在启动 mirai 之前自动安装。通信桥版本将与 GraphicalMirai 版本保持一致。

## 工作机理

读取 `-Dgraphicalmirai.bridge.port` 提供的端口连接到 GraphicalMirai，进行进程间通信。暂定为使用 TCP Socket 通信。

## 可选操作

通信桥将会作为默认启用的可选组件，可到设置中禁用通信桥以及依赖通信桥的相关功能。

## 为什么不用 MAH?

> [mirai-api-http](https://github.com/project-mirai/mirai-api-http) 是 project-mirai 开发的官方通用 http 接口，**简称 MAH**

有人需要 v1 的 MAH，有人需要 v2 的 MAH，若 GraphicalMirai 使用 MAH 通信将可能出现用户与启动器需求上的冲突。
