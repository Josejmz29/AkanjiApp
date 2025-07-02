import { defineStore } from "pinia";

export const useUserStore = defineStore("user", {
  state: () => {
    const storedToken = localStorage.getItem("jwt");
    // Considera inválidos "", "null", "undefined"
    const isValidToken =
      storedToken && storedToken !== "null" && storedToken !== "undefined";
    return {
      token: isValidToken ? storedToken : null,
    };
  },
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
