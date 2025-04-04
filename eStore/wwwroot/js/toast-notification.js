/**
 * Toast notification system
 * Uses Bootstrap 5 Toast component
 */

/**
 * Show a toast notification
 * @param {string} message - The message to display
 * @param {string} title - The title of the toast (optional)
 * @param {string} type - The type of toast: 'success', 'error', 'warning', 'info' (default: 'info')
 * @param {number} delay - The delay in milliseconds before the toast disappears (default: 5000)
 */
window.showToast = function(message, title = '', type = 'info', delay = 5000) {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '1090';
        document.body.appendChild(toastContainer);
    }
    
    // Get icon and background color based on type
    let icon, bgClass;
    switch (type.toLowerCase()) {
        case 'success':
            icon = '<i class="bi bi-check-circle-fill me-2"></i>';
            bgClass = 'bg-success';
            break;
        case 'error':
            icon = '<i class="bi bi-x-circle-fill me-2"></i>';
            bgClass = 'bg-danger';
            break;
        case 'warning':
            icon = '<i class="bi bi-exclamation-triangle-fill me-2"></i>';
            bgClass = 'bg-warning';
            break;
        case 'info':
        default:
            icon = '<i class="bi bi-info-circle-fill me-2"></i>';
            bgClass = 'bg-info';
            break;
    }
    
    // Create a random ID for the toast
    const toastId = 'toast-' + Math.random().toString(36).substring(2, 9);
    
    // Create toast HTML
    const toastHTML = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="${delay}">
            <div class="toast-header ${bgClass} text-white">
                ${icon}
                <strong class="me-auto">${title || (type.charAt(0).toUpperCase() + type.slice(1))}</strong>
                <small>Just now</small>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    // Add toast to container
    toastContainer.innerHTML += toastHTML;
    
    // Initialize and show the toast
    const toastElement = document.getElementById(toastId);
    const bsToast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: delay
    });
    
    bsToast.show();
    
    // Remove toast from DOM after it's hidden
    toastElement.addEventListener('hidden.bs.toast', function() {
        toastElement.remove();
    });
    
    return toastId; // Return the ID in case we want to manipulate the toast later
};

/**
 * Success toast shorthand function
 * @param {string} message - The message to display
 * @param {string} title - Optional title
 */
window.showSuccessToast = function(message, title = 'Success') {
    return window.showToast(message, title, 'success', 5000);
};

/**
 * Error toast shorthand function
 * @param {string} message - The message to display
 * @param {string} title - Optional title
 */
window.showErrorToast = function(message, title = 'Error') {
    return window.showToast(message, title, 'error', 8000);
};

/**
 * Warning toast shorthand function
 * @param {string} message - The message to display
 * @param {string} title - Optional title
 */
window.showWarningToast = function(message, title = 'Warning') {
    return window.showToast(message, title, 'warning', 6000);
};

/**
 * Info toast shorthand function
 * @param {string} message - The message to display
 * @param {string} title - Optional title
 */
window.showInfoToast = function(message, title = 'Info') {
    return window.showToast(message, title, 'info', 4000);
}; 