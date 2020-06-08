using System;

namespace FocLauncher.ModInfo
{
    public class ModInfoException : Exception
    {
        public ModInfoException()
        {
        }

        public ModInfoException(string message) : base(message)
        {
        }
    }
}