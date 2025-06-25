import { defineStore } from "pinia";
import { ref } from "vue";

export const useUserStore = defineStore("user", {
  state: () => ({
    token: localStorage.getItem("jwt") || null,
  }),
  getters: {
    isLoggedIn: (state) => !!state.token,
  },
  actions: {
    setToken(token) {
      this.token = token;
      localStorage.setItem("jwt", token);
    },
    logout() {
      this.token = null;
      localStorage.removeItem("jwt");
    },
  },
});
