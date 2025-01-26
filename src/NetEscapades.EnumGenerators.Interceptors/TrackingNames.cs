namespace NetEscapades.EnumGenerators.Interceptors;

/// <summary>
/// Names that are attached to incremental generator stages for tracking
/// </summary>
public class TrackingNames
{
    public const string InitialExtraction = nameof(InitialExtraction);
    public const string InitialExternalExtraction = nameof(InitialExternalExtraction);
    public const string InitialInterceptable = nameof(InitialInterceptable);
    public const string InitialInterceptableOnly = nameof(InitialInterceptableOnly);
    public const string RemovingNulls = nameof(RemovingNulls);
    public const string InterceptedLocations = nameof(InterceptedLocations);
    public const string Settings = nameof(Settings);
    public const string EnumInterceptions = nameof(EnumInterceptions);
    public const string ExternalInterceptions = nameof(ExternalInterceptions);
    public const string AdditionalInterceptions = nameof(AdditionalInterceptions);
}