<template>
  <v-toolbar color="blue-grey-darken-4">
    <v-app-bar-title @click="goHome" class="cursor-pointer">Akanji</v-app-bar-title>
    <v-spacer />
    <v-btn text @click="goToDoiManager">DOI Manager</v-btn> <!-- Nueva opción -->
    <v-btn text @click="goToExtraerFunder">Extraer Funders</v-btn> <!-- Nueva opción -->
    <v-btn text @click="logout">Logout</v-btn>
  </v-toolbar>
</template>

<script setup>
import { useRouter } from 'vue-router';
import { useUserStore } from '@/stores/user';

const router = useRouter();
const userStore = useUserStore();

const goHome = () => router.push('/');
const goToDoiManager = () => router.push('/DOImanager'); // Ruta a DOI Manager
const goToExtraerFunder = () => router.push('/ExtraerFunder'); // Ruta a Extraer Funders

const logout = () => {
  userStore.logout();
  router.push('/login');
};
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
</style>
