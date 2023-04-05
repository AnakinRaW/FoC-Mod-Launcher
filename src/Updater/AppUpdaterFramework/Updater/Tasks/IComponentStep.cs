using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.CommonUtilities.SimplePipeline;

namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

internal interface IComponentStep : IProgressStep
{
    IProductComponent Component { get; }
}