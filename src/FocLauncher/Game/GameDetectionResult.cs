namespace FocLauncher.Game
{
    public class GameDetectionResult
    {
        public GameType FocType { get; set; }

        public string FocExePath { get; internal set; }

        public string EawExePath { get; internal set; }

        public DetectionError Error { get; internal set; }

        public bool IsError => Error != DetectionError.None;
    }
}
