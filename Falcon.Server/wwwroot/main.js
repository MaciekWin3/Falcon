Vue.createApp({
    data() {
        return {
            message: 'Hello Vue!',
            stats: null
        }
    },
    async created() {
        fetch("https://api.github.com/repos/MaciekWin3/chess-clock-react-native")
            .then(response => response.json())
            .then(data => this.stats = data);
    }
}).mount('#app')