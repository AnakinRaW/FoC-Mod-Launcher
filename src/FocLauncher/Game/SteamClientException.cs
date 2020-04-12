using System;

namespace FocLauncher.Game
{
    public class SteamClientException : Exception
    {
        public SteamClientException(string message) : base(message)
        {
        }
    }
}
