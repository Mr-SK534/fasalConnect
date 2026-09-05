// frontend/src/context/AuthContext.jsx

import { createContext, useState, useEffect } from "react";
import * as authService from "../services/authService";

export const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // On app load, if a token exists, try to restore the session
  useEffect(() => {
    const restoreSession = async () => {
      const storedToken = localStorage.getItem("token");
      if (!storedToken) {
        setIsLoading(false);
        return;
      }
      try {
        const restoredUser = await authService.getMe();
        setUser(restoredUser);
        setToken(storedToken);
      } catch (err) {
        // Token invalid/expired — clear it silently
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        setUser(null);
        setToken(null);
      } finally {
        setIsLoading(false);
      }
    };
    restoreSession();
  }, []);

  const login = async (email, password) => {
    setError(null);
    try {
      const data = await authService.login(email, password);
      localStorage.setItem("token", data.token);
      localStorage.setItem("user", JSON.stringify(data.user));
      setToken(data.token);
      setUser(data.user);
      return data.user;
    } catch (err) {
      const message =
        err.response?.data?.error?.message || "Login failed. Check your credentials.";
      setError(message);
      throw new Error(message);
    }
  };

  const register = async (payload) => {
    setError(null);
    try {
      const data = await authService.register(payload);
      localStorage.setItem("token", data.token);
      localStorage.setItem("user", JSON.stringify(data.user));
      setToken(data.token);
      setUser(data.user);
      return data.user;
    } catch (err) {
      const message =
        err.response?.data?.error?.message || "Registration failed. Please try again.";
      setError(message);
      throw new Error(message);
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{ user, token, isLoading, error, login, register, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}