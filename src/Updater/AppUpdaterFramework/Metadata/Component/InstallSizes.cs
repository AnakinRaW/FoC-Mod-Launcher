namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public record struct InstallationSize(long SystemDrive, long ProductDrive)
{
    public long Total => SystemDrive + ProductDrive;
}