using System;
using System.Xml.Serialization;

namespace AvaloniaFirstApp.Models;

[Serializable]
public enum SearchObjectType : byte
{
    [XmlEnum]
    None = 0,
    [XmlEnum]
    Circle,
    [XmlEnum]
    Line,
    [XmlEnum]
    NotDirectLine,
}
