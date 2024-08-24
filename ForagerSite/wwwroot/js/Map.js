

window.map = null;
var markers = {};
var tempMarker = null; 

window.initializeMap = function (json, currentUserId) {

    let userFindsViewModels = JSON.parse(json);
    
    if (window.map) {
        console.log('Map already initialized, updating markers.');
        updateMarkers(userFindsViewModels, currentUserId);
        return;
    }

    window.map = L.map('map').setView([51.505, -0.09], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(window.map);

    updateMarkers(userFindsViewModels, currentUserId);
    window.map.on('click', function (e) {
        //var tempId = 'temp-' + Date.now();
        //var newMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(window.map);
        if (tempMarker) {
            window.map.removeLayer(tempMarker); // Remove the existing temporary marker
            tempMarker = null;
        }

        var tempId = 'temp-' + Date.now();
        tempMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(window.map);

        //markers[tempId] = {
        //    marker: newMarker,
        //    lat: e.latlng.lat,
        //    lng: e.latlng.lng
        //};

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

                                <button type="button" onclick="createFind('${tempId}', ${e.latlng.lat}, ${e.latlng.lng})">Save</button>
                            </form>`;

        //newMarker.bindPopup(popupContent).openPopup();
        tempMarker.bindPopup(popupContent);
        tempMarker.openPopup(); 

        tempMarker.on('popupclose', function () {
            map.removeLayer(tempMarker);
            tempMarker = null;
        });
    });
}

function updateMarkers(userFindsViewModels, currentUserId) {
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
                            popupContent += `<button type="button" onclick="updateFind('${findId}')">Edit</button>
                                             <button type="button" onclick="deleteFind('${findId}')">Delete</button>`;
                        }
                        console.log("viewmodel id:", viewModel.user.UsrId, "currentUserId :", currentUserId)

                        marker.bindPopup(popupContent);
                        markers[findId] = { marker: marker, lat: lat, lng: lng };
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
window.updateFind = function (findId) {   

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
                                    <button type="button" onclick="submitUpdatedFind('${findId}', ${markerData.lat}, ${markerData.lng})">Save</button>
                                </form>`;

            const marker = markers[findId].marker;
            marker.getPopup().setContent(popupContent).openOn(marker._map);
        })
        .catch(error => {
            console.error('Error updating find:', error);
        });
}

window.submitUpdatedFind = function (findId, lat, lng) {

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
        lng
    ).then(userFindsViewModels => {
        console.log('Find updated successfully');
        initializeMap(userFindsViewModels);
    }).catch(error => {
        console.error('Error updating find:', error);
    });
}



window.createFind = function (tempId, lat, lng) {

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
        lng).then(userFindsViewModels => {
            console.log('Find created successfully');
            initializeMap(userFindsViewModels);
        }).catch(error => {
            console.error('Error creating find:', error);
        });            
}
   

window.deleteFind = function (findId) {
    if (!findId) {
        console.error('Invalid findId:', findId);
        return;
    }

    DotNet.invokeMethodAsync('ForagerSite', 'DeleteFind', findId)
        .then(userFindsViewModels => {
            console.log('Find deleted successfully');
            initializeMap(userFindsViewModels);
        }).catch(error => {
            console.error('Error deleting find:', error);
        });

        
}

