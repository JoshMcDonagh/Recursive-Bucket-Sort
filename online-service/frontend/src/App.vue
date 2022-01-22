<template>
    <header>
        <h1>Joshua Marland's Sorting Algorithm Comparison</h1>
        <div id="links">
            <router-link to="/">Results</router-link>
            <router-link to="/upload">Upload</router-link>
            <router-link to="/createKey">Keys</router-link>
        </div>
    </header>
    <section>
        <router-view></router-view>
    </section>
</template>

<style lang="scss" scoped>
section {
    margin: 2rem;
    max-width: 100%;
}

header {
    display: flex;
    flex-direction: row;
    width: 100vw;
    height: 64px;
    background-color: #fd7e7e;
    justify-content: space-between;
    box-shadow: 0px 0px 15px 5px rgba(0,0,0,0.31);

    h1 {
        display: flex;
        margin-left: 1rem;
        align-self: center;
        font-size: 24px;
    }

    #links {
        display: flex;
        flex-direction: row;
        margin-right: 45%;
        font-size: 20px;
        align-self: center;

        a {
            text-decoration: none;
            font-weight: bold;
            margin-right: 1rem;

            &:hover {
                text-decoration: underline;
            }
        }
    }
}
</style>