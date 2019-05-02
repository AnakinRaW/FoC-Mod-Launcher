using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocLauncherApp.Utilities
{
    public class ResourceExtractor
    {
        private string Assembly { get; }

        public string ResourcePath { get; }

        public ResourceExtractor()
        {
            Assembly = typeof(Bootstrapper).Namespace;
            ResourcePath = "";
        }

        /// <summary>
        /// Allows to specify a special resource Path
        /// </summary>
        /// <param name="resourcePath">Do not enter a "." at last character</param>
        public ResourceExtractor(string resourcePath)
        {
            Assembly = typeof(Bootstrapper).Namespace;
            ResourcePath = resourcePath.TrimEnd('.') + @".";
        }


        /// <summary>
        /// Extracts internal resources out of the assembly into a directory if the files are not already in it.
        /// Creates Directory if it does not exists.
        /// </summary>
        /// <param name="directory">Target directory</param>
        /// <param name="files">Set of Files </param>
        /// <param name="overrideFile">Default: false. Tells to overwrite files or not</param>
        public void ExtractFilesIfRequired(string directory, IEnumerable<string> files, bool overrideFile = false)
        {
            if (string.IsNullOrEmpty(Assembly))
                throw new ResourceExtractorException("Assembly not set: '" + Assembly + "'");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            foreach (var file in files.Where(file => !File.Exists(Path.Combine(directory, file)) || overrideFile))
                InternalExtractFile(directory, file);
        }

        private void InternalExtractFile(string directory, string file)
        {
            var resource = Assembly + @"." + ResourcePath + file;
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null || stream == Stream.Null)
                    throw new ResourceExtractorException($"Unable to find the resource: {resource}");

                using (var fileStream = new FileStream(Path.Combine(directory, file), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
