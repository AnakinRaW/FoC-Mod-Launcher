using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocLauncher.Game
{
    internal struct GameDetectionResult
    {
        public GameType FocType { get; set; }

        public string FocPath { get; set; }

        public string EawPath { get; set; }

        public DetectionError Error { get; set; }

        public bool IsError { get; set; }
    }
}
