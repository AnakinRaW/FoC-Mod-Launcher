﻿using System;
using System.Linq;
using System.Threading;
using NLog;

namespace FocLauncherHost.Updater.Tasks
{
    internal abstract class UpdateTask : IDependencyUpdateTask
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        internal bool IsDisposed { get; private set; }

        protected internal ILogger Logger { get; private set; }

        public Exception Error { get; internal set; }

        ~UpdateTask()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // TODO: Decide return type
        public void Run(CancellationToken token)
        {
            Logger = _logger;
            Logger.Trace($"BEGIN: {this}");
            try
            {
                ExecuteTask(token);
                Logger.Trace($"END: {this}");
            }
            catch (OperationCanceledException ex)
            {
                Error = ex.InnerException;
                throw;
            }
            catch (AggregateException ex)
            {
                if (!ex.IsExceptionType<OperationCanceledException>())
                    LogFaultException(ex);
                else
                    Error = ex.InnerExceptions.FirstOrDefault(p => Extensions.IsExceptionType<OperationCanceledException>(p))?.InnerException;
                throw;
            }
            catch (Exception e)
            {
                LogFaultException(e);
                throw;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        protected abstract void ExecuteTask(CancellationToken token);

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return; 
            IsDisposed = true;
        }

        private void LogFaultException(Exception ex)
        { 
            Error = ex; 
            Logger?.Error(ex, ex.InnerException?.Message ?? ex.Message);
        }
    }
}