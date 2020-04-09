namespace FocLauncher.Game.Detection
{
    public class LocalGameDetector : GameDetector
    {
        protected override GameDetection DetectGamesCore()
        {
            var localResult = FindGamesFromExecutingPath();
            Logger.Trace("Local path game detection result:");
            Logger.Trace("\t" + localResult);
            return localResult;
        }

        private static GameDetection FindGamesFromExecutingPath()
        {
            // TODO
            //var currentPath = Directory.GetCurrentDirectory();

            //if (!File.Exists(Path.Combine(currentPath, "swfoc.exe")))
            //    return;

            //if (Path.GetFullPath(currentPath) == Path.GetFullPath(result.FocPath))
            //    return;

            //var newResult = default(GameDetectionResult);
            //newResult.FocPath = currentPath;

            //var gameType = GameTypeHelper.GetGameType(newResult);
            //newResult.FocType = gameType;
            //if (!Eaw.FindInstallationRelativeToFoc(newResult.FocPath, gameType, out var eawPath))
            //{
            //    newResult.EawPath = result.EawPath;
            //    result = newResult;
            //    return;
            //}
            //newResult.EawPath = eawPath;
            //result = newResult;

            return GameDetection.NotInstalled;
        }
    }
}