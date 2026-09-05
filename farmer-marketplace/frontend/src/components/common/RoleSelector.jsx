// frontend/src/components/common/RoleSelector.jsx

import { ROLE_OPTIONS } from "../../utils/roles";

export default function RoleSelector({ value, onChange }) {
  return (
    <div className="grid grid-cols-3 gap-2">
      {ROLE_OPTIONS.map((option) => (
        <button
          key={option.value}
          type="button"
          onClick={() => onChange(option.value)}
          className={`px-3 py-2 rounded-lg border text-sm font-medium transition-colors ${
            value === option.value
              ? "bg-green-600 text-white border-green-600"
              : "bg-white text-gray-700 border-gray-300 hover:border-green-400"
          }`}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}