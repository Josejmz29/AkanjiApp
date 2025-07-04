<template>
  <v-container class="fill-height ">
    <v-window v-model="vistaActual" class="w-100">

      <!-- Vista 1: Formulario de búsqueda -->
      <v-window-item :value="1">
        <v-container>
          <v-row class="d-flex justify-center align-center" align="center">
            <v-col class="d-flex justify-center align-center">
              <v-card class="pa-6" width="600">
                <v-card-title>Buscar documento por DOI</v-card-title>
                <v-card-text>
                  <v-form @submit.prevent="buscarDoi">
                    <v-text-field v-model="doi" label="Introduce el DOI" required />
                    <v-btn type="submit" color="primary" class="mt-2">Buscar</v-btn>
                    <v-alert v-if="error" type="error" class="mt-4">{{ error }}</v-alert>
                  </v-form>
                </v-card-text>
              </v-card>
            </v-col>

          </v-row>

        </v-container>

      </v-window-item>

      <!-- Vista 2: Resultados -->
      <v-window-item :value="2">

        <v-container class="pa-6 ma-6">
          <v-row>
            <v-col class="d-flex justify-center align-center">
              <v-card class="pa-6" width="950" style="overflow-y: auto;">
                <v-card-title class="text-h5">Datos del Documento</v-card-title>
                <v-card-text>

                  <v-text-field v-model="documento.title" label="Título" outlined class="mb-4" readonly />

                  <v-textarea v-model="documento.description" label="Descripción" outlined class="mb-4" readonly />

                  <v-card class="mt-4" elevation="2" max-height="200" style="overflow-y: auto;">
                    <v-card-title class="text-h6">Autores</v-card-title>
                    <v-divider></v-divider>
                    <v-list>
                      <v-list-item v-for="(autor, index) in documento.creators" :key="index">
                        <v-list-item-content>
                          <v-list-item-title>
                            {{ autor.name }} {{ autor.apellido }}
                          </v-list-item-title>
                        </v-list-item-content>
                      </v-list-item>
                    </v-list>
                  </v-card>


                  <v-container>
                    <v-row>
                      <v-col cols="12" md="6">
                        <v-text-field v-model="documento.publisher" label="Editorial" outlined class="ma-2" readonly />
                        <v-select v-model="resource_type" label="Resource Type"
                          :items="['Publication / Journal article', 'Publication / Conference Paper']" outlined
                          class="ma-2" />

                        <v-card class="ma-2 pa-3" outlined max-height="200" style="overflow-y: auto;">
                          <v-card-title class="text-subtitle-1">Subjects</v-card-title>
                          <v-divider></v-divider>
                          <v-list>
                            <v-list-item v-for="(item, index) in documento.subjects" :key="index">
                              <v-row class="w-auto" align="center" justify="space-between">
                                <v-col cols="auto">
                                  {{ item.subject }}
                                </v-col>

                              </v-row>
                            </v-list-item>
                          </v-list>


                        </v-card>



                      </v-col>

                      <v-col cols="12" md="6">
                        <v-text-field v-model="documento.language" label="Language" outlined class="ma-2" readonly />
                        <v-text-field v-model="documento.version" label="Version" outlined class="ma-2" readonly />

                      </v-col>
                    </v-row>


                    <v-card class="ma-2 pa-3" outlined>
                      <v-card-title class="text-subtitle-1">Funders</v-card-title>
                      <v-list>
                        <v-list-item v-for="(funder, index) in documento.funders" :key="index" class="px-0">
                          <v-row class="w-100" align="center" justify="space-between">
                            <v-col cols="5">
                              <v-text-field v-model="funder.name" label="Funder Name" dense hide-details readonly />
                            </v-col>
                            <v-col cols="5">
                              <v-text-field v-model="funder.grant_number" label="Grant Number" dense hide-details
                                readonly />
                            </v-col>

                          </v-row>
                        </v-list-item>
                      </v-list>


                    </v-card>

                  </v-container>

                  <v-divider></v-divider>

                  <v-container>
                    <v-card class="pa-4">
                      <v-file-input label="Sube los archivos" prepend-icon="mdi-upload" multiple show-size
                        :counter="true" accept=".pdf,.csv,.zip,.json,.jpg,.jpeg,.png,.tif,.tiff"
                        @change="handleFileUpload" outlined />

                      <v-alert v-if="uploadStore.uploadSuccess" type="success" class="mt-4">
                        Archivos subidos con éxito:
                        <ul>
                          <li v-for="(file, index) in uploadStore.uploadedFiles" :key="index">
                            <strong>{{ file.name }}</strong>
                          </li>
                        </ul>
                      </v-alert>
                    </v-card>
                  </v-container>

                  <v-container>
                    <v-row justify="end">
                      <v-tooltip v-if="uploadStore.uploadedFiles.length !== 0" location="top">
                        <template #activator="{ props }">
                          <v-btn class="px-3 mx-6" color="green" v-bind="props" @click="subir"
                            :disabled="uploadStore.uploadedFiles.length === 0">
                            Publicar en Zenodo
                          </v-btn>
                        </template>
                        <span> Esta acción es irreversible. La publicación no se puede borrar una vez
                          realizada.</span>
                      </v-tooltip>


                      <v-btn class="px-3 mx-6" color="green" @click="subirBorrador"
                        :disabled="uploadStore.uploadedFiles.length === 0">
                        Subir documento a Zenodo (Borrador)
                      </v-btn>

                      <v-btn class="px-3 mx-6" color="primary" @click="volver" width="90">Volver</v-btn>
                    </v-row>
                    <v-row>
                      <v-alert v-if="subidaExitosa" type="success" class="mt-4">
                        ¡El documento se ha subido correctamente a Zenodo!
                      </v-alert>

                      <v-alert v-if="subidaExitosa === false" type="error" class="mt-4">
                        No se pudo subir el documento a Zenodo. Inténtalo de nuevo.
                      </v-alert>
                    </v-row>
                  </v-container>

                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
        </v-container>

      </v-window-item>

      <!-- Vista 3: Confirmación de subida -->
      <v-window-item :value="3">
        <v-container>

          <v-row class="d-flex justify-center align-center ">
            <v-col class="d-flex justify-center align-center flex-column">
              <h1 class="text-h4 mb-4">✅ Documento subido con éxito a Zenodo</h1>
              <v-btn color="primary" class="mt-6" @click="volver">Volver al inicio</v-btn>
            </v-col>

          </v-row>

        </v-container>
      </v-window-item>


    </v-window>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import { obtenerPorDoi } from '@/api/api.js'
