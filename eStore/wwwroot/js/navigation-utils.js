/**
 * Utility functions for page navigation
 */

/**
 * Navigate to a URL with a full page reload
 * This is more reliable than standard navigation when dealing with Blazor components
 * @param {string} url - The URL to navigate to
 * @param {boolean} forceReload - Whether to force a hard reload (defaults to true)
 */
window.navigateWithReload = function(url, forceReload = true) {
    console.log(`Navigating to ${url} with forceReload=${forceReload}`);
    
    if (forceReload) {
        // This will cause a complete page reload, clearing all state
        window.location.replace(url);
    } else {
        // This will update the URL but may retain some state
        window.location.href = url;
    }
};

/**
 * Navigate to a URL after a delay
 * Useful when you need to show a message before navigating
 * @param {string} url - The URL to navigate to
 * @param {number} delayMs - The delay in milliseconds before navigation
 * @param {boolean} forceReload - Whether to force a hard reload
 */
window.navigateWithDelayAndReload = function(url, delayMs = 2000, forceReload = true) {
    console.log(`Will navigate to ${url} after ${delayMs}ms with forceReload=${forceReload}`);
    
    setTimeout(() => {
        window.navigateWithReload(url, forceReload);
    }, delayMs);
    
    return true; // Return true so calling code knows the navigation has been scheduled
};

/**
 * Emergency navigation - handles navigation in case of errors
 * Tries multiple approaches to ensure navigation works
 * @param {string} url - The URL to navigate to
 */
window.emergencyNavigate = function(url) {
    console.log(`Emergency navigation to ${url}`);
    
    try {
        // Try the standard approach first
        window.location.replace(url);
    } catch (e) {
        console.error('First navigation attempt failed:', e);
        
        // If that fails, try a different approach
        setTimeout(() => {
            try {
                window.location.href = url;
            } catch (e2) {
                console.error('Second navigation attempt failed:', e2);
                
                // Last resort: reload the page
                window.location.reload();
            }
        }, 500);
    }
}; 