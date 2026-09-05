import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import en from "./i18n/locales/en.json";
import hi from "./i18n/locales/hi.json";
import bn from "./i18n/locales/bn.json";
import mr from "./i18n/locales/mr.json";

i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    hi: { translation: hi },
    bn: { translation: bn },
    mr: { translation: mr },
  },
  lng: localStorage.getItem("fasalconnect_lang") || "en",
  fallbackLng: "en",
  interpolation: { escapeValue: false },
});

export default i18n;
