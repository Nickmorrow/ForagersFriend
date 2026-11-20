
window.map = window.map || null;
var markers = {};
var tempMarker = null;
var userMarker = null;
var dotNetObjectReference = null;
var viewportRefreshTimeout = null;

window.getSelectedValues = (selectElement) => {
    return Array.from(selectElement.selectedOptions).map(o => o.value);
};

function reportViewport() {
    if (!window.map || !window.dotNetObjectReference) return;

    const bounds = window.map.getBounds();
    window.dotNetObjectReference.invokeMethodAsync(
        'OnViewportChanged',
        bounds.getSouth(),
        bounds.getWest(),
        bounds.getNorth(),
        bounds.getEast())
        .catch(err => console.error("Error reporting map viewport:", err));
}

function scheduleViewportRefresh() {
    clearTimeout(viewportRefreshTimeout);
    viewportRefreshTimeout = setTimeout(reportViewport, 200);
}

window.requestViewportRefresh = function () {
    scheduleViewportRefresh();
};

function onMapClick(e) {
    if (window.tempMarker) {
        window.map.removeLayer(window.tempMarker);
        window.tempMarker = null;
    }

    const lat = e.latlng.lat;
    const lng = e.latlng.lng;

    window.tempMarker = L.marker([lat, lng]).addTo(window.map);

    const popupContent = `
        <div>
            <strong>Create New Find Here?</strong><br/>
            <button id="open-create-form">Create Find</button>
        </div>
    `;

    window.tempMarker.bindPopup(popupContent).openPopup();

    // Attach click handler to the button inside this popup
    setTimeout(() => {
        const button = document.getElementById("open-create-form");
        if (button) {
            button.addEventListener("click", function () {
                // Close popup before triggering .NET
                window.tempMarker.closePopup();

                if (window.dotNetObjectReference) {
                    window.dotNetObjectReference
                        .invokeMethodAsync("TriggerCreateForm", lat, lng)
                        .catch(err => console.error("Error triggering create form:", err));
                } else {
                    console.error("DotNetObjectReference is not set.");
                }
            });
        }
    }, 10);

    window.tempMarker.on('popupclose', function () {
        window.map.removeLayer(window.tempMarker);
        window.tempMarker = null;
    });
}

window.openDirections = function (lat, lng) {
    const latLng = `${lat},${lng}`;
    const isIos = /iPad|iPhone|iPod/.test(navigator.userAgent);

    const url = isIos
        ? `https://maps.apple.com/?daddr=${latLng}&dirflg=d`
        : `https://www.google.com/maps/dir/?api=1&destination=${latLng}`;

    window.open(url, "_blank");
};

window.focusFindAfterDelay = function (lat, lng, ms) {
    setTimeout(() => {
        if (window.map) window.map.setView([lat, lng], 16, { animate: true });
    }, ms || 400);
};

window.initializeMap = function (json, currentUserId, mapFilter, userName, focusLat, focusLng) {
    let pins = JSON.parse(json);
    const hasFocus = focusLat !== null && focusLat !== undefined &&
        focusLng !== null && focusLng !== undefined &&
        Number.isFinite(Number(focusLat)) && Number.isFinite(Number(focusLng));
    const focusLocation = hasFocus ? [Number(focusLat), Number(focusLng)] : null;

    // Always get the current container
    const mapContainer = document.getElementById('map');

    if (!mapContainer) {
        console.error("Map container with id 'map' not found.");
        return;
    }

    // If a map exists but its container is gone or different, destroy it
    if (window.map) {
        // Leaflet stores the DOM element in _container
        if (!window.map._container || window.map._container !== mapContainer) {
            try {
                window.map.remove();
            } catch (e) {
                console.warn("Error removing existing map:", e);
            }
            window.map = null;
        }
    }

    // If we still have a valid map, just update markers and bail
    if (window.map) {
        console.log('Map already initialized, updating markers.');
        updateMarkers(pins);
        if (focusLocation) {
            window.map.setView(focusLocation, 16, { animate: false });
            scheduleViewportRefresh();
        }
        return;
    }

    // ---- from here on, we are creating a new map instance ----

    const defaultLatLng = [51.505, -0.09];
    let latLng = defaultLatLng;

    const northAmericaBounds = [
        [7.0, -168.0],  // Southwest corner (lat, lng)
        [83.0, -30.0]   // Northeast corner (lat, lng)
    ];

    function initMap(centerLatLng, zoom = 13, showLocationMarker = true) {
        window.map = L.map('map', {
            center: centerLatLng,
            zoom: zoom,
            maxBounds: northAmericaBounds,      // Restrict panning to North America
            maxBoundsViscosity: 1.0             // Stickiness when reaching bounds
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            minZoom: 4,
            maxZoom: 18
        }).addTo(window.map);

        if (showLocationMarker) {
            L.marker(centerLatLng)
                .addTo(window.map)
                .bindPopup("<b>Your Location</b>")
                .openPopup();
        }

        window.map.on('click', onMapClick);
        window.map.on('moveend', scheduleViewportRefresh);

        updateMarkers(pins);
        scheduleViewportRefresh();
    }

    if (focusLocation) {
        initMap(focusLocation, 16, false);
        return;
    }

    // Try geolocation, fall back to default
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function (position) {
                latLng = [position.coords.latitude, position.coords.longitude];
                initMap(latLng);
            },
            function (error) {
                console.error("Error obtaining location: ", error);
                initMap(defaultLatLng);
            }
        );
    } else {
        console.error("Geolocation is not supported by this browser.");
        initMap(defaultLatLng);
    }
};


