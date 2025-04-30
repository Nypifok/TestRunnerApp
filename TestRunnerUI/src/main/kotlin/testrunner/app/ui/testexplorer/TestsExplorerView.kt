package testrunner.app.ui.testexplorer

import kotlinx.coroutines.*
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.onEach
import org.koin.core.parameter.parametersOf
import org.koin.java.KoinJavaComponent.inject
import testrunner.app.domain.entities.Outcome
import testrunner.app.domain.entities.Test
import testrunner.app.ui.testexplorer.components.*
import testrunner.app.viewmodel.TestExplorerViewModel
import java.awt.BorderLayout
import java.awt.Dimension
import java.awt.FlowLayout
import javax.swing.*
import javax.swing.event.TreeSelectionEvent
import javax.swing.event.TreeSelectionListener
import javax.swing.tree.DefaultMutableTreeNode
import javax.swing.tree.TreePath

class TestsExplorerView : JPanel() {
    private val testExplorerScope = CoroutineScope(SupervisorJob() + Dispatchers.Default)

    private val viewModel: TestExplorerViewModel by inject(TestExplorerViewModel::class.java) {
        parametersOf(
            testExplorerScope
        )
    }

    private val toolPanel: ToolPanel = ToolPanel()
    private val statsPanel: StatsPanel = StatsPanel()
    private val testsTree: TestsTree = TestsTree()
    private val testSummary: TestSummaryPanel = TestSummaryPanel()

    init {
        //UI
        layout = BoxLayout(this, BoxLayout.Y_AXIS)
        val utilityBar = JPanel(BorderLayout())
        utilityBar.layout = FlowLayout(FlowLayout.LEFT)
        utilityBar.add(toolPanel)
        utilityBar.add(statsPanel)


        val treePanel = JPanel(BorderLayout())
        treePanel.add(JLabel(" Tests Explorer"), BorderLayout.NORTH)
        treePanel.add(JScrollPane(testsTree), BorderLayout.CENTER)
        treePanel.minimumSize = Dimension(200, 200)

        val summaryPanel = JPanel(BorderLayout())
        summaryPanel.add(JLabel("Summary:"), BorderLayout.NORTH)
        summaryPanel.add(JScrollPane(testSummary), BorderLayout.CENTER)
        summaryPanel.minimumSize = Dimension(200, 200)


        val mainPanel = JPanel(BorderLayout())
        val splitPane = JSplitPane(JSplitPane.HORIZONTAL_SPLIT, treePanel, summaryPanel)
        splitPane.resizeWeight = .5
        mainPanel.add(splitPane, BorderLayout.CENTER)


        add(utilityBar, BorderLayout.NORTH)
        add(mainPanel, BorderLayout.CENTER)

        //Logic
        addDefaultListeners()
        observeTests()
    }


    private fun addDefaultListeners() {
        toolPanel.setRunAllTestsActionListener {
            testExplorerScope.launch {
                toolPanel.enableCancellation()
                toolPanel.disableTestRunning()
                delay(1)
                viewModel.runAllTests()
                toolPanel.enableTestRunning()
                toolPanel.disableCancellation()
            }
        }

        toolPanel.setRunSelectedTestsActionListener() {
            testExplorerScope.launch {
                toolPanel.disableTestRunning()
                toolPanel.enableCancellation()
                delay(1)
                val selectedIds = testsTree.getSelectedIds()
                viewModel.runSelectedTests(viewModel.tests.value.filter{it.id in selectedIds})
                toolPanel.enableTestRunning()
                toolPanel.disableCancellation()
            }
        }
        toolPanel.setCancelActionListener() {
            testExplorerScope.launch {
                toolPanel.disableTestRunning()
                delay(1)
                viewModel.cancelOperation()
                toolPanel.disableCancellation()
                toolPanel.enableTestRunning()
            }
        }
        testsTree.addTreeSelectionListener { event: TreeSelectionEvent ->
            val selectedNode = event.path.lastPathComponent as? DefaultMutableTreeNode
            val userObject = selectedNode?.userObject
            if (userObject is TestTreeNode) {
                testSummary.changeTarget(userObject)
            }
            if (userObject is TestGroupTreeNode) {
                testSummary.changeTarget(userObject)
            }
        }
    }


    private fun observeTests() {
        viewModel.tests.onEach { updatedTests ->
                SwingUtilities.invokeLater {
                    updateStatsPanel(updatedTests)
                    testsTree.rebuildTree(updatedTests)
                }
            }.launchIn(testExplorerScope)
    }

    private fun updateStatsPanel(tests: List<Test>) {
        statsPanel.updateTotalCounter(tests.size)
        statsPanel.updateFailedCounter(tests.filter { it.outcome == Outcome.Failed }.size)
        statsPanel.updatePassedCounter(tests.filter { it.outcome == Outcome.Passed }.size)
    }

    fun changeTargets(fileNames: List<String>) {
        testExplorerScope.launch {
            toolPanel.disableTestRunning()
            toolPanel.enableCancellation()
            testsTree.clearTree()
            delay(1)
            viewModel.discoverTests(fileNames)
            toolPanel.enableTestRunning()
            toolPanel.disableCancellation()
        }
    }
}