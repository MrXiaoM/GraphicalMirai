package top.mrxiaom.graphicalmirai.packets.`in`

import kotlinx.serialization.Serializable
import okhttp3.internal.wait
import top.mrxiaom.graphicalmirai.GraphicalMiraiBridge

@Serializable
class InLoginVerify : IPacketIn {
    override suspend fun handle() {
        GraphicalMiraiBridge.defQR.complete(true)
    }
}