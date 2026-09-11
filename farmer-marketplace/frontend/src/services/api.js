// frontend/src/services/api.js

import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "https://farmer-marketplace-api-fh0g.onrender.com/api",
  headers: {
    "Content-Type": "application/json",
  },
});

// Attach JWT automatically to every request, if one exists
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  if (config.data instanceof FormData) {
    delete config.headers["Content-Type"];
    delete config.headers["content-type"];
  }
  return config;
});

// Global 401 handling — only clear token & redirect if session validation explicitly fails (/auth/me)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const requestUrl = error.config?.url || "";
    const isSessionCheck = requestUrl.includes("/auth/me") || requestUrl.includes("/auth/refresh");

    if (error.response?.status === 401 && isSessionCheck) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  },
);

export const getApiBaseUrl = () => {
  if (import.meta.env.DEV) {
    return "";
  }
  const configured = import.meta.env.VITE_API_BASE_URL || "https://farmer-marketplace-api-fh0g.onrender.com/api";
  return configured.replace(/\/api\/?$/, "");
};

export const resolveImageUrl = (url) => {
  if (!url) return "";
  if (url.startsWith("http://") || url.startsWith("https://") || url.startsWith("data:")) {
    return url;
  }
  const base = getApiBaseUrl();
  const path = url.startsWith("/") ? url : `/${url}`;
  return `${base}${path}`;
};

export default api;
