

window.map = null;
var markers = {};
var tempMarker = null;
var userMarker = null;

window.initializeMap = function (json, currentUserId, mapFilter) {

    let userFindsViewModels = JSON.parse(json);

    if (window.map) {
        console.log('Map already initialized, updating markers.');
        updateMarkers(userFindsViewModels, currentUserId, mapFilter);
        return;
    }

    const defaultLatLng = [51.505, -0.09];
    let latLng = defaultLatLng;

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            latLng = [position.coords.latitude, position.coords.longitude];
            initMap(latLng);
        }, function (error) {
            console.error("Error obtaining location: ", error);
            initMap(defaultLatLng);
        });
    } else {
        console.error("Geolocation is not supported by this browser.");
        initMap(defaultLatLng);
    }

    function initMap(latLng) {
        window.map = L.map('map').setView(latLng, 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(window.map);

        L.marker(latLng)
            .addTo(window.map)
            .bindPopup("<b>Your Location</b>")
            .openPopup();

        updateMarkers(userFindsViewModels, currentUserId, mapFilter);

    }
}

window.updateMarkers = function (userFindsViewModels, currentUserId, mapFilter) {

    window.map.on('click', function (e) {
        if (window.tempMarker) {
            window.map.removeLayer(window.tempMarker);
            window.tempMarker = null;
        }

        let tempId = 'temp-' + Date.now();
        window.tempMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(window.map);

        const formHtml = document.getElementById('find-form-container').innerHTML;
        const popupContent = formHtml.replace(/UserFindForm/g, `UserFindForm_${tempId}`);

        setTimeout(() => {
            const saveButton = document.querySelector(`#UserFindForm_${tempId} #saveButton`);
            if (saveButton) {
                saveButton.addEventListener('click', () => {
                    createFind(tempId, e.latlng.lat, e.latlng.lng, currentUserId, mapFilter);
                });
            }
        }, 10);

        console.log('Popup Content:', popupContent);

        window.tempMarker.bindPopup(popupContent).openPopup();
        window.tempMarker.on('popupclose', function () {
            window.map.removeLayer(window.tempMarker);
            window.tempMarker = null;
        });
    });

    Object.keys(markers).forEach(key => {
        markers[key].marker.remove();
    });
    markers = {};

    const templateHtml = document.getElementById('find-popup-template').innerHTML;

    userFindsViewModels.forEach(viewModel => {

        viewModel.userFindLocations.forEach(location => {

            if (location.UslLatitude !== undefined && location.UslLongitude !== undefined) {
                var lat = parseFloat(location.UslLatitude);
                var lng = parseFloat(location.UslLongitude);

                if (!isNaN(lat) && !isNaN(lng)) {
                    var marker = L.marker([lat, lng]).addTo(window.map);
                    var find = viewModel.userFinds.find(find => find.UsFId === location.UslUsfId);

                    if (find && find.UsFId) {
                        var findId = find.UsFId;

                        var popupContent = templateHtml
                            .replace('{UsfName}', find.UsfName || '')
                            .replace('{UssUsername}', viewModel.userSecurity.UssUsername || '')
                            .replace('{UsfSpeciesName}', find.UsfSpeciesName || '')
                            .replace('{UsfSpeciesType}', find.UsfSpeciesType || '')
                            .replace('{UsfUseCategory}', find.UsfUseCategory || '')
                            .replace('{UsfFeatures}', find.UsfFeatures || '')
                            .replace('{UsfLookAlikes}', find.UsfLookAlikes || '')
                            .replace('{UsfHarvestMethod}', find.UsfHarvestMethod || '')
                            .replace('{UsfTastesLike}', find.UsfTastesLike || '')
                            .replace('{UsfDescription}', find.UsfDescription || '');

                        if (viewModel.user.UsrId === currentUserId) {
                            popupContent = popupContent.replace('class="edit-button"', `onclick="updateFind('${findId}', '${currentUserId}', '${mapFilter}')"`)
                                .replace('class="delete-button"', `onclick="deleteFind('${findId}', '${currentUserId}', '${mapFilter}')"`);
                        }

                        marker.bindPopup(popupContent);
                        markers[findId] = { marker: marker, lat: lat, lng: lng, originalPopupContent: popupContent };
                    } else {
                        console.error("Find or UsFId is undefined:", find, location);
                    }
                } else {
                    console.error("Latitude or Longitude is not a valid number:", location.UslLatitude, location.UslLongitude);
                }
            } else {
                console.error("Latitude or Longitude is undefined:", location);
            }
        });
    });
}

