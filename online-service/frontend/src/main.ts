import { createApp } from "vue";
import { createRouter, createWebHistory } from "vue-router";
import App from "./App.vue";
import ResultSetList from "./ResultSetList.vue";
import ResultSet from "./ResultSet.vue";
import Upload from "./Upload.vue";
import CreateKey from "./CreateKey.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: "/",
            component: ResultSetList
        },
        {
            path: "/set/:id",
            component: ResultSet
        },
        {
            path: "/upload",
            component: Upload
        },
        {
            path: "/createKey",
            component: CreateKey
        }
    ]
})

const app = createApp(App);
app.use(router)
app.mount("#app");