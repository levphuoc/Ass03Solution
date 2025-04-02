// Authentication related functions

// Force a complete page reload after logout
function forceLogout() {
    // Clear any local storage or session data
    localStorage.clear();
    sessionStorage.clear();
    
    // Force a hard navigation to the logout URL
    window.location.replace("/api/auth/logout");
    
    return true;
} 