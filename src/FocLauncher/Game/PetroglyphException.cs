using System;

namespace FocLauncher.Game
{
    public class PetroglyphException : Exception
    {
        public PetroglyphException()
        {
        }

        public PetroglyphException(string message) : base(message)
        {
        }

        public PetroglyphException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}