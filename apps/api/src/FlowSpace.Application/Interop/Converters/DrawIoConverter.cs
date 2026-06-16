using System.Xml;
using FlowSpace.Domain.Entities;
using System.Text;
using System.Xml.Linq;

namespace FlowSpace.Application.Interop.Converters;

public static class DrawIoConverter
{
    private static readonly XmlReaderSettings SecureXmlSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };

    public static string ToDrawIoXml(Board board, List<Node> nodes, List<Edge> edges)
    {
        var root = new XElement("mxGraphModel",
            new XElement("root",
                new XElement("mxCell", new XAttribute("id", "0")),
                new XElement("mxCell", new XAttribute("id", "1"), new XAttribute("parent", "0")),
                nodes.Select(n => new XElement("mxCell",
                    new XAttribute("id", n.Id.ToString()),
                    new XAttribute("value", n.Type),
                    new XAttribute("style", GetDrawIoStyle(n)),
                    new XAttribute("vertex", "1"),
                    new XAttribute("parent", "1"),
                    new XElement("mxGeometry",
                        new XAttribute("x", n.X),
                        new XAttribute("y", n.Y),
                        new XAttribute("width", n.Width ?? 100),
                        new XAttribute("height", n.Height ?? 50),
                        new XAttribute("as", "geometry")
                    )
                )),
                edges.Select(e => new XElement("mxCell",
                    new XAttribute("id", e.Id.ToString()),
                    new XAttribute("edge", "1"),
                    new XAttribute("parent", "1"),
                    new XAttribute("source", e.SourceNodeId.ToString()),
                    new XAttribute("target", e.TargetNodeId.ToString()),
                    new XElement("mxGeometry",
                        new XAttribute("relative", "1"),
                        new XAttribute("as", "geometry")
                    )
                ))
            )
        );

        var doc = new XDocument(
            new XElement("mxfile",
                new XAttribute("host", "FlowSpace"),
                new XAttribute("type", "device"),
                new XElement("diagram",
                    new XAttribute("id", board.Id.ToString()),
                    new XAttribute("name", board.Name),
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(root.ToString()))
                )
            )
        );

        return doc.ToString();
    }

    public static (List<Node> Nodes, List<Edge> Edges, string BoardName) FromDrawIoXml(Guid boardId, string xmlContent)
    {
        using var stringReader = new StringReader(xmlContent);
        using var xmlReader = XmlReader.Create(stringReader, SecureXmlSettings);
        
        var doc = XDocument.Load(xmlReader);
        var diagram = doc.Descendants("diagram").FirstOrDefault();
        if (diagram == null) throw new InvalidOperationException("Invalid draw.io file: missing diagram node.");

        var boardName = diagram.Attribute("name")?.Value ?? "Imported draw.io Board";
        var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(diagram.Value));
        
        using var innerReader = new StringReader(decodedContent);
        using var innerXmlReader = XmlReader.Create(innerReader, SecureXmlSettings);
        var model = XDocument.Load(innerXmlReader);

        var cells = model.Descendants("mxCell").ToList();
        var nodeMap = new Dictionary<string, Guid>();
        var nodes = new List<Node>();
        var edges = new List<Edge>();

        // Parse Vertices (Nodes)
        foreach (var cell in cells.Where(c => c.Attribute("vertex")?.Value == "1"))
        {
            var oldId = cell.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
            var type = cell.Attribute("value")?.Value ?? "Rectangle";
            var geo = cell.Element("mxGeometry");
            var x = double.TryParse(geo?.Attribute("x")?.Value, out var valX) ? valX : 0;
            var y = double.TryParse(geo?.Attribute("y")?.Value, out var valY) ? valY : 0;
            var w = double.TryParse(geo?.Attribute("width")?.Value, out var valW) ? valW : 100;
            var h = double.TryParse(geo?.Attribute("height")?.Value, out var valH) ? valH : 50;

            var newId = Guid.NewGuid();
            nodeMap[oldId] = newId;

            var node = Node.Create(newId, boardId, type, x, y);
            node.Update(x, y, w, h);
            nodes.Add(node);
        }

        // Parse Edges
        foreach (var cell in cells.Where(c => c.Attribute("edge")?.Value == "1"))
        {
            var sourceId = cell.Attribute("source")?.Value;
            var targetId = cell.Attribute("target")?.Value;

            if (sourceId != null && targetId != null && nodeMap.ContainsKey(sourceId) && nodeMap.ContainsKey(targetId))
            {
                edges.Add(Edge.Create(Guid.NewGuid(), boardId, nodeMap[sourceId], nodeMap[targetId]));
            }
        }

        return (nodes, edges, boardName);
    }

    private static string GetDrawIoStyle(Node node) => node.Type.ToLower() switch
    {
        "rectangle" => "rounded=0;whiteSpace=wrap;html=1;",
        "circle" => "ellipse;whiteSpace=wrap;html=1;",
        "diamond" => "rhombus;whiteSpace=wrap;html=1;",
        _ => "whiteSpace=wrap;html=1;"
    };
}
