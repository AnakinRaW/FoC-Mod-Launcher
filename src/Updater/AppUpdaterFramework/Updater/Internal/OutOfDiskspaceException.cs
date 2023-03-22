namespace AnakinRaW.AppUpdaterFramework.Updater;

internal class OutOfDiskspaceException : UpdateException {
    public OutOfDiskspaceException() : base(nameof(OutOfDiskspaceException)) {
    }

    public OutOfDiskspaceException(string message) : base(message) {
    }
}