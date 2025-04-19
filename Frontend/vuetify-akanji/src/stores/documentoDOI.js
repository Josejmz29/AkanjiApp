import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useDocumentoDOIStore = defineStore('documentoDOI', () => {
  // Documento actual
  const documento = ref(null)

  // Guardar el documento obtenido por DOI
  const setDocumento = (nuevoDocumento) => {
    documento.value = nuevoDocumento
  }

  // Limpiar el documento
  const limpiarDocumento = () => {
    documento.value = null
  }

  return {
    documento,
    setDocumento,
    limpiarDocumento,
  }
})
