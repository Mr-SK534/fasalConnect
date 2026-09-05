// frontend/src/components/auth/AuthInput.jsx
//
// Drop-in styled input matching the FarmerConnect auth design.
// Use it exactly like a normal <input> plus a `label` prop — all other
// props (value, onChange, type, required, placeholder...) pass through.
//
//   <AuthInput label="Email or phone" type="email" value={email}
//              onChange={(e) => setEmail(e.target.value)} />

export default function AuthInput({ label, className = "", ...inputProps }) {
  return (
    <div>
      {label && (
        <label className="block text-sm text-white/90 mb-1.5">{label}</label>
      )}
      <input
        className={`glass-panel w-full px-4 py-3 rounded-xl text-white placeholder-white/50 border border-white/10 focus:outline-none focus:ring-2 focus:ring-green-400 focus:border-transparent ${className}`}
        {...inputProps}
      />
    </div>
  );
}
