
function initializeGoogleMap() {
    var map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: -34.397, lng: 150.644 },
        zoom: 8
    });

    google.maps.event.addListener(map, 'click', function (event) {
        placeMarker(event.latLng, map);
    });
}

function placeMarker(location, map) {
    var marker = new google.maps.Marker({
        position: location,
        map: map
    });

    map.panTo(location);
}
