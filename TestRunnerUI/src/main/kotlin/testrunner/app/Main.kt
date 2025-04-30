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
    val freePort=findFreePort()
    val args = listOf("--port", "$freePort")
    val hostPath = ConfigurationManager.config.uiHostPath;
    val hostProcess = ProcessBuilder(listOf(hostPath) + args)
        .directory(File(".\\"))
        .redirectErrorStream(true)
        .start()

    Runtime.getRuntime().addShutdownHook(Thread {
        hostProcess?.destroy()
        hostProcess?.waitFor()
    })
    startKoin {
        modules(appModule)
        modules(modelModule(5128))
        modules(viewModelModule)
    }
    println(freePort)

    createUI()
}
fun createUI(){
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