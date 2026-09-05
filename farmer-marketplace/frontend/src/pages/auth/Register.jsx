// frontend/src/pages/auth/Register.jsx

import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../hooks/useAuth";
import RoleSelector from "../../components/common/RoleSelector";
import { ROLES, DASHBOARD_PATH_BY_ROLE } from "../../utils/roles";

export default function Register() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();

  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [role, setRole] = useState(ROLES.FARMER);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);
    try {
      const payload = { name, phone, password, role };
      if (email.trim()) {
        payload.email = email;
      }
      const user = await register(payload);
      const redirectPath = DASHBOARD_PATH_BY_ROLE[user.role] || "/";
      navigate(redirectPath);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const changeLanguage = (lng) => {
    i18n.changeLanguage(lng);
  };

  return (
    <div
      className="min-h-screen w-full bg-cover bg-center relative flex items-center justify-center px-4 py-8"
      style={{ backgroundImage: `url('/hero.jpg')` }}
    >
      {/* Glassmorphism card - no overlay on background */}
      <div className="relative z-10 w-full max-w-md bg-transparent backdrop-blur-none p-8 rounded-2xl shadow-2xl border border-white/5">
        {/* Language Switcher */}
        <div className="flex justify-end mb-4">
          <select
            value={i18n.language}
            onChange={(e) => changeLanguage(e.target.value)}
            className="bg-white/20 border border-white/30 text-white rounded-lg px-3 py-1 text-sm backdrop-blur-sm focus:outline-none focus:ring-2 focus:ring-white/50"
          >
            <option value="en" className="text-gray-900">English</option>
            <option value="hi" className="text-gray-900">हिन्दी</option>
            <option value="bn" className="text-gray-900">বাংলা</option>
            <option value="mr" className="text-gray-900">मराठी</option>
          </select>
        </div>

        <div className="text-center mb-6">
          <div className="text-4xl mb-2">🌿</div>
          <h1 className="text-3xl font-bold text-white drop-shadow-lg">
            {t('register.heading')}
          </h1>
          <p className="text-white/90 text-sm mt-1 drop-shadow-lg">
            {t('register.subtitle')}
          </p>
        </div>

        {error && (
          <div className="mb-4 text-sm text-red-100 bg-red-600/80 backdrop-blur-sm border border-red-400/50 rounded-lg px-3 py-2">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <RoleSelector value={role} onChange={setRole} />
          </div>

          <div>
            <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
              {t('register.fullName')}
            </label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400 shadow-lg"
              placeholder={t('register.namePlaceholder')}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
              {t('register.phoneNumber')}
            </label>
            <input
              type="tel"
              required
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400 shadow-lg"
              placeholder={t('register.phonePlaceholder')}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
              {t('register.emailOptional')}
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400 shadow-lg"
              placeholder={t('register.emailPlaceholder')}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
              {t('register.password')}
            </label>
            <div style={{ position: "relative" }}>
              <input
                type={showPassword ? "text" : "password"}
                required
                minLength={6}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-4 py-3 pr-12 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400 shadow-lg"
                placeholder={t('register.passwordPlaceholder')}
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-600 hover:text-gray-800 focus:outline-none"
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? (
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88" />
                  </svg>
                ) : (
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
                    <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                )}
              </button>
            </div>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-green-600 text-white py-3 rounded-lg font-bold hover:bg-green-700 disabled:opacity-50 shadow-xl transition transform hover:scale-[1.02]"
          >
            {isSubmitting ? t('register.registeringButton') : t('register.registerButton')}
          </button>
        </form>

        <p className="text-sm text-white font-bold mt-6 text-center drop-shadow">
          {t('register.alreadyHaveAccount')}{" "}
          <Link to="/login" className="text-yellow-300 font-extrabold hover:underline">
            {t('register.loginLink')}
          </Link>
        </p>
      </div>
    </div>
  );
}
