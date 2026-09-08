// frontend/src/components/auth/AuthLayout.jsx
//
// Reusable full-bleed background layout for Login/Register/any auth screen.
// Does NOT contain form logic — just the visual shell. Compose it around
// your existing Login.jsx / Register.jsx content, e.g.:
//
//   <AuthLayout>
//     <BrandPanel />
//     <div className="space-y-4">...your existing form fields...</div>
//   </AuthLayout>
//
// Expects a background image at /hero.jpg in the public/ folder (or pass
// a different path via the `backgroundImage` prop).

export default function AuthLayout({ children, backgroundImage = "/hero.jpg" }) {
  return (
    <div
      className="min-h-screen w-full bg-cover bg-center relative"
      style={{ backgroundImage: `url(${backgroundImage})` }}
    >
      {/* Light overlay for readability */}
      <div className="absolute inset-0 bg-white/60 backdrop-blur-sm" />

      <div className="relative z-10 min-h-screen w-full px-6 py-8 md:px-16 md:py-10">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-10 md:gap-16 items-center max-w-7xl mx-auto">
          {children}
        </div>
      </div>
    </div>
  );
}
