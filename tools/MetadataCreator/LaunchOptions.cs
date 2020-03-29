using CommandLine;
using FocLauncher;

namespace MetadataCreator
{
    internal class LaunchOptions
    {
        [Option('r', "originRoot", HelpText = "The base path for generated origin links")]
        public string? OriginPathRoot { get; set; }

        [Option('o', "output", HelpText = "The path where the generated .xml shall be stored")]
        public string? Output { get; set; }

        [Option('d', "directory", Required = true, HelpText = "The root directory from which the .xml shall be generated. " +
                                                              "Each 'build type' (Stable, Beta, etc.) has to be in a sub directory. " +
                                                              "Supported names are:" + 
                                                              nameof(ApplicationType.Stable) + ", " + 
                                                              nameof(ApplicationType.Beta) + ", " +
                                                              nameof(ApplicationType.Test) + ", ")]
        public string Directory { get; set; }

        [Option('a', "applicationName", HelpText = "The name of the application's .exe file")]
        public string? ApplicationFileName { get; set; }
    }
}
