window.findGallery = {
    scrollToChild: function (container, index) {
        if (!container) return;
        const child = container.children[index];
        if (!child) return;
        child.scrollIntoView({ behavior: "smooth", inline: "center", block: "nearest" });
    }
};
