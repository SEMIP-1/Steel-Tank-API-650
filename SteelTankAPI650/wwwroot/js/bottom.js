// wwwroot/js/bottom.js
const bottomApi = "/api/bottomplate";

function initBottomPage() {
    if (!window.Api650State) return;
    const btn = document.getElementById("btn-bottom-calc");
    if (!btn) return;
    btn.addEventListener("click", calculateBottom);

    // Fill from global tank state
    const tank = Api650State.data.tank || {};
    document.getElementById("bp-diameter").value = tank.diameter ?? "";
    document.getElementById("bp-height").value   = tank.liquidHeight ?? "";
    document.getElementById("bp-sg").value       = tank.specificGravity ?? 1.0;
    document.getElementById("bp-ca").value       = tank.corrosionAllowance ?? 0;

    // If shell result exists, use first course thickness & stresses as hint
    const shellResult = Api650State.data.shellResult;
    if (shellResult) {
        const courses = shellResult.courses ?? shellResult.Courses;
        if (courses && courses.length > 0) {
            const first = courses[0];
            const treq  = first.requiredThickness ?? first.RequiredThickness;
            document.getElementById("bp-t1").value = treq ?? "";

            // If you later store Sd, St in result, you can prefill bp-sd, bp-st here
        }
    }
}

async function calculateBottom() {
    const resultDiv = document.getElementById("bottom-result");
    resultDiv.innerHTML = "";

    const D  = parseFloat(document.getElementById("bp-diameter").value);
    const H  = parseFloat(document.getElementById("bp-height").value);
    const G  = parseFloat(document.getElementById("bp-sg").value);
    const CA = parseFloat(document.getElementById("bp-ca").value);

    const t1 = parseFloat(document.getElementById("bp-t1").value);
    const Sd = parseFloat(document.getElementById("bp-sd").value);
    const St = parseFloat(document.getElementById("bp-st").value);

    if (!D || !H || !G || !t1 || !Sd) {
        alert("Please fill D, H, SG, t1, and Sd.");
        return;
    }

    // Update tank in global state from here too
    Api650State.setTank({
        diameter: D,
        liquidHeight: H,
        specificGravity: G,
        corrosionAllowance: CA
    });

    // ⚠ Adjust payload to exactly match your BottomPlateInput model
    const payload = {
        diameter: D,
        liquidLevel: H,
        specificGravity: G,
        corrosionAllowance: CA,
        firstShellThickness: t1,
        firstShellSd: Sd,
        firstShellSt: St
    };

    try {
        const res = await fetch(bottomApi, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const t = await res.text();
            throw new Error(t || ("HTTP " + res.status));
        }

        const data = await res.json();

        // Save result to global state
        Api650State.setBottomResult(data);

        resultDiv.innerHTML = `
            <div class="card">
                <div class="card-header">Bottom Plate Result</div>
                <div class="card-body">
                    <pre class="mb-0">${JSON.stringify(data, null, 2)}</pre>
                </div>
            </div>
        `;
    } catch (err) {
        console.error(err);
        resultDiv.innerHTML =
            `<div class="alert alert-danger">Bottom calculation failed: ${err.message}</div>`;
    }
}

(function () {
    if (document.getElementById("bottom-page")) {
        initBottomPage();
    }
})();
