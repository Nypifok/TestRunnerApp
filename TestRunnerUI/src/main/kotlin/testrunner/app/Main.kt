package testrunner.app

import com.formdev.flatlaf.FlatDarculaLaf
import org.koin.core.component.KoinComponent
import org.koin.core.context.startKoin
import testrunner.app.configuration.ConfigurationManager
import testrunner.app.di.appModule
import testrunner.app.di.modelModule
import testrunner.app.di.viewModelModule
import testrunner.app.ui.MainView
import java.io.File
import java.net.ServerSocket

object DI : KoinComponent

fun main() {
    val currentPid = ProcessHandle.current().pid()
    val freePort = findFreePort()
    val args = listOf("--port", "$freePort", "--parent", currentPid.toString())
    val hostPath = ConfigurationManager.config.uiHostPath;
    val hostProcess = ProcessBuilder(listOf(hostPath) + args)
        .directory(File(hostPath).parentFile)
        .redirectErrorStream(true)
        .start()

    val reader = hostProcess.inputStream.bufferedReader()
    while (true) {
        val line = reader.readLine() ?: break
        if (line.contains("UIHOST_READY")) {
            break
        }
    }

    Runtime.getRuntime().addShutdownHook(Thread {
        hostProcess?.destroy()
        hostProcess?.destroyForcibly()
        hostProcess?.waitFor()
    })
    startKoin {
        modules(appModule)
        modules(modelModule(freePort))
        modules(viewModelModule)
    }

    createUI()
}

fun createUI() {
    FlatDarculaLaf.setup()
    javax.swing.SwingUtilities.invokeLater {
        MainView()
    }
}

fun findFreePort(): Int {
    ServerSocket(0).use { socket ->
        return socket.localPort
    }
}