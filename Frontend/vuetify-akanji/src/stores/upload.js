import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUploadStore = defineStore('upload', () => {
  const uploadedFileName = ref('');
  const uploadSuccess = ref(false);
  const uploadResponse = ref(null);

  function setUploadInfo(name, file) {
    uploadedFileName.value = name;
    uploadSuccess.value = true;
    uploadResponse.value = file;
  }

  function resetUpload() {
    uploadedFileName.value = '';
    uploadSuccess.value = false;
    uploadResponse.value = null;
  }

  return {
    uploadedFileName,
    uploadSuccess,
    uploadResponse,
    setUploadInfo,
    resetUpload,
  };
});