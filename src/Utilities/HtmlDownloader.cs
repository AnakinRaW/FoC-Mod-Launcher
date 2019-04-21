using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace FocLauncher.Utilities
{
    internal class HtmlDownloader
    {
        private static string _steamUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=";

        public static async Task<HtmlDocument> GetSteamModPageDocument(string workshopId)
        {
            try
            {
                var address = $"{_steamUrl}{workshopId}";
                var client = new WebClient();
                var reply = await client.DownloadStringTaskAsync(address);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(reply);
                return htmlDocument;
            }
            catch
            {
                return null;
            }
        }
    }
}
