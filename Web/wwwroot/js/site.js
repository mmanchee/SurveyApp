// Function to update progress bar (call this from specific page models or views)
function updateProgressBar(currentPage, totalPages) {
    const container = document.getElementById('survey-progress-container');
    const progressBar = document.getElementById('progress-bar');
    const progressText = document.getElementById('progress-text');

    if (!container || !progressBar || !progressText) {
        console.warn('Progress bar elements not found.');
        return;
    }

    if (totalPages > 0 && currentPage > 0) {
        const percentage = Math.max(0, Math.min(100, (currentPage / totalPages) * 100));
        progressBar.style.width = percentage + '%';
        progressBar.setAttribute('aria-valuenow', percentage);
        progressText.textContent = `Page ${currentPage} of ${totalPages}`;
        container.style.display = 'block'; // Show the progress bar
    } else {
        container.style.display = 'none'; // Hide if invalid values
    }
}


// Function to toggle 'Other' text boxes based on selection
function setupOtherFieldToggle(controlId, otherValue, otherTextboxId, required = false) {
    const control = document.getElementById(controlId); // Could be select, radio group container, etc.
    const otherTextbox = document.getElementById(otherTextboxId);

    if (!control || !otherTextbox) {
        console.warn(`Toggle setup failed: Control '${controlId}' or Textbox '${otherTextboxId}' not found.`);
        return;
    }

    const toggleVisibility = () => {
        let isOtherSelected = false;

        // Handle <select> dropdown
        if (control.tagName === 'SELECT') {
            isOtherSelected = control.value === otherValue;
        }
        // Handle radio buttons (assuming they share a name and are inside the container 'controlId')
        else {
            const checkedRadio = control.querySelector(`input[type="radio"]:checked`);
            if (checkedRadio) {
                isOtherSelected = checkedRadio.value === otherValue;
            }
        }
        // Add logic for checkboxes if needed (check if 'Other' checkbox is checked)


        if (isOtherSelected) {
            otherTextbox.style.display = 'block';
            otherTextbox.querySelector('input, textarea')?.removeAttribute('disabled');
            if (required) {
                otherTextbox.querySelector('input, textarea')?.setAttribute('required', 'required');
            }

        } else {
            otherTextbox.style.display = 'none';
            otherTextbox.querySelector('input, textarea')?.setAttribute('disabled', 'disabled'); // Disable to prevent submission
            otherTextbox.querySelector('input, textarea')?.removeAttribute('required');
            // Optionally clear the value when hidden:
            // otherTextbox.querySelector('input, textarea').value = '';
        }
    };

    // Attach event listener
    // For <select>, listen to 'change'
    if (control.tagName === 'SELECT') {
        control.addEventListener('change', toggleVisibility);
    }
    // For radio buttons container, listen to 'change' event bubbling up
    else {
        control.addEventListener('change', (event) => {
            if (event.target.type === 'radio') {
                toggleVisibility();
            }
        });
    }

    // Initial check on page load
    toggleVisibility();
}


// Example setup call (needs to be called after DOM is ready, e.g., in specific page scripts or DOMContentLoaded)
// document.addEventListener('DOMContentLoaded', function() {
//     // Example for Favorite Color Radio Buttons (assuming radios are inside a div with id="favColorGroup")
//     setupOtherFieldToggle('favColorGroup', 'Other', 'favColorOtherTextbox');
//
//     // Example for Favorite Season Dropdown
//     setupOtherFieldToggle('SurveyResponse_FavoriteSeason', 'Other', 'favSeasonOtherTextbox');
// });

// --- Add more site-wide JS if needed ---

// Console log helper for debugging in browser
console.log("site.js loaded");