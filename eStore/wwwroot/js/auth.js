// Authentication related functions

// Global reference to the NavMenu component
let navMenuRef = null;

// Register NavMenu reference for callbacks
window.registerNavMenu = (dotnetRef) => {
    navMenuRef = dotnetRef;
    console.log("NavMenu registered successfully");
    return true;
};

// Force a complete page reload after logout
function forceLogout() {
    // Clear any local storage or session data
    localStorage.clear();
    sessionStorage.clear();
    
    // Force a hard navigation to the logout URL
    window.location.replace("/api/auth/logout");
    
    return true;
}

// Function to refresh auth state in NavMenu
window.refreshAuthState = async () => {
    if (navMenuRef) {
        try {
            await navMenuRef.invokeMethodAsync('RefreshAuthState');
            console.log("Auth state refreshed");
            return true;
        } catch (error) {
            console.error("Error refreshing auth state:", error);
            return false;
        }
    }
    return false;
};

// Listen for page visibility changes to refresh auth state
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
        window.refreshAuthState();
    }
});

// Call refreshAuthState when page loads or when navigating back
window.addEventListener('pageshow', (event) => {
    // Also refresh when navigating back (bfcache)
    if (event.persisted) {
        window.refreshAuthState();
    }
});

// Listen for storage events (for logout in other tabs)
window.addEventListener('storage', (event) => {
    if (event.key === null) {
        // localStorage was cleared
        window.refreshAuthState();
    }
}); 