<template>
    <div>
        <label for="api-key">Api Key:</label>
        <input name="api-key" type="text" v-model="key" />
        <label for="is-admin">Is Admin?</label>
        <input name="is-admin" type="checkbox" v-model="isAdmin" />
        <p>New key: {{ newKey }}</p>
        <button @click="createKey">Create key</button>
    </div>
</template>

<script>
export default {
    data() {
        return {
            key: "",
            newKey: "",
            isAdmin: false
        };
    },

    mounted() {
        if (this.$route.query.error) {
            alert("Server returned an error: " + this.$route.query.error);
        }
    },

    methods: {
        createKey() {
            fetch("/api/token", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    ApiKey: this.key,
                    IsAdmin: this.isAdmin
                })
            }).then(r => { if (!r.ok) throw r; else return r.json(); })
            .then(j => this.newKey = j.apiKey)
            .catch(r => r.text())
            .then(t => this.newKey = t);
        }
    },
};
</script>

<style lang="scss" scoped>
div {
    display: flex;
    flex-direction: column;
    background-color: #eee;
    padding: 1rem;
    width: 800px;

    input {
        margin-bottom: 1rem;
    }

    label {
        font-size: 18px;
        margin-right: 1rem;
    }
}
</style>