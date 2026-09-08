// frontend/src/components/auth/AuthButton.jsx
//
// Primary green CTA button matching the "Sign in" button in the design.
// Use in place of a plain <button>:
//
//   <AuthButton type="submit" disabled={isSubmitting}>
//     {isSubmitting ? "Signing in..." : "Sign in"}
//   </AuthButton>

export default function AuthButton({ children, className = "", ...buttonProps }) {
  return (
    <button
      className={`w-full bg-green-600 hover:bg-green-500 text-white font-semibold py-3.5 rounded-xl transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-lg shadow-green-900/30 ${className}`}
      {...buttonProps}
    >
      {children}
    </button>
  );
}
