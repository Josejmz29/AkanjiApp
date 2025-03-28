<template>
  <v-app theme="dark" class="fill-height">
    <v-container>
      <v-toolbar color="black" density="comfortable">
        <!-- Texto "Akanji" con navegación a Home -->
        <v-app-bar-title class="cursor-pointer" @click="goToHome">
          Akanji
        </v-app-bar-title>

        <v-spacer></v-spacer>

        <!-- Botón de Login con navegación -->
        <v-btn color="primary" variant="outlined" @click="goToLogin">
          Login
        </v-btn>
      </v-toolbar>

      <v-main class="fill-height">
        
        <DOImanager />
      </v-main>
    </v-container>
  </v-app>
</template>

<script setup>
import { useRouter } from 'vue-router';
import DOImanager from './pages/DOImanager.vue';

const router = useRouter();

const goToHome = () => {
  router.push('/'); // Redirige a Home
};

const goToLogin = () => {
  router.push('/login'); // Redirige a Login
};
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
</style>