import { subirDocumentoPorDoi } from '@/api/api.js'
import { subirDocumentoPorDoiBorrador } from '@/api/api.js'
import { useDocumentoDOIStore } from '@/stores/documentoDOI'
import { useUploadStore } from '@/stores/upload';



const uploadStore = useUploadStore();
const documentoDOIStore = useDocumentoDOIStore()



const doi = ref('')
const error = ref(null)
const vistaActual = ref(1)
const documento = ref({})
const resource_type = ref('')
const subidaExitosa = ref(null);


const handleFileUpload = (event) => {
  const files = event.target.files;
  if (!files || files.length === 0) return;

  uploadStore.setUploadFiles(Array.from(files)); // Nueva función para múltiples
};


const subir = async () => {
  try {
    const files = uploadStore.uploadedFiles;
    const doi = documentoDOIStore.documento.doi

    if (!files || files.length === 0 || !doi)
      throw new Error('Archivo(s) o DOI no definidos.');

    let resourceType;
    console.log('Tipo de recurso:', resource_type.value)
    if (resource_type.value === 'Publication / Journal article') {
      resourceType = 'publication-article'
    } else if (resource_type.value === 'Publication / Conference Paper') {
      resourceType = 'publication-conferencepaper'
    } else {
      resourceType = 'default'
    }

    const response = await subirDocumentoPorDoi(files, doi, resourceType);

    subidaExitosa.value = true
    console.log('Respuesta subida:', response)

    vistaActual.value = 3

    //Reinicia todo tras 5 segundos con recarga
    //setTimeout(() => { window.location.reload() }, 5000)

  } catch (error) {
    subidaExitosa.value = false
    console.error('Error al subir el documento:', error)
  }
}

const subirBorrador = async () => {
  try {
    const files = uploadStore.uploadedFiles;
    const doi = documentoDOIStore.documento.doi

    if (!files || files.length === 0 || !doi)
      throw new Error('Archivo(s) o DOI no definidos.');

    let resourceType;
    console.log('Tipo de recurso:', resource_type.value)
    if (resource_type.value === 'Publication / Journal article') {
      resourceType = 'publication-article'
    } else if (resource_type.value === 'Publication / Conference Paper') {
      resourceType = 'publication-conferencepaper'
    } else {
      resourceType = 'default'
    }

    const response = await subirDocumentoPorDoiBorrador(files, doi, resourceType);

    subidaExitosa.value = true
    console.log('Respuesta subida:', response)

    vistaActual.value = 3

    //Reinicia todo tras 5 segundos con recarga
    //setTimeout(() => { window.location.reload() }, 5000)

  } catch (error) {
    subidaExitosa.value = false
    console.error('Error al subir el documento:', error)
  }
}

const resetearTodo = () => {
  vistaActual.value = 1;
  doi.value = '';
  error.value = null;
  documento.value = {};
  subidaExitosa.value = null;
  uploadStore.resetUpload(); // Asegúrate de tener esta función en tu store
  documentoDOIStore.setDocumento({});
};

const buscarDoi = async () => {
  try {
    error.value = null


    const res = await obtenerPorDoi(doi.value)
    console.log(res);

    documentoDOIStore.setDocumento(res)
    //documento.value = { ...res, subjects: res.subjects || [] }
    documento.value = res
    vistaActual.value = 2
  } catch (err) {
    if (err)
      error.value = err.message || 'Error al buscar DOI'
  }
}

const volver = () => {
  resetearTodo();
}



const nuevoSubject = ref('')



</script>
