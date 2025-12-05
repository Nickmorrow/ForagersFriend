
window.map = null;
var markers = {};
var tempMarker = null;
var userMarker = null;
var dotNetObjectReference = null;
window.initializeMap = function (json, currentUserId, mapFilter, userName) {
    let userFindsViewModels = JSON.parse(json);

    if (window.map) {
        console.log('Map already initialized, updating markers.');
        updateMarkers(userFindsViewModels, currentUserId, mapFilter, userName);
        return;
    }

    const defaultLatLng = [51.505, -0.09];
    let latLng = defaultLatLng;

    const northAmericaBounds = [
        [7.0, -168.0], // Southwest corner (latitude, longitude)
        [83.0, -30.0]  // Northeast corner (latitude, longitude)
    ];

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
        window.map = L.map('map', {
            center: latLng,
            zoom: 13,
            maxBounds: northAmericaBounds, // Restrict panning to North America
            maxBoundsViscosity: 1.0 // Stickiness when reaching bounds
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            minZoom: 4, // Minimum zoom level to focus on North America
            maxZoom: 18 // Maximum zoom level
        }).addTo(window.map);

        L.marker(latLng)
            .addTo(window.map)
            .bindPopup("<b>Your Location</b>")
            .openPopup();

        updateMarkers(userFindsViewModels, currentUserId, mapFilter, userName);
    }
}
window.setDotNetObjectReference = function (reference) {
    dotNetObjectReference = reference;
};

