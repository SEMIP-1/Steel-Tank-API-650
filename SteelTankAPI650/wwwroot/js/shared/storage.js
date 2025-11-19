window.appState = {
    tank: {},
    settings: {},

    save(name, value) {
        localStorage.setItem(name, JSON.stringify(value));
    },

    load(name) {
        let v = localStorage.getItem(name);
        return v ? JSON.parse(v) : null;
    }
};
