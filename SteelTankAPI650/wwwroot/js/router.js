const routes = {
    shell: "/pages/shell.html",
    materials: "/pages/materials.html",
    settings: "/pages/settings-placeholder.html"
};

async function loadPage() {
    // hash like "#/shell" → "shell"
    const hash = location.hash.replace("#/", "") || "shell";
    const page = routes[hash];

    // Unknown route
    if (!page) {
        document.getElementById("app-content").innerHTML =
            `<div class="alert alert-danger">Page not found.</div>`;
        return;
    }

    // Load HTML fragment
    let res = await fetch(page);
    if (!res.ok) {
        document.getElementById("app-content").innerHTML =
            `<div class="alert alert-danger">Page not found.</div>`;
        return;
    }

    let html = await res.text();
    document.getElementById("app-content").innerHTML = html;

    // Remove previous page script if any
    const prevScript = document.getElementById("page-script");
    if (prevScript) prevScript.remove();

    // Load page-specific JS if exists
    const scriptPath = `/js/${hash}.js`;
    fetch(scriptPath).then(r => {
        if (r.ok) {
            const s = document.createElement("script");
            s.src = scriptPath;
            s.id = "page-script";
            document.body.appendChild(s);
        }
    });
}

window.addEventListener("hashchange", loadPage);
loadPage();   // <—— FIXED
