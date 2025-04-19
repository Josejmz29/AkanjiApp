<template>
    <v-toolbar color="black" class="px-4 py-2">
  <v-app-bar-title
    @click="goHome"
    class="cursor-pointer mr-4 font-weight-bold"
    style="font-size: 24px; color: white; text-transform: uppercase;"
  >
    Akanji
  </v-app-bar-title>

  <v-spacer />

  <v-btn
    variant="outlined"
    color="primary"
    class="ml-4"
    @click="goToLogin"
  >
    Login
  </v-btn>
</v-toolbar>
  </template>
  
  <script setup>
  import { useRouter } from 'vue-router';
  
  const router = useRouter();
  const goHome = () => router.push('/');
  const goToLogin = () => router.push('/login');
  </script>
  
  <style scoped>
  .cursor-pointer {
    cursor: pointer;
  }
  </style>
  