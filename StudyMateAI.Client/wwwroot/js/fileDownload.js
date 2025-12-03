/**
 * Helpers para descargar archivos desde Blazor
 */

/**
 * Descarga un archivo desde un arreglo de bytes
 * @param {string} fileName - Nombre del archivo a descargar
 * @param {Uint8Array} fileContent - Contenido del archivo en bytes
 */
function downloadFileFromBytes(fileName, fileContent) {
    const blob = new Blob([fileContent], { type: 'application/octet-stream' });
    downloadFile(fileName, blob);
}

/**
 * Descarga un archivo Word (.docx) desde bytes
 * @param {string} fileName - Nombre del archivo a descargar
 * @param {Uint8Array} fileContent - Contenido del archivo en bytes
 */
function downloadDocxFile(fileName, fileContent) {
    const blob = new Blob([fileContent], { 
        type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' 
    });
    downloadFile(fileName, blob);
}

/**
 * Descarga un archivo PDF desde bytes
 * @param {string} fileName - Nombre del archivo a descargar
 * @param {Uint8Array} fileContent - Contenido del archivo en bytes
 */
function downloadPdfFile(fileName, fileContent) {
    const blob = new Blob([fileContent], { type: 'application/pdf' });
    downloadFile(fileName, blob);
}

/**
 * Función auxiliar privada para manejar la descarga del archivo
 * @param {string} fileName - Nombre del archivo
 * @param {Blob} blob - Blob del archivo
 */
function downloadFile(fileName, blob) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName || 'descarga';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

/**
 * Abre un archivo en una nueva pestaña (útil para visualizar en lugar de descargar)
 * @param {Uint8Array} fileContent - Contenido del archivo en bytes
 * @param {string} mimeType - Tipo MIME del archivo
 */
function openFileInNewTab(fileContent, mimeType) {
    const blob = new Blob([fileContent], { type: mimeType });
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
    // Nota: URL.revokeObjectURL(url) se debería llamar después de que se cargue el archivo
    // pero el navegador lo maneja automáticamente con '_blank'
}
