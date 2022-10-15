package top.mrxiaom.graphicalmirai.packets.`in`

import kotlinx.serialization.Serializable
import top.mrxiaom.graphicalmirai.GraphicalMiraiBridge

@Serializable
class InSolveSliderCaptcha(
    val ticket: String
) : IPacketIn {
    override suspend fun handle() {
        if (!GraphicalMiraiBridge.sliderDef.complete(ticket)) {
            GraphicalMiraiBridge.logger.warning("通信桥收到一个滑块验证回调 ticket，但此时并没有进行滑块验证。如需查看 ticket 详见日志 (VERBOSE)。")
        }
        GraphicalMiraiBridge.logger.verbose("slider captcha ticket: $ticket")
    }
}
