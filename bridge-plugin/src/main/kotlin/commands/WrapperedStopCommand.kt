package top.mrxiaom.graphicalmirai.commands

import net.mamoe.mirai.console.command.BuiltInCommands.StopCommand
import net.mamoe.mirai.console.command.BuiltInCommands.StopCommand.handle
import net.mamoe.mirai.console.command.CommandManager.INSTANCE.register
import net.mamoe.mirai.console.command.CommandManager.INSTANCE.unregister
import net.mamoe.mirai.console.command.CommandSender
import net.mamoe.mirai.console.command.ConsoleCommandOwner
import net.mamoe.mirai.console.command.SimpleCommand
import top.mrxiaom.graphicalmirai.GraphicalMiraiBridge

/**
 * 不明原因死活不会执行 onDisable
 * 只好包装一下 /stop
 */
object WrapperedStopCommand : SimpleCommand(
    owner = ConsoleCommandOwner,
    primaryName = StopCommand.primaryName,
    secondaryNames =  StopCommand.secondaryNames,
    description = StopCommand.description
) {
    internal fun hack(){
        StopCommand.unregister()
        register()
    }
    @Handler
    suspend fun CommandSender.stop() {
        GraphicalMiraiBridge.disable()
        handle()
    }
}