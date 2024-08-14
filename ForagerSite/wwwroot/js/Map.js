function initializeMap() {
    var map = L.map('map').setView([51.505, -0.09], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    map.on('click', function (e) {
        var newMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(map);

        var popupContent = `<form id="UserFindForm">
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

                                <button type="button" onclick="saveFind(${e.latlng.lat}, ${e.latlng.lng})">Save</button>
                            </form>`;

        newMarker.bindPopup(popupContent).openPopup();
    });
}

function saveFind(lat, lng) {
    var form = document.getElementById('UserFindForm');
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
    ).then(() => {
        console.log('Find saved successfully!');
    }).catch((error) => {
        console.error('Error saving find:', error);
    });
    //DotNet.invokeMethodAsync('ForagerSite', 'SaveFind', {
    //    Name: findName,
    //    SpeciesName: speciesName,
    //    SpeciesType: speciesType,
    //    UseCategory: useCategory,
    //    Features: features,
    //    Lookalikes: lookalikes,
    //    HarvestMethod: harvestMethod,
    //    TastesLike: tastesLike,
    //    Description: description,
    //    Latitude: lat,
    //    Longitude: lng
    //}).then(() => {
    //    console.log('Find saved successfully!');
    //}).catch((error) => {
    //    console.error('Error saving find:', error);
    //});
}
