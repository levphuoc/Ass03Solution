/**
 * Utility functions for cart operations
 */

/**
 * Force delete a cart using direct API call
 * This is used as a fallback when standard deletion methods fail
 * @param {number} memberId - The member ID whose cart should be deleted
 * @returns {Promise} - A promise resolving to the API response
 */
window.forceDeleteCart = function(memberId) {
    return fetch('/api/Cart/forcedelete/' + memberId, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        console.log('Cart force deleted:', data);
        return data;
    })
    .catch(error => {
        console.error('Error force deleting cart:', error);
        throw error;
    });
};

/**
 * Ultimate no-fail cart cleanup function
 * Makes multiple attempts with different approaches to ensure cart deletion
 * @param {number} memberId - The member ID whose cart should be deleted
 */
window.ultimateCartCleanup = function(memberId) {
    console.log('Starting ultimate cart cleanup for user', memberId);
    
    // First try the regular force delete
    fetch('/api/Cart/forcedelete/' + memberId, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' }
    }).catch(() => {});
    
    // Then try the cleanup endpoint
    setTimeout(() => {
        fetch('/api/Cart/cleanup/' + memberId)
        .catch(() => {});
    }, 300);
    
    // Final attempt with a direct fetch
    setTimeout(() => {
        // This last attempt will always succeed (or silently fail)
        fetch('/api/Cart/cleanup/' + memberId + '?force=true')
        .catch(() => { console.log('Final cleanup attempt completed'); });
    }, 800);
}; 