// ============================================================================
// MATERIALS PAGE SCRIPT
// ============================================================================
const MATERIAL_STATE_KEY = "Api650_Materials";

let materialsState = []; 
let materialModal;

// SPA entry point
window.initMaterialsPage = async function () {

    // Load modal
    const modalEl = document.getElementById("materialModal");
    if (modalEl) materialModal = new bootstrap.Modal(modalEl);

    // Buttons
    document.getElementById("btn-add-material").onclick = openAddMaterial;
    document.getElementById("btn-save-material").onclick = saveMaterial;

    // Load materials from API into table + state
    await loadMaterials();
};

// ============================================================================
// LOAD MATERIALS FROM API
// ============================================================================
async function loadMaterials() {
    try {
        const res = await fetch("/api/materials");
        const data = await res.json();

        materialsState = data;
        localStorage.setItem(MATERIAL_STATE_KEY, JSON.stringify(materialsState));

        // Update shell dropdowns later
        if (window.Api650State) {
            Api650State.setMaterials(materialsState);
        }

        renderMaterialsTable();

    } catch (err) {
        alert("Failed to load materials: " + err.message);
    }
}

// ============================================================================
// RENDER TABLE
// ============================================================================
function renderMaterialsTable() {
    const tbody = document.getElementById("materials-table");
    tbody.innerHTML = "";

    materialsState.forEach(m => {
        const grade = m.grade ?? m.Grade;

        tbody.innerHTML += `
            <tr>
                <td>${grade}</td>
                <td>${m.sd_MPa ?? m.Sd_MPa}</td>
                <td>${m.stMultiplier ?? m.StMultiplier}</td>
                <td>${m.density ?? m.Density}</td>
                <td>${m.note ?? m.Note ?? ""}</td>

                <td>
                    <button class="btn btn-warning btn-sm me-2" 
                            onclick="editMaterial('${grade}')">Edit</button>

                    <button class="btn btn-danger btn-sm"
                            onclick="deleteMaterial('${grade}')">Delete</button>
                </td>
            </tr>
        `;
    });
}

// ============================================================================
// OPEN ADD MODAL
// ============================================================================
function openAddMaterial() {
    document.getElementById("materialModalTitle").innerText = "Add Material";
    document.getElementById("mat-original-grade").value = "";

    clearMaterialForm();
    materialModal.show();
}

// ============================================================================
// CLEAR FORM
// ============================================================================
function clearMaterialForm() {
    document.getElementById("mat-grade").value = "";
    document.getElementById("mat-sd").value = "";
    document.getElementById("mat-st").value = "";
    document.getElementById("mat-density").value = "";
    document.getElementById("mat-note").value = "";
}

// ============================================================================
// EDIT MATERIAL
// ============================================================================
function editMaterial(grade) {
    const m = materialsState.find(x => (x.grade ?? x.Grade) === grade);
    if (!m) return;

    document.getElementById("materialModalTitle").innerText = "Edit Material";

    document.getElementById("mat-original-grade").value = grade;
    document.getElementById("mat-grade").value = m.grade ?? m.Grade;
    document.getElementById("mat-sd").value = m.sd_MPa ?? m.Sd_MPa;
    document.getElementById("mat-st").value = m.stMultiplier ?? m.StMultiplier;
    document.getElementById("mat-density").value = m.density ?? m.Density;
    document.getElementById("mat-note").value = m.note ?? m.Note ?? "";

    materialModal.show();
}

// ============================================================================
// SAVE MATERIAL (ADD or UPDATE)
// ============================================================================
function saveMaterial() {
    const original = document.getElementById("mat-original-grade").value;

    const mat = {
        grade: document.getElementById("mat-grade").value,
        sd_MPa: parseFloat(document.getElementById("mat-sd").value),
        stMultiplier: parseFloat(document.getElementById("mat-st").value),
        density: parseFloat(document.getElementById("mat-density").value),
        note: document.getElementById("mat-note").value
    };

    const url = original ? `/api/materials/${original}` : "/api/materials";
    const method = original ? "PUT" : "POST";

    fetch(url, {
        method: method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(mat)
    })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t) });

            return loadMaterials(); // reload table
        })
        .then(() => {
            materialModal.hide();
        })
        .catch(err => alert("Error: " + err.message));
}

// ============================================================================
// DELETE MATERIAL
// ============================================================================
function deleteMaterial(grade) {
    if (!confirm(`Delete material '${grade}'?`)) return;

    fetch(`/api/materials/${grade}`, { method: "DELETE" })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t) });

            return loadMaterials();
        })
        .catch(err => alert("Error: " + err.message));
}
