﻿namespace TaskBasedUpdater.Restart
{
    public interface IUpdateOptions : IRestartOptions
    {
        bool Update { get; set; }

        string Payload { get; set; }
    }
}