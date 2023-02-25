namespace AnakinRaW.AppUpaterFramework.Updater;

internal class OutOfDiskspaceException : UpdaterException {
    public OutOfDiskspaceException() : base(nameof(OutOfDiskspaceException)) {
    }

    public OutOfDiskspaceException(string message) : base(message) {
    }
}