using System.Threading.Tasks;

namespace FocLauncherHost.Updater
{
    internal class UpdateManager
    {
        /*
         * Updater Order:
         * 1. Check for update: UpdaterProgram
         * 2. Check for update: Bootstrapper
         * 3. Check for update: Launcher/Theme
         * IF NO updates
         *  DONE
         * ELSE
         *  Ask User to proceed
         *  1. Perform Update: UpdaterProgram
         *  1. Perform Update: Launcher/Theme
         *  3. Perform Update Bootstrapper and restart
         */
        public async Task CheckAndPerformUpdateAsync()
        {

        }
    }
}
