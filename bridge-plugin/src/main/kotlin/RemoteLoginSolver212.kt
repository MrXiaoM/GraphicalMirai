package top.mrxiaom.graphicalmirai

import net.mamoe.mirai.Bot
import net.mamoe.mirai.utils.LoginSolver
import net.mamoe.mirai.utils.MiraiExperimentalApi
import net.mamoe.mirai.utils.StandardCharImageLoginSolver
import net.mamoe.mirai.utils.SwingSolver

object RemoteLoginSolver212 : LoginSolver() {
    override val isSliderCaptchaSupported: Boolean = true
    val std = StandardCharImageLoginSolver()
    @OptIn(MiraiExperimentalApi::class)
    override suspend fun onSolvePicCaptcha(bot: Bot, data: ByteArray): String {
        return SwingSolver.onSolvePicCaptcha(bot, data)
    }

    override suspend fun onSolveSliderCaptcha(bot: Bot, url: String): String {
        GraphicalMiraiBridge.waitingForTicket(url)
        return std.onSolveSliderCaptcha(bot, url)
    }

    override suspend fun onSolveUnsafeDeviceLoginVerify(bot: Bot, url: String): String {
        GraphicalMiraiBridge.waitingForLoginVeriy(url)
        return std.onSolveUnsafeDeviceLoginVerify(bot, url)
    }
}