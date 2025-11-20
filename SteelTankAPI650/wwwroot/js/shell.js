// ============================================================================
// SHELL DESIGN PAGE SCRIPT
// ============================================================================

// Global localStorage key
const SHELL_STATE_KEY = "Api650_ShellState";

// Page State (loaded/saved to localStorage)
let shellState = {
    diameter: "",
    height: "",
    sg: "1.0",
    ca: "2",
    courses: [] // { height: "", grade: "" }
};

// ============================================================================
// PAGE INITIALIZATION
// ============================================================================
window.initShellPage = async function () {

    // 1) Load state from localStorage
    loadShellState();

    // 2) Load materials (for dropdowns)
    const materials = await loadMaterials();

    // 3) Restore tank parameter inputs
    document.getElementById("shell-diameter").value = shellState.diameter;
    document.getElementById("shell-height").value   = shellState.height;
    document.getElementById("shell-sg").value       = shellState.sg;
    document.getElementById("shell-ca").value       = shellState.ca;

    // 4) Rebuild course table
    rebuildCourseTable(materials);

    // 5) Hook up buttons
    document.getElementById("btn-add-course").onclick = () => {
        shellState.courses.push({ height: "", grade: "" });
        saveShellState();
        rebuildCourseTable(materials);
    };

    document.getElementById("btn-shell-calc").onclick = calculateShell;
};

// ============================================================================
// LOAD MATERIALS FROM API
// ============================================================================
async function loadMaterials() {
    try {
        const res = await fetch("/api/materials");
        return await res.json();
    } catch (err) {
        console.error("Material load failed:", err);
        return [];
    }
}

// ============================================================================
// COURSE TABLE
// ============================================================================
function rebuildCourseTable(materials) {
    const tbody = document.getElementById("course-body");
    const template = document.getElementById("course-row-template");

    tbody.innerHTML = "";

    shellState.courses.forEach((course, index) => {
        const clone = template.content.cloneNode(true);

        // Assign course number
        clone.querySelector(".course-number").innerText = index + 1;

        // Height
        const heightInput = clone.querySelector(".course-height");
        heightInput.value = course.height;
        heightInput.onchange = (e) => {
            course.height = e.target.value;
            saveShellState();
        };

        // Material dropdown
        const matSelect = clone.querySelector(".course-material");
        matSelect.innerHTML = `<option value="">Select...</option>` +
            materials.map(m => `
                <option value="${m.grade}" ${course.grade === m.grade ? "selected" : ""}>
                    ${m.grade}
                </option>
            `).join("");

        matSelect.onchange = (e) => {
            course.grade = e.target.value;
            saveShellState();
        };

        // Remove button
        clone.querySelector(".btn-remove-course").onclick = () => {
            shellState.courses.splice(index, 1);
            saveShellState();
            rebuildCourseTable(materials);
        };

        tbody.appendChild(clone);
    });
}

// ============================================================================
// STATE SAVE / LOAD
// ============================================================================
function saveShellState() {
    shellState.diameter = document.getElementById("shell-diameter").value;
    shellState.height   = document.getElementById("shell-height").value;
    shellState.sg       = document.getElementById("shell-sg").value;
    shellState.ca       = document.getElementById("shell-ca").value;

    localStorage.setItem(SHELL_STATE_KEY, JSON.stringify(shellState));
}

function loadShellState() {
    const saved = localStorage.getItem(SHELL_STATE_KEY);
    if (saved) {
        shellState = JSON.parse(saved);
    }
}

// ============================================================================
// CALCULATE SHELL API CALL
// ============================================================================
async function calculateShell() {

    saveShellState();

    if (!shellState.diameter || !shellState.height) {
        alert("Please enter diameter and liquid height.");
        return;
    }

    if (shellState.courses.length === 0) {
        alert("Add at least one shell course.");
        return;
    }

    // Build payload
    const payload = {
        diameter: parseFloat(shellState.diameter),
        liquidLevel: parseFloat(shellState.height),
        specificGravity: parseFloat(shellState.sg),
        corrosionAllowance: parseFloat(shellState.ca),
        testSpecificGravity: 1.0,
        jointEfficiency: 0.85,
        testStressMultiplier: 0.6,
        shellCourses: shellState.courses.map((c, i) => ({
            courseNumber: i + 1,
            height: parseFloat(c.height || 0),
            materialGrade: c.grade
        }))
    };

    try {
        const res = await fetch("/api/shelldesign/calculate", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const t = await res.text();
            throw new Error(t);
        }

        const data = await res.json();

        renderShellResult(data);

    } catch (err) {
        console.error(err);
        document.getElementById("shell-result").innerHTML =
            `<div class="alert alert-danger">Error: ${err.message}</div>`;
    }
}

// ============================================================================
// RENDER RESULT TABLE
// ============================================================================
function renderShellResult(data) {
    const resultDiv = document.getElementById("shell-result");

    if (!data || !data.courses) {
        resultDiv.innerHTML =
            `<div class="alert alert-warning">No results returned.</div>`;
        return;
    }

    let rows = data.courses.map(c => `
        <tr>
            <td>${c.courseNumber}</td>
            <td>${c.height}</td>
            <td>${c.material}</td>
            <td>${c.td_OneFoot ?? ""}</td>
            <td>${c.tt_OneFoot ?? ""}</td>
            <td>${c.td_Variable ?? ""}</td>
            <td>${c.tt_Variable ?? ""}</td>
            <td>${c.requiredThickness}</td>
            <td>${c.testThickness}</td>
            <td>${c.governingMethod}</td>
        </tr>
        `).join("");

    resultDiv.innerHTML = `
        <div class="card shadow-sm">
            <div class="card-header fw-semibold">Shell Calculation Results</div>
            <div class="card-body p-0">
                <table class="table table-bordered table-sm mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>Course</th>
                            <th>Height</th>
                            <th>Material</th>
                            <th>td (1-ft)</th>
                            <th>tt (1-ft)</th>
                            <th>td (VDP)</th>
                            <th>tt (VDP)</th>
                            <th>Required t (mm)</th>
                            <th>Test t (mm)</th>
                            <th>Governing</th>
                        </tr>
                    </thead>
                    <tbody>${rows}</tbody>
                </table>
            </div>
        </div>
    `;
}
