import { defineStore } from "pinia";
import { ref } from "vue";

export const useUploadStore = defineStore("upload", () => {
  const uploadedFileName = ref("");
  const uploadSuccess = ref(false);

  const uploadedFiles = ref([]); // En lugar de solo uno

  function setUploadInfo(name, file) {
    uploadedFileName.value = name;
    uploadSuccess.value = true;
  }

  function resetUpload() {
    uploadedFileName.value = "";
    uploadSuccess.value = false;
    uploadedFiles.value = [];
  }
  function setUploadFiles(fileList) {
    uploadedFiles.value = fileList;
    uploadSuccess.value = true;
  }

  return {
    uploadedFileName,
    uploadSuccess,

    uploadedFiles, // Nueva propiedad para múltiples archivos
    setUploadFiles, // Nueva función para manejar múltiples archivos
    setUploadInfo,
    resetUpload,
  };
});
