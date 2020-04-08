namespace FocLauncher.Game
{
    public class GameDetection
    {
        public GameType FocType { get; set; }

        public string FocExePath { get; internal set; }

        public string EawExePath { get; internal set; }

        public DetectionResult Result { get; internal set; }

        public bool IsError => Result != DetectionResult.None;
    }
}
