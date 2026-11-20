namespace ForagerSite.Services.Interfaces
{
    public interface IMapService
    {
        double? pendingLat { get; set; }
        double? pendingLng { get; set; }

        event Action? OnCreateFormRequested;
        event Action<Guid>? OnMarkerSelected;
        event Func<double, double, double, double, Task>? ViewportChanged;

        Task OnMarkerClicked(string findId);
        Task TriggerCreateForm(double lat, double lng);
    }
}
