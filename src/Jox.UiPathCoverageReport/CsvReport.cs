namespace Jox.UiPathCoverageReport;

class CsvReport : IReport
{
    public string Separator { get; set; } = ";";

    public void Print(Tree tree, TextWriter output) => PrintNode(tree, output, prefix: "");

    void PrintNode(TreeNode node, TextWriter output, string prefix)
    {
        prefix = prefix.Length == 0 ? node.Name : string.Concat(prefix, Separator, node.Name);
        if (node.IsLeaf && !node.IsMetadata)
        {
            output.WriteLine(prefix);
        }
        else
        {
            foreach (var child in node)
            {
                PrintNode(child, output, prefix);
            }
        }
    }
}
