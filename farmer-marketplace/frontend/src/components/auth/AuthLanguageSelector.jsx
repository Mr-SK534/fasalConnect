// frontend/src/components/auth/AuthLanguageSelector.jsx
//
// Visual language dropdown matching the design's top-right selector.
// Purely presentational + controlled — wire the value/onChange to your
// LanguageContext whenever that's built; not coupled to it here.
//
//   <AuthLanguageSelector value={lang} onChange={setLang} />

const LANGUAGES = [
  { code: "en", label: "English" },
  { code: "hi", label: "हिन्दी" },
  { code: "mr", label: "मराठी" },
  { code: "kn", label: "ಕನ್ನಡ" },
];

export default function AuthLanguageSelector({ value = "en", onChange }) {
  return (
    <div className="w-full md:w-56 md:ml-auto">
      <select
        value={value}
        onChange={(e) => onChange?.(e.target.value)}
        className="glass-panel w-full appearance-none px-4 py-3 rounded-xl text-white border border-white/10 focus:outline-none focus:ring-2 focus:ring-green-400 cursor-pointer"
      >
        {LANGUAGES.map((lang) => (
          <option key={lang.code} value={lang.code} className="text-black">
            {lang.label}
          </option>
        ))}
      </select>
    </div>
  );
}
