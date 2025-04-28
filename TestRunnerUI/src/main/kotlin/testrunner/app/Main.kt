package testrunner.app

import org.koin.core.context.startKoin
import processing_notification_service.TestSessionServiceOuterClass.DiscoverTestsRequest
import java.io.File
import java.net.ServerSocket


fun main() {

    val args = listOf("--port", "${findFreePort()}")
    val hostPath="H:\\TestRunnerApp\\TestRunnerBackend\\src\\TestRunner.UIHost\\bin\\Release\\net8.0\\win-x64\\publish\\TestRunner.UIHost.exe"
    val hostProcess = ProcessBuilder(listOf(hostPath) + args)
        .directory(File(".\\"))
        .redirectErrorStream(true)
        .start()

    Runtime.getRuntime().addShutdownHook(Thread {
        hostProcess?.destroy()
        hostProcess?.waitFor()
    })

}

fun findFreePort(): Int {
    ServerSocket(0).use { socket ->
        return socket.localPort
    }
}