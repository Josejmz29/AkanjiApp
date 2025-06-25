// auth.js (servicio de autenticación)
import axios from "axios";
import api from "@/api/api.js";

const BASE_URL = "http://localhost:5215"; // Cambia esto según tu configuración

const config = {
  headers: {
    "Content-Type": "application/json",
  },
};
export const login = async (email, password) => {
  try {
    const res = await axios.post(
      `${BASE_URL}/api/auth/login`,
      {
        email,
        password,
      },
      config
    );

    const token = res.data.token;
    // Configura cabecera por defecto para futuras llamadas
    api.defaults.headers.common["Authorization"] = `Bearer ${token}`;

    // Guarda el token en localStorage o sessionStorage
    localStorage.setItem("jwt", token); // o sessionStorage.setItem('jwt', token);
    return token;
  } catch (error) {
    throw error.response?.data || { message: "Error en login" };
  }
};

export const registrarUsuario = async ({
  NombreCompleto,
  Email,
  Password,
  ZenodoToken,
}) => {
  const response = await axios.post(
    `${BASE_URL}/api/auth/register`,
    {
      NombreCompleto,
      Email,
      Password,
      ZenodoToken,
    },
    config
  );
  return response.data;
};

export const logout = () => {
  localStorage.removeItem("jwt");
  delete api.defaults.headers.common["Authorization"];
};

export const isLoggedIn = () => {
  const token = localStorage.getItem("jwt");
  return !!token;
};
