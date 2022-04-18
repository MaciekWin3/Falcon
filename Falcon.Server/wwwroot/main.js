Vue.createApp({
    data() {
        return {
            message: 'Hello Vue!',
            number: 1,
            stats: null
        }
    },
    mounted() {
        fetch("https://api.github.com/repos/MaciekWin3/chess-clock-react-native")
            .then(response => response.json())
            .then(data => this.stats = data);
    }
}).mount('#app')