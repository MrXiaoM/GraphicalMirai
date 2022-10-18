package top.mrxiaom.graphicalmirai

import kotlinx.coroutines.coroutineScope
import net.mamoe.mirai.Bot
import net.mamoe.mirai.network.UnsupportedSmsLoginException
import net.mamoe.mirai.utils.*

object RemoteLoginSolver213 : LoginSolver() {
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

    override suspend fun onSolveDeviceVerification(
        bot: Bot,
        requests: DeviceVerificationRequests,
    ): DeviceVerificationResult {
        requests.fallback?.let { fallback ->
            GraphicalMiraiBridge.waitingForLoginVeriy(fallback.url)
        }
        requests.sms?.let { sms ->
            GraphicalMiraiBridge.waitingForSmsVerify(sms.countryCode, sms.phoneNumber)
        }
        return std.onSolveDeviceVerification(bot, requests)
    }

    override suspend fun onSolveUnsafeDeviceLoginVerify(bot: Bot, url: String): String {
        GraphicalMiraiBridge.waitingForLoginVeriy(url)
        return std.onSolveUnsafeDeviceLoginVerify(bot, url)
    }
}