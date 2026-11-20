using ForagerSite.Services.Interfaces;
using Microsoft.JSInterop;

namespace ForagerSite.Services
{
    public class MapService : IMapService
    {
        public double? pendingLat { get; set; }
        public double? pendingLng { get; set; }

        public event Action? OnCreateFormRequested;
        public event Action<Guid>? OnMarkerSelected;
        public event Func<double, double, double, double, Task>? ViewportChanged;

        [JSInvokable("OnMarkerClicked")]
        public Task OnMarkerClicked(string findId)
        {
            if (Guid.TryParse(findId, out var guid))
                OnMarkerSelected?.Invoke(guid);
            return Task.CompletedTask;
        }

        [JSInvokable("OnViewportChanged")]
        public async Task OnViewportChanged(double south, double west, double north, double east)
        {
            if (ViewportChanged is not null)
                await ViewportChanged.Invoke(south, west, north, east);
        }

        [JSInvokable("TriggerCreateForm")]
        public Task TriggerCreateForm(double lat, double lng)
        {
            pendingLat = lat;
            pendingLng = lng;
            OnCreateFormRequested?.Invoke();
            return Task.CompletedTask;
        }
    }
}
