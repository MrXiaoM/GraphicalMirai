plugins {
    val kotlinVersion = "1.7.10"
    kotlin("jvm") version kotlinVersion
    kotlin("plugin.serialization") version kotlinVersion

    id("net.mamoe.mirai-console") version "2.13.0-RC"
}

group = "top.mrxiaom.graphicalmirai"
version = "0.1.0"

repositories {
    mavenCentral()
}

mirai {
    jvmTarget = JavaVersion.VERSION_11
}

tasks.withType<net.mamoe.mirai.console.gradle.BuildMiraiPluginV2> {
    doLast {
        copy {
            from(archiveFile.get().asFile)
            into(File(projectDir.parent))
            rename { "bridge.jar" }
        }
    }
}
