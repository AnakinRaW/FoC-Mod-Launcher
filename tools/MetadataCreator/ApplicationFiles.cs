using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocLauncher;

namespace MetadataCreator
{
    internal class ApplicationFiles
    {
        public ApplicationType Type { get; }

        public ApplicationFiles(ApplicationType type)
        {
            Type = type;
        }

        public IEnumerable<FileInfo> AllFiles => Files.ToList().Append(Executable);

        public FileInfo? Executable { get; set; }

        public ICollection<FileInfo>? Files { get; } = new HashSet<FileInfo>();

        public bool Validate(bool mustBeFilled = false)
        {
            if (Executable is null && mustBeFilled)
                return false;
            return !Files.Any() || !(Executable is null);
        }
    }
}