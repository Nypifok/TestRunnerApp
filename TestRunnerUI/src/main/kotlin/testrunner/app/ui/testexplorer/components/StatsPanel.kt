package testrunner.app.ui.testexplorer.components

import com.formdev.flatlaf.extras.FlatSVGIcon
import java.awt.FlowLayout
import javax.swing.*

class StatsPanel() : JToolBar() {
    private val passedTestsButton = JToggleButton(FlatSVGIcon("icons/passed.svg"))
    private val failedTestsButton = JToggleButton(FlatSVGIcon("icons/failed.svg"))

    private val totalText = "Total: "
    private val totalTestsLabel = JLabel(totalText + 0)

    init {
        layout = FlowLayout(FlowLayout.LEFT, 6, 0)
        this.add(totalTestsLabel)

        passedTestsButton.toolTipText = "Passed total"
        passedTestsButton.setSelected(true)
        passedTestsButton.text = "0"
        this.add(passedTestsButton)

        failedTestsButton.toolTipText = "Failed total"
        failedTestsButton.setSelected(true)
        failedTestsButton.text = "0"
        this.add(failedTestsButton)

        this.addSeparator()
    }

    fun updateTotalCounter(total: Int) {
        totalTestsLabel.text = totalText + total
    }

    fun updateFailedCounter(failed: Int) {
        failedTestsButton.text = failed.toString()
    }

    fun updatePassedCounter(passed: Int) {
        passedTestsButton.text = passed.toString()
    }
}