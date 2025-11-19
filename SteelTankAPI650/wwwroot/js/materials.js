const materialsApi = "/api/Materials";
let materialModal = null;

document.addEventListener("DOMContentLoaded", function () {
    const modalEl = document.getElementById("materialModal");
    if (modalEl) {
        materialModal = new bootstrap.Modal(modalEl);
        loadMaterials();
    }
});

// ---------------------- LOAD ALL MATERIALS ----------------------
function loadMaterials() {
    fetch(materialsApi)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById("materials-table");
            if (!tbody) return;

            tbody.innerHTML = data.map(m => `
                <tr>
                    <td>${m.Grade}</td>
                    <td>${m.Sd_MPa}</td>
                    <td>${m.StMultiplier}</td>
                    <td>${m.Density}</td>
                    <td>${m.Note ?? ""}</td>
                    <td>
                        <button class="btn btn-sm btn-warning" onclick="editMaterial('${m.Grade}')">Edit</button>
                        <button class="btn btn-sm btn-danger" onclick="deleteMaterial('${m.Grade}')">Delete</button>
                    </td>
                </tr>
            `).join("");
        });
}

// ---------------------- ADD MATERIAL ----------------------
function openMaterialModal() {
    document.getElementById("materialModalTitle").innerText = "Add Material";
    document.getElementById("mat-original-grade").value = "";
    clearMaterialForm();
    materialModal.show();
}

function clearMaterialForm() {
    ["mat-grade", "mat-sd", "mat-st", "mat-density", "mat-note"]
        .forEach(id => document.getElementById(id).value = "");
}

// ---------------------- EDIT MATERIAL ----------------------
function editMaterial(grade) {
    fetch(`${materialsApi}/${encodeURIComponent(grade)}`)
        .then(r => r.json())
        .then(m => {
            document.getElementById("materialModalTitle").innerText = "Edit Material";
            document.getElementById("mat-original-grade").value = m.Grade;

            document.getElementById("mat-grade").value = m.Grade;
            document.getElementById("mat-sd").value = m.Sd_MPa;
            document.getElementById("mat-st").value = m.StMultiplier;
            document.getElementById("mat-density").value = m.Density;
            document.getElementById("mat-note").value = m.Note ?? "";

            materialModal.show();
        });
}

// ---------------------- SAVE MATERIAL (ADD OR EDIT) ----------------------
function saveMaterial() {
    const originalGrade = document.getElementById("mat-original-grade").value;

    const mat = {
        Grade: document.getElementById("mat-grade").value,
        Sd_MPa: parseFloat(document.getElementById("mat-sd").value),
        StMultiplier: parseFloat(document.getElementById("mat-st").value),
        Density: parseFloat(document.getElementById("mat-density").value),
        Note: document.getElementById("mat-note").value
    };

    const method = originalGrade ? "PUT" : "POST";
    const url = originalGrade
        ? `${materialsApi}/${encodeURIComponent(originalGrade)}`
        : materialsApi;

    fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(mat)
    })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t); });
            materialModal.hide();
            loadMaterials();
        })
        .catch(err => alert("Error: " + err.message));
}

// ---------------------- DELETE MATERIAL ----------------------
function deleteMaterial(grade) {
    if (!confirm(`Delete material '${grade}' ?`)) return;

    fetch(`${materialsApi}/${encodeURIComponent(grade)}`, { method: "DELETE" })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t); });
            loadMaterials();
        })
        .catch(err => alert("Error: " + err.message));
}
