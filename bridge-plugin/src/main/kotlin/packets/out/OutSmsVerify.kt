package top.mrxiaom.graphicalmirai.packets.out

import kotlinx.serialization.Serializable

@Serializable
data class OutSmsVerify(
    val countryCode: String,
    val phoneNumber: String
) : IPacketOut("SmsVerify")
