package top.mrxiaom.graphicalmirai

import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.runBlocking
import net.mamoe.mirai.console.extension.PluginComponentStorage
import net.mamoe.mirai.console.plugin.jvm.JvmPluginDescription
import net.mamoe.mirai.console.plugin.jvm.KotlinPlugin
import net.mamoe.mirai.event.broadcast
import top.mrxiaom.graphicalmirai.events.BridgeDataPreReceive
import java.io.BufferedWriter
import java.io.DataInputStream
import java.io.OutputStreamWriter
import java.io.PrintWriter
import java.net.Socket

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
    private lateinit var socket: Socket
    private lateinit var input: DataInputStream
    private lateinit var output: PrintWriter
    private val sliderDef = CompletableDeferred<String>()
    override fun PluginComponentStorage.onLoad() {
        val port = System.getProperty("graphicalmirai.bridge.port")?.toInt()
        if (port == null) {
            logger.warning("找不到通信桥端口，禁用插件功能")
            logger.warning("在不使用 GraphicalMirai 时请不要使用该插件")
            return
        }
        runBlocking(Dispatchers.IO) {
            try {
                logger.info("正在连接到 GraphicalMirai (:$port)")
                socket = Socket("127.0.0.1", port)
                logger.info(":${socket.localPort} 已连接到 GraphicalMirai (:$port)")
                output = PrintWriter(BufferedWriter(OutputStreamWriter(socket.getOutputStream())), true)
                input = DataInputStream(socket.getInputStream())
                while (socket.isConnected) {
                    receiveData(input.readUTF())
                }
            } catch (t: Throwable) {
                logger.error(t)
            }
        }
        contributeBotConfigurationAlterer { _, conf ->
            conf.loginSolver = RemoteLoginSolver
            conf
        }
    }

    private fun receiveData(data: String) {
        logger.verbose("received: $data")
        launch {
            if (BridgeDataPreReceive(data).broadcast().isCancelled) return@launch
            if (data.startsWith("SolveSliderCaptcha,")) {
                val ticket = data.substring(19)
                if (!sliderDef.complete(ticket)) {
                    logger.warning("通信桥收到一个滑块验证回调 ticket，但此时并没有进行滑块验证。如需查看 ticket 详见日志 (VERBOSE)。")
                }
                logger.verbose("slider captcha ticket: $ticket")
                return@launch
            }
        }
    }

    /**
     * 发送消息到 GraphicalMirai
     *
     * @param data 消息内容
     * @return 是否发送成功
     */
    fun sendRawData(data: String): Boolean {
        if (!this::socket.isInitialized || !this::output.isInitialized) return false
        output.println(data)
        return !output.checkError()
    }

    internal suspend fun waitingForTicket(url: String): String {
        output.println("SolveSliderCaptcha,$url")
        return sliderDef.await()
    }

    override fun onDisable() {
        if (this::output.isInitialized) output.close()
        if (this::input.isInitialized) input.close()
        if (this::socket.isInitialized) socket.close()
    }
}