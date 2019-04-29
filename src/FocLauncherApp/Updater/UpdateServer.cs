using System;
using System.IO;
using System.Net;
using FocLauncherApp.Utilities;

namespace FocLauncherApp.Updater
{
    public class UpdateServer
    {
        public string ServerRootAddress { get; }

        public UpdateServer(string baseAddress)
        {
            ServerRootAddress = baseAddress;
        }

        public bool IsRunning() => UrlExists(string.Empty);

        public string DownloadString(string resource)
        {
            string result;
            try
            {
                var webClient = new WebClient();

                var url = UrlCombine.Combine(ServerRootAddress, resource);

                result = webClient.DownloadString(url);
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

        public bool UrlExists(string resource)
        {
            var request = (HttpWebRequest)WebRequest.Create(ServerRootAddress + resource);
            request.Method = "HEAD";
            request.Timeout = 5000;
            try
            {
                request.GetResponse();
                request.Abort();
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response && (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound))
                    return true;
                return false;
            }
            return true;
        }

        public void DownloadFile(string resource, string storagePath)
        {
            if (resource == null || storagePath == null)
                return;
            try
            {
                var webClient = new WebClient();
                var s = ServerRootAddress + resource;
                if (!Directory.Exists(Path.GetDirectoryName(storagePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(storagePath));
                var uri = new Uri(s);
                webClient.DownloadFile(uri, storagePath);
            }
            catch (Exception)
            {
                //Ignored
            }
        }
    }
}