window.setDotNetObjectReference = function (reference) {
    dotNetObjectReference = reference;
};

function escapeMapHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, character => ({
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;'
    })[character]);
}

window.updateMapPins = function (pinsJson, currentUserId, mapFilter, userName) {
    if (!window.map) return;

    const pins = typeof pinsJson === 'string' ? JSON.parse(pinsJson) : pinsJson;
    Object.keys(markers).forEach(key => markers[key].marker.remove());
    markers = {};

    const templateHtml = document.getElementById('find-popup-template')?.innerHTML ?? '';

    pins.forEach(pin => {
        const lat = Number(pin.latitude);
        const lng = Number(pin.longitude);
        if (!pin.findId || !Number.isFinite(lat) || !Number.isFinite(lng)) return;

        const marker = L.marker([lat, lng]).addTo(window.map);
        const imageHtml = pin.previewImageUrl
            ? `<img src="${escapeMapHtml(pin.previewImageUrl)}" alt="${escapeMapHtml(pin.name)}" style="width: 100px; height: 100px; margin-bottom: 5px;">`
            : '';
        const popupContent = templateHtml
            .replace('{findName}', escapeMapHtml(pin.name))
            .replace('{Images}', imageHtml);

        marker.bindPopup(popupContent);
        markers[pin.findId] = { marker, lat, lng, originalPopupContent: popupContent };

        marker.on('click', function () {
            if (window.dotNetObjectReference) {
                window.dotNetObjectReference
                    .invokeMethodAsync('OnMarkerClicked', pin.findId)
                    .catch(err => console.error('Error in OnMarkerClicked:', err));
            }
        });
    });
};

