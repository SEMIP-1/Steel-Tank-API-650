const platesApi = "/api/platesizes";
let plateModal;

function initPlatesPage() {
    const modalEl = document.getElementById("plateModal");
    if (!modalEl) return;
    plateModal = new bootstrap.Modal(modalEl);
    loadPlates();
}

function loadPlates() {
    fetch(platesApi)
        .then(r => {
            if (!r.ok) throw new Error("HTTP " + r.status);
            return r.json();
        })
        .then(data => {
            const tbody = document.getElementById("plates-table");
            if (!tbody) return;
            let rows = "";
            data.forEach(p => {
                const thk = p.thicknessMM ?? p.ThicknessMM ?? p.thickness ?? "";
                rows += `
                    <tr>
                        <td>${thk}</td>
                        <td>
                            <button class="btn btn-sm btn-warning me-1"
                                    type="button"
                                    onclick="editPlate(${thk})">
                                Edit
                            </button>
                            <button class="btn btn-sm btn-danger"
                                    type="button"
                                    onclick="deletePlate(${thk})">
                                Delete
                            </button>
                        </td>
                    </tr>`;
            });
            tbody.innerHTML = rows;
        })
        .catch(err => {
            alert("Failed to load plate sizes: " + err.message);
            console.error(err);
        });
}

function openPlateModal() {
    if (!plateModal) return;
    document.getElementById("plateModalTitle").innerText = "Add Plate Size";
    document.getElementById("plate-original-thk").value = "";
    document.getElementById("plate-thk").value = "";
    plateModal.show();
}

function editPlate(thk) {
    document.getElementById("plateModalTitle").innerText = "Edit Plate Size";
    document.getElementById("plate-original-thk").value = thk;
    document.getElementById("plate-thk").value = thk;
    plateModal.show();
}

function savePlate() {
    if (!plateModal) return;

    const original = document.getElementById("plate-original-thk").value;
    const thk = parseFloat(document.getElementById("plate-thk").value);

    if (!thk) {
        alert("Thickness is required.");
        return;
    }

    const payload = { thicknessMM: thk };

    const method = original ? "PUT" : "POST";
    const url    = original
        ? `${platesApi}/${encodeURIComponent(original)}`
        : platesApi;

    fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t || ("HTTP " + r.status)); });
            plateModal.hide();
            loadPlates();
        })
        .catch(err => {
            alert("Error saving plate: " + err.message);
            console.error(err);
        });
}

function deletePlate(thk) {
    if (!confirm(`Delete plate size ${thk} mm ?`)) return;

    fetch(`${platesApi}/${encodeURIComponent(thk)}`, { method: "DELETE" })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t || ("HTTP " + r.status)); });
            loadPlates();
        })
        .catch(err => {
            alert("Error deleting plate: " + err.message);
            console.error(err);
        });
}

(function () {
    if (document.getElementById("plates-page")) {
        initPlatesPage();
    }
})();
