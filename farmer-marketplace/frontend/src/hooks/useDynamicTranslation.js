// Custom dynamic translation hook removed.
export function useDynamicTranslation() {
  return {
    translateDynamic: (text) => text,
    currentLang: "en",
    changeLanguage: () => {},
  };
}
