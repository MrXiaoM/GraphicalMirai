package top.mrxiaom.graphicalmirai.packets.out

import kotlinx.serialization.Serializable

@Serializable
data class OutLoginVerify(
    val url: String
) : IPacketOut("LoginVerify")
