<template>
    <v-container class="fill-height d-flex justify-center align-center">
      <v-card class="pa-6" width="500">
        <v-card-title class="text-h5 text-center">Buscar DOI</v-card-title>
  
        <v-card-text>
          <!-- Campo de entrada DOI -->
          <v-text-field
            v-model="doi"
            label="Introduce un DOI"
            prepend-inner-icon="mdi-magnify"
            :error-messages="errorMessage"
            required
          />
  
          <!-- Botón Buscar -->
          <v-btn color="primary" block class="mt-2" @click="searchDoi">
            Buscar
          </v-btn>
  
          <!-- Mostrar resultado si existe -->
          <v-alert v-if="doiData" type="info" class="mt-4">
            <strong>Título:</strong> {{ doiData.title }} <br>
            <strong>Autor:</strong> {{ doiData.author }} <br>
            <strong>Año:</strong> {{ doiData.year }}
          </v-alert>
  
          <!-- Botón para subir archivos -->
          <v-btn v-if="doiData" color="success" block class="mt-4">
            Subir Archivos
          </v-btn>
        </v-card-text>
      </v-card>
    </v-container>
  </template>
  
  <script setup>
  import { ref } from 'vue';
  
  const doi = ref('');
  const errorMessage = ref('');
  const doiData = ref(null);
  
  const searchDoi = () => {
    // Validación simple del DOI
    if (!doi.value.match(/^10\.\d{4,9}\/[-._;()/:A-Z0-9]+$/i)) {
      errorMessage.value = 'DOI no válido. Introduce un DOI correcto.';
      doiData.value = null;
      return;
    }
  
    errorMessage.value = '';
    
    // Simulación de datos (aquí se llamaría a la API en el futuro)
    doiData.value = {
      title: 'Ejemplo de Publicación Científica',
      author: 'John Doe',
      year: '2023',
    };
  };
  </script>
  