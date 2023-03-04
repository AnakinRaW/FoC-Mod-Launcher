using System;
using AnakinRaW.CommonUtilities.Hashing;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public readonly struct ComponentIntegrityInformation
{
    public static readonly ComponentIntegrityInformation None = new(Array.Empty<byte>(), HashType.None);

    public byte[] Hash { get; } = Array.Empty<byte>();

    public HashType HashType { get; }

    public ComponentIntegrityInformation(byte[] hash, HashType hashType)
    {
        Requires.NotNull(hash, nameof(hash));
        Hash = hash;
        HashType = hashType;
    }
}