﻿using CommandLine;

namespace MetadataCreator
{
    internal class LaunchOptions
    {
        [Option('r', "originRoot", HelpText = "The base path for generated origin links")]
        public string? OriginPathRoot { get; set; }

        [Option('o', "output", HelpText = "The path where the generated .xml shall be stored")]
        public string? Output { get; set; }

        [Option('t', "type", Default = nameof(FocLauncher.ApplicationType.Stable), 
            HelpText = "Supported types are:" + nameof(FocLauncher.ApplicationType.Stable) + ", " + 
                       nameof(FocLauncher.ApplicationType.Beta) + ", " + 
                       nameof(FocLauncher.ApplicationType.Test))]
        public string ApplicationType { get; set; }

        [Option('b', "build", Required = false, Default = "Release", HelpText = "Specifies the build type of the applications files that should be considered. " +
                                                                                "Release is default option.")]
        public string BuildType { get; set; }


        [Option('m', "integrationMode", Required = false, HelpText = "Specifies how to integrate the new created xml into the current existing one." +
                                                                     "Values: " +
                                                                     "0 (no integration - new file will replace old); " +
                                                                     "1 (Integrate - the targeted application type will be replaced and others will be kept);")]
        public int? XmlIntegrationMode { get; set; }


        [Option('f', "metadataFile", HelpText = "The absolute location of the current metadata XML file. Used for integration mode -m")]
        public string? CurrentMetadataFile { get; set; }
    }
}
