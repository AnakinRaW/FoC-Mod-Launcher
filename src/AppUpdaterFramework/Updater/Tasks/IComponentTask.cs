using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.CommonUtilities.TaskPipeline;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal interface IComponentTask : ITask
{
    IProductComponent Component { get; }
}