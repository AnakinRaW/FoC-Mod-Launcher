using System.Windows;

namespace FocLauncher.Services;

internal interface IWindowService
{
    void SetMainWindow(Window window);

    void ShowWindow();

    void SetOwner(Window window);

    void DisableOwner(Window window);

    void EnableOwner(Window window);
}