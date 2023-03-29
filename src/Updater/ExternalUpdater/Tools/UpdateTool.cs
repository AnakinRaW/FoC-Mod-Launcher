using System;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.Options;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ExternalUpdater.Tools;

internal sealed class UpdateTool : ProcessTool<UpdateOptions>
{
    public UpdateTool(UpdateOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    public override async Task<ExternalUpdaterResult> Run()
    {
        try
        {
            await WaitForProcessExitAsync();
            var updateItems = await Options.GetUpdateInformationAsync(ServiceProvider);
            
            var updater = new Utilities.ExternalUpdater(updateItems, ServiceProvider);
            var updateResult = updater.Run();
            
            Logger?.LogDebug($"Updated with result: {updateResult}");
            StartProcess(updateResult);
            return updateResult;
        }
        finally
        {
            if (!string.IsNullOrEmpty(Options.UpdateFile)) 
                FileSystem.File.Delete(Options.UpdateFile!);
        }
    }
}