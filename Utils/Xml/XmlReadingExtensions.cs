using System.Xml;
using System.Xml.XPath;

namespace Utils.Xml;

public static class XmlReadingExtensions
{
    public static XmlNodeList GetChildNodes(this XmlNode root, string nodeName)
    {
        var node = root.SelectSingleNode(nodeName);
        if (node is null)
            throw new XPathException($"Not found node with name '{nodeName}'");

        return node.ChildNodes;
    }
}
