namespace AnakinRaW.AppUpdaterFramework.Updater;

internal class OutOfDiskspaceException : UpdaterException {
    public OutOfDiskspaceException() : base(nameof(OutOfDiskspaceException)) {
    }

    public OutOfDiskspaceException(string message) : base(message) {
    }
}