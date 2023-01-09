namespace AnakinRaW.ProductMetadata.Component;

public record struct InstallationSize(long SystemDrive, long ProductDrive)
{
    public long Total => SystemDrive + ProductDrive;
}