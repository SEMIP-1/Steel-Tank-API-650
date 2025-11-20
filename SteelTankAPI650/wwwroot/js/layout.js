(function () {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const sidebar = document.getElementById("sidebar");

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener("click", () => {
            sidebar.classList.toggle("collapsed");
            document.body.classList.toggle("sidebar-collapsed");
        });
    }

    // ----- THEME (manual only, Option B) -----
    const THEME_KEY = "api650-theme";

    function applyTheme(theme) {
        document.body.classList.remove("theme-light", "theme-dark");
        const cls = theme === "dark" ? "theme-dark" : "theme-light";
        document.body.classList.add(cls);
        localStorage.setItem(THEME_KEY, theme);
    }

    const savedTheme = localStorage.getItem(THEME_KEY) || "light";
    applyTheme(savedTheme);

    document.querySelectorAll(".theme-option").forEach(btn => {
        btn.addEventListener("click", () => {
            const theme = btn.getAttribute("data-theme") || "light";
            applyTheme(theme);
        });
    });

    // ----- ACTIVE LINK HIGHLIGHT -----
    function updateActiveLinks(route) {
        document.querySelectorAll(".sidebar-link").forEach(a => {
            const r = a.getAttribute("data-route");
            a.classList.toggle("active", r === route);
        });

        document.querySelectorAll(".top-nav-link").forEach(a => {
            const r = a.getAttribute("data-route");
            a.classList.toggle("active", r === route);
        });
    }

    // Expose to router
    window.api650Layout = { updateActiveLinks };

    // ----- Login placeholder -----
    const loginLink = document.getElementById("loginLink");
    if (loginLink) {
        loginLink.addEventListener("click", () => {
            alert("Login feature will be implemented in a later phase.");
        });
    }
})();
