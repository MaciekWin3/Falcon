document.addEventListener('alpine:init', async () => {
    Alpine.data('level', () => ({
        data: {},
        async init() {
            this.data = await this.fetchData();
        },
        async fetchData() {
            try {
                let response = await fetch("https://api.github.com/repos/MaciekWin3/Falcon");
                return await response.json();
            }
            catch (error) {
                return "";
            }
        }
    }))
})