window.updateMarkers = function (userFindsViewModels, currentUserId, mapFilter, userName) {
    //Create new find
    //window.map.on('click', function (e) {
    //    if (window.tempMarker) {
    //        window.map.removeLayer(window.tempMarker);
    //        window.tempMarker = null;
    //    }

    //    let tempId = 'temp-' + Date.now();
    //    window.tempMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(window.map);

    //    const formHtml = document.getElementById('find-form-container').innerHTML;
    //    const popupContent = formHtml.replace(/UserFindForm/g, `UserFindForm_${tempId}`);

    //    setTimeout(() => {
    //        const saveButton = document.querySelector(`#UserFindForm_${tempId} #saveButton`);
    //        if (saveButton) {
    //            saveButton.addEventListener('click', () => {
    //                createFind(tempId, e.latlng.lat, e.latlng.lng, currentUserId, mapFilter, userName);
    //            });
    //        }

    //        const imageUploadInput = document.querySelector(`#UserFindForm_${tempId} #imageUpload`);
    //        const imagePreviewContainer = document.querySelector(`#UserFindForm_${tempId} #imagePreview`);
    //        imageUploadInput.addEventListener('change', function () {
    //            let files = Array.from(imageUploadInput.files);
    //            imagePreviewContainer.innerHTML = ''; // Clear existing previews

    //            files.forEach((file, index) => {
    //                const reader = new FileReader();
    //                reader.onload = function (e) {
    //                    // Create image preview
    //                    const img = document.createElement('img');
    //                    img.src = e.target.result;
    //                    img.style.maxWidth = '100px';
    //                    img.style.marginRight = '10px';

    //                    // Create remove button
    //                    const removeButton = document.createElement('button');
    //                    removeButton.innerText = 'Remove';
    //                    removeButton.type = 'button';
    //                    removeButton.style.marginLeft = '5px';

    //                    // Prevent map click event propagation
    //                    removeButton.addEventListener('click', function (event) {
    //                        event.stopPropagation(); // Stop the event from propagating to Leaflet

    //                        img.remove(); // Remove image preview
    //                        removeButton.remove(); // Remove button

    //                        // Remove the file from the list and update the input
    //                        files.splice(index, 1);
    //                        const dataTransfer = new DataTransfer();
    //                        files.forEach(file => dataTransfer.items.add(file)); // Re-add remaining files
    //                        imageUploadInput.files = dataTransfer.files;
    //                    });

    //                    imagePreviewContainer.appendChild(img);
    //                    imagePreviewContainer.appendChild(removeButton);
    //                };
    //                reader.readAsDataURL(file); // Read the file for preview
    //            });
    //        });

    //    }, 10);

    //    window.tempMarker.bindPopup(popupContent).openPopup();
    //    window.tempMarker.on('popupclose', function () {
    //        window.map.removeLayer(window.tempMarker);
    //        window.tempMarker = null;
    //    });
    //});
    window.map.on('click', function (e) {
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

        setTimeout(() => {
            const button = document.getElementById("open-create-form");
            if (button) {
                button.addEventListener("click", function () {
                    // ✅ Close popup before triggering .NET
                    window.tempMarker.closePopup();

                    if (window.dotNetObjectReference) {
                        window.dotNetObjectReference.invokeMethodAsync("TriggerCreateForm", lat, lng)
                            .catch(err => console.error("Error triggering create form:", err));
                    }
                });
            }
        }, 10);

        window.tempMarker.on('popupclose', function () {
            window.map.removeLayer(window.tempMarker);
            window.tempMarker = null;
        });
    });


    // Display all finds on map
    Object.keys(markers).forEach(key => {
        markers[key].marker.remove();
    });
    markers = {};

    const templateHtml = document.getElementById('find-popup-template').innerHTML;

    userFindsViewModels.forEach(viewModel => {

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
                        if (find.findImages && find.findImages.length > 0) {
                            imageHtml = find.findImages.map(image => {
                                if (image && image.imageData && typeof image.imageData === 'string') {
                                    const fullUrl = `https://localhost:7007${image.imageData}`;
                                    return `<img src="${fullUrl}" alt="Image" style="max-width: 100%; height: auto; margin-bottom: 5px;">`;
                                } else {
                                    console.error('Invalid image data:', image);
                                    return '';
                                }
                            }).join('');
                        }
                        if (find.findImages && find.findImages.length > 0) {
                            const image = find.findImages[0]; // Access the first image
                            if (image && image.imageData && typeof image.imageData === 'string') {
                                const fullUrl = `https://localhost:7007${image.imageData}`;
                                imageHtml = `<img src="${fullUrl}" alt="Image" style="max-width: 100%; height: auto; margin-bottom: 5px;">`;
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
                            .replace('id="details-button"', `onclick="getDetails('${findId}', '${currentUserId}', '${mapFilter}', '${viewModel.userId}', '${viewModel.userName}', '${userName}')"`);
                        //if (viewModel.userId === currentUserId) {
                        //    const serializedViewModel = JSON.stringify(currentViewModel).replace(/"/g, '&quot;');
                        //    popupContent = popupContent           
                        //        .replace('id="edit-button"', `onclick="updateFind('${findId}', '${currentUserId}', '${mapFilter}', '${userName}', '${serializedViewModel}')"`)
                        //        .replace('id="delete-button"', `onclick="deleteFind('${findId}', '${currentUserId}', '${mapFilter}')"`);
                        //} else {
                        //    popupContent = popupContent.replace('id="edit-button"', 'style="display: none;"')
                        //        .replace('id="delete-button"', 'style="display: none;"');
                        //}
                    
                        marker.bindPopup(popupContent);
                        markers[findId] = { marker: marker, lat: lat, lng: lng, originalPopupContent: popupContent };
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
window.getDetails = function (findId, currentUserId, mapFilter, findUserId, findUserName, userName) {
    
    if (dotNetObjectReference) {
        dotNetObjectReference.invokeMethodAsync('GetDetails',
            findId,
            mapFilter,
            findUserId,
            findUserName
        )
            .then(userFindsViewModels => {
                console.log('Find updated successfully');
                initializeMap(userFindsViewModels, currentUserId, mapFilter, userName);
            })
            .catch(error => {
                console.error('Error updating find:', error);
            });
    } else {
        console.error('DotNetObjectReference is not set.');
    }
}

