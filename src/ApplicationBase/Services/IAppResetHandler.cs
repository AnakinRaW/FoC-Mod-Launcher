namespace AnakinRaW.ApplicationBase.Services;

internal interface IAppResetHandler
{
    void ResetIfNecessary();

    void ResetApplication();
}