package top.mrxiaom.graphicalmirai.events

import net.mamoe.mirai.event.AbstractEvent
import net.mamoe.mirai.event.CancellableEvent

class BridgeDataPreReceive(
    val data: String
) : AbstractEvent(), CancellableEvent
