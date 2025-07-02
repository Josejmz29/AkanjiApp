<template>
  <v-app theme="dark" class="fill-height">

    <!-- Navbar dinámica -->
    <NavbarGuest v-if="showNavbar && !isLoggedIn" />
    <NavbarUser v-if="showNavbar && isLoggedIn" />


    <v-container fluid class="fill-height d-flex ">
      <v-main class="fill-height">

        <router-view class="fill-height" />
      </v-main>
    </v-container>
  </v-app>
</template>

<script setup>
import { useRouter } from 'vue-router';

import { useUserStore } from '@/stores/user';
import NavbarGuest from './components/NavbarGuest.vue';
import NavbarUser from './components/NavbarUser.vue';

const userStore = useUserStore();
const route = useRoute();

const hideNavbarOnRoutes = ['/login', '/register'];

const showNavbar = computed(() => !hideNavbarOnRoutes.includes(route.path));
const isLoggedIn = computed(() => userStore.isLoggedIn);

console.log('isLoggedIn', isLoggedIn.value);
const router = useRouter();


</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
</style>
