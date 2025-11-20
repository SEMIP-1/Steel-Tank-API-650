// ============================================================================
// HOME PAGE
// ============================================================================
window.initHomePage = function () {

    const tank = Api650State.data.tank;

    // Fill inputs
    document.getElementById("home-diameter").value = tank.diameter;
    document.getElementById("home-height").value   = tank.liquidHeight;
    document.getElementById("home-sg").value       = tank.specificGravity;
    document.getElementById("home-ca").value       = tank.corrosionAllowance;
    document.getElementById("home-je").value       = tank.jointEfficiency;
    document.getElementById("home-testmult").value = tank.testMultiplier;

    // Save on input change
    document.querySelectorAll("#home-diameter, #home-height, #home-sg, #home-ca, #home-je, #home-testmult")
        .forEach(input => {
            input.oninput = () => saveHomeData();
        });
};

function saveHomeData() {
    Api650State.setTank({
        diameter: document.getElementById("home-diameter").value,
        liquidHeight: document.getElementById("home-height").value,
        specificGravity: document.getElementById("home-sg").value,
        corrosionAllowance: document.getElementById("home-ca").value,
        jointEfficiency: document.getElementById("home-je").value,
        testMultiplier: document.getElementById("home-testmult").value
    });
}
