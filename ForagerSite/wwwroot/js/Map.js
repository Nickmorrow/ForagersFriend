
var markers = {};
window.initializeMap = function () {
    var map = L.map('map').setView([51.505, -0.09], 13);
    //alert('initializeMap called');
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    map.on('click', function (e) {
        var tempId = 'temp-' + Date.now(); 
        var newMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(map);

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

                                <button type="button" onclick="saveFind('${tempId}', ${e.latlng.lat}, ${e.latlng.lng})">Save</button>
                            </form>`;

        newMarker.bindPopup(popupContent).openPopup();

    });
}

window.saveFind = function (tempId, lat, lng) {
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

    DotNet.invokeMethodAsync('ForagerSite', 'SaveFind',
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
    ).then(result => {

        var { userFindId, userFindLocationId } = result;

        console.log('Find saved successfully!');

        if (typeof markers === 'undefined') {
            console.error('Markers is not defined!');
            return;
        }

        var markerData = markers[tempId];
        delete markers[tempId];
        markers[userFindLocationId] = markerData;

        var popupContent = `<p><strong>Name:</strong> ${findName}</p>
                            <p><strong>Species Name:</strong> ${speciesName}</p>
                            <p><strong>Species Type:</strong> ${speciesType}</p>
                            <p><strong>Use Category:</strong> ${useCategory}</p>
                            <p><strong>Distinguishing Features:</strong> ${features}</p>
                            <p><strong>Dangerous Lookalikes:</strong> ${lookalikes}</p>
                            <p><strong>Harvest Method:</strong> ${harvestMethod}</p>
                            <p><strong>Tastes Like:</strong> ${tastesLike}</p>
                            <p><strong>Notes:</strong> ${description}</p>
                            <button type="button" onclick="editFind(${lat}, ${lng}, this)">Edit</button>`;

        var marker = markers[userFindLocationId].marker;

        marker.getPopup().setContent(popupContent);

        //marker.setPopupContent(popupContent);

    }).catch((error) => {
        console.error('Error saving find:', error);
    });

}












