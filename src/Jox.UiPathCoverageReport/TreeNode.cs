namespace Jox.UiPathCoverageReport;

class TreeNode : IComparable<TreeNode>, IEnumerable<TreeNode>
{
    private readonly ISet<TreeNode> children = new SortedSet<TreeNode>();
    public bool IsMetadata { get; set; } = false;
    public string Name { get; }

    public TreeNode(string name)
    {
        Name = name;
    }

    public TreeNode GetOrAdd(string name)
    {
        var child = children.FirstOrDefault(n => n.Name == name);
        if (child == null)
        {
            child = new TreeNode(name);
            children.Add(child);
        }
        return child;
    }

    public bool IsLeaf => children.Count == 0;

    public int CompareTo(TreeNode other) => Name.CompareTo(other.Name);

    public IEnumerator<TreeNode> GetEnumerator() => children.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
