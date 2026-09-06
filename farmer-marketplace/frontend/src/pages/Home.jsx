// frontend/src/pages/Home.jsx
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";

// ─── Config ────────────────────────────────────────────────────────────────
const BG_IMAGE = ""; // e.g. "/hero.jpg" — leave empty for 3D mesh grid background
const ENABLE_I18N = false;

const LANGUAGES = [
  { code: "en", label: "English" },
  { code: "hi", label: "हिन्दी" },
  { code: "mr", label: "मराठी" },
  { code: "bn", label: "বাংলা" },
];

const FEATURES = [
  {
    icon: "🌾",
    title: "Sell at your price, not theirs",
    body: "List your produce directly. No mandi agent, no commission layer. Buyers see your real price — you keep what you earn.",
    stat: "+131%",
    statLabel: "avg. farmer income gain",
  },
  {
    icon: "📦",
    title: "Bulk orders, without the chaos",
    body: "A restaurant needs 200 kg of tomatoes. One farmer has 80 kg. Our aggregator quietly pools nearby FPO members and fulfils the order — you just confirm and get paid.",
    stat: "3 steps",
    statLabel: "list → order → paid",
  },
  {
    icon: "🗺️",
    title: "Routes that make delivery cheaper",
    body: "Our optimizer plans the shortest path across all active pickups in your zone. Less fuel, faster delivery, fresher produce at the buyer's door.",
    stat: "< 24 hrs",
    statLabel: "farm to buyer",
  },
  {
    icon: "📈",
    title: "Know what to grow next season",
    body: "Our demand forecast reads last season's orders and tells you which crops are trending in your region — before you plant, not after.",
    stat: "ML-powered",
    statLabel: "crop demand forecast",
  },
];

const HOW_IT_WORKS = [
  { role: "Farmer / FPO", steps: ["Register & verify profile", "List produce directly with custom pricing", "Fulfill pooled orders and receive same-day payout"] },
  { role: "Buyer", steps: ["Browse direct farm listings nearby", "Order directly with zero agent markups", "Track regional delivery in real-time"] },
];

// ─── Advanced Interactive 3D Tilt Hook ───────────────────────────────────────
function useInteractive3D(ref, intensity = 20) {
  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const handleMouseMove = (e) => {
      const rect = el.getBoundingClientRect();
      const x = (e.clientX - rect.left) / rect.width - 0.5;
      const y = (e.clientY - rect.top) / rect.height - 0.5;

      el.style.transform = `perspective(1000px) rotateX(${y * -intensity}deg) rotateY(${x * intensity}deg) translateZ(10px)`;
      el.style.setProperty("--glow-x", `${(x + 0.5) * 100}%`);
      el.style.setProperty("--glow-y", `${(y + 0.5) * 100}%`);
    };

    const handleMouseLeave = () => {
      el.style.transform = "perspective(1000px) rotateX(0deg) rotateY(0deg) translateZ(0px)";
    };

    el.addEventListener("mousemove", handleMouseMove);
    el.addEventListener("mouseleave", handleMouseLeave);
    return () => {
      el.removeEventListener("mousemove", handleMouseMove);
      el.removeEventListener("mouseleave", handleMouseLeave);
    };
  }, [ref, intensity]);
}

// ─── Sub-Components ─────────────────────────────────────────────────────────
function LanguageSelector({ lang, onChange }) {
  return (
    <div className="relative group">
      <select
        value={lang}
        onChange={(e) => onChange(e.target.value)}
        className="bg-emerald-950/40 border border-emerald-500/30 text-emerald-200 text-sm rounded-xl px-3 py-2 backdrop-blur-md focus:outline-none focus:ring-2 focus:ring-emerald-400 cursor-pointer shadow-lg shadow-black/50 transition-all hover:border-emerald-400/60"
      >
        {LANGUAGES.map((l) => (
          <option key={l.code} value={l.code} className="bg-slate-900 text-white">
            {l.label}
          </option>
        ))}
      </select>
    </div>
  );
}

