package top.mrxiaom.graphicalmirai

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.serialization.KSerializer
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.jsonObject
import kotlinx.serialization.json.jsonPrimitive
import net.mamoe.mirai.console.command.CommandManager.INSTANCE.register
import net.mamoe.mirai.console.extension.PluginComponentStorage
import net.mamoe.mirai.console.plugin.jvm.JvmPluginDescription
import net.mamoe.mirai.console.plugin.jvm.KotlinPlugin
import net.mamoe.mirai.event.broadcast
import net.mamoe.mirai.utils.LoginSolver
import top.mrxiaom.graphicalmirai.commands.WrapperedStopCommand
import top.mrxiaom.graphicalmirai.events.BridgeDataPreReceive
import top.mrxiaom.graphicalmirai.packets.`in`.IPacketIn
import top.mrxiaom.graphicalmirai.packets.out.IPacketOut
import top.mrxiaom.graphicalmirai.packets.out.OutLoginVerify
import top.mrxiaom.graphicalmirai.packets.out.OutSmsVerify
import top.mrxiaom.graphicalmirai.packets.out.OutSolveSliderCaptcha
import java.io.BufferedWriter
import java.io.Closeable
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
    /**
     * GraphicalMirai 登录解决器代理，用于通知启动器弹出相应 UI。
     * 支持 2.13 起加入的短信验证码验证，兼容 2.13 以下的版本
     */
    internal val loginSolver by lazy {
        try {
            LoginSolver::onSolveDeviceVerification.isOpen
            RemoteLoginSolver213
        } catch (_:Throwable) {
            RemoteLoginSolver212
        }
    }
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
        logger.info("正在使用登录解决器代理 ${loginSolver.javaClass.simpleName}")
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
            conf.loginSolver = loginSolver
            conf
        }
    }

    override fun onEnable() {
        WrapperedStopCommand.hack()
    }

    private suspend fun receiveData(data: String) {
        logger.verbose("received: $data")
        if (BridgeDataPreReceive(data).broadcast().isCancelled) {
            logger.verbose("cancelled by other plugins")
            return
        }
        val jsonElement = Json.parseToJsonElement(data)
        val type = jsonElement.jsonObject["type"]?.jsonPrimitive?.content
        val packetSerializer = packagesIn[type] ?: return
        val packet = Json.decodeFromJsonElement(packetSerializer, jsonElement)
        packet.handle()
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

    internal fun waitingForTicket(url: String) {
        sendPacket(OutSolveSliderCaptcha(url))
    }

    internal fun waitingForLoginVeriy(url: String) {
        sendPacket(OutLoginVerify(url))
    }

    fun waitingForSmsVerify(countryCode: String?, phoneNumber: String?) {
        sendPacket(OutSmsVerify(countryCode ?: "", phoneNumber ?: ""))
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

private fun Closeable?.closeQuietly() {
    try {
        this?.close()
    } catch (_: Throwable) {
    }
}
