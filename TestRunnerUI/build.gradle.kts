import com.google.protobuf.gradle.id

val grpcVersion = "1.72.0"
val grpcKotlinVersion = "1.4.3"
val protobufVersion = "4.30.0"



group = "dev.nypifok"
version = "1.0-SNAPSHOT"

application {
    mainClass.set("testrunner.app.MainKt")
}

java {
    toolchain {
        languageVersion.set(JavaLanguageVersion.of(17))
    }
}

plugins {
    kotlin("jvm") version "2.1.20"
    kotlin("plugin.serialization") version "2.1.20"
    id("com.google.protobuf") version "0.9.5"
    application
}

repositories {
    mavenCentral()
    google()
}




dependencies {
    implementation("io.grpc:grpc-netty-shaded:${grpcVersion}")
    implementation("io.grpc:grpc-protobuf-lite:${grpcVersion}")
    implementation("io.grpc:grpc-protobuf:${grpcVersion}")
    implementation("io.grpc:grpc-stub:${grpcVersion}")
    implementation("io.grpc:grpc-kotlin-stub:${grpcKotlinVersion}")
    implementation("com.google.protobuf:protobuf-javalite:${protobufVersion}")
    implementation("com.google.protobuf:protobuf-java:${protobufVersion}")
    implementation("com.google.protobuf:protobuf-kotlin-lite:${protobufVersion}")
    implementation("org.jetbrains.kotlinx:kotlinx-serialization-json-jvm:1.8.1")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.10.2")
    implementation("io.insert-koin:koin-core:4.0.4")
    //UI section

    implementation("com.formdev:flatlaf:3.6")
    implementation("com.formdev:flatlaf-extras:3.6")
}

protobuf {
    protoc {
        artifact = "com.google.protobuf:protoc:${protobufVersion}"
    }

    plugins {
        id("grpc") {
            artifact = "io.grpc:protoc-gen-grpc-java:${grpcVersion}"
        }
        id("grpckt") {
            artifact = "io.grpc:protoc-gen-grpc-kotlin:${grpcKotlinVersion}:jdk8@jar"
        }
    }

    generateProtoTasks {
        all().forEach { task ->
            task.plugins {
                id("grpc") {
                    option("lite")
                }
                id("grpckt") {
                    option("lite")
                }
                task.builtins {
                    id("kotlin")
                }
            }
        }
    }
}

sourceSets {
    main {
        proto {
            srcDir("../Shared/Protos")
        }

        kotlin {
            srcDirs(
                "build/generated/source/proto/main/grpckt"
            )
        }
    }
}

tasks.test {
    useJUnitPlatform()
}