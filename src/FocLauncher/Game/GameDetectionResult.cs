namespace FocLauncher.Game
{
    public struct GameDetectionResult
    {
        public GameType FocType { get; set; }

        public string FocPath { get; set; }

        public string EawPath { get; set; }

        public DetectionError Error { get; set; }

        public bool IsError { get; set; }
    }
}
