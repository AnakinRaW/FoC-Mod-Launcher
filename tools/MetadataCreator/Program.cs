using System;
using CommandLine;

namespace MetadataCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = (Parser.Default.ParseArguments<LaunchOptions>(args) as Parsed<LaunchOptions>)?.Value;

            Console.ReadKey();
        }
    }
}
