package testrunner.app.ui.testexplorer.components

import testrunner.app.domain.entities.Test
import java.awt.BorderLayout
import javax.swing.BoxLayout
import javax.swing.JLabel
import javax.swing.JPanel

class TestSummaryPanel : JPanel() {


    private val testNameLabel = JLabel()
    private val testDurationLabel = JLabel()
    private val errorMessageLabel = JLabel()
    private val stacktraceLabel = JLabel()

    init {
        layout = BoxLayout(this, BoxLayout.Y_AXIS)
        add(testNameLabel)
        add(testDurationLabel)
        add(errorMessageLabel)
        add(stacktraceLabel)
    }

    fun changeTarget(testTreeNode: TestTreeNode, tests: List<Test>) {
        testNameLabel.text = testTreeNode.treeNodeName
        val currentTest = tests.find { it.id == testTreeNode.id }
        if (currentTest != null && currentTest.durationMs > 0) {
            testDurationLabel.text = "Duration: ${currentTest.durationMs}ms"
        }
        errorMessageLabel.text = currentTest?.errorMessage
        stacktraceLabel.text = currentTest?.errorStackTrace
    }

    fun changeTarget(testGroupTreeNode: TestGroupTreeNode, tests: List<Test>) {
        testNameLabel.text = testGroupTreeNode.treeGroupNodeName
        val currentTests = tests.filter { it.id in testGroupTreeNode.innerIds }
        var totalDuration = 0
        for (currentTest in currentTests) {
            totalDuration += currentTest.durationMs
        }
        if (totalDuration > 0) {
            testDurationLabel.text = "Total duration: ${totalDuration}ms"
        }
    }

}