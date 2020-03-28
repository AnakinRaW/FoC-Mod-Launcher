using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FocLauncher.Utilities
{
    public static class PathUtilities
    {
        // Based on: https://stackoverflow.com/questions/1410127/c-sharp-test-if-user-has-write-access-to-a-folder
        public static bool UserHasDirectoryAccessRights(string path, FileSystemRights accessRights)
        {
            var isInRoleWithAccess = false;
            try
            {
                var di = new DirectoryInfo(path);
                var acl = di.GetAccessControl();

                var rules = acl.GetAccessRules(true, true,
                    // If Windows 7
                    Environment.OSVersion.VersionString.StartsWith("6.1")
                        ? typeof(SecurityIdentifier)
                        : typeof(NTAccount));

                var currentUser = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(currentUser);
                foreach (AuthorizationRule rule in rules)
                {
                    var fsAccessRule = rule as FileSystemAccessRule;
                    if (fsAccessRule == null)
                        continue;

                    if ((fsAccessRule.FileSystemRights & accessRights) > 0)
                    {
                        var ntAccount = rule.IdentityReference as NTAccount;
                        if (ntAccount == null)
                            continue;

                        if (principal.IsInRole(ntAccount.Value))
                        {
                            if (fsAccessRule.AccessControlType == AccessControlType.Deny)
                                return false;
                            isInRoleWithAccess = true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            return isInRoleWithAccess;
        }
    }
}
