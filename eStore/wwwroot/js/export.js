window.downloadFile = (fileName, contentType, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(link.href);
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
        const imgData = canvas.toDataURL('images/png');

        // Add chart title
        doc.setFontSize(14);
        doc.text("Sales Chart", 14, finalY + 15);

        // Add chart image below the title
        doc.addImage(imgData, 'PNG', 14, finalY + 20, 180, 90);
    }

    doc.save(filename);
};

