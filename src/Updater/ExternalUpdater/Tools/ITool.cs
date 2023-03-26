using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.CLI;

namespace AnakinRaW.ExternalUpdater.Tools;

internal interface ITool
{
    Task<ExternalUpdaterResult> Run();
}