package testrunner.app.ui

import testrunner.app.ui.testexplorer.TestsExplorerView
import java.awt.BorderLayout
import java.awt.Dimension
import java.awt.event.ActionEvent
import javax.swing.*
import javax.swing.filechooser.FileNameExtensionFilter

class MainView() : JFrame() {

    private val testExplorer = TestsExplorerView()

    init {
        title = "Test Runner"
        defaultCloseOperation = EXIT_ON_CLOSE
        extendedState = MAXIMIZED_BOTH
        isVisible = true
        minimumSize = Dimension(800, 600)

        val menuBar = JMenuBar()
        val fileMenu = JMenu("File")
        val openItem = JMenuItem("Select files")

        openItem.addActionListener { e: ActionEvent ->
            val fileChooser = JFileChooser()

            val filter = FileNameExtensionFilter("Test builds", "dll", "exe")
            fileChooser.isMultiSelectionEnabled = true
            fileChooser.fileSelectionMode = JFileChooser.FILES_ONLY
            fileChooser.fileFilter = filter
            fileChooser.isAcceptAllFileFilterUsed = false

            val result = fileChooser.showOpenDialog(this)

            if (result == JFileChooser.APPROVE_OPTION) {
                val selectedFiles = fileChooser.selectedFiles
                testExplorer.changeTargets(selectedFiles.map { it.absolutePath })
            }
        }

        fileMenu.add(openItem)
        menuBar.add(fileMenu)


        jMenuBar = menuBar
        add(testExplorer, BorderLayout.CENTER)
    }
}