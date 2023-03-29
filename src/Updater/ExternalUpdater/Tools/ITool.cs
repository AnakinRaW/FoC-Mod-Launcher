using System.Threading.Tasks;

namespace AnakinRaW.ExternalUpdater.Tools;

internal interface ITool
{
    Task<ExternalUpdaterResult> Run();
}