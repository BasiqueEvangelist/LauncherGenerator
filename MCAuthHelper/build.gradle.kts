plugins {
    application
}

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.google.code.gson:gson:2.8.0")
}

tasks.withType<JavaCompile>().configureEach {
    options.encoding = "UTF-8"

    val targetVersion = 8
    if (JavaVersion.current().isJava9Compatible) {
        options.release.set(targetVersion)
    }
}

application {
    mainClass.set("me.basiqueevangelist.launchergenerator.authhelper.MinecraftAuthHelper")
}
