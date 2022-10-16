package top.mrxiaom.graphicalmirai

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.serialization.KSerializer
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import net.mamoe.mirai.console.extension.PluginComponentStorage
import net.mamoe.mirai.console.plugin.jvm.JvmPluginDescription
import net.mamoe.mirai.console.plugin.jvm.KotlinPlugin
import okhttp3.internal.closeQuietly
import top.mrxiaom.graphicalmirai.commands.WrapperedStopCommand
import top.mrxiaom.graphicalmirai.packets.`in`.IPacketIn
import top.mrxiaom.graphicalmirai.packets.out.IPacketOut
import top.mrxiaom.graphicalmirai.packets.out.OutLoginVerify
import top.mrxiaom.graphicalmirai.packets.out.OutSolveSliderCaptcha
import java.io.BufferedWriter
import java.io.DataInputStream
import java.io.OutputStreamWriter
import java.io.PrintWriter
import java.net.Socket
import java.net.SocketException

object GraphicalMiraiBridge : KotlinPlugin(
    JvmPluginDescription(
        id = "top.mrxiaom.graphicalmirai.bridge",
        name = "GraphicalMiraiBridge",
        version = "0.1.1",
    ) {
        author("MrXiaoM")
        info("""GraphicalMirai 通信桥""")
    }
) {
    val packagesIn = mapOf<String, KSerializer<out IPacketIn>>(

    )

    private lateinit var socket: Socket
    private var input: DataInputStream? = null
    private var output: PrintWriter? = null

    override fun PluginComponentStorage.onLoad() {
        val port = System.getProperty("graphicalmirai.bridge.port")?.toInt()
        if (port == null) {
            logger.warning("找不到通信桥端口，禁用插件功能")
            logger.warning("在不使用 GraphicalMirai 时请不要使用该插件")
            return
        }
        logger.info("正在连接到 GraphicalMirai (:$port)")
        socket = Socket("127.0.0.1", port)
        logger.info(":${socket.localPort} 已连接到 GraphicalMirai (:$port)")
        output = PrintWriter(BufferedWriter(OutputStreamWriter(socket.getOutputStream())), true)
        input = DataInputStream(socket.getInputStream())
        launch(Dispatchers.IO) {
            try {
                while (!socket.isClosed) {
                    val data = input?.readUTF() ?: continue
                    receiveData(data)
                }

            } catch (t: Throwable) {
                if (t is SocketException && t.message == "Socket closed") {
                    logger.info("和 GraphicalMirai 断开连接")
                } else {
                    logger.error(t)
                }
            }
        }

        contributeBotConfigurationAlterer { _, conf ->
            conf.loginSolver = RemoteLoginSolver
            conf
        }
    }

    override fun onEnable() {
        WrapperedStopCommand.hack()
    }

    private fun receiveData(data: String) {
        logger.verbose("received: $data")
        val jsonElement = Json.parseToJsonElement(data)
        val type = jsonElement.jsonObject["type"]?.jsonPrimitive?.content
        val packetSerializer = packagesIn[type] ?: return
        val packet = Json.decodeFromJsonElement(packetSerializer, jsonElement)
        launch {
            packet.handle()
        }
    }

    /**
     * 发送消息到 GraphicalMirai
     *
     * @param data 消息内容
     * @return 是否发送成功
     */
    fun sendRawData(data: String): Boolean {
        if (!this::socket.isInitialized || output == null) return false
        output?.println(data)
        return !((output?.checkError()) ?: true)
    }

    inline fun <reified T : IPacketOut> sendPacket(packet: T): Boolean {
        val json = Json.encodeToString(packet)
        return sendRawData(json)
    }

    internal suspend fun waitingForTicket(url: String) {
        sendPacket(OutSolveSliderCaptcha(url))
    }

    internal suspend fun waitingForLoginVeriy(url: String) {
        sendPacket(OutLoginVerify(url))
    }
    internal fun disable() {
        try {
            if (!socket.isClosed) {
                logger.info("正在断开连接")
                socket.close()
                output?.closeQuietly()
                input?.closeQuietly()
            }
        } catch (_: UninitializedPropertyAccessException) {
        }
    }
}