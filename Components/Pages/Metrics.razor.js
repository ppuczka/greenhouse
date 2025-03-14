function downloadFile(fileUrl, fileName) {
    let link = document.createElement('a');
    link.href = fileUrl;
    link.download = fileName;
    link.click();
}
