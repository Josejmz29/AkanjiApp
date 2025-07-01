<template>
    <v-container>
        <v-card class="mx-auto my-12 pa-6" max-width="600">
            <v-card-title>Subir PDF y Extraer Funders</v-card-title>
            <v-card-text>
                <v-file-input label="Selecciona un archivo PDF" accept="application/pdf" v-model="selectedFile"
                    outlined></v-file-input>

                <v-btn :disabled="!selectedFile || loading" class="mt-4" color="primary" @click="handleUpload">
                    {{ loading ? "Procesando..." : "Extraer Funders" }}
                </v-btn>

                <v-alert v-if="errorMessage" type="error" class="mt-4">
                    {{ errorMessage }}
                </v-alert>

                <v-card v-if="responseData" class="mt-4 pa-4">
                    <v-card-title class="text-h6 font-weight-bold">Funders extraídos:</v-card-title>

                    <v-list>
                        <v-list-item v-for="(funder, index) in responseData" :key="index" class="pa-4">
                            <v-card class="pa-4 w-100">
                                <v-card-text>
                                    <div class="text-body-1 mb-2"><strong>Nombre:</strong> {{ funder.name || 'N/A' }}
                                    </div>
                                    <div class="text-body-1 mb-2"><strong>Identificador:</strong> {{ funder.identifier
                                        || 'N/A' }}</div>
                                    <div class="text-body-1 mb-2"><strong>Esquema:</strong> {{ funder.scheme || 'N/A' }}
                                    </div>
                                    <div class="text-body-1 mb-2"><strong>Número de subvención:</strong> {{
                                        funder.grant_number || 'N/A' }}</div>
                                </v-card-text>
                            </v-card>
                        </v-list-item>
                    </v-list>
                </v-card>

            </v-card-text>
        </v-card>
    </v-container>
</template>

<script setup>
import { ref } from "vue";
import { extraerFunders } from "@/api/api";

const selectedFile = ref(null);
const responseData = ref(null);
const errorMessage = ref("");
const loading = ref(false);

async function handleUpload() {
    if (!selectedFile.value) return;

    responseData.value = null;
    errorMessage.value = "";
    loading.value = true;

    try {
        const response = await extraerFunders(selectedFile.value);
        if (response.status === 200) {
            responseData.value = response.data.funders;
            console.log("Funders extraídos:", response.data.funders);
        } else {
            errorMessage.value = `Error ${response.status}: ${response.statusText}`;
        }
    } catch (error) {
        errorMessage.value = error.response
            ? `Error ${error.response.status}: ${error.response.data}`
            : `Error: ${error.message}`;
    } finally {
        loading.value = false;
    }
}
</script>

<style scoped>
pre {
    white-space: pre-wrap;
    word-break: break-word;
}
</style>
