﻿
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
                    createFind(tempId, e.latlng.lat, e.latlng.lng, currentUserId, mapFilter, userName);
                });
            }

            const imageUploadInput = document.querySelector(`#UserFindForm_${tempId} #imageUpload`);
            const imagePreviewContainer = document.querySelector(`#UserFindForm_${tempId} #imagePreview`);
            imageUploadInput.addEventListener('change', function () {
                let files = Array.from(imageUploadInput.files);
                imagePreviewContainer.innerHTML = ''; // Clear existing previews

                files.forEach((file, index) => {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        // Create image preview
                        const img = document.createElement('img');
                        img.src = e.target.result;
                        img.style.maxWidth = '100px';
                        img.style.marginRight = '10px';

                        // Create remove button
                        const removeButton = document.createElement('button');
                        removeButton.innerText = 'Remove';
                        removeButton.type = 'button';
                        removeButton.style.marginLeft = '5px';

                        // Prevent map click event propagation
                        removeButton.addEventListener('click', function (event) {
                            event.stopPropagation(); // Stop the event from propagating to Leaflet

                            img.remove(); // Remove image preview
                            removeButton.remove(); // Remove button

                            // Remove the file from the list and update the input
                            files.splice(index, 1);
                            const dataTransfer = new DataTransfer();
                            files.forEach(file => dataTransfer.items.add(file)); // Re-add remaining files
                            imageUploadInput.files = dataTransfer.files;
                        });

                        imagePreviewContainer.appendChild(img);
                        imagePreviewContainer.appendChild(removeButton);
                    };
                    reader.readAsDataURL(file); // Read the file for preview
                });
            });

        }, 10);

        window.tempMarker.bindPopup(popupContent).openPopup();
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
                            .replace('{features}', find.features || '')
                            .replace('{lookAlikes}', find.lookAlikes || '')
                            .replace('{harvestMethod}', find.harvestMethod || '')
                            .replace('{tastesLike}', find.tastesLike || '')
                            .replace('{description}', find.description || '')
                            .replace('id="details-button"', `onclick="getDetails('${findId}', '${currentUserId}', '${mapFilter}', '${viewModel.userId}', '${viewModel.userName}', '${userName}')"`);
                        if (viewModel.userId === currentUserId) {
                            const serializedViewModel = JSON.stringify(currentViewModel).replace(/"/g, '&quot;');
                            popupContent = popupContent           
                                .replace('id="edit-button"', `onclick="updateFind('${findId}', '${currentUserId}', '${mapFilter}', '${userName}', '${serializedViewModel}')"`)
                                .replace('id="delete-button"', `onclick="deleteFind('${findId}', '${currentUserId}', '${mapFilter}')"`);
                        } else {
                            popupContent = popupContent.replace('id="edit-button"', 'style="display: none;"')
                                .replace('id="delete-button"', 'style="display: none;"');
                        }
                    
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
window.createFind = function (tempId, lat, lng, currentUserId, mapFilter, userName) {
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

    formData.append('userName', userName); // Pass the username

    fetch('/api/upload/upload', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(uploadedFileUrls => {
            // Handle the rest of the form submission with uploadedFileUrls and other form data
            dotNetObjectReference.invokeMethodAsync('CreateFind',
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
                mapFilter,
                uploadedFileUrls // Pass the uploaded file URLs
            ).then(userFindsViewModels => {
                initializeMap(userFindsViewModels, currentUserId, mapFilter, userName);
            }).catch(error => {
                console.error('Error creating find:', error);
            });
        })
        .catch(error => {
            console.error('Error uploading files:', error);
        });
};
window.updateFind = function (findId, currentUserId, mapFilter, userName, serializedViewModel) {

    if (!findId) {
        console.error('Invalid findId:', findId);
        return;
    }
    const currentViewModel = JSON.parse(serializedViewModel);
    console.log(currentViewModel);
    var find = currentViewModel.finds.find(find => find.findId === findId);

    var markerData = markers[findId];
    if (!markerData) {
        console.error('Marker not found for findId:', findId);
        return;
    }
    const formHtml = document.getElementById('update-form-container').innerHTML;

    if (!formHtml) {
        console.error('Update form container not found!');
        return;
    }
    const popupContent = formHtml.replace(/UpdateFindForm/g, `UpdateFindForm_${findId}`);

    const marker = markers[findId].marker;
    marker.getPopup().setContent('').openOn(marker._map);

    setTimeout(() => {
        const popupElement = marker.getPopup().getElement().querySelector('.leaflet-popup-content');
        if (popupElement) {

            popupElement.innerHTML = popupContent;          
            const form = document.getElementById(`UpdateFindForm_${findId}`);           
           
            if (form) {
                // Populate the form with existing data
                form.querySelector('#findName').value = find.findName || '';
                form.querySelector('#speciesName').value = find.speciesName || '';
                form.querySelector('#speciesType').value = find.speciesType || '';
                form.querySelector('#useCategory').value = find.useCategory || '';
                form.querySelector('#features').value = find.features || '';
                form.querySelector('#lookAlikes').value = find.lookAlikes || '';
                form.querySelector('#harvestMethod').value = find.harvestMethod || '';
                form.querySelector('#tastesLike').value = find.tastesLike || '';
                form.querySelector('#description').value = find.description || '';

                // Load the existing images into the gallery
                const imageGallery = form.querySelector('#existingImageGallery');
                imageGallery.innerHTML = ''; // Clear any existing content

                var deletedImageUrls = [];

                if (find.findImages && find.findImages.length > 0) {
                    find.findImages.forEach(image => {
                        if (image && image.imageData && typeof image.imageData === 'string') {
                            console.log('Image URL:', `https://localhost:7007${image.imageData}`); // Debug log for image URL

                            const imageDiv = document.createElement('div');
                            imageDiv.style.display = 'inline-block';
                            imageDiv.style.position = 'relative';
                            imageDiv.style.marginRight = '10px';

                            const img = document.createElement('img');
                            img.src = `https://localhost:7007${image.imageData}`;
                            img.style.maxWidth = '100px';
                            img.style.marginBottom = '5px';
                            img.alt = 'Image';

                            const removeButton = document.createElement('button');
                            removeButton.innerText = 'Remove';
                            removeButton.type = 'button';
                            removeButton.style.position = 'absolute';
                            removeButton.style.top = '0';
                            removeButton.style.right = '0';

                            removeButton.addEventListener('click', function () {
                                imageDiv.remove(); // Remove from UI
                                event.stopPropagation();
                                deleteImage(userName, image.imageData);
                                var deletedImageUrl = image.imageData;
                                deletedImageUrls.push(deletedImageUrl);// Delete image from server & DB
                            });

                            imageDiv.appendChild(img);
                            imageDiv.appendChild(removeButton);
                            imageGallery.appendChild(imageDiv);
                        } else {
                            console.error('Invalid image data:', image);
                        }
                    });
                } else {
                    console.log('No images found for this find.');
                }
                // Handle new image uploads with previews
                const imageUploadInput = form.querySelector('#imageUpload');
                const newImagePreviewContainer = form.querySelector('#newImagePreview');
                imageUploadInput.addEventListener('change', function () {
                    const files = Array.from(imageUploadInput.files);
                    newImagePreviewContainer.innerHTML = ''; // Clear previous previews

                    files.forEach((file, index) => {
                        const reader = new FileReader();
                        reader.onload = function (e) {
                            const img = document.createElement('img');
                            img.src = e.target.result;
                            img.style.maxWidth = '100px';
                            img.style.marginRight = '10px';

                            const removeButton = document.createElement('button');
                            removeButton.innerText = 'Remove';
                            removeButton.type = 'button';
                            removeButton.addEventListener('click', function () {
                                img.remove(); // Remove preview image
                                removeButton.remove();
                                event.stopPropagation();
                                // Remove file from the input
                                const dataTransfer = new DataTransfer();
                                files.splice(index, 1); // Remove file from the list
                                files.forEach(file => dataTransfer.items.add(file));
                                imageUploadInput.files = dataTransfer.files;
                            });
                            newImagePreviewContainer.appendChild(img);
                            newImagePreviewContainer.appendChild(removeButton);
                        };
                        reader.readAsDataURL(file);
                    });
                });
                // Handle the Save button click
                const saveButton = form.querySelector('#updateButton');
                if (saveButton) {
                    saveButton.addEventListener('click', () => {
                        console.log('Save button clicked');
                        submitUpdatedFind(findId, markerData.lat, markerData.lng, currentUserId, mapFilter, userName, deletedImageUrls);
                    });
                }
            } else {
                console.error('Form not found for ID:', `UpdateFindForm_${findId}`);
            }
            
        }
    }, 10);
    marker.once('popupclose', function () {
        marker.setPopupContent(markerData.originalPopupContent).openPopup();
    });
};
window.deleteImage = function (userName, imageUrl) {
    if (confirm('Are you sure you want to delete this image?')) {
        fetch('/api/upload/delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                fileUrl: imageUrl,
                userName: userName // Ensure userName is available in the scope
            }),
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                console.log('Image deleted successfully.');
            } else {
                console.error('Error deleting image:', result.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
    }
};
window.submitUpdatedFind = function (findId, lat, lng, currentUserId, mapFilter, userName, deletedImageUrls) {
    const form = document.querySelector(`#UpdateFindForm_${findId}`);

    if (!form) {
        console.error('Form not found for findId:', findId);
        return;
    }
    const formData = new FormData(form);

    formData.append('findId', findId);
    formData.append('lat', lat);
    formData.append('lng', lng);
    formData.append('mapFilter', mapFilter);
    formData.append('userName', userName);

    // Fetch the image files from the form and append them
    const imageUploadInput = form.querySelector('#imageUpload');
    if (imageUploadInput && imageUploadInput.files.length > 0) {
        Array.from(imageUploadInput.files).forEach(file => {
            formData.append('files', file);
        });
    }
    var uploadedUrls = [];
    // Send the form data to the server
    fetch('/api/upload/upload', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
        .then(uploadResult => {
            uploadedUrls = uploadResult;
        //if (uploadResult.length > 0) {
            // If file upload is successful, update the find with the URLs
            dotNetObjectReference.invokeMethodAsync('UpdateFind',
                findId,
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
                mapFilter,
                uploadedUrls,
                deletedImageUrls // Send the uploaded file URLs to the server
            )
            .then(userFindsViewModels => {
                console.log('Find updated successfully');
                initializeMap(userFindsViewModels, currentUserId, mapFilter, userName);
            })
            .catch(error => {
                console.error('Error updating find:', error);
            });
        //} else {
        //    console.error('File upload failed');
        //}
    })
    .catch(error => {
        console.error('Error uploading files:', error);
    });
};
window.deleteFind = function (findId, currentUserId, mapFilter, userName) {
    if (confirm("Are you sure you want to delete this discovery?")) {
        if (!findId) {
            console.error('Invalid findId:', findId);
            return;
        }
        dotNetObjectReference.invokeMethodAsync('DeleteFind', findId, mapFilter)
        .then(userFindsViewModels => {
            console.log('Find deleted successfully');
            initializeMap(userFindsViewModels, currentUserId, mapFilter, userName);
            //updateMarkers(userFindsViewModels, currentUserId);
        }).catch(error => {
            console.error('Error deleting find:', error);
        });
    }
};
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

