// frontend/src/components/auth/BrandPanel.jsx
//
// Reusable branding block (logo + title + tagline + feature bullets)
// used on the left side of the auth screens (top, on mobile). Drop it
// into AuthLayout alongside your form content.

const FEATURES = [
  "Demand forecasting for farmers",
  "Smart route optimization",
  "Razorpay-powered payments",
];

export default function BrandPanel({ logoSrc = "/logo.png" }) {
  return (
    <div className="text-white">
      <img src={logoSrc} alt="FarmerConnect" className="h-14 w-14 mb-6" />

      <h1 className="brand-title text-5xl md:text-6xl font-bold mb-4">
        FarmerConnect
      </h1>

      <p className="text-lg md:text-xl text-white/90 mb-1">
        Connecting farmers, FPOs, and buyers
      </p>
      <p className="text-sm md:text-base text-white/70 mb-8">
        across India — fresh produce, fair prices.
      </p>

      <ul className="space-y-2 hidden md:block">
        {FEATURES.map((feature) => (
          <li key={feature} className="flex items-center gap-2 text-white/90 text-sm">
            <span className="h-1.5 w-1.5 rounded-full bg-green-400 flex-shrink-0" />
            {feature}
          </li>
        ))}
      </ul>
    </div>
  );
}
