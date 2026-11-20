namespace ForagerSite.Features.Map;

public sealed record MapFindScope(bool IncludeMine, bool IncludeFriends);

public sealed record MapViewport(
    double South,
    double West,
    double North,
    double East);

public sealed record MapPinRequest(
    MapViewport Viewport,
    MapFindScope Scope);

public sealed record MapPin(
    Guid FindId,
    Guid OwnerId,
    string OwnerName,
    string? PreviewImageUrl,
    string Name,
    string? SpeciesName,
    string? SpeciesType,
    string? UseCategory,
    DateTime FindDate,
    double Latitude,
    double Longitude);

public sealed record MapPinPage(
    IReadOnlyList<MapPin> Pins,
    bool HasMore);
