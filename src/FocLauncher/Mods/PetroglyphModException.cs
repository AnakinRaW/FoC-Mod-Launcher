using System;
using FocLauncher.Game;

namespace FocLauncher.Mods
{
    public class PetroglyphModException : PetroglyphException
    {
        public PetroglyphModException()
        {
        }

        public PetroglyphModException(string message) : base(message)
        {
        }

        public PetroglyphModException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}