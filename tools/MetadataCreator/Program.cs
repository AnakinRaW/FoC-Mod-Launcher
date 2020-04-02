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
                if (string.IsNullOrEmpty(launchOptions.XmlOutput))
                    launchOptions.XmlOutput = Directory.GetCurrentDirectory();
                if (string.IsNullOrEmpty(launchOptions.OriginPathRoot))
                    launchOptions.OriginPathRoot = DefaultFileRootPath;
                if (launchOptions.XmlIntegrationMode < 0 || launchOptions.XmlIntegrationMode > 2)
                    throw new InvalidOperationException($"Value {launchOptions.XmlIntegrationMode} is not supported for parameter --integrationMode (-m).");
                _launchOptions = launchOptions;
            });
            if (_launchOptions is null)
                return;

            _launchOptions.XmlIntegrationMode = 2;
            _launchOptions.CurrentMetadataFile = LauncherConstants.UpdateMetadataPath;

            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var files = GetFilesByExtensions(dir, true, SupportedFileEndings);
            var applicationFileInfos = GetApplicationFiles(files.ToList(), _launchOptions.BuildType).ToList();
            if (applicationFileInfos.Count != LauncherConstants.ApplicationFileNames.Length)
                throw new InvalidOperationException();

            if (!Enum.TryParse<ApplicationType>(_launchOptions.ApplicationType, true, out var applicationType))
                throw new InvalidOperationException($"Could not parse '{_launchOptions.ApplicationType}' into a real ApplicationType");


            var applicationFiles = new ApplicationFiles(applicationType);
            try
            {
                FillData(applicationFileInfos, applicationFiles);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (!applicationFiles.Validate())
                throw new InvalidOperationException("The file set was not valid");

            var product = CreateProduct(applicationFiles);

            var catalog = CreateCatalogOrIntegrate(product);


            var serializer = new XmlSerializer(typeof(Catalogs));
            var outputFile = Path.Combine(_launchOptions.XmlOutput, LauncherConstants.UpdateMetadataFileName);
            using var file = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new XmlTextWriter(file, Encoding.UTF8) {Formatting = Formatting.Indented};
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(writer, catalog, ns);
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

        private static Catalogs CreateCatalogOrIntegrate(ProductCatalog product)
        {
            var catalog = new Catalogs {Products = new List<ProductCatalog> {product}};

            if (_launchOptions.XmlIntegrationMode == 0)
                return catalog;

            if (product.ApplicationType == ApplicationType.Stable && _launchOptions.XmlIntegrationMode == 1)
                return catalog;


            if (!Uri.TryCreate(_launchOptions.CurrentMetadataFile, UriKind.RelativeOrAbsolute, out var metadataUri))
            {
                Console.WriteLine("Unable to get the current metadata file");
                return catalog;
            }

            using var metadataStream = new MemoryStream();
            if (!Downloader.Download(metadataUri, metadataStream))
            {
                Console.WriteLine("Unable to get the current metadata file");
                return catalog;
            }
            
            Catalogs currentCatalog;
            try
            {
                var parser = new XmlObjectParser<Catalogs>(metadataStream);
                currentCatalog = parser.Parse();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return catalog;
            }

            // TODO: Find existing and integrate/add
            return currentCatalog;
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


        internal static void FillData(IEnumerable<FileInfo> files, in ApplicationFiles data)
        {
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
