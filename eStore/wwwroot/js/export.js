window.downloadFile = (fileName, contentType, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(link.href);
};
window.downloadFileWithDialog = async (filename, content, mimeType) => {
    const blob = new Blob([new Uint8Array(content)], { type: mimeType });

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.generatePdfFromTable = async (tableId, chartCanvasId, filename) => {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();

    const table = document.getElementById(tableId);
    if (!table) return;

    // Export table first using jsPDF autotable
    await doc.autoTable({ html: `#${tableId}` });

    // Add space after table
    let finalY = doc.lastAutoTable.finalY || 20;

    // Export chart image
    const canvas = document.getElementById(chartCanvasId);
    if (canvas) {
        const imgData = canvas.toDataURL('image/png');

        // Add chart title
        doc.setFontSize(14);
        doc.text("Sales Chart", 14, finalY + 15);

        // Add chart image below the title
        doc.addImage(imgData, 'PNG', 14, finalY + 20, 180, 90);
    }

    doc.save(filename);
};
window.saveChartImageToServer = async (canvasId, endpointUrl, filename) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const base64Image = canvas.toDataURL("image/png").split(',')[1]; // remove prefix

    const payload = {
        fileName: filename,
        base64: base64Image
    };

    await fetch(endpointUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    });
};

window.saveChartImageToLocal = (canvasId, filename) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.warn("Canvas not found!");
        return;
    }

    const link = document.createElement('a');
    link.href = canvas.toDataURL('image/png');
    link.download = filename;
    link.click();
};

window.getChartImageBase64 = (canvasId) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.warn("❌ Canvas not found for ID:", canvasId);
        return null;
    }

    try {
        const base64 = canvas.toDataURL('image/png').split(',')[1];
        console.log(" base64 chart image generated:", base64.substring(0, 30), "...");
        return base64;
    } catch (e) {
        console.error(" Error generating chart base64:", e);
        return null;
    }
};

window.pickFileAndUploadToFirebase = async (dotNetHelper) => {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/*";

    input.onchange = async (event) => {
        const file = event.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = () => {
            const base64 = reader.result.split(',')[1]; // remove "data:image/png;base64,"
            dotNetHelper.invokeMethodAsync("OnChartFilePicked", base64, file.name);
        };
        reader.readAsDataURL(file);
    };

    input.click();
};

window.saveChartImageToLocal = function (canvasId, fileName = "chart.png") {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    canvas.toBlob(function (blob) {
        const link = document.createElement("a");
        link.download = fileName;
        link.href = URL.createObjectURL(blob);
        link.click();
        URL.revokeObjectURL(link.href);
    }, "image/png");
};


