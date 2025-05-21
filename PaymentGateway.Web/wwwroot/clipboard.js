window.copyTextToClipboard = (text) => {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        return navigator.clipboard.writeText(text);
    } else {
        throw new Error("Clipboard API недоступен без HTTPS");
    }
};