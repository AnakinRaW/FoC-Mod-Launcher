using System;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Services;

namespace FocLauncher.Update.ProductMetadata
{
    internal class LauncherProductService : ProductServiceBase
    {
        public LauncherProductService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IProductReference CreateProductReference(Version? newVersion, string? branch)
        {
            throw new NotImplementedException();
        }

        protected override IInstalledProduct BuildProduct()
        {
            throw new NotImplementedException();
        }
    }
}
