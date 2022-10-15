package top.mrxiaom.graphicalmirai

import kotlinx.coroutines.coroutineScope
import net.mamoe.mirai.Bot
import net.mamoe.mirai.utils.LoginSolver
import net.mamoe.mirai.utils.MiraiExperimentalApi
import net.mamoe.mirai.utils.StandardCharImageLoginSolver
import net.mamoe.mirai.utils.SwingSolver

object RemoteLoginSolver : LoginSolver() {
    override val isSliderCaptchaSupported: Boolean = true
    val std = StandardCharImageLoginSolver()
    @OptIn(MiraiExperimentalApi::class)
    override suspend fun onSolvePicCaptcha(bot: Bot, data: ByteArray): String {
        return SwingSolver.onSolvePicCaptcha(bot, data)
    }

    override suspend fun onSolveSliderCaptcha(bot: Bot, url: String): String = coroutineScope {
        GraphicalMiraiBridge.waitingForTicket(url)
        return@coroutineScope std.onSolveSliderCaptcha(bot, url)
    }

    @OptIn(MiraiExperimentalApi::class)
    override suspend fun onSolveUnsafeDeviceLoginVerify(bot: Bot, url: String): String {
        return GraphicalMiraiBridge.waitingForLoginVeriy(url)
    }
}