<template>
  <v-toolbar color="blue-grey-darken-4">
    <v-app-bar-title @click="goHome" class="cursor-pointer">Akanji</v-app-bar-title>
    <v-spacer />
    <v-btn text @click="logout">Logout</v-btn>
  </v-toolbar>
</template>

<script setup>
import { useRouter } from 'vue-router';

import { useUserStore } from '@/stores/user';
const router = useRouter();
const goHome = () => router.push('/');

const userStore = useUserStore();

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