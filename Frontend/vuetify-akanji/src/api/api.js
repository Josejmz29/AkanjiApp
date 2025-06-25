// src/api/api.js
import axios from "axios";

import { useUserStore } from "@/stores/user";

const api = axios.create({
  baseURL: "http://localhost:5215",
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Detecta token expirado o inválido
    if (error.response && error.response.status === 401) {
      const userStore = useUserStore();
      userStore.logout(); // Borra el token
      console.log("Token expirado o inválido, redirigiendo al login");
      window.location.href = "/login"; // Redirección forzada
    }
    return Promise.reject(error);
  }
);
export default api;

function handleError(error) {
  if (error.response) {
    console.error("Error response:", error.response);
    return error.response.data || { message: error.message };
  } else if (error.request) {
    console.error("Error request:", error.request);
    return { message: "No se pudo conectar con el servidor" };
  } else {
    console.error("General error:", error.message);
    return { message: error.message };
  }
}

// Documentos CRUD
export const getDocumentos = async () => {
  try {
    const res = await api.get("/Documentoes");
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

export const getDocumentoById = async (id) => {
  try {
    const res = await api.get(`/Documentoes/${id}`);
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

export const createDocumento = async (documento) => {
  try {
    const res = await api.post("/Documentoes", documento);
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

export const updateDocumento = async (id, documento) => {
  try {
    await api.put(`/Documentoes/${id}`, documento);
  } catch (error) {
    throw handleError(error);
  }
};

export const deleteDocumento = async (id) => {
  try {
    await api.delete(`/Documentoes/${id}`);
  } catch (error) {
    throw handleError(error);
  }
};

export const obtenerPorDoi = async (doi) => {
  try {
    const sanitizedDoi = doi.trim();

    const res = await api.get("/doi", { params: { doi } });

    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

// Zenodo
export const subirDocumentoConMetadata = async (file, dto) => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append(
    "dto",
    new Blob([JSON.stringify(dto)], { type: "application/json" })
  );

  try {
    const res = await api.post("/api/zenodo/subir", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

export const subirDocumentoPorDoi = async (file, doi) => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("doi", doi);

  try {
    const res = await api.post("/api/zenodo/subirDOi", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

export const subirDocumentoPorDoiBorrador = async (
  files,
  doi,
  resourceType
) => {
  const formData = new FormData();

  files.forEach((file) => formData.append("archivos", file)); // plural
  formData.append("doi", doi);
  formData.append("resourceType", resourceType);

  try {
    const res = await api.post("/api/zenodo/subirDOi-borradorV2", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
        // Authorization: `Bearer ${token}`,
      },
    });
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};

export const registrarUsuario = async ({ nombreCompleto, email, password }) => {
  try {
    const response = await axios.post("/api/auth/register", {
      nombreCompleto,
      email,
      password,
    });

    return response.data; // por ejemplo: { message: "Usuario registrado con éxito" }
  } catch (error) {
    // Puedes personalizar el error según cómo lo manejes en el backend
    if (error.response && error.response.data) {
      throw new Error(
        error.response.data.message || "Error al registrar usuario"
      );
    }
    throw new Error("Error de red o del servidor.");
  }
};
