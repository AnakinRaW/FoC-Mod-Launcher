using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Updater;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

public interface IUpdateResultHandler
{
    Task Handle(UpdateResult result);
}