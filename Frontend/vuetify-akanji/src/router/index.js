/**
 * router/index.ts
 *
 * Automatic routes for `./src/pages/*.vue`
 */

// Composables
import { useUserStore } from "@/stores/user";
import { createRouter, createWebHistory } from "vue-router/auto";
import { setupLayouts } from "virtual:generated-layouts";
import { routes } from "vue-router/auto-routes";

const protectedRoutes = [
  "/DOImanager", // esta es tu ruta protegida
];

protectedRoutes.forEach((path) => {
  const route = routes.find((r) => r.path === path);
  if (route) {
    route.meta = { ...(route.meta || {}), requiresAuth: true };
  } else {
    console.warn(`Ruta protegida no encontrada: ${path}`);
  }
});

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: setupLayouts(routes),
});

// Workaround for https://github.com/vitejs/vite/issues/11804
router.onError((err, to) => {
  if (err?.message?.includes?.("Failed to fetch dynamically imported module")) {
    if (!localStorage.getItem("vuetify:dynamic-reload")) {
      console.log("Reloading page to fix dynamic import error");
      localStorage.setItem("vuetify:dynamic-reload", "true");
      location.assign(to.fullPath);
    } else {
      console.error("Dynamic import error, reloading page did not fix it", err);
    }
  } else {
    console.error(err);
  }
});

router.isReady().then(() => {
  localStorage.removeItem("vuetify:dynamic-reload");
});

// Guard global
router.beforeEach((to, from, next) => {
  const userStore = useUserStore();

  if (to.meta.requiresAuth && !userStore.isLoggedIn) {
    // Si la ruta requiere auth y el usuario no está logueado, redirigir a login
    next("/login");
  } else {
    next(); // continuar normalmente
  }
});

export default router;
