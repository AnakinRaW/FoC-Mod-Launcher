using System.Xml;
using HtmlAgilityPack;

namespace FocLauncher.Game
{
    internal class WorkshopNameResolver
    {
        public string GetName(HtmlDocument htmlDocument, string workshopId)
        {
            if (htmlDocument == null)
                return workshopId;
            var node = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'workshopItemTitle')]");
            return node.InnerHtml;
        }
    }
}
