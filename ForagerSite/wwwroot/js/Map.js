function initializeMap() {
    var map = L.map('map').setView([51.505, -0.09], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    map.on('click', function (e) {
        var newMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(map);
        newMarker.bindPopup('You clicked here.').openPopup();
    });
}
