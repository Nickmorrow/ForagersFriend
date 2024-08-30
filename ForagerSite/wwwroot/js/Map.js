

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

    // Default location if no user location is available
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

function updateMarkers(userFindsViewModels, currentUserId, mapFilter) {

    window.map.on('click', function (e) {
        if (window.tempMarker) {
            window.map.removeLayer(window.tempMarker);
            window.tempMarker = null;
        }

        let tempId = 'temp-' + Date.now();
        window.tempMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(window.map);

        var popupContent = `<form id="UserFindForm_${tempId}">
                                            <label for="findName">Name:</label>
                                            <input type="text" id="UsfName" name="findName"><br>

                                            <label for="speciesName">Species Name:</label>
                                            <input type="text" id="UsfSpeciesName" name="speciesName"><br>

                                            <label for="speciesType">Species Type:</label>
                                            <input type="text" id="UsfSpeciesType" name="speciesType"><br>

                                            <label for="useCategory">Use Category:</label>
                                            <input type="text" id="UsfUseCategory" name="useCategory"><br>

                                            <label for="features">Distinguishing Features:</label>
                                            <input type="text" id="UsfFeatures" name="features"><br>

                                            <label for="lookalikes">Dangerous Lookalikes:</label>
                                            <input type="text" id="UsfLookAlikes" name="lookalikes"><br>

                                            <label for="harvestMethod">Harvest Method:</label>
                                            <input type="text" id="UsfHarvestMethod" name="harvestMethod"><br>

                                            <label for="tastesLike">Tastes Like:</label>
                                            <input type="text" id="UsfTastesLike" name="tastesLike"><br>

                                            <label for="description">Notes:</label>
                                            <input type="text" id="UsfDescription" name="description"><br>

                                            <button type="button" onclick="createFind('${tempId}', ${e.latlng.lat}, ${e.latlng.lng},'${currentUserId}','${mapFilter}')">Save</button>
                                        </form>`;

        window.tempMarker.bindPopup(popupContent);
        window.tempMarker.openPopup();

        window.tempMarker.on('popupclose', function () {
            window.map.removeLayer(window.tempMarker);
            window.tempMarker = null;
        });
    });

    Object.keys(markers).forEach(key => {
        markers[key].marker.remove();
    });
    markers = {}; 

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

                        var popupContent = `<p><strong>Name:</strong> ${find.UsfName}</p> 
                        <p><strong>Discovered by:</strong> ${viewModel.userSecurity.UssUsername}</p>
                        <p><strong>Species Name:</strong> ${find.UsfSpeciesName}</p>
                        <p><strong>Species Type:</strong> ${find.UsfSpeciesType}</p>
                        <p><strong>Use Category:</strong> ${find.UsfUseCategory}</p>
                        <p><strong>Distinguishing Features:</strong> ${find.UsfFeatures}</p>
                        <p><strong>Dangerous Lookalikes:</strong> ${find.UsfLookAlikes}</p>
                        <p><strong>Harvest Method:</strong> ${find.UsfHarvestMethod}</p>
                        <p><strong>Tastes Like:</strong> ${find.UsfTastesLike}</p>
                        <p><strong>Notes:</strong> ${find.UsfDescription}</p>`;                       

                        if (viewModel.user.UsrId === currentUserId) {
                            popupContent += `<button type="button" onclick="updateFind('${findId}','${currentUserId}','${mapFilter}')">Edit</button>
                                             <button type="button" onclick="deleteFind('${findId}','${currentUserId}','${mapFilter}')">Delete</button>`;
                        }
                        console.log("viewmodel id:", viewModel.user.UsrId, "currentUserId :", currentUserId)

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

            var popupContent = `<form id="UserFindForm_${findId}">
                                    <label for="findName">Name:</label>
                                    <input type="text" id="UsfName" name="findName" value="${find.usfName || ''}"><br>
                                    <label for="speciesName">Species Name:</label>
                                    <input type="text" id="UsfSpeciesName" name="speciesName" value="${find.usfSpeciesName || ''}"><br>
                                    <label for="speciesType">Species Type:</label>
                                    <input type="text" id="UsfSpeciesType" name="speciesType" value="${find.usfSpeciesType || ''}"><br>
                                    <label for="useCategory">Use Category:</label>
                                    <input type="text" id="UsfUseCategory" name="useCategory" value="${find.usfUseCategory || ''}"><br>
                                    <label for="features">Distinguishing Features:</label>
                                    <input type="text" id="UsfFeatures" name="features" value="${find.usfFeatures || ''}"><br>
                                    <label for="lookalikes">Dangerous Lookalikes:</label>
                                    <input type="text" id="UsfLookAlikes" name="lookalikes" value="${find.usfLookAlikes || ''}"><br>
                                    <label for="harvestMethod">Harvest Method:</label>
                                    <input type="text" id="UsfHarvestMethod" name="harvestMethod" value="${find.usfHarvestMethod || ''}"><br>
                                    <label for="tastesLike">Tastes Like:</label>
                                    <input type="text" id="UsfTastesLike" name="tastesLike" value="${find.usfTastesLike || ''}"><br>
                                    <label for="description">Notes:</label>
                                    <input type="text" id="UsfDescription" name="description" value="${find.usfDescription || ''}"><br>
                                    <button type="button" onclick="submitUpdatedFind('${findId}', ${markerData.lat}, ${markerData.lng}, '${currentUserId}','${mapFilter}')">Save</button>
                                </form>`;

            const marker = markers[findId].marker;
            marker.getPopup().setContent(popupContent).openOn(marker._map);
            marker.once('popupclose', function () {
                // Restore the original popup content if the edit popup is closed without saving
                marker.setPopupContent(markerData.originalPopupContent).openPopup();
            });
        })
        .catch(error => {
            console.error('Error updating find:', error);
        });
}

window.submitUpdatedFind = function (findId, lat, lng, currentUserId, mapFilter) {

    const form = document.getElementById(`UserFindForm_${findId}`);
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

    console.log('mapfilter:',mapFilter);

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

    DotNet.invokeMethodAsync('ForagerSite', 'CreateFind',
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

