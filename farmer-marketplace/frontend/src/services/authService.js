// frontend/src/services/authService.js

import api from "./api";

/**
 * Matches backend LoginDto: { email, password }
 * Returns AuthResponseDto: { token, user: { id, name, email, role, ... } }
 */
export const login = async (email, password) => {
  const response = await api.post("/auth/login", { email, password });
  return response.data;
};

/**
 * Matches backend RegisterDto:
 * { name, email, password, role, phone?, location?, preferredLanguage?, fpoId? }
 * Role must be exactly one of: "Farmer" | "Buyer" | "FpoAdmin" | "PlatformAdmin"
 * (matches the C# UserRole enum in Models/User.cs — note it's "PlatformAdmin",
 * not "Admin")
 */
export const register = async (payload) => {
  const response = await api.post("/auth/register", payload);
  return response.data;
};

/**
 * Restores the session on page refresh using the stored token.
 * Returns UserResponseDto.
 */
export const getMe = async () => {
  const response = await api.get("/auth/me");
  return response.data;
};