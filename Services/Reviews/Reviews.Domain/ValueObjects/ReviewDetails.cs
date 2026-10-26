using Reviews.Domain.Common;

namespace Reviews.Domain.ValueObjects;

public sealed record ReviewDetails : ValueObject
{
    public int? BatteryLifeHours { get; }
    public int? CameraScore { get; }
    public int? KeyboardRating { get; }
    public string? Thermals { get; }
    public IReadOnlyList<string>? UsedFor { get; }

    public ReviewDetails(
        int? batteryLifeHours = null,
        int? cameraScore = null,
        int? keyboardRating = null,
        string? thermals = null,
        IEnumerable<string>? usedFor = null)
    {
        BatteryLifeHours = batteryLifeHours;
        CameraScore = cameraScore;
        KeyboardRating = keyboardRating;
        Thermals = thermals;

        UsedFor = usedFor is null
            ? null
            : Array.AsReadOnly(usedFor.ToArray());
    }
}