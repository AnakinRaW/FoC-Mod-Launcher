using System.Xml;

namespace FocLauncher.Utilities
{
    public static class XmlTools
    {
        public static string GetNodeValue(string filePath, string nodePath)
        {
            var xml = new XmlDocument();
            xml.Load(filePath);
            var selectSingleNode = xml.SelectSingleNode(nodePath);
            return selectSingleNode?.InnerText;
        }
    }
}
