// wwwroot/js/router.js

const routes = {
    home: {
        html: "html/home.html",
        script: "js/home.js",
        init: "initHomePage"
    },
    shell: {
        html: "html/shell.html",
        script: "js/shell.js",
        init: "initShellPage"
    },
    bottom: {
        html: "html/bottom.html",
        script: "js/bottom.js",
        init: "initBottomPage"
    },
    materials: {
        html: "html/materials.html",
        script: "js/materials.js",
        init: "initMaterialsPage"
    }
};

async function loadRoute() {
    const hash = location.hash.replace("#/", "") || "home";
    const route = routes[hash];
    const container = document.getElementById("app-content");

    if (!route) {
        container.innerHTML = `<div class="alert alert-danger">Page not found.</div>`;
        return;
    }

    try {
        const res = await fetch(route.html);
        const html = await res.text();
        container.innerHTML = html;
    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger">Failed to load page.</div>`;
        return;
    }

    await ensureScriptLoaded(route.script);

    if (route.init && typeof window[route.init] === "function") {
        window[route.init]();
    }
}

function ensureScriptLoaded(src) {
    return new Promise((resolve, reject) => {
        const existing = Array.from(document.scripts).find(s => s.src.includes(src));
        if (existing) return resolve();

        const s = document.createElement("script");
        s.src = src;
        s.onload = resolve;
        s.onerror = reject;
        document.body.appendChild(s);
    });
}

window.addEventListener("hashchange", loadRoute);
window.addEventListener("load", loadRoute);
