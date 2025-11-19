document.addEventListener("DOMContentLoaded", function () {
    const layout = document.getElementById("layout");
    const toggleBtn = document.getElementById("sidebarToggle");

    if (layout && toggleBtn) {
        toggleBtn.addEventListener("click", function () {
            layout.classList.toggle("sidebar-collapsed");
        });
    }
});