window.updateFind = function (findId, currentUserId, mapFilter) {

    if (!findId) {
        console.error('Invalid findId:', findId);
        return;
    }

    DotNet.invokeMethodAsync('ForagerSite', 'GetFindById', findId)
        .then(find => {

            var markerData = markers[findId];
            if (!markerData) {
                console.error('Marker not found for findId:', findId);
                return;
            }
            const formHtml = document.getElementById('update-form-container').innerHTML;
            const popupContent = formHtml.replace(/UpdateFindForm/g, `UpdateFindForm_${findId}`);
                

            setTimeout(() => {

                console.log('Document HTML:', document.documentElement.innerHTML);

                const form = document.querySelector(`#UpdateFindForm_${findId}`);
                if (form) {
                    console.log('Form found:', form);

                    form.querySelector('#UsfName').value = find.usfName || '';
                    form.querySelector('#UsfSpeciesName').value = find.usfSpeciesName || '';
                    form.querySelector('#UsfSpeciesType').value = find.usfSpeciesType || '';
                    form.querySelector('#UsfUseCategory').value = find.usfUseCategory || '';
                    form.querySelector('#UsfFeatures').value = find.usfFeatures || '';
                    form.querySelector('#UsfLookAlikes').value = find.usfLookAlikes || '';
                    form.querySelector('#UsfHarvestMethod').value = find.usfHarvestMethod || '';
                    form.querySelector('#UsfTastesLike').value = find.usfTastesLike || '';
                    form.querySelector('#UsfDescription').value = find.usfDescription || '';

                    console.log(form);
                    const saveButton = form.querySelector('#updateButton');
                    if (saveButton) {
                        console.log('Save button found:', saveButton);

                        saveButton.addEventListener('click', () => {
                            console.log('Save button clicked');
                            submitUpdatedFind(findId, markerData.lat, markerData.lng, currentUserId, mapFilter);
                        });
                    } else {
                        console.error('Save button not found within the form');
                    }
                } else {
                    console.error('Form not found for ID:', `UpdateFindForm_${findId}`);
                }
            }, 10);

            const marker = markers[findId].marker;
            marker.getPopup().setContent(popupContent).openOn(marker._map);
            marker.once('popupclose', function () {
                marker.setPopupContent(markerData.originalPopupContent).openPopup();
            });
        })
        .catch(error => {
            console.error('Error updating find:', error);
        });
}

window.submitUpdatedFind = function (findId, lat, lng, currentUserId, mapFilter) {

    const form = document.querySelector(`#UpdateFindForm_${findId}`);    

    if (!form) {
        console.error('Form not found for findId:', findId);
        return;
    }

    const formData = new FormData(form);

    DotNet.invokeMethodAsync('ForagerSite', 'UpdateFind',
        findId, // GUID
        formData.get('findName'),
        formData.get('speciesName'),
        formData.get('speciesType'),
        formData.get('useCategory'),
        formData.get('features'),
        formData.get('lookalikes'),
        formData.get('harvestMethod'),
        formData.get('tastesLike'),
        formData.get('description'),
        lat,
        lng,
        mapFilter
    ).then(userFindsViewModels => {
        console.log('Find updated successfully');
        initializeMap(userFindsViewModels, currentUserId, mapFilter);
        //updateMarkers(userFindsViewModels, currentUserId);
    }).catch(error => {
        console.error('Error updating find:', error);
    });
}

window.createFind = function (tempId, lat, lng, currentUserId, mapFilter) {
    console.log('createFind called with:', tempId, lat, lng, currentUserId, mapFilter);

    console.log('mapfilter:', mapFilter);

    if (tempMarker) {
        tempMarker.closePopup();
        tempMarker = null;
    }

    if (!tempId) {
        console.error('Invalid tempId:', tempId);
        return;
    }

    const form = document.getElementById(`UserFindForm_${tempId}`);
    if (!form) {
        console.error('Form not found for tempId:', tempId);
        return;
    }

    const formData = new FormData(form);

    const selectedSpeciesTypes = Array.from(form.elements['speciesType'].selectedOptions).map(option => option.value);
    const speciesType = selectedSpeciesTypes.join(',');

    DotNet.invokeMethodAsync('ForagerSite', 'CreateFind',
        formData.get('findName'),
        formData.get('speciesName'),
        speciesType,
        formData.get('useCategory'),
        formData.get('features'),
        formData.get('lookalikes'),
        formData.get('harvestMethod'),
        formData.get('tastesLike'),
        formData.get('description'),
        lat,
        lng,
        mapFilter
    ).then(userFindsViewModels => {
        console.log('Find created successfully');
        initializeMap(userFindsViewModels, currentUserId, mapFilter);
        //updateMarkers(userFindsViewModels, currentUserId);
    }).catch(error => {
        console.error('Error creating find:', error);
    });
}

window.deleteFind = function (findId, currentUserId, mapFilter) {
    if (!findId) {
        console.error('Invalid findId:', findId);
        return;
    }

    DotNet.invokeMethodAsync('ForagerSite', 'DeleteFind', findId, mapFilter)
        .then(userFindsViewModels => {
            console.log('Find deleted successfully');
            initializeMap(userFindsViewModels, currentUserId, mapFilter);
            //updateMarkers(userFindsViewModels, currentUserId);
        }).catch(error => {
            console.error('Error deleting find:', error);
        });


}

