// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
    document.addEventListener('DOMContentLoaded', function () {
        console.log('DOM loaded - initializing image upload');

    const inputfile = document.getElementById("fileUpload");
    const profilePreview = document.getElementById("profilePreview");
    const uploadForm = document.getElementById("uploadForm");

    if (inputfile) {
        console.log('File upload element found');
    inputfile.addEventListener("change", uploadImage);
        } else {
        console.log('File upload element not found - check ID');
        }

    function uploadImage() {
        console.log('uploadImage function called');

    if (inputfile.files && inputfile.files[0]) {
                const file = inputfile.files[0];
    console.log('File selected:', file.name, file.type, file.size);

    // Validate file type
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!validTypes.includes(file.type)) {
        showNotification('Please upload a valid image file (JPEG, PNG, GIF, or WEBP)', 'error');
    inputfile.value = ''; // Clear the file input
    return;
                }

    // Validate file size (max 5MB)
    const maxSize = 5 * 1024 * 1024; // 5MB
                if (file.size > maxSize) {
        showNotification('File size must be less than 5MB', 'error');
    inputfile.value = ''; // Clear the file input
    return;
                }

    // Preview the image
    let imgLink = URL.createObjectURL(file);
    if (profilePreview) {
        profilePreview.src = imgLink;
    console.log('Preview image updated');
                }

    // Auto-submit the form after a short delay to show preview
    setTimeout(function () {
                    if (uploadForm) {
        console.log('Submitting image upload form');
    uploadForm.submit();
                    } else {
        console.error('Upload form not found');
                    }
                }, 100);
            } else {
        console.log('No file selected');
            }
        }

    // Helper function to show notifications
    function showNotification(message, type) {
        console.log('Notification:', type, message);

    // Check if a notification container exists
    let notificationContainer = document.getElementById('notificationContainer');
    if (!notificationContainer) {
        // Create one if it doesn't exist
        notificationContainer = document.createElement('div');
    notificationContainer.id = 'notificationContainer';
    notificationContainer.style.position = 'fixed';
    notificationContainer.style.top = '20px';
    notificationContainer.style.right = '20px';
    notificationContainer.style.zIndex = '9999';
    document.body.appendChild(notificationContainer);
            }

    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type === 'error' ? 'danger' : 'success'} alert-dismissible fade show`;
    alertDiv.style.marginTop = '10px';
    alertDiv.innerHTML = `
    ${message}
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            notificationContainer.appendChild(alertDiv);

            // Auto-remove after 3 seconds
            setTimeout(() => {
                if (alertDiv && alertDiv.remove) {
                    alertDiv.remove();
                }
            }, 3000);
        }
    });


// picture for land

    //document.addEventListener('DOMContentLoaded', function() {
    //    const dropArea = document.getElementById('drop-area');
    //const fileInput = document.getElementById('Imagefile');
    //const previewImage = document.getElementById('previewImage');

    //// Click on drop area triggers file input
    //dropArea.addEventListener('click', function() {
    //    fileInput.click();
    //    });

    //// Handle file selection
    //fileInput.addEventListener('change', function(e) {
    //        if (this.files && this.files[0]) {
    //            const file = this.files[0];

    //// Validate file type
    //const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    //if (!validTypes.includes(file.type)) {
    //    alert('Please upload a valid image (JPEG, PNG, GIF, or WEBP)');
    //this.value = '';
    //return;
    //            }

    //            // Validate file size (max 5MB)
    //            if (file.size > 5 * 1024 * 1024) {
    //    alert('File size must be less than 5MB');
    //this.value = '';
    //return;
    //            }

    //// Preview image
    //const reader = new FileReader();
    //reader.onload = function(e) {
    //    previewImage.src = e.target.result;
    //previewImage.style.width = '80px';
    //previewImage.style.height = '80px';
    //            };
    //reader.readAsDataURL(file);
    //        }
    //    });

    //    // Drag and drop handlers
    //    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    //    dropArea.addEventListener(eventName, preventDefaults, false);
    //    });

    //function preventDefaults(e) {
    //    e.preventDefault();
    //e.stopPropagation();
    //    }
        
    //    ['dragenter', 'dragover'].forEach(eventName => {
    //    dropArea.addEventListener(eventName, highlight, false);
    //    });
        
    //    ['dragleave', 'drop'].forEach(eventName => {
    //    dropArea.addEventListener(eventName, unhighlight, false);
    //    });

    //function highlight() {
    //    dropArea.style.backgroundColor = '#e9ecef';
    //dropArea.style.border = '2px dashed #007bff';
    //    }

    //function unhighlight() {
    //    dropArea.style.backgroundColor = '#f8f9fa';
    //dropArea.style.border = '1px solid #dee2e6';
    //    }

    //dropArea.addEventListener('drop', handleDrop, false);

    //function handleDrop(e) {
    //        const dt = e.dataTransfer;
    //const files = dt.files;
    //fileInput.files = files;

    //// Trigger change event to preview
    //const event = new Event('change', {bubbles: true });
    //fileInput.dispatchEvent(event);
    //    }
    //});



