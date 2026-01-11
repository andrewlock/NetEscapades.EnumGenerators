
// Based on https://github.com/dotnet/aspnetcore/blob/main/src/Shared/RoslynUtils/WellKnownTypeData.cs#

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace NetEscapades.EnumGenerators.RoslynUtils;

internal static class WellKnownTypeData
{
    public enum WellKnownType
    {
        NetEscapades_EnumGenerators_EnumParseOptions,
    }

    public static string[] WellKnownTypeNames =
    [
        "NetEscapades.EnumGenerators.EnumParseOptions",
    ];
}