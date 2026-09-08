// frontend/src/components/auth/LoginRoleToggle.jsx
//
// The "Sign in as" 2x2 grid from the design (Farmer/FPO, Buyer,
// Platform admin, FPO admin). This is intentionally separate from
// components/common/RoleSelector.jsx (which is used at registration
// with only 3 options) since this one includes Platform Admin and is
// purely a login-screen UI affordance — LoginDto only needs
// email/password, so wire this to local state in Login.jsx however
// you'd like (e.g. just for UX/redirect hinting), not necessarily sent
// to the backend.
//
// Usage:
//   const [signInAs, setSignInAs] = useState(ROLES.FARMER);
//   <LoginRoleToggle value={signInAs} onChange={setSignInAs} />

import { ROLES } from "../../utils/roles";

const OPTIONS = [
  { value: ROLES.FARMER, label: "Farmer / FPO" },
  { value: ROLES.BUYER, label: "Buyer" },
  { value: ROLES.PLATFORM_ADMIN, label: "Platform admin" },
  { value: ROLES.FPO_ADMIN, label: "FPO admin" },
];

export default function LoginRoleToggle({ value, onChange }) {
  return (
    <div className="grid grid-cols-2 gap-3">
      {OPTIONS.map((option) => {
        const isSelected = value === option.value;
        return (
          <button
            key={option.value}
            type="button"
            onClick={() => onChange(option.value)}
            className={`flex items-center gap-2 px-4 py-3 rounded-xl text-sm font-medium border transition-colors ${
              isSelected
                ? "bg-blue-900/70 border-blue-400 text-white"
                : "glass-panel-light border-white/10 text-white/80 hover:border-white/30"
            }`}
          >
            <span
              className={`h-4 w-4 rounded border flex-shrink-0 ${
                isSelected ? "bg-blue-400 border-blue-400" : "border-white/40"
              }`}
            />
            {option.label}
          </button>
        );
      })}
    </div>
  );
}
