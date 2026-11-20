namespace ForagerSite.Features.Finds;

public sealed record ProfileFindSummary(
    Guid FindId,
    Guid OwnerId,
    string Name,
    string? SpeciesName,
    string? SpeciesType,
    DateTime FindDate,
    string? UseCategory,
    string? Accessibility,
    double? Latitude,
    double? Longitude,
    string? PreviewImageUrl);
