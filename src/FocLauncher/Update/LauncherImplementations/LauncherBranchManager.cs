using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product;
using Flurl;

namespace FocLauncher.Update.LauncherImplementations;

public class LauncherBranchManager : BranchManager
{
    internal const string StableBranchNameConst = "stable";

    private const string BranchLookupFileName = "branches";
    private const string ManifestFileName = "manifest.json";

    private static readonly Url BranchLookupUrl =
        LauncherConstants.LauncherRootUrl.AppendPathSegment(BranchLookupFileName);

    public override string StableBranchName => StableBranchNameConst;

    public LauncherBranchManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task<IEnumerable<ProductBranch>> GetAvailableBranches()
    {

        var branchesData = await new WebClient().DownloadDataTaskAsync(BranchLookupUrl.ToUri());
        var branchNames = Encoding.UTF8.GetString(branchesData).Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (!branchNames.Any())
            throw new InvalidOperationException($"No branches detected in '{BranchLookupUrl}'");
        var branches = new List<ProductBranch>();
        foreach (var name in branchNames)
        {
            var isPrerelease = !name.Equals(StableBranchName, StringComparison.InvariantCultureIgnoreCase);
            branches.Add(new ProductBranch(name, BuildManifestUri(name), isPrerelease));
        }
        return branches;
    }

    protected override Uri BuildManifestUri(string branchName)
    {
        return new Uri(@"C:\manifest.json", UriKind.Absolute);
        //return LauncherConstants.LauncherRootUrl.AppendPathSegments(branchName, ManifestFileName).ToUri();
    }
}