function Hero3DCard() {
  const cardRef = useRef(null);
  useInteractive3D(cardRef, 25);

  return (
    <div className="relative group perspective-1000 py-6">
      {/* Dynamic 3D Ambient Shadow */}
      <div className="absolute -inset-1 bg-gradient-to-r from-emerald-500 to-amber-500 rounded-3xl blur-2xl opacity-30 group-hover:opacity-60 transition duration-500" />

      {/* Main Card */}
      <div
        ref={cardRef}
        style={{ transformStyle: "preserve-3d", transition: "transform 0.1s ease-out" }}
        className="relative w-full max-w-sm mx-auto rounded-3xl bg-gradient-to-b from-slate-900/90 via-emerald-950/80 to-slate-900/90 border border-emerald-500/30 backdrop-blur-xl p-6 shadow-[0_20px_50px_rgba(0,0,0,0.8)]"
      >
        {/* Specular Lighting Overlay */}
        <div
          className="pointer-events-none absolute inset-0 rounded-3xl transition-opacity duration-300 opacity-0 group-hover:opacity-100"
          style={{
            background: "radial-gradient(400px circle at var(--glow-x, 50%) var(--glow-y, 50%), rgba(255,255,255,0.12), transparent 40%)",
          }}
        />

        {/* Dynamic Badge - Floating Layer */}
        <div 
          style={{ transform: "translateZ(35px)" }} 
          className="absolute -top-3 -right-3 bg-gradient-to-r from-amber-400 to-yellow-500 text-slate-950 text-xs font-black px-3.5 py-1.5 rounded-full shadow-lg shadow-amber-500/20 uppercase tracking-wider ring-2 ring-amber-300/50"
        >
          No Middlemen
        </div>

        {/* Card Header */}
        <div style={{ transform: "translateZ(15px)" }} className="flex items-center justify-between mb-4">
          <span className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-bold tracking-wider uppercase">
            <span className="w-2 h-2 rounded-full bg-emerald-400 animate-ping" />
            Live Listing
          </span>
          <span className="text-xs text-slate-400 font-mono">ID: #FC-8902</span>
        </div>

        {/* Product Details Layer */}
        <div style={{ transform: "translateZ(25px)" }} className="flex items-center gap-4 my-6 p-3 rounded-2xl bg-slate-800/40 border border-white/5">
          <div className="h-16 w-16 rounded-2xl bg-emerald-500/20 border border-emerald-400/30 flex items-center justify-center text-4xl shadow-inner">
            🍅
          </div>
          <div>
            <h3 className="text-white font-bold text-lg tracking-tight">Grade A Tomatoes</h3>
            <p className="text-emerald-400 text-sm font-medium">Ramesh Kumar</p>
            <p className="text-slate-400 text-xs">Nashik, Maharashtra</p>
          </div>
        </div>

        {/* Pricing Layer */}
        <div style={{ transform: "translateZ(30px)" }} className="flex items-end justify-between pt-2">
          <div>
            <div className="text-3xl font-black text-white tracking-tight">
              ₹18<span className="text-sm font-normal text-slate-400">/kg</span>
            </div>
            <div className="text-xs font-medium text-emerald-400 mt-0.5">420 kg batch available</div>
          </div>
          <button className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-semibold px-5 py-2.5 rounded-xl shadow-lg shadow-emerald-900/50 transition-all transform hover:scale-105 active:scale-95">
            Buy Batch
          </button>
        </div>
      </div>
    </div>
  );
}

function FeatureCard3D({ icon, title, body, stat, statLabel }) {
  const cardRef = useRef(null);
  useInteractive3D(cardRef, 15);

  return (
    <div
      ref={cardRef}
      style={{ transformStyle: "preserve-3d", transition: "transform 0.15s ease-out" }}
      className="group relative bg-slate-900/60 border border-emerald-500/20 rounded-3xl p-7 backdrop-blur-lg flex flex-col justify-between shadow-xl hover:border-emerald-400/50 hover:shadow-emerald-900/20 transition-all duration-300"
    >
      <div
        className="pointer-events-none absolute inset-0 rounded-3xl transition-opacity duration-300 opacity-0 group-hover:opacity-100"
        style={{
          background: "radial-gradient(350px circle at var(--glow-x, 50%) var(--glow-y, 50%), rgba(16, 185, 129, 0.1), transparent 50%)",
        }}
      />
      
      <div>
        <div style={{ transform: "translateZ(20px)" }} className="w-14 h-14 rounded-2xl bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-3xl mb-5 shadow-inner">
          {icon}
        </div>
        <h3 style={{ transform: "translateZ(15px)" }} className="text-white font-bold text-lg mb-2">
          {title}
        </h3>
        <p style={{ transform: "translateZ(10px)" }} className="text-slate-400 text-sm leading-relaxed mb-6">
          {body}
        </p>
      </div>

      <div style={{ transform: "translateZ(25px)" }} className="pt-4 border-t border-white/10">
        <div className="text-amber-400 font-extrabold text-2xl tracking-tight">{stat}</div>
        <div className="text-slate-400 text-xs font-medium uppercase tracking-wider mt-0.5">{statLabel}</div>
      </div>
    </div>
  );
}

