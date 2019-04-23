namespace FocLauncher.Game
{
    public struct DebugOptions
    {
        public bool UseDebug { get; }
        public bool IgnoreAsserts { get; }
        public bool NoArtProcess { get; }

        public DebugOptions(bool useDebug, bool ignoreAsserts, bool noArt)
        {
            UseDebug = useDebug;
            IgnoreAsserts = ignoreAsserts;
            NoArtProcess = noArt;
        }
    }
}