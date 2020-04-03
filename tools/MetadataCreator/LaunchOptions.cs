using CommandLine;

namespace MetadataCreator
{
    internal class LaunchOptions
    {
        [Option('r', "originRoot", HelpText = "The base path for generated origin links")]
        public string? OriginPathRoot { get; set; }

        [Option('o', "output", HelpText = "The path where the generated .xml shall be stored. Default is the current directory.")]
        public string? XmlOutput { get; set; }

        [Option('t', "type", Default = nameof(FocLauncher.ApplicationType.Stable), 
            HelpText = "Supported types are:" + nameof(FocLauncher.ApplicationType.Stable) + ", " + 
                       nameof(FocLauncher.ApplicationType.Beta) + ", " + 
                       nameof(FocLauncher.ApplicationType.Test))]
        public string ApplicationType { get; set; }

        [Option('b', "build", Required = false, Default = "Release", HelpText = "Specifies the build type of the applications files that should be considered. " +
                                                                                "Release is default option.")]
        public string BuildType { get; set; }


        [Option('m', "integrationMode", Required = false, Default = 0, HelpText = "Specifies how to integrate the new created xml into the current existing one." +
                                                                     "Values: " +
                                                                     "0 (no integration - new file will replace old); " +
                                                                     "1 (product integration - replace a product from the current metadata but keeping other products)" + 
                                                                     "2 (dependency integration - only replaced changed dependencies (full check) and removes dependencies that are not existent in the new metadata);" +
                                                                     "3 (dependency version integration - only replaced changed dependencies (with version changes) and removes dependencies that are not existent in the new metadata);")]
        public IntegrationMode XmlIntegrationMode { get; set; }


        [Option('f', "metadataFile", HelpText = "The absolute location (local or web) of the current metadata XML file. Used for integration mode -m")]
        public string? CurrentMetadataFile { get; set; }

        [Option('l', "filesCopyLocation", Required = false, HelpText = "Copies the new files (only application files) to the location where the current metadata exists (f). " +
                                                               "The application files will be placed into a folder matching the name of the application type (t)" +
                                                               "When not specified the files will not be copied.")]
        public string? FilesCopyLocation { get; set; }

        [Option('s', "sourceDir", HelpText = "The directory from where to search for possible application files. Default is the current directory")]
        public string? SourceDirectory { get; set; }
    }
}
