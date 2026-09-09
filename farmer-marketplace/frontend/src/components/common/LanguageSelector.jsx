// frontend/src/components/common/LanguageSelector.jsx

import React, { useState, useEffect } from "react";
import { FiGlobe, FiChevronDown } from "react-icons/fi";
import i18n from "../../i18n";

const LANGUAGES = [
  { code: "en", label: "English", native: "English", flag: "🇬🇧" },
  { code: "hi", label: "Hindi", native: "हिंदी", flag: "🇮🇳" },
  { code: "mr", label: "Marathi", native: "मराठी", flag: "🚩" },
  { code: "ta", label: "Tamil", native: "தமிழ்", flag: "🏛️" },
  { code: "gu", label: "Gujarati", native: "ગુજરાતી", flag: "🌾" },
];

export default function LanguageSelector({ className = "" }) {
  const [selectedLang, setSelectedLang] = useState(() => {
    return localStorage.getItem("fasalconnect_lang") || localStorage.getItem("i18nextLng") || i18n.language || "en";
  });
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    const activeCode = selectedLang.split("-")[0];
    localStorage.setItem("fasalconnect_lang", activeCode);
    localStorage.setItem("i18nextLng", activeCode);
    if (i18n.language !== activeCode) {
      i18n.changeLanguage(activeCode);
    }
  }, [selectedLang]);

  const currentLangObj = LANGUAGES.find((l) => l.code === selectedLang.split("-")[0]) || LANGUAGES[0];

  const handleSelect = (code) => {
    setSelectedLang(code);
    localStorage.setItem("fasalconnect_lang", code);
    localStorage.setItem("i18nextLng", code);

    // Explicitly notify react-i18next engine to switch language instantly across all pages
    i18n.changeLanguage(code);

    setIsOpen(false);
    window.dispatchEvent(new Event("storage"));
  };

  return (
    <div className={`relative inline-block text-left ${className}`}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 rounded-xl border border-[#e5d8b6] bg-white px-3 py-1.5 text-xs font-bold text-slate-800 shadow-xs hover:bg-slate-50 transition focus:outline-hidden"
        title="Change Platform Language"
      >
        <FiGlobe className="text-[#2e7d32]" size={16} />
        <span className="font-extrabold">{currentLangObj.flag} {currentLangObj.native}</span>
        <FiChevronDown size={14} className={`text-slate-500 transition-transform ${isOpen ? "rotate-180" : ""}`} />
      </button>

      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
          />
          <div className="absolute right-0 z-50 mt-2 w-44 rounded-2xl border border-emerald-200 bg-white p-1.5 shadow-xl ring-1 ring-black/5 animate-in fade-in zoom-in-95 duration-100">
            <div className="px-3 py-1.5 text-[10px] font-extrabold uppercase tracking-wider text-slate-400 border-b border-slate-100 mb-1">
              Select Language / भाषा चुनें
            </div>
            {LANGUAGES.map((lang) => (
              <button
                key={lang.code}
                onClick={() => handleSelect(lang.code)}
                className={`flex w-full items-center justify-between rounded-xl px-3 py-2 text-xs font-bold transition ${
                  selectedLang.startsWith(lang.code)
                    ? "bg-[#2e7d32] text-white shadow-xs"
                    : "text-slate-700 hover:bg-emerald-50 hover:text-[#1b5e20]"
                }`}
              >
                <span className="flex items-center gap-2">
                  <span>{lang.flag}</span>
                  <span>{lang.native}</span>
                </span>
                <span className="text-[10px] opacity-75 font-normal">({lang.label})</span>
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
