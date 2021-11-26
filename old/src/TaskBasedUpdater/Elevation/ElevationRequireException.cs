﻿using System;
using System.Collections.Generic;

namespace TaskBasedUpdater.Elevation
{
    public class ElevationRequireException : Exception
    {
        public IEnumerable<ElevationRequestData> Requests { get; }

        public ElevationRequireException(IEnumerable<ElevationRequestData> requests)
        {
            Requests = requests;
        }
    }
}