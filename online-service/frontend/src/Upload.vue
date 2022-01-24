<template>
    <form action="/api/upload" method="post" enctype="multipart/form-data">
        <label for="api-key">Api Key:</label>
        <input name="api-key" type="text" />
        <label for="file">File:</label>
        <input name="file" type="file" />
        <button>Submit</button>
    </form>
</template>

<script>
export default {
    mounted() {
        if(this.$route.query.error) {
            alert("Server returned an error: " + this.$route.query.error)
        }
    }
}
</script>

<style lang="scss" scoped>
form {
    display: flex;
    flex-direction: column;
    background-color: #eee;
    padding: 1rem;
    width: 400px;

    input {
        margin-bottom: 1rem;
    }

    label {
        font-size: 18px;
        margin-right: 1rem;
    }
}
</style>