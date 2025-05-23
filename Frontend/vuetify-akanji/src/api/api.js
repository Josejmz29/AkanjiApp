// src/api/api.js
import axios from "axios";

const api = axios.create({
  baseURL: "http://localhost:5215",
  headers: {
    "Content-Type": "application/json",
  },
});

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

export const subirDocumentoPorDoiBorrador = async (file, doi) => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("doi", doi);

  try {
    const res = await api.post("/api/zenodo/subirDOi-borrador", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return res.data;
  } catch (error) {
    throw handleError(error);
  }
};