// Wait for DOM to be fully loaded rating
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded - initializing');

    // Check if elements exist before adding event listeners
    const ratingLink = document.querySelector('[data-bs-target="#ratingModal"]');
    if (ratingLink) {
        console.log('Rating link found');
        // Your existing code for rating link
    } else {
        console.log('Rating link not found on this page');
    }
});

    
//$(document).ready(function () {
//    let selectedStars = null;

//    // Load ratings when modal opens
//    $('[data-bs-target="#ratingModal"]').on('click', function () {
//        loadWebsiteRatings();
//        loadWebsiteRatingStats();
//        resetForm();
//    });

//    // Show comment field on star selection
//    $('.star-rating-modal input').on('change', function () {
//        selectedStars = $(this).val();
//        $('#commentField').slideDown(200);
//    });

//    // Submit rating
//    $('#submitRatingBtn').on('click', function () {
//        if (selectedStars) {
//            saveWebsiteRating(selectedStars);
//        } else {
//            showMessage('Please select a rating', 'error');
//        }
//    });

//    // Load all user ratings
//    function loadWebsiteRatings() {
//        $('#ratingsList').html('<div class="text-center text-muted py-3">Loading...</div>');

//        $.ajax({
//            url: '/api/Rating?handler=GetWebsiteRatings',
//            type: 'GET',
//            success: function (ratings) {
//                displayRatings(ratings);
//            },
//            error: function () {
//                $('#ratingsList').html('<div class="text-center text-muted py-3">No ratings yet. Be the first!</div>');
//            }
//        });
//    }

//    // Load statistics
//    function loadWebsiteRatingStats() {
//        $.ajax({
//            url: '/api/Rating?handler=GetWebsiteStats',
//            type: 'GET',
//            success: function (response) {
//                updateRatingDisplay(response);
//                $('#userCount').text(response.totalRatings || 0);
//            },
//            error: function () {
//                updateRatingDisplay({ averageStars: 0, totalRatings: 0 });
//                $('#userCount').text(0);
//            }
//        });
//    }

//    // Display ratings: Profile Picture | Username: Stars
//    function displayRatings(ratings) {
//        const container = $('#ratingsList');
//        container.empty();

//        if (!ratings || ratings.length === 0) {
//            container.html('<div class="text-center text-muted py-3">No ratings yet. Be the first!</div>');
//            return;
//        }

//        ratings.forEach(rating => {
//            const userName = rating.userName || 'Anonymous';
//            const initial = userName.charAt(0).toUpperCase();
//            const hasImage = rating.profilePictureUrl && rating.profilePictureUrl !== null;

//            // Profile picture or default avatar
//            let avatarHtml = '';
//            if (hasImage) {
//                avatarHtml = `<img src="${rating.profilePictureUrl}" alt="${userName}" onerror="this.onerror=null;this.parentElement.innerHTML='<div class=\'default-avatar\'>${initial}</div>';">`;
//            } else {
//                avatarHtml = `<div class="default-avatar">${initial}</div>`;
//            }

