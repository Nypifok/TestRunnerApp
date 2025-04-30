package testrunner.app.ui.testexplorer.components.helpers

import com.formdev.flatlaf.extras.FlatSVGIcon
import testrunner.app.domain.entities.Outcome
import testrunner.app.ui.testexplorer.components.TestGroupTreeNode
import testrunner.app.ui.testexplorer.components.TestTreeNode
import java.awt.Component
import javax.swing.Icon
import javax.swing.JTree
import javax.swing.tree.DefaultMutableTreeNode
import javax.swing.tree.DefaultTreeCellRenderer

class TestTreeCellRenderer : DefaultTreeCellRenderer() {
    override fun getTreeCellRendererComponent(
        tree: JTree,
        value: Any,
        selected: Boolean,
        expanded: Boolean,
        leaf: Boolean,
        row: Int,
        hasFocus: Boolean
    ): Component {
        val component = super.getTreeCellRendererComponent(tree, value, selected, expanded, leaf, row, hasFocus)
        if (value is DefaultMutableTreeNode) {
            val userObject = value.userObject
            if (userObject is TestTreeNode) {
                text = userObject.treeNodeName
                icon = getIconByOutcome(userObject.outcome)
            }
            if (userObject is TestGroupTreeNode) {
                text = userObject.treeGroupNodeName
                icon = getIconByOutcome(userObject.outcome)
            }
        }

        return component
    }
    private fun getIconByOutcome(outcome: Outcome): Icon {
        return when (outcome) {
            Outcome.None -> FlatSVGIcon("icons/adjust.svg")
            Outcome.Passed -> FlatSVGIcon("icons/passed.svg")
            Outcome.Skipped -> FlatSVGIcon("icons/adjust.svg")
            Outcome.Failed -> FlatSVGIcon("icons/failed.svg")
            Outcome.NotFound -> FlatSVGIcon("icons/adjust.svg")
        }
    }
}