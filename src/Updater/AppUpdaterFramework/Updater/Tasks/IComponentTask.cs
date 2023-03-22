using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.CommonUtilities.TaskPipeline;

namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

internal interface IComponentTask : ITask
{
    IProductComponent Component { get; }
}