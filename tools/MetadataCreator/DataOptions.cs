using System.Collections.Generic;
using CommandLine;

namespace MetadataCreator
{
    internal class DataOptions
    {
        [Option("exe", HelpText = "The main executable of the application")]
        public string Executable { get; set; }

        [Option('f', "applicationFiles", Separator = ';', HelpText = "List of all applications files separated with an ';'")]
        public IEnumerable<string> ApplicationFiles { get; set; }


        [Option("exeAlpha", HelpText = "The main executable of the application (Alpha Build)")]
        public string? AlphaExecutable { get; set; }

        [Option('a', "alphaFiles", Separator = ';', HelpText = "List of all (Alpha Build) applications files separated with an ';'")]
        public IEnumerable<string>? AlphaFiles { get; set; }


        [Option("exeBeta", HelpText = "The main executable of the application (Beta Build)")]
        public string? BetaExecutable { get; set; }

        [Option('b', "betaFiles", Separator = ';', HelpText = "List of all (Beta Build) applications files separated with an ';'")]
        public IEnumerable<string>? BetaFiles { get; set; }


        [Option("exeTest", HelpText = "The main executable of the application (Test Build)")]
        public string? TestExecutable { get; set; }

        [Option('t', "betaFiles", Separator = ';', HelpText = "List of all (Test Build) applications files separated with an ';'")]
        public IEnumerable<string>? TestFiles { get; set; }
    }
}