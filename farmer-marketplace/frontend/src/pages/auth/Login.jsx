// frontend/src/pages/auth/Login.jsx

import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../hooks/useAuth";
import { DASHBOARD_PATH_BY_ROLE } from "../../utils/roles";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);
    try {
      const user = await login(email, password);
      const redirectPath = DASHBOARD_PATH_BY_ROLE[user.role] || "/";
      navigate(redirectPath);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="min-h-screen w-full bg-cover bg-center relative flex items-center justify-center px-4 py-8"
      style={{ backgroundImage: `url('/hero.jpg')` }}
    >
      {/* Glassmorphism card - keeping original backdrop & background opacity intact */}
      <div className="relative z-10 w-full max-w-xl bg-transparent backdrop-blur-none p-8 sm:p-12 rounded-3xl shadow-2xl border border-white/10">
        <div className="mb-6 flex justify-end">
          <select
            value={i18n.language}
            onChange={(e) => {
              i18n.changeLanguage(e.target.value);
              localStorage.setItem("fasalconnect_lang", e.target.value);
            }}
            className="rounded-xl border border-white/40 bg-white/20 px-4 py-2.5 text-lg font-bold text-white backdrop-blur-sm focus:outline-none focus:ring-2 focus:ring-white/50 cursor-pointer"
            aria-label="Select language"
          >
            <option value="en" className="text-gray-900 font-semibold">
              English
            </option>
            <option value="hi" className="text-gray-900 font-semibold">
              हिन्दी
            </option>
            <option value="bn" className="text-gray-900 font-semibold">
              বাংলা
            </option>
            <option value="mr" className="text-gray-900 font-semibold">
              मराठी
            </option>
          </select>
        </div>

        <div className="text-center mb-8">
          <div className="text-6xl mb-3">🌿</div>
          <h1 className="text-5xl font-black text-white drop-shadow-xl tracking-tight">
            {t("login.heading")}
          </h1>
          <p className="text-white/95 text-xl font-semibold mt-3 drop-shadow-md">
            {t("login.subtitle")}
          </p>
        </div>

        {error && (
          <div className="mb-6 text-lg font-bold text-red-100 bg-red-600/80 backdrop-blur-sm border border-red-400/50 rounded-2xl px-5 py-3.5 shadow-md">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-7">
          <div>
            <label className="block text-xl font-black text-white mb-2.5 drop-shadow-md">
              {t("login.emailOrPhone")}
            </label>
            <input
              type="text"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-5 py-4.5 text-2xl font-medium text-slate-900 bg-white/90 backdrop-blur-sm border border-white/40 rounded-2xl focus:outline-none focus:ring-4 focus:ring-green-400/50 shadow-xl placeholder:text-gray-500"
              placeholder={t("login.emailOrPhonePlaceholder")}
            />
          </div>

          <div>
            <label className="block text-xl font-black text-white mb-2.5 drop-shadow-md">
              {t("login.password")}
            </label>
            <div className="relative">
              <input
                type={showPassword ? "text" : "password"}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-5 py-4.5 pr-16 text-2xl font-medium text-slate-900 bg-white/90 backdrop-blur-sm border border-white/40 rounded-2xl focus:outline-none focus:ring-4 focus:ring-green-400/50 shadow-xl placeholder:text-gray-500"
                placeholder="••••••••"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-700 hover:text-gray-900 focus:outline-none p-1"
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? (
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={2}
                    stroke="currentColor"
                    className="w-8 h-8"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88"
                    />
                  </svg>
                ) : (
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={2}
                    stroke="currentColor"
                    className="w-8 h-8"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z"
                    />
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                    />
                  </svg>
                )}
              </button>
            </div>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-green-600 text-white py-4.5 rounded-2xl text-2xl font-black hover:bg-green-700 disabled:opacity-50 shadow-2xl transition transform hover:scale-[1.02] active:scale-[0.98]"
          >
            {isSubmitting ? t("login.signingIn") : t("login.signIn")}
          </button>
        </form>

        <p className="text-xl text-white font-extrabold mt-8 text-center drop-shadow-md">
          {t("login.noAccount")}{" "}
          <Link
            to="/register"
            className="text-yellow-300 font-black hover:underline ml-1"
          >
            {t("login.register")}
          </Link>
        </p>
      </div>
    </div>
  );
}