<template>
    <h2>Recording {{$route.params.id}}</h2>
    <br/>
    <button style="margin-bottom: 1rem;" @click="exportToCsv">Export to CSV</button>
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

function exportToCsv() {
    let result = "SortingAlgorithm,Duration(seconds),ArraySize(elements),Language,ArrayDistribution,ArrayElementType,OperatingSystem,Architecture,CPU,SortingAlgorithmImplementationVersion\n"

    recordings.value?.forEach(r => {
        result += r.sortingAlgorithm + ","
        result += r.duration + ","
        result += r.arraySize + ","
        result += r.language + ","
        result += r.metadata.arrayDistribution + ","
        result += r.metadata.arrayElementType + ","
        result += r.metadata.operatingSystem + ","
        result += r.metadata.architecture + ","
        result += r.metadata.cpu + ","
        result += r.metadata.sortingAlgorithmImplementationVersion + ","
        result += "\n"
    });

    const blob = new Blob([result], {type: "text/csv"});
    const elem = window.document.createElement("a");
    elem.href = window.URL.createObjectURL(blob);
    elem.download = "results.csv";
    document.body.appendChild(elem);
    elem.click();
    document.body.removeChild(elem);
}
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