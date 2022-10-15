plugins {
    val kotlinVersion = "1.7.10"
    kotlin("jvm") version kotlinVersion
    kotlin("plugin.serialization") version kotlinVersion

    id("net.mamoe.mirai-console") version "2.12.3"
}

group = "top.mrxiaom.graphicalmirai"
version = "0.1.0"

repositories {
    maven("https://maven.aliyun.com/repository/central")
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