window.updateMarkers = function (userFindsDCs, currentUserId, mapFilter, userName) {
    
    // Display all finds on map
    Object.keys(markers).forEach(key => {
        markers[key].marker.remove();
    });
    markers = {};

    const templateHtml = document.getElementById('find-popup-template').innerHTML;

    userFindsDCs.forEach(viewModel => {

        var currentViewModel = viewModel;

        viewModel.finds.forEach(find => {

            if (find.findLocation.latitude !== undefined && find.findLocation.longitude !== undefined) {
                var lat = parseFloat(find.findLocation.latitude);
                var lng = parseFloat(find.findLocation.longitude);

                if (!isNaN(lat) && !isNaN(lng)) {
                    var marker = L.marker([lat, lng]).addTo(window.map);
                    //var find = viewModel.finds.find(find => find.findId === location.locFindId);

                    if (find && find.findId) {

                        var findId = find.findId;

                        let imageHtml = '';
                        //if (find.findImages && find.findImages.length > 0) {
                        //    imageHtml = find.findImages.map(image => {
                        //        if (image && image.imageData && typeof image.imageData === 'string') {
                        //            const fullUrl = image.imageData;
                        //            return `<img src="${fullUrl}" alt="Image" style="max-width: 100%; height: auto; margin-bottom: 5px;">`;
                        //        } else {
                        //            console.error('Invalid image data:', image);
                        //            return '';
                        //        }
                        //    }).join('');
                        //}
                        if (find.findImages && find.findImages.length > 0) {
                            const image = find.findImages[0]; // Access the first image
                            if (image && image.imageData && typeof image.imageData === 'string') {
                                const fullUrl = image.imageData;
                                imageHtml = `<img src="${fullUrl}" alt="Image" style="width: 100px; height: 100px; margin-bottom: 5px; ">`;
                            } else {
                                console.error('Invalid image data:', image);
                                imageHtml = ''; // Fallback in case of invalid data
                            }
                        } else {
                            imageHtml = ''; // Handle case when no images are present
                        }
                        var popupContent = templateHtml
                            .replace('{findName}', find.findName || '')
                            .replace('{username}', viewModel.userName || '')
                            .replace('{speciesName}', find.speciesName || '')
                            .replace('{Images}', imageHtml)
                            .replace('{speciesType}', find.speciesType || '')
                            .replace('{useCategory}', find.useCategory || '')
                            //.replace('{features}', find.features || '')
                            //.replace('{lookAlikes}', find.lookAlikes || '')
                            //.replace('{harvestMethod}', find.harvestMethod || '')
                            //.replace('{tastesLike}', find.tastesLike || '')
                            //.replace('{description}', find.description || '')
                            
                    
                        marker.bindPopup(popupContent);
                        markers[findId] = { marker: marker, lat: lat, lng: lng, originalPopupContent: popupContent };

                        marker.on('click', function () {
                            if (window.dotNetObjectReference) {
                                window.dotNetObjectReference
                                    .invokeMethodAsync('OnMarkerClicked', findId)
                                    .catch(err => console.error('Error in OnMarkerClicked:', err));
                            } else {
                                console.error('DotNetObjectReference is not set.');
                            }
                        });

                    } else {
                        console.error("Find or findId is undefined:", find, location);
                    }
                } else {
                    console.error("Latitude or Longitude is not a valid number:", find.location.UslLatitude, find.location.UslLongitude);
                }
            } else {
                console.error("Latitude or Longitude is undefined:", location);
            }
        });
    });
}
window.centerMapOnFind = function (lat, lng) {
    if (window.map) {
        window.map.setView([lat, lng], 13);
    }
};

window.scrollToFindRow = function (findId) {
    const el = document.getElementById('find-row-' + findId);
    if (!el) {
        console.warn('No row found for findId', findId);
        return;
    }

    el.scrollIntoView({
        behavior: 'smooth',
        block: 'center'
    });

    // Optional highlight
    el.classList.add('find-highlight');
    setTimeout(() => el.classList.remove('find-highlight'), 1500);
};

window.invalidateForagerMapSize = function () {
    if (window.map) {
        requestAnimationFrame(() => window.map.invalidateSize());
    }
};

window.scrollToCreateForm = function () {
    const el = document.getElementById('create-form-row');
    if (!el) {
        console.warn('create-form-row element not found');
        return;
    }

    el.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
    });
};

// keep a reference so we can remove/replace it



window.searchBoundsLayer = window.searchBoundsLayer || null;



window.clearSearchBounds = function () {
    if (!window.map) return;

    if (window.searchBoundsLayer) {
        try { window.map.removeLayer(window.searchBoundsLayer); } catch { }
        window.searchBoundsLayer = null;
    }
};


let geoBoundaryLayer = null;

window.drawGeoBoundaryFromJson = function (featureJson) {
    if (!window.map) return;

    // kill bbox rectangle if it's still around
    if (window.clearSearchBounds) window.clearSearchBounds();

    if (geoBoundaryLayer) {
        geoBoundaryLayer.remove();
        geoBoundaryLayer = null;
    }

    const feature = JSON.parse(featureJson);

    geoBoundaryLayer = L.geoJSON(feature, {
        style: { weight: 2, fillOpacity: 0.12 }
    }).addTo(window.map);

    window.map.fitBounds(geoBoundaryLayer.getBounds(), { padding: [20, 20] });
};


window.clearGeoBoundary = function () {
    if (geoBoundaryLayer) {
        geoBoundaryLayer.remove();
        geoBoundaryLayer = null;
    }
};








