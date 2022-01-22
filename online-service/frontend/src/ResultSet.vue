<template>
    <h2>Recording {{$route.params.id}}</h2>
    <br/>
    <section id="recordings">
        <table>
            <thead>
                <tr>
                    <th>Sorting Algorithm</th>
                    <th>Duration</th>
                    <th>ArraySize</th>
                    <th>Language</th>
                    <th>Array Distribution</th>
                    <th>Array Element Type</th>
                    <th>OS</th>
                    <th>Arch</th>
                    <th>CPU</th>
                    <th>Algorithm Version</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="rec in recordings" :key="rec.id">
                    <td>{{rec.sortingAlgorithm}}</td>
                    <td>{{rec.duration}}</td>
                    <td>{{rec.arraySize}}</td>
                    <td>{{rec.language}}</td>
                    <td>{{rec.metadata.arrayDistribution}}</td>
                    <td>{{rec.metadata.arrayElementType}}</td>
                    <td>{{rec.metadata.operatingSystem}}</td>
                    <td>{{rec.metadata.architecture}}</td>
                    <td>{{rec.metadata.cpu}}</td>
                    <td>{{rec.metadata.sortingAlgorithmImplementationVersion}}</td>
                </tr>
            </tbody>
        </table>
    </section>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute()
const recordings = ref<any[]>()

fetch("/api/results", {
    method: "POST",
    headers: {
        "Content-Type": "application/json"
    },
    body: JSON.stringify({
        ResultSetId: route.params.id
    })
}).then(r => r.json()).then(j => recordings.value = j)
</script>

<style lang="scss" scoped>
#recordings {
    display: flex;
    flex-direction: column;
    max-width: 100%;
    padding: 1rem;
    background-color: #eee;
}

td {
    text-align: center;
}
</style>