//            // Generate stars
//            let starsHtml = '';
//            for (let i = 1; i <= 5; i++) {
//                if (i <= rating.stars) {
//                    starsHtml += '<span class="star">★</span>';
//                } else {
//                    starsHtml += '<span class="star empty">☆</span>';
//                }
//            }

//            const ratingHtml = `
//                    <div class="rating-item">
//                        <div class="rating-avatar">
//                            ${avatarHtml}
//                        </div>
//                        <div class="rating-info">
//                            <div class="rating-user">
//                                ${escapeHtml(userName)}:
//                            </div>
//                            <div class="rating-stars">
//                                ${starsHtml}
//                            </div>
//                            ${rating.comment ? `<div class="rating-comment">💬 ${escapeHtml(rating.comment)}</div>` : ''}
//                        </div>
//                    </div>
//                `;

//            container.append(ratingHtml);
//        });
//    }

//    // Save rating with comment
//    function saveWebsiteRating(stars) {
//        const token = $('input[name="__RequestVerificationToken"]').val();
//        const comment = $('#ratingComment').val();

//        const submitBtn = $('#submitRatingBtn');
//        submitBtn.prop('disabled', true).html('<i class="fa-regular fa-spinner fa-spin"></i> Saving...');

//        $.ajax({
//            url: '/api/Rating?handler=SaveWebsiteRating',
//            type: 'POST',
//            data: { stars: stars, comment: comment },
//            headers: { 'RequestVerificationToken': token },
//            success: function (response) {
//                if (response.success) {
//                    loadWebsiteRatings();
//                    loadWebsiteRatingStats();
//                    showMessage('Thank you for rating!', 'success');
//                    resetForm();
//                    submitBtn.prop('disabled', false).html('<i class="fa-regular fa-star"></i> Submit Rating');
//                } else {
//                    showMessage(response.message || 'Failed to save rating', 'error');
//                    submitBtn.prop('disabled', false).html('<i class="fa-regular fa-star"></i> Submit Rating');
//                }
//            },
//            error: function (xhr) {
//                let message = 'Error saving rating';
//                if (xhr.status === 401) message = 'Please login to rate';
//                showMessage(message, 'error');
//                submitBtn.prop('disabled', false).html('<i class="fa-regular fa-star"></i> Submit Rating');
//            }
//        });
//    }

//    function updateRatingDisplay(data) {
//        $('#modalAverageRating').text(data.averageStars.toFixed(1));
//        $('#modalRatingCount').text(`(${data.totalRatings} ${data.totalRatings === 1 ? 'rating' : 'ratings'})`);

//        const container = $('#modalStarsDisplay');
//        container.empty();

//        const fullStars = Math.floor(data.averageStars);
//        for (let i = 1; i <= fullStars; i++) {
//            container.append('<span class="star">★</span>');
//        }
//        for (let i = fullStars + 1; i <= 5; i++) {
//            container.append('<span class="star empty">☆</span>');
//        }
//    }

//    function showMessage(message, type) {
//        const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
//        $('#ratingMessage').html(`
//                <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
//                    ${message}
//                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
//                </div>
//            `);
//        setTimeout(() => {
//            $('#ratingMessage .alert').fadeOut(300, function () { $(this).remove(); });
//        }, 3000);
//    }

//    function resetForm() {
//        $('.star-rating-modal input').prop('checked', false);
//        $('#ratingComment').val('');
//        $('#commentField').hide();
//        selectedStars = null;
//    }

//    function escapeHtml(text) {
//        if (!text) return '';
//        const div = document.createElement('div');
//        div.textContent = text;
//        return div.innerHTML;
//    }

//    $('#ratingModal').on('hidden.bs.modal', function () {
//        $('#ratingMessage').empty();
//        resetForm();
//    });
//});