using System.IO;
using System.Linq;

namespace FocLauncher.Game
{
    public abstract class AbstractFocGame : PetroglyphGame
    {
        protected AbstractFocGame(string gameDirectory) : base(gameDirectory)
        {
        }

        public override bool IsGameAiClear()
        {
            if (Directory.Exists(Path.Combine(GameDirectory, @"Data\Scripts\")))
                return false;
            var xmlDir = Path.Combine(GameDirectory, @"Data\XML\");
            if (!Directory.Exists(xmlDir))
                return false;
            var number = Directory.EnumerateFiles(xmlDir).Count();
            if (number != DefaultXmlFileCount)
                return false;
            if (Directory.Exists(Path.Combine(xmlDir, @"AI\")))
                return false;
            return true;
        }
    }
}