// Ensure Bootstrap is properly initialized for modals
window.initializeModal = (modalId) => {
    // Make sure bootstrap is loaded
    if (typeof bootstrap !== 'undefined') {
        // Get the modal element
        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            // Create a new modal instance
            const modal = new bootstrap.Modal(modalElement);
            return true;
        }
    }
    return false;
};

// Function to show a modal
window.showModal = (modalId) => {
    if (typeof bootstrap !== 'undefined') {
        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
            return true;
        }
    }
    return false;
};

// Function to hide a modal
window.hideModal = (modalId) => {
    if (typeof bootstrap !== 'undefined') {
        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
                return true;
            }
        }
    }
    return false;
};

// Hàm xử lý cho các nút CUD
window.categoryOperations = {
    // Hàm tạo mới category
    createCategory: function(dotnetRef) {
        console.log("JavaScript: Creating category...");
        try {
            dotnetRef.invokeMethodAsync('CreateCategoryFromJS')
                .then(result => {
                    console.log("JavaScript: Category created result:", result);
                })
                .catch(error => {
                    console.error("JavaScript: Error creating category:", error);
                });
        } catch (ex) {
            console.error("JavaScript: Exception in createCategory:", ex);
        }
        return true;
    },
    
    // Hàm cập nhật category
    updateCategory: function(dotnetRef) {
        console.log("JavaScript: Updating category...");
        try {
            dotnetRef.invokeMethodAsync('UpdateCategoryFromJS')
                .then(result => {
                    console.log("JavaScript: Category updated result:", result);
                })
                .catch(error => {
                    console.error("JavaScript: Error updating category:", error);
                });
        } catch (ex) {
            console.error("JavaScript: Exception in updateCategory:", ex);
        }
        return true;
    },
    
    // Hàm xóa category
    deleteCategory: function(dotnetRef, categoryId) {
        console.log("JavaScript: Deleting category:", categoryId);
        try {
            dotnetRef.invokeMethodAsync('DeleteCategoryFromJS', categoryId)
                .then(result => {
                    console.log("JavaScript: Category deleted result:", result);
                })
                .catch(error => {
                    console.error("JavaScript: Error deleting category:", error);
                });
        } catch (ex) {
            console.error("JavaScript: Exception in deleteCategory:", ex);
        }
        return true;
    }
};

// Hàm để kiểm tra URL hình ảnh CORS
window.imageUtils = {
    // Chuyển đổi URL hình ảnh nếu cần thiết
    convertImageUrl: function(url) {
        if (!url || url.trim() === '') {
            return { url: "", needsProxy: false };
        }

        // Kiểm tra URL từ Facebook hoặc các dịch vụ có vấn đề với CORS
        if (url.includes('facebook.com') || 
            url.includes('fbcdn.net') || 
            url.includes('fbsbx.com')) {
            // Chuyển đổi URL thành proxy URL
            const proxyUrl = `/api/proxy/image?url=${encodeURIComponent(url)}`;
            return { url: proxyUrl, needsProxy: true };
        }

        return { url: url, needsProxy: false };
    },

    // Kiểm tra URL hình ảnh có hợp lệ không
    validateImageUrl: function(url) {
        return new Promise((resolve, reject) => {
            if (!url || url.trim() === '') {
                resolve({ isValid: true, message: "", convertedUrl: "" }); // Empty URL is considered valid
                return;
            }

            // Kiểm tra và chuyển đổi URL nếu cần
            const { url: convertedUrl, needsProxy } = this.convertImageUrl(url);
            
            // Nếu URL cần proxy, trả về URL đã chuyển đổi
            if (needsProxy) {
                resolve({ 
                    isValid: true, 
                    message: "This image will be loaded through our proxy server to avoid CORS issues.", 
                    convertedUrl: convertedUrl 
                });
                return;
            }

            // Tạo đối tượng Image để kiểm tra URL
            const img = new Image();
            let timeout = null;

            // Thiết lập timeout để xử lý trường hợp tải hình ảnh quá lâu
            timeout = setTimeout(() => {
                img.src = ''; // Dừng tải hình ảnh
                resolve({ 
                    isValid: false, 
                    message: "Image URL verification timed out. The URL might be invalid or the server might be slow to respond.", 
                    convertedUrl: ""
                });
            }, 5000); // 5 giây timeout

            img.onload = function() {
                clearTimeout(timeout);
                resolve({ isValid: true, message: "", convertedUrl: "" });
            };

            img.onerror = function() {
                clearTimeout(timeout);
                resolve({ 
                    isValid: false, 
                    message: "Unable to load image from this URL. The URL might be invalid or the image might be protected by CORS policies.", 
                    convertedUrl: ""
                });
            };

            img.src = url;
        });
    }
}; 