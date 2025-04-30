package testrunner.app.ui.testexplorer.components

import com.formdev.flatlaf.extras.FlatSVGIcon
import java.awt.event.ActionListener
import javax.swing.JButton
import javax.swing.JToolBar

class ToolPanel() : JToolBar() {
    private val runSelectedButton = JButton(FlatSVGIcon("icons/play_arrow.svg"))
    private val runAllButton = JButton(FlatSVGIcon("icons/play_all_arrow.svg"))
    private val cancelButton = JButton(FlatSVGIcon("icons/stop.svg"))
    init {
        runAllButton.toolTipText = "Run all"
        this.add(runAllButton)

        runSelectedButton.toolTipText = "Run selected"
        this.add(runSelectedButton)

        cancelButton.toolTipText = "Cancel"
        cancelButton.isEnabled = false
        this.add(cancelButton)

        this.addSeparator()
    }

    fun disableTestRunning(){
        runAllButton.setEnabled(false)
        runSelectedButton.setEnabled(false)
    }
    fun enableTestRunning(){
        runAllButton.setEnabled(true)
        runSelectedButton.setEnabled(true)
    }
    fun enableCancellation(){
        cancelButton.setEnabled(true)
    }
    fun disableCancellation(){
        cancelButton.setEnabled(false)
    }
    fun setRunAllTestsActionListener(listener: ActionListener){
        for (l in runAllButton.actionListeners) {
            runAllButton.removeActionListener(listener)
        }

        runAllButton.addActionListener(listener)
    }
    fun setRunSelectedTestsActionListener(listener: ActionListener){
        for (l in runSelectedButton.actionListeners) {
            runSelectedButton.removeActionListener(listener)
        }

        runSelectedButton.addActionListener(listener)
    }
    fun setCancelActionListener(listener: ActionListener){
        for (l in cancelButton.actionListeners) {
            cancelButton.removeActionListener(listener)
        }

        cancelButton.addActionListener(listener)
    }
}