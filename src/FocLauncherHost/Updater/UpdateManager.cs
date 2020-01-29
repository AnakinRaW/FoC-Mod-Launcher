using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FocLauncherHost.Utilities;

namespace FocLauncherHost.Updater
{
    internal class UpdateManager
    {
        public Uri UpdateMetadataLocation { get; }

        public UpdateManager(string versionMetadataPath)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateMetadataLocation = metadataUri;
        }

        public async Task<UpdateInformation> CheckAndPerformUpdateAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var updateInformation = new UpdateInformation();

            var stream = await GetMetadataStreamAsync(cts.Token);

            if (stream == null || stream.Length == 0)
            {
                updateInformation.Result = UpdateResult.Error;
                updateInformation.Message = "Unable to get the update metadata stream";
                return updateInformation;
            }


            return updateInformation;
        }


        public async Task<Stream> GetMetadataStreamAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Stream metadataStream = new MemoryStream();
            if (UpdateMetadataLocation.Scheme == Uri.UriSchemeFile)
                await StreamUtilities.CopyFileToStreamAsync(UpdateMetadataLocation.LocalPath, metadataStream, cancellation);

            if (UpdateMetadataLocation.Scheme == Uri.UriSchemeHttp ||
                UpdateMetadataLocation.Scheme == Uri.UriSchemeHttps)
            {
                throw new NotImplementedException();
            }
            return null;
        }
    }

    public class UpdateInformation
    {
        public UpdateResult Result { get; set; }

        public bool RequiresUserNotification { get; set; }

        public string Message { get; set; }
    }

    public enum UpdateResult
    {
        Success,
        Error,
        HashMismatch
    }
}
