package top.mrxiaom.graphicalmirai

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import net.mamoe.mirai.Bot
import net.mamoe.mirai.console.extensions.BotConfigurationAlterer
import net.mamoe.mirai.console.plugin.jvm.JvmPluginDescription
import net.mamoe.mirai.console.plugin.jvm.KotlinPlugin
import net.mamoe.mirai.utils.BotConfiguration
import net.mamoe.mirai.utils.info
import java.io.BufferedWriter
import java.io.DataInputStream
import java.io.OutputStreamWriter
import java.io.PrintWriter
import java.net.Socket
import java.util.concurrent.CompletableFuture

object GraphicalMiraiBridge : KotlinPlugin(
    JvmPluginDescription(
        id = "top.mrxiaom.graphicalmirai.bridge",
        name = "GraphicalMiraiBridge",
        version = "0.1.0",
    ) {
        author("MrXiaoM")
        info("""GraphicalMirai 通信桥""")
    }
), BotConfigurationAlterer {
    val bots = mutableMapOf<Long, CompletableFuture<String>>()
    lateinit var socket: Socket
    lateinit var input: DataInputStream
    lateinit var output: PrintWriter
    var disable = false
    override fun onEnable() {
        val port = System.getProperty("graphicalmirai.bridge.port")?.toInt()
        if (port == null) {
            logger.warning("找不到通信桥端口，禁用插件功能")
            // 在插件启用时无法卸载插件 (恼)
            disable = true
            return
        }
        // TODO 等待 C# 端实现服务端
        launch {
            withContext(Dispatchers.IO) {
                try {
                    socket = Socket("127.0.0.1", port)
                    output = PrintWriter(BufferedWriter(OutputStreamWriter(socket.getOutputStream())), true)
                    input = DataInputStream(socket.getInputStream())
                    while (socket.isConnected) {
                        receiveData(input.readUTF())
                    }
                } catch (t: Throwable) {
                    logger.error(t)
                }
            }
        }
        logger.info { "Plugin loaded" }
    }

    fun receiveData(data: String) {
        if (data.startsWith("SolveSliderCaptcha,")) {
            val params = data.substring(19)
            val botId = params.substringBefore(',').toLong()
            val ticket = params.substringAfter(',')
            bots[botId]?.complete(ticket) ?: run {
                logger.warning("收到了机器人 $botId 的滑块验证 ticket，但无法找到可使用 ticket 的滑块验证请求")
                logger.warning(ticket)
            }
            return
        }
    }

    suspend fun waitingForTicket(bot: Bot, url: String): String {
        val future = CompletableFuture<String>()
        bots[bot.id] = future
        output.println("SolveSliderCaptcha,${bot.id},$url")
        return withContext(Dispatchers.IO) {
            future.get()
        }.also { bots.remove(bot.id) }
    }

    override fun onDisable() {
        if (!disable) {
            output.close()
            input.close()
            socket.close()
        }
    }

    override fun alterConfiguration(botId: Long, configuration: BotConfiguration): BotConfiguration {
        if (!disable) configuration.loginSolver = RemoteLoginSolver
        return configuration
    }
}