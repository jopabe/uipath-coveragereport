namespace Jox.UiPathCoverageReport;

class LineGraphReport : IReport
{
    public void Print(Tree tree, TextWriter output) => PrintNode(tree, output, indent: "");

    static void PrintNode(TreeNode node, TextWriter output, string indent)
    {
        output.WriteLine(node.Name);
        if (node.IsLeaf)
        {
            return;
        }

        TreeNode child = null;
        foreach (var next in node)
        {
            if (child != null)
            {
                output.Write(indent);
                output.Write(" ├─");
                PrintNode(child, output, indent + " │ ");
            }
            child = next;
        }
        // last child gets nice corner
        output.Write(indent);
        output.Write(" └─");
        PrintNode(child, output, indent + "   ");
    }
}
