// ============================================================================
// GLOBAL STATE MANAGER
// ============================================================================

window.Api650State = {

    key: "Api650_GlobalState",

    // Default State
    data: {
        tank: {
            diameter: "",
            liquidHeight: "",
            specificGravity: "1.0",
            corrosionAllowance: "2",
            jointEfficiency: "0.85",
            testMultiplier: "0.6"
        },

        materials: [],        // list from API
        shellCourses: [],     // for shell page
        shellResult: null,    // API result
        bottomResult: null    // API result
    },

    // Load state from localStorage
    load() {
        const saved = localStorage.getItem(this.key);
        if (saved) {
            try {
                this.data = JSON.parse(saved);
            } catch { }
        }
    },

    // Save state to localStorage
    save() {
        localStorage.setItem(this.key, JSON.stringify(this.data));
    },

    // ========== TANK ==========
    setTank(tankData) {
        this.data.tank = { ...this.data.tank, ...tankData };
        this.save();
    },

    // ========== MATERIALS ==========
    setMaterials(matList) {
        this.data.materials = matList;
        this.save();
    },

    // ========== SHELL ==========
    setShellCourses(courses) {
        this.data.shellCourses = courses;
        this.save();
    },

    setShellResult(result) {
        this.data.shellResult = result;
        this.save();
    },

    // ========== BOTTOM ==========
    setBottomResult(result) {
        this.data.bottomResult = result;
        this.save();
    }
};

// Load state immediately
Api650State.load();