// ─── Main Page ───────────────────────────────────────────────────────────────
export default function Home() {
  const [lang, setLang] = useState("en");

  const handleLangChange = (code) => {
    setLang(code);
    if (ENABLE_I18N) {
      // i18n handler integration
    }
  };

  return (
    <div className="min-h-screen bg-[#060D06] text-white font-sans selection:bg-emerald-500 selection:text-black overflow-x-hidden">
      {/* ── 3D Grid Background Layer ────────────────────────────────── */}
      <div className="fixed inset-0 pointer-events-none z-0 opacity-20">
        <div className="absolute inset-0 bg-[linear-gradient(to_right,#0f291e_1px,transparent_1px),linear-gradient(to_bottom,#0f291e_1px,transparent_1px)] bg-[size:4rem_4rem] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_0%,#000_70%,transparent_100%)]" />
      </div>

      {/* ── Navbar ─────────────────────────────────────────────────────── */}
      <nav className="sticky top-0 z-50 border-b border-emerald-500/10 bg-[#060D06]/80 backdrop-blur-xl">
        <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="h-9 w-9 rounded-xl bg-gradient-to-tr from-emerald-500 to-amber-400 p-0.5 shadow-lg shadow-emerald-500/20">
              <div className="h-full w-full bg-slate-950 rounded-[10px] flex items-center justify-center font-black text-amber-400">
                F
              </div>
            </div>
            <span className="text-amber-400 font-extrabold text-xl tracking-wider">
              Fasal<span className="text-white">Connect</span>
            </span>
          </div>

          <div className="flex items-center gap-4">
            <LanguageSelector lang={lang} onChange={handleLangChange} />
            <Link to="/login" className="text-sm font-semibold text-slate-300 hover:text-white transition-colors">
              Sign in
            </Link>
            <Link
              to="/register"
              className="text-sm bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-bold px-5 py-2.5 rounded-xl shadow-lg shadow-emerald-900/40 transition-all hover:scale-105 active:scale-95"
            >
              Get started
            </Link>
          </div>
        </div>
      </nav>

      {/* ── Hero Section ───────────────────────────────────────────────── */}
      <section className="relative z-10 pt-16 pb-24">
        {/* Volumetric Glow Spheres */}
        <div className="absolute top-1/4 left-10 w-96 h-96 bg-emerald-600/15 rounded-full blur-[120px] pointer-events-none" />
        <div className="absolute top-1/3 right-10 w-96 h-96 bg-amber-500/10 rounded-full blur-[120px] pointer-events-none" />

        <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          {/* Left Column Content */}
          <div className="lg:col-span-7">
            <div className="inline-flex items-center gap-2 bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-bold px-4 py-1.5 rounded-full mb-6 uppercase tracking-wider backdrop-blur-md">
              <span className="w-2 h-2 rounded-full bg-emerald-400" />
              Built for Bharat's Agricultural Future
            </div>

            <h1 className="text-5xl sm:text-6xl lg:text-7xl font-black text-white leading-[1.1] tracking-tight mb-6">
              Fresh produce. <br />
              <span className="bg-gradient-to-r from-amber-300 via-amber-400 to-yellow-500 bg-clip-text text-transparent">
                Fair price.
              </span>{" "}
              Zero agents.
            </h1>

            <p className="text-slate-400 text-lg sm:text-xl leading-relaxed mb-8 max-w-xl">
              FasalConnect links farmers directly with enterprise buyers and regional markets across India—eliminating extra margins and ensuring transparent transactions.
            </p>

            <div className="flex flex-wrap gap-4 mb-12">
              <Link
                to="/register"
                className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-bold px-8 py-4 rounded-2xl shadow-xl shadow-emerald-900/40 transition-all hover:scale-105 active:scale-95 text-base"
              >
                Start Selling Today
              </Link>
              <Link
                to="/register?role=Buyer"
                className="bg-slate-900/80 border border-emerald-500/30 hover:border-emerald-400/60 text-white font-bold px-8 py-4 rounded-2xl transition-all hover:bg-slate-800/80 backdrop-blur-md text-base"
              >
                Find Fresh Produce
              </Link>
            </div>

            {/* Trust Badges */}
            <div className="pt-6 border-t border-white/10 flex flex-wrap gap-6 text-xs font-semibold text-slate-400 uppercase tracking-wider">
              <span className="flex items-center gap-2">🔒 Razorpay Secured</span>
              <span className="flex items-center gap-2">🌐 4 Languages</span>
              <span className="flex items-center gap-2">📱 WhatsApp Support</span>
            </div>
          </div>

          {/* Right Column Interactive Card */}
          <div className="lg:col-span-5">
            <Hero3DCard />
          </div>
        </div>
      </section>

      {/* ── Floating Stats Strip ──────────────────────────────────────── */}
      <section className="relative z-10 border-y border-emerald-500/10 bg-slate-950/60 backdrop-blur-xl">
        <div className="max-w-7xl mx-auto px-6 py-12 grid grid-cols-2 lg:grid-cols-4 gap-8">
          {[
            { n: "+131%", label: "Income vs Traditional Mandi" },
            { n: "₹0", label: "Hidden Fees or Commissions" },
            { n: "< 24h", label: "Farm to Door Delivery" },
            { n: "100%", label: "Direct Payment Assurance" },
          ].map(({ n, label }) => (
            <div key={label} className="text-center">
              <div className="text-4xl lg:text-5xl font-black bg-gradient-to-r from-amber-300 to-amber-500 bg-clip-text text-transparent mb-1">
                {n}
              </div>
              <div className="text-slate-400 text-xs font-semibold uppercase tracking-wider max-w-[15ch] mx-auto">
                {label}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* ── 3D Feature Grid ───────────────────────────────────────────── */}
      <section className="relative z-10 max-w-7xl mx-auto px-6 py-28">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <h2 className="text-3xl sm:text-5xl font-black text-white tracking-tight mb-4">
            Built for Modern Agritech
          </h2>
          <p className="text-slate-400 text-base sm:text-lg">
            Engineered specifically to solve aggregation, pricing transparency, and regional logistics challenges across India.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {FEATURES.map((f) => (
            <FeatureCard3D key={f.title} {...f} />
          ))}
        </div>
      </section>

      {/* ── Workflow Section ──────────────────────────────────────────── */}
      <section className="relative z-10 border-t border-emerald-500/10 bg-slate-950/40 backdrop-blur-md py-24">
        <div className="max-w-7xl mx-auto px-6">
          <h2 className="text-3xl sm:text-4xl font-black text-white text-center mb-16 tracking-tight">
            How FasalConnect Works
          </h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {HOW_IT_WORKS.map(({ role, steps }) => (
              <div
                key={role}
                className="bg-slate-900/40 border border-emerald-500/20 rounded-3xl p-8 backdrop-blur-xl relative overflow-hidden group hover:border-emerald-500/40 transition-all duration-300"
              >
                <div className="text-xs font-black uppercase tracking-widest text-amber-400 bg-amber-400/10 border border-amber-400/20 inline-block px-3 py-1 rounded-full mb-6">
                  For {role}
                </div>
                <ol className="space-y-6">
                  {steps.map((step, i) => (
                    <li key={step} className="flex items-start gap-4">
                      <span className="flex-shrink-0 w-8 h-8 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 font-bold text-sm flex items-center justify-center">
                        {i + 1}
                      </span>
                      <span className="text-slate-300 font-medium text-sm leading-relaxed pt-1">{step}</span>
                    </li>
                  ))}
                </ol>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── Call to Action Banner ─────────────────────────────────────── */}
      <section className="relative z-10 max-w-7xl mx-auto px-6 py-24">
        <div className="relative rounded-3xl bg-gradient-to-r from-emerald-950/80 via-slate-900 to-emerald-950/80 border border-emerald-500/30 p-12 lg:p-16 text-center overflow-hidden shadow-2xl">
          <div className="absolute -top-24 -left-24 w-72 h-72 bg-emerald-500/20 rounded-full blur-3xl pointer-events-none" />
          
          <h2 className="text-3xl sm:text-5xl font-black text-white mb-4 tracking-tight">
            Ready to Take Direct Control?
          </h2>
          <p className="text-slate-400 text-base sm:text-lg mb-8 max-w-xl mx-auto">
            Join thousands of independent farmers, FPOs, and business buyers streamlining agri-trade today.
          </p>

          <div className="flex flex-wrap gap-4 justify-center">
            <Link
              to="/register"
              className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-bold px-8 py-4 rounded-2xl shadow-xl shadow-emerald-900/50 transition-all hover:scale-105 active:scale-95"
            >
              Register Account
            </Link>
            <Link
              to="/login"
              className="bg-slate-900/80 border border-white/20 hover:border-white/40 text-white font-bold px-8 py-4 rounded-2xl transition-all"
            >
              Sign In
            </Link>
          </div>
        </div>
      </section>

      {/* ── Footer ─────────────────────────────────────────────────────── */}
      <footer className="relative z-10 border-t border-white/10 px-6 py-8 bg-slate-950">
        <div className="max-w-7xl mx-auto flex flex-col md:flex-row items-center justify-between gap-4 text-slate-500 text-xs font-medium">
          <span>© 2026 FasalConnect — Team Zenith, SIH</span>
          <div className="flex gap-6">
            <a href="https://wa.me/911234567890" target="_blank" rel="noopener noreferrer" className="hover:text-slate-300 transition-colors">
              WhatsApp Support
            </a>
            <Link to="/login" className="hover:text-slate-300 transition-colors">Sign in</Link>
            <Link to="/register" className="hover:text-slate-300 transition-colors">Register</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}