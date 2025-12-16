using Microsoft.JSInterop;
using System.Globalization;

public static class DirectionsHelper
{
    public static async Task OpenAsync(IJSRuntime js, double lat, double lng)
    {
        await js.InvokeVoidAsync(
            "openDirections",
            lat.ToString(CultureInfo.InvariantCulture),
            lng.ToString(CultureInfo.InvariantCulture)
        );
    }
}
