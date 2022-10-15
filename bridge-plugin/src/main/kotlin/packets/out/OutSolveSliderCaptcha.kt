package top.mrxiaom.graphicalmirai.packets.out

import kotlinx.serialization.Serializable

@Serializable
data class OutSolveSliderCaptcha(
    val url: String
) : IPacketOut("SolveSliderCaptcha")
