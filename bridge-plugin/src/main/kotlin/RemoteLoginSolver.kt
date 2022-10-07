package top.mrxiaom.graphicalmirai

import net.mamoe.mirai.Bot
import net.mamoe.mirai.utils.LoginSolver
import net.mamoe.mirai.utils.MiraiExperimentalApi
import net.mamoe.mirai.utils.SwingSolver

object RemoteLoginSolver : LoginSolver() {
    @OptIn(MiraiExperimentalApi::class)
    override suspend fun onSolvePicCaptcha(bot: Bot, data: ByteArray): String {
        return SwingSolver.onSolvePicCaptcha(bot, data)
    }

    override suspend fun onSolveSliderCaptcha(bot: Bot, url: String): String {
        return GraphicalMiraiBridge.waitingForTicket(bot, url)
    }

    @OptIn(MiraiExperimentalApi::class)
    override suspend fun onSolveUnsafeDeviceLoginVerify(bot: Bot, url: String): String {
        return SwingSolver.onSolveUnsafeDeviceLoginVerify(bot, url)
    }
}