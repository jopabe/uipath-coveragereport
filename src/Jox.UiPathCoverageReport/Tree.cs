using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Jox.UiPathCoverageReport;

class Tree : TreeNode
{
    public Tree(string name) : base(name)
    {
    }

    public bool AddComment(string comment) => GetOrAdd($"({comment})").IsMetadata = true;

    public void AddFile(string path)
    {
        if (Directory.Exists(path))
        {
            path = Path.Combine(path, "project.json");
        }
        AddFile(new FileInfo(path));
    }

    private void AddFile(FileInfo mainFile)
    {
        var projectDir = mainFile.Directory;
        if (mainFile.Name.Equals("project.json", StringComparison.OrdinalIgnoreCase))
        {
            // JSON parsing would be better but requires additional dll
            var mainRegex = new Regex(@"""main"": ""(?<filename>[^""]+\.xaml)"",");
            foreach (var line in File.ReadLines(mainFile.FullName))
            {
                var match = mainRegex.Match(line);
                if (match.Success)
                {
                    mainFile = new FileInfo(Path.Combine(mainFile.DirectoryName, match.Groups["filename"].Value));
                    break;
                }
            }
        }

        AddUiPathXaml(mainFile, projectDir);
    }

    private void AddUiPathXaml(FileInfo xamlFile, DirectoryInfo projectDir)
    {
        var doc = XDocument.Load(xamlFile.FullName);
        var controls = doc.Descendants("{http://schemas.uipath.com/workflow/activities}Target");
        foreach (var control in controls)
        {
            AddSelector(control);
        }

        // if the UiPath XAML file references other files, add them to the tree as well
        var externalFiles = doc.Descendants("{http://schemas.uipath.com/workflow/activities}InvokeWorkflowFile");
        foreach (var externalFile in externalFiles)
        {
            var externalFileName = externalFile.Attribute("WorkflowFileName").Value;
            if (AddComment($"including {externalFileName}"))
            {
                // only process external file the first time we see it
                AddUiPathXaml(new FileInfo(Path.Combine(projectDir.FullName, externalFileName)), projectDir);
            }
        }
    }

    private void AddSelector(XElement control)
    {
        var windowScope = control.Ancestors("{http://schemas.uipath.com/workflow/activities}WindowScope").FirstOrDefault();

        var lineage = XElement.Parse($"<x>{ParseSelectorAttribute(windowScope)}{ParseSelectorAttribute(control)}</x>").Elements();
        TreeNode app;
        TreeNode parent = this;
        foreach (var wnd in lineage)
        {
            var appName = wnd.Attribute("app")?.Value;
            if (appName != null)
            {
                app = GetOrAdd(appName);
                parent = app;
            }
            var controlName = wnd.Attribute("ctrlname")?.Value;
            if (controlName != null)
            {
                parent = parent.GetOrAdd(controlName);
            }
        }
    }

    private static string ParseSelectorAttribute(XElement element)
    {
        if (element == null)
        {
            return string.Empty;
        }
        var attributeValue = element.Attribute("Selector").Value;
        if (attributeValue == "{x:Null}")
        {
            return string.Empty;
        }
        var decoded = WebUtility.HtmlDecode(attributeValue);
        if (decoded.Length > 4 && decoded.Substring(0, 2) == "[\"")
        {
            decoded = decoded.Substring(2, decoded.Length - 4);
        }
        return decoded.Replace("omit:", "").Replace("&", "");
    }


    public static Tree Build(IReadOnlyList<string> files)
    {
        Tree tree;
        if (files.Count == 1)
        {
            var xamlFile = files.First();
            tree = new Tree(xamlFile);
            tree.AddFile(xamlFile);
        }
        else
        {
            tree = new Tree("[multiple sources]");
            foreach (var file in files)
            {
                tree.AddComment($"given source: {file}");
                tree.AddFile(file);
            }
        }

        return tree;
    }
}
