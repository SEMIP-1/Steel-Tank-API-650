// =======================
// SHELL DESIGN PAGE JS
// =======================

const materialsApi = "/api/materials";
let materialsList = [];

// When shell page loads
document.addEventListener("DOMContentLoaded", () => {
    if (!document.getElementById("btn-add-course")) return; // not on this page

    loadMaterials();
    setupShellEvents();
});

// Load materials for dropdowns
function loadMaterials() {
    fetch(materialsApi)
        .then(r => r.json())
        .then(data => {
            materialsList = data;
        });
}

function setupShellEvents() {
    document.getElementById("btn-add-course").addEventListener("click", addCourseRow);
    document.getElementById("btn-shell-calc").addEventListener("click", calculateShell);
}

let courseCounter = 0;

function addCourseRow() {
    courseCounter++;

    const tbody = document.getElementById("course-body");

    const row = document.createElement("tr");
    row.innerHTML = `
        <td>${courseCounter}</td>

        <td>
            <input class="form-control course-height" type="number" step="0.01">
        </td>

        <td>
            <select class="form-control course-material">
                <option value="">-- Select Material --</option>
                ${materialsList.map(m => `<option value="${m.grade || m.Grade}">${m.grade || m.Grade}</option>`).join("")}
            </select>
        </td>

        <td>
            <button class="btn btn-sm btn-danger" onclick="removeCourseRow(this)">X</button>
        </td>
    `;

    tbody.appendChild(row);
}

function removeCourseRow(btn) {
    btn.closest("tr").remove();
}

// ================================
// CALL SHELL DESIGN API
// ================================
function calculateShell() {
    const diameter = parseFloat(document.getElementById("shell-diameter").value);
    const height = parseFloat(document.getElementById("shell-height").value);
    const sg = parseFloat(document.getElementById("shell-sg").value);
    const ca = parseFloat(document.getElementById("shell-ca").value);

    const courseRows = document.querySelectorAll("#course-body tr");
    const courses = [];

    let courseNumber = 1;
    courseRows.forEach(row => {
        const h = parseFloat(row.querySelector(".course-height").value);
        const mat = row.querySelector(".course-material").value;

        if (!h || !mat) return;

        courses.push({
            courseNumber: courseNumber++,
            height: h,
            materialGrade: mat
        });
    });

    const payload = {
        diameter: diameter,
        liquidLevel: height,
        specificGravity: sg,
        testSpecificGravity: sg,
        corrosionAllowance: ca,
        jointEfficiency: 0.85,
        testStressMultiplier: 0.6,
        shellCourses: courses
    };

    console.log("Sending shell design payload:", payload);

    fetch("/api/shell/calculate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    })
        .then(r => r.json())
        .then(result => displayResult(result))
        .catch(err => alert("Error: " + err));
}

// ================================
// DISPLAY RESULTS
// ================================
function displayResult(data) {
    let html = `
        <h4>Shell Design Results</h4>
        <table class="table table-bordered">
            <thead>
                <tr>
                    <th>Course</th>
                    <th>Material</th>
                    <th>Design t (mm)</th>
                    <th>Test t (mm)</th>
                    <th>Required (mm)</th>
                    <th>Method</th>
                </tr>
            </thead>
            <tbody>
    `;

    data.courses.forEach(c => {
        html += `
            <tr>
                <td>${c.courseNumber}</td>
                <td>${c.material}</td>
                <td>${c.td_Variable?.toFixed(2) ?? "-"}</td>
                <td>${c.tt_Variable?.toFixed(2) ?? "-"}</td>
                <td>${c.requiredThickness?.toFixed(2) ?? "-"}</td>
                <td>${c.governingMethod}</td>
            </tr>
        `;
    });

    html += "</tbody></table>";

    document.getElementById("shell-result").innerHTML = html;
}
