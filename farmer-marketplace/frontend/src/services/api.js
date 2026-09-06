// frontend/src/services/api.js

import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.DEV
    ? "/api"
    : import.meta.env.VITE_API_BASE_URL || "http://localhost:5084/api",
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

// Global 401 handling — if the token is invalid/expired, clear it and
// redirect to login rather than leaving the user in a broken state
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const requestUrl = error.config?.url || "";
    const isAuthRequest =
      requestUrl.includes("/auth/login") ||
      requestUrl.includes("/auth/register");

    if (error.response?.status === 401 && !isAuthRequest) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  },
);

export default api;
