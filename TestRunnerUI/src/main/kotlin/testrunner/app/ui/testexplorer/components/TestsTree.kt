package testrunner.app.ui.testexplorer.components

import testrunner.app.domain.entities.Outcome
import testrunner.app.domain.entities.Test
import testrunner.app.ui.testexplorer.components.helpers.TestTreeCellRenderer
import javax.swing.JLabel
import javax.swing.JTree
import javax.swing.tree.*

data class TestTreeNode(val id: String, val treeNodeName: String, val outcome: Outcome)
data class TestGroupTreeNode(val treeGroupNodeName: String, var outcome: Outcome, val innerIds: MutableList<String>)
class TestsTree : JTree() {
    private val root = DefaultMutableTreeNode(null)
    private val nodeMap = mutableMapOf<String, DefaultMutableTreeNode>()
    init {
        selectionModel.selectionMode = TreeSelectionModel.DISCONTIGUOUS_TREE_SELECTION
        model = DefaultTreeModel(root)
        showsRootHandles = true
        isRootVisible = false
        setCellRenderer(TestTreeCellRenderer())
        buildTree()
    }


    private fun buildTree() {
        rebuildTree(emptyList())
    }
    fun clearTree(){
        root.removeAllChildren()
        nodeMap.clear()
    }
    fun rebuildTree(tests: List<Test>) {
        val expandedPaths = getExpandedPaths()

        for (test in tests) {
            val parts = parseTestName(test.fullyQualifiedName)
            var currentPath = ""
            var parentNode = root
            val pathNodes = mutableListOf<DefaultMutableTreeNode>()

            for ((index, part) in parts.withIndex()) {
                currentPath = if (currentPath.isEmpty()) part else "$currentPath.$part"
                val isLeaf = index == parts.lastIndex

                val node = nodeMap.getOrPut(currentPath) {
                    val nodeObject = if (isLeaf) {
                        TestTreeNode(test.id, part, test.outcome)
                    } else {
                        TestGroupTreeNode(currentPath, Outcome.None, mutableListOf())
                    }
                    val newNode = DefaultMutableTreeNode(nodeObject)
                    parentNode.add(newNode)
                    newNode
                }

                if (isLeaf) {
                    node.userObject = TestTreeNode(test.id, part, test.outcome)
                }

                pathNodes.add(node)
                parentNode = node
            }

            for (node in pathNodes) {
                val obj = node.userObject
                if (obj is TestGroupTreeNode) {
                    obj.innerIds.add(test.id)
                }
            }
        }

        propagateFailures(root)
        (model as DefaultTreeModel).reload()
        restoreExpandedPaths(expandedPaths)
    }

    private fun parseTestName(fullName: String): List<String> {
        val parenIndex = fullName.indexOf('(')
        val nameToSplit = if (parenIndex != -1) fullName.substring(0, parenIndex) else fullName
        val params = if (parenIndex != -1) fullName.substring(parenIndex) else ""

        val parts = nameToSplit.split('.')
        val methodNameWithParams = parts.last() + params

        val classParts = parts.dropLast(1).flatMap { it.split('+') }

        return classParts + methodNameWithParams
    }

    private fun propagateFailures(node: DefaultMutableTreeNode): Outcome {
        if (node.isLeaf) {
            val test = node.userObject
            return if (test is TestTreeNode) test.outcome else Outcome.None
        }

        var hasFailed = false
        var allPassed = true

        for (i in 0 until node.childCount) {
            val child = node.getChildAt(i) as DefaultMutableTreeNode
            val childOutcome = propagateFailures(child)

            if (childOutcome == Outcome.Failed) {
                hasFailed = true
            }
            if (childOutcome != Outcome.Passed) {
                allPassed = false
            }
        }

        val finalOutcome = when {
            hasFailed -> Outcome.Failed
            allPassed -> Outcome.Passed
            else -> Outcome.None
        }

        val userObject = node.userObject
        if (userObject is TestGroupTreeNode) {
            userObject.outcome = finalOutcome
        }

        return finalOutcome
    }

    private fun getExpandedPaths(): List<TreePath> {
        val paths = mutableListOf<TreePath>()
        for (i in 0 until rowCount) {
            val path = getPathForRow(i)
            if (isExpanded(path)) {
                paths.add(path)
            }
        }
        return paths
    }

    private fun restoreExpandedPaths(expandedPaths: List<TreePath>) {
        for (path in expandedPaths) {
            expandPath(path)
        }
    }

    fun getSelectedIds(): List<String> {
        val result = mutableListOf<String>()
        val paths: Array<TreePath>? = selectionPaths
        if (paths.isNullOrEmpty()) return emptyList()
        for (path in paths) {
            val node = path.lastPathComponent as DefaultMutableTreeNode
            val innerData= node.userObject
            if (innerData is TestTreeNode)
                result.add(innerData.id)
            if(innerData is TestGroupTreeNode)
                result.addAll(innerData.innerIds)
        }
        return result
    }
}