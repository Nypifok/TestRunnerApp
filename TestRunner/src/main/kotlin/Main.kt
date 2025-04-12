package dev.nypifok
import javax.swing.*
//TIP To <b>Run</b> code, press <shortcut actionId="Run"/> or
// click the <icon src="AllIcons.Actions.Execute"/> icon in the gutter.
fun main() {
    SwingUtilities.invokeLater {
        createUI()
    }
}

fun createUI() {
    val frame = JFrame("Моё первое приложение")
    frame.defaultCloseOperation = JFrame.EXIT_ON_CLOSE
    frame.setSize(400, 300)

    val label = JLabel("Привет, мир!", SwingConstants.CENTER)
    frame.contentPane.add(label)

    frame.isVisible = true
}