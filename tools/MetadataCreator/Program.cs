using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CommandLine;
using FocLauncher;
using FocLauncher.Properties;
using FocLauncher.UpdateMetadata;
using FocLauncher.Utilities;

namespace MetadataCreator
{
    internal class Program
    {
        private const string DefaultFileRootPath = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher-Builds/master";
        public static readonly string[] SupportedFileEndings = {".exe", ".dll"};


        private static LaunchOptions _launchOptions;


        private static void Main(string[] args)
        {
            LaunchOptions options = null;
            Parser.Default.ParseArguments<LaunchOptions>(args).WithParsed(launchOptions =>
            {
                if (string.IsNullOrEmpty(launchOptions.Output))
                    launchOptions.Output = Directory.GetCurrentDirectory();
                if (string.IsNullOrEmpty(launchOptions.OriginPathRoot))
                    launchOptions.OriginPathRoot = DefaultFileRootPath;
                _launchOptions = launchOptions;
            });
            if (_launchOptions is null)
                return;

            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var files = GetFilesByExtensions(dir, true, SupportedFileEndings);
            var applicationFiles = GetApplicationFiles(files.ToList(), _launchOptions.BuildType).ToList();
            if (applicationFiles.Count() != LauncherConstants.ApplicationFileNames.Length)
                throw new InvalidOperationException();

            if (!Enum.TryParse<ApplicationType>(_launchOptions.ApplicationType, true, out var applicationType))
                throw new InvalidOperationException($"Could not parse '{_launchOptions.ApplicationType}' into a real ApplicationType");

            
            var data = new ApplicationFilesSet();
            try
            {
                FillData(applicationType, applicationFiles, data.GetFilesDataFromApplicationType(applicationType));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (!data.Validate())
                throw new InvalidOperationException("The file set was not valid");

            var catalogs = CreateCatalogs(data);

            var serializer = new XmlSerializer(typeof(Catalogs));
            var outputFile = Path.Combine(_launchOptions.Output, LauncherConstants.UpdateMetadataFileName);
            using var file = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new XmlTextWriter(file, Encoding.UTF8) {Formatting = Formatting.Indented};
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(writer, catalogs, ns);
            file.Dispose();
            
            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            if (!validator.Validate(outputFile))
            {
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
                throw new InvalidOperationException("Created .xml is not valid");
            }

            Console.WriteLine(File.ReadAllText(outputFile));
            Console.ReadKey();
        }

        private static IEnumerable<FileInfo> GetApplicationFiles(IReadOnlyCollection<FileInfo> files, string buildType)
        {
            foreach (var fileName in LauncherConstants.ApplicationFileNames)
            {
                var foundFile = files.FirstOrDefault(x =>
                    x.Name.Equals(fileName) && x.Directory != null && x.Directory.Name.Equals(buildType));
                if (foundFile is null)
                    throw new FileNotFoundException($"File '{fileName}' was not found as {buildType}-Build");
                yield return foundFile;
            }
        }


        internal static Catalogs CreateCatalogs(ApplicationFilesSet dataSet)
        {
            var catalog = new Catalogs {Products = new List<ProductCatalog>()};

            foreach (var applicationFiles in dataSet)
            {
                var p = CreateProduct(applicationFiles);
                catalog.Products.Add(p);
            }
            return catalog;
        }

        private static ProductCatalog CreateProduct(ApplicationFiles applicationFiles)
        {
            var product = new ProductCatalog
            {
                Name = LauncherConstants.ProductName,
                Author = LauncherConstants.Author,
                ApplicationType = applicationFiles.Type,
                Dependencies = new List<Dependency> {CreateDependency(applicationFiles.Executable, true)}
            };
            foreach (var file in applicationFiles.Files) 
                product.Dependencies.Add(CreateDependency(file));
            return product;
        }

        private static Dependency CreateDependency(FileInfo file, bool isLauncherExecutable = false)
        {
            var dependency= new Dependency();
            dependency.Name = file.Name;
            var destination = isLauncherExecutable ? LauncherConstants.ExecutablePathVariable : LauncherConstants.ApplicationBaseVariable;
            dependency.Destination = $"%{destination}%";
            dependency.Version = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;
            dependency.Sha2 = FileHashHelper.GetFileHash(file.FullName, FileHashHelper.HashType.Sha256);
            dependency.Size = file.Length;
            dependency.Origin = UrlCombine.Combine(_launchOptions.OriginPathRoot, file.Directory?.Name, file.Name);
            return dependency;
        }


        internal static void FillData(ApplicationType applicationType, IEnumerable<FileInfo> files, in ApplicationFiles data)
        {
            var typeName = Enum.GetName(typeof(ApplicationType), applicationType);
            if (string.IsNullOrEmpty(typeName))
                return;
          
            foreach (var file in files)
            {
                if (file.Name.Equals(LauncherConstants.LauncherFileName))
                {
                    if (data.Executable != null)
                        throw new InvalidOperationException("Cannot set the main executable file twice for a data set");
                    data.Executable = file;
                    continue;
                }
                data.Files.Add(file);
            }

        }

        internal static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, bool includeSubs = false, params string[] extensions)
        {
            if (extensions == null)
                extensions = new[] { ".*" };
            var files = dir.EnumerateFiles("*.*", includeSubs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return files.Where(f => extensions.Contains(f.Extension));
        }
    }
}
