using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ExternalUpdater.Options;

[Verb("update", HelpText = "Updates the given application and restarts it")]
public sealed record UpdateOptions : ExternalUpdaterOptions
{
    private IReadOnlyCollection<UpdateInformation>? _updateItems;
    
    [Option("updatePayload", Required = false, HelpText = "Payload in base64 format which is required for the update")]
    public string? Payload { get; init; }

    [Option("updateFile", Required = false, HelpText = "JSON file where update data is stored.")]
    public string? UpdateFile { get; init; }

    public async Task<IReadOnlyCollection<UpdateInformation>> GetUpdateInformationAsync(IServiceProvider serviceProvider)
    {
        return _updateItems ??= await CreateUpdateInformation(serviceProvider).ConfigureAwait(false);
    }

    internal IReadOnlyCollection<UpdateInformation> GetUpdateInformation(IServiceProvider serviceProvider)
    {
        var infoTask = Task.Run(async () => await GetUpdateInformationAsync(serviceProvider));
        infoTask.Wait();
        return infoTask.Result;
    }

    private async Task<IReadOnlyCollection<UpdateInformation>> CreateUpdateInformation(IServiceProvider serviceProvider)
    {
        var information = await GetFromFile(serviceProvider);
        if (information is not null)
            return information;
        information = await GetFromPayload();
        return information ?? Array.Empty<UpdateInformation>();
    }
    
    private async Task<IReadOnlyCollection<UpdateInformation>?> GetFromFile(IServiceProvider serviceProvider)
    {
        var fs = serviceProvider.GetRequiredService<IFileSystem>();
        if (string.IsNullOrEmpty(UpdateFile) || !fs.File.Exists(UpdateFile))
            return null;
#if NETSTANDARD2_1
        await using var fileStream = fs.FileStream.New(UpdateFile, FileMode.Open, FileAccess.Read, FileShare.Read);
#else
        using var fileStream = fs.FileStream.New(UpdateFile, FileMode.Open, FileAccess.Read, FileShare.Read);
#endif
        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<UpdateInformation>>(fileStream, JsonSerializerOptions.Default);

    }

    private async Task<IReadOnlyCollection<UpdateInformation>?> GetFromPayload()
    {
        if (string.IsNullOrEmpty(Payload))
            return null;

        var decoded = Convert.FromBase64String(Payload!);
#if NETSTANDARD2_1
        await using var ms = new MemoryStream(decoded, false);
#else
        using var ms = new MemoryStream(decoded, false);
#endif
        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<UpdateInformation>>(ms, JsonSerializerOptions.Default);
    }
}