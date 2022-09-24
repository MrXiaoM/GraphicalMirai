package top.mrxiaom.graphicalmirai

import net.mamoe.mirai.console.plugin.jvm.JvmPluginDescription
import net.mamoe.mirai.console.plugin.jvm.KotlinPlugin
import net.mamoe.mirai.utils.info

object GraphicalMiraiBridge : KotlinPlugin(
    JvmPluginDescription(
        id = "top.mrxiaom.graphicalmirai.bridge",
        name = "GraphicalMiraiBridge",
        version = "0.1.0",
    ) {
        author("MrXiaoM")
        info("""GraphicalMirai 通信桥""")
    }
) {
    override fun onEnable() {
        val port = System.getProperty("graphicalmirai.bridge.port").toInt()
        // TODO 等待 C# 端实现服务端

        logger.info { "Plugin loaded" }
    }
}