package testrunner.app.ui.testexplorer.components

import javax.swing.JLabel
import javax.swing.JPanel

class TestSummaryPanel : JPanel() {


    private val testNameLabel = JLabel()
    private val testDurationLabel = JLabel()
    init {
        add(testNameLabel)
        add(testDurationLabel)
    }

    fun changeTarget(testTreeNode: TestTreeNode) {
        testNameLabel.text = testTreeNode.treeNodeName
    }

    fun changeTarget(testGroupTreeNode: TestGroupTreeNode) {
        testNameLabel.text = testGroupTreeNode.treeGroupNodeName
    }

}