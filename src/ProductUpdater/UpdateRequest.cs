using Sklavenwalker.ProductMetadata.Manifest;
using Validation;

namespace Sklavenwalker.ProductUpdater
{
    public class UpdateRequest
    {
        public ManifestLocation ManifestLocation { get; }

        public UpdateRequest(ManifestLocation manifestLocation)
        {
            Requires.NotNull(manifestLocation, nameof(manifestLocation));
            ManifestLocation = manifestLocation;
        }
    }
}
