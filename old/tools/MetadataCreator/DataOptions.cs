using System;
using System.Collections;
using System.Collections.Generic;
using FocLauncher;

namespace MetadataCreator
{
    internal class ApplicationFilesSet : IEnumerable<ApplicationFiles>
    {
        public ApplicationFiles StableFiles { get; } = new ApplicationFiles(ApplicationType.Stable);
        //public ApplicationFiles AlphaFiles { get; } = new ApplicationFiles(ApplicationType.Alpha);
        public ApplicationFiles BetaFiles { get; } = new ApplicationFiles(ApplicationType.Beta);
        public ApplicationFiles TestFiles { get; } = new ApplicationFiles(ApplicationType.Test);

        public ApplicationFiles GetFilesDataFromApplicationType(ApplicationType applicationType)
        {
            switch (applicationType)
            {
                case ApplicationType.Stable:
                    return StableFiles;
                case ApplicationType.Beta:
                    return BetaFiles;
                case ApplicationType.Test:
                    return TestFiles;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool Validate()
        {
            if (!StableFiles.Validate(true))
                return false;
            //if (!AlphaFiles.Validate())
            //    return false;
            if (!BetaFiles.Validate())
                return false;
            if (!TestFiles.Validate())
                return false;
            return true;
        }

        public IEnumerator<ApplicationFiles> GetEnumerator()
        {
            if (StableFiles.Validate(true))
                yield return StableFiles;
            if (BetaFiles.Validate(true))
                yield return BetaFiles;
            //if (AlphaFiles.Validate(true))
            //    yield return AlphaFiles;
            if (TestFiles.Validate(true))
                yield return TestFiles;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}