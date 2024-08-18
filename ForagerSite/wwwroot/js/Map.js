

window.map = null;

var markers = {};
window.initializeMap = function (findLocations) {

    if (window.map) {
        console.log('Map already initialized, updating markers.');
        updateMarkers(findLocations);
        return;
    }

    window.map = L.map('map').setView([51.505, -0.09], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(window.map);

    updateMarkers(findLocations);
    window.map.on('click', function (e) {
        var tempId = 'temp-' + Date.now();
        var newMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(window.map);

        markers[tempId] = {
            marker: newMarker,
            lat: e.latlng.lat,
            lng: e.latlng.lng
        };

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

        newMarker.bindPopup(popupContent).openPopup();
    });
}

function updateMarkers(userFindLocations) {
    // Clear existing markers
    for (var key in markers) {
        if (markers.hasOwnProperty(key)) {
            markers[key].marker.remove();
        }
    }
    markers = {}; // Reset markers object

    // Add new markers
    userFindLocations.forEach(location => {
        var marker = L.marker([location.uslLatitude, location.uslLongitude]).addTo(window.map);
        var findId = location.userFind.usfId;

        var popupContent = `<p><strong>Name:</strong> ${location.userFind.usfName}</p>                            
                            <p><strong>Species Name:</strong> ${location.userFind.usfSpeciesName}</p>
                            <p><strong>Species Type:</strong> ${location.userFind.usfSpeciesType}</p>
                            <p><strong>Use Category:</strong> ${location.userFind.usfUseCategory}</p>
                            <p><strong>Distinguishing Features:</strong> ${location.userFind.usfFeatures}</p>
                            <p><strong>Dangerous Lookalikes:</strong> ${location.userFind.usfLookAlikes}</p>
                            <p><strong>Harvest Method:</strong> ${location.userFind.usfHarvestMethod}</p>
                            <p><strong>Tastes Like:</strong> ${location.userFind.usfTastesLike}</p>
                            <p><strong>Notes:</strong> ${location.userFind.usfDescription}</p>
                            <button type="button" onclick="updateFind('${findId}')">Edit</button>
                            <button type="button" onclick="deleteFind('${findId}')">Delete</button>`;                                                                        
        marker.bindPopup(popupContent);
        markers[findId] = { marker: marker, lat: location.uslLatitude, lng: location.uslLongitude };
    });
}

window.updateFind = function (findId) {   

    // Ensure the findId is defined and valid before calling the .NET method
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

            // Update the popup content and open it on the map
            var marker = markers[findId].marker;
            marker.getPopup().setContent(popupContent).openOn(marker._map);
        })
        .catch(error => {
            console.error('Error updating find:', error);
        });
}

window.submitUpdatedFind = function (findId, lat, lng) {
    var form = document.getElementById(`UserFindForm_${findId}`);
    if (!form) {
        console.error('Form element not found!');
        return;
    }

    var findName = form.UsfName.value;
    var speciesName = form.UsfSpeciesName.value;
    var speciesType = form.UsfSpeciesType.value;
    var useCategory = form.UsfUseCategory.value;
    var features = form.UsfFeatures.value;
    var lookalikes = form.UsfLookAlikes.value;
    var harvestMethod = form.UsfHarvestMethod.value;
    var tastesLike = form.UsfTastesLike.value;
    var description = form.UsfDescription.value;

    DotNet.invokeMethodAsync('ForagerSite', 'UpdateFind',
        findId,
        findName,
        speciesName,
        speciesType,
        useCategory,
        features,
        lookalikes,
        harvestMethod,
        tastesLike,
        description,
        lat,
        lng
    ).then(userFindLocations => {
        console.log('Find saved successfully!');
        initializeMap(userFindLocations); // Refresh the map markers after saving
    }).catch((error) => {
        console.error('Error saving find:', error);
    });   
}

window.createFind = function (tempId, lat, lng) {
    var form = document.getElementById(`UserFindForm_${tempId}`);
    if (!form) {
        console.error('Form element not found!');
        return;
    }

    var findName = form.UsfName.value;
    var speciesName = form.UsfSpeciesName.value;
    var speciesType = form.UsfSpeciesType.value;
    var useCategory = form.UsfUseCategory.value;
    var features = form.UsfFeatures.value;
    var lookalikes = form.UsfLookAlikes.value;
    var harvestMethod = form.UsfHarvestMethod.value;
    var tastesLike = form.UsfTastesLike.value;
    var description = form.UsfDescription.value;

    DotNet.invokeMethodAsync('ForagerSite', 'CreateFind',
        findName,
        speciesName,
        speciesType,
        useCategory,
        features,
        lookalikes,
        harvestMethod,
        tastesLike,
        description,
        lat,
        lng
    ).then(userFindLocations => {
        console.log('Find saved successfully!');
        initializeMap(userFindLocations); // Refresh the map markers after saving
    }).catch((error) => {
        console.error('Error saving find:', error);
    });
}    

window.deleteFind = function (findId) {
    if (confirm('Are you sure you want to delete this find?')) {
        DotNet.invokeMethodAsync('ForagerSite', 'DeleteFind', findId)
            .then(userFindLocations => {
                console.log('Find deleted successfully!');
                initializeMap(userFindLocations); // Refresh the map markers after deletion
            })
            .catch((error) => {
                console.error('Error deleting find:', error);
            });
    }
};

