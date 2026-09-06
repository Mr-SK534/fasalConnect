// frontend/src/pages/Home.jsx
import { useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";

const LANGUAGES = [
  { code: "en", label: "English" },
  { code: "hi", label: "हिन्दी" },
  { code: "mr", label: "मराठी" },
  { code: "bn", label: "বাংলা" },
];

// ─── Feature Keys Config ──────────────────────────────────────────────────
const FEATURES = [
  {
    icon: "🌾",
    titleKey: "home.features.sellPrice.title",
    bodyKey: "home.features.sellPrice.body",
    stat: "+131%",
    statLabelKey: "home.features.sellPrice.statLabel",
  },
  {
    icon: "📦",
    titleKey: "home.features.bulkOrders.title",
    bodyKey: "home.features.bulkOrders.body",
    stat: "3 steps",
    statLabelKey: "home.features.bulkOrders.statLabel",
  },
  {
    icon: "🗺️",
    titleKey: "home.features.cheaperDelivery.title",
    bodyKey: "home.features.cheaperDelivery.body",
    stat: "< 24 hrs",
    statLabelKey: "home.features.cheaperDelivery.statLabel",
  },
  {
    icon: "📈",
    titleKey: "home.features.demandForecast.title",
    bodyKey: "home.features.demandForecast.body",
    stat: "ML-powered",
    statLabelKey: "home.features.demandForecast.statLabel",
  },
];

// ─── Interactive 3D Tilt Hook ────────────────────────────────────────────────
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
function LanguageSelector({ currentLang, onChange }) {
  return (
    <div className="relative group">
      <select
        value={currentLang}
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
  const { t } = useTranslation();
  const cardRef = useRef(null);
  useInteractive3D(cardRef, 25);

  return (
    <div className="relative group perspective-1000 py-6">
      <div className="absolute -inset-1 bg-gradient-to-r from-emerald-500 to-amber-500 rounded-3xl blur-2xl opacity-30 group-hover:opacity-60 transition duration-500" />

      <div
        ref={cardRef}
        style={{ transformStyle: "preserve-3d", transition: "transform 0.1s ease-out" }}
        className="relative w-full max-w-sm mx-auto rounded-3xl bg-gradient-to-b from-slate-900/90 via-emerald-950/80 to-slate-900/90 border border-emerald-500/30 backdrop-blur-xl p-6 shadow-[0_20px_50px_rgba(0,0,0,0.8)]"
      >
        <div
          className="pointer-events-none absolute inset-0 rounded-3xl transition-opacity duration-300 opacity-0 group-hover:opacity-100"
          style={{
            background: "radial-gradient(400px circle at var(--glow-x, 50%) var(--glow-y, 50%), rgba(255,255,255,0.12), transparent 40%)",
          }}
        />

        <div 
          style={{ transform: "translateZ(35px)" }} 
          className="absolute -top-3 -right-3 bg-gradient-to-r from-amber-400 to-yellow-500 text-slate-950 text-xs font-black px-3.5 py-1.5 rounded-full shadow-lg shadow-amber-500/20 uppercase tracking-wider ring-2 ring-amber-300/50"
        >
          {t("home.badgeNoMiddlemen", "No Middlemen")}
        </div>

        <div style={{ transform: "translateZ(15px)" }} className="flex items-center justify-between mb-4">
          <span className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-bold tracking-wider uppercase">
            <span className="w-2 h-2 rounded-full bg-emerald-400 animate-ping" />
            {t("home.liveListing", "Live Listing")}
          </span>
          <span className="text-xs text-slate-400 font-mono">ID: #FC-8902</span>
        </div>

        <div style={{ transform: "translateZ(25px)" }} className="flex items-center gap-4 my-6 p-3 rounded-2xl bg-slate-800/40 border border-white/5">
          <div className="h-16 w-16 rounded-2xl bg-emerald-500/20 border border-emerald-400/30 flex items-center justify-center text-4xl shadow-inner">
            🍅
          </div>
          <div>
            <h3 className="text-white font-bold text-lg tracking-tight">{t("home.sampleCrop", "Grade A Tomatoes")}</h3>
            <p className="text-emerald-400 text-sm font-medium">Ramesh Kumar</p>
            <p className="text-slate-400 text-xs">Nashik, Maharashtra</p>
          </div>
        </div>

        <div style={{ transform: "translateZ(30px)" }} className="flex items-end justify-between pt-2">
          <div>
            <div className="text-3xl font-black text-white tracking-tight">
              ₹18<span className="text-sm font-normal text-slate-400">/kg</span>
            </div>
            <div className="text-xs font-medium text-emerald-400 mt-0.5">{t("home.sampleAvailable", "420 kg batch available")}</div>
          </div>
          <button className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-semibold px-5 py-2.5 rounded-xl shadow-lg shadow-emerald-900/50 transition-all transform hover:scale-105 active:scale-95">
            {t("home.buyBatch", "Buy Batch")}
          </button>
        </div>
      </div>
    </div>
  );
}

function FeatureCard3D({ icon, titleKey, bodyKey, stat, statLabelKey }) {
  const { t } = useTranslation();
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
          {t(titleKey)}
        </h3>
        <p style={{ transform: "translateZ(10px)" }} className="text-slate-400 text-sm leading-relaxed mb-6">
          {t(bodyKey)}
        </p>
      </div>

      <div style={{ transform: "translateZ(25px)" }} className="pt-4 border-t border-white/10">
        <div className="text-amber-400 font-extrabold text-2xl tracking-tight">{stat}</div>
        <div className="text-slate-400 text-xs font-medium uppercase tracking-wider mt-0.5">{t(statLabelKey)}</div>
      </div>
    </div>
  );
}

// ─── Main Page ───────────────────────────────────────────────────────────────
export default function Home() {
  const { t, i18n } = useTranslation();

  const handleLangChange = (code) => {
    i18n.changeLanguage(code);
  };

  return (
    <div className="min-h-screen bg-[#060D06] text-white font-sans selection:bg-emerald-500 selection:text-black overflow-x-hidden">
      {/* Background Matrix Grid */}
      <div className="fixed inset-0 pointer-events-none z-0 opacity-20">
        <div className="absolute inset-0 bg-[linear-gradient(to_right,#0f291e_1px,transparent_1px),linear-gradient(to_bottom,#0f291e_1px,transparent_1px)] bg-[size:4rem_4rem] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_0%,#000_70%,transparent_100%)]" />
      </div>

      {/* Navbar */}
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
            <LanguageSelector currentLang={i18n.language} onChange={handleLangChange} />
            <Link to="/login" className="text-sm font-semibold text-slate-300 hover:text-white transition-colors">
              {t("nav.signIn", "Sign in")}
            </Link>
            <Link
              to="/register"
              className="text-sm bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-bold px-5 py-2.5 rounded-xl shadow-lg shadow-emerald-900/40 transition-all hover:scale-105 active:scale-95"
            >
              {t("nav.getStarted", "Get started")}
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="relative z-10 pt-16 pb-24">
        <div className="absolute top-1/4 left-10 w-96 h-96 bg-emerald-600/15 rounded-full blur-[120px] pointer-events-none" />
        <div className="absolute top-1/3 right-10 w-96 h-96 bg-amber-500/10 rounded-full blur-[120px] pointer-events-none" />

        <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          <div className="lg:col-span-7">
            <div className="inline-flex items-center gap-2 bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-bold px-4 py-1.5 rounded-full mb-6 uppercase tracking-wider backdrop-blur-md">
              <span className="w-2 h-2 rounded-full bg-emerald-400" />
              {t("home.badgeTagline", "Built for Bharat's Agricultural Future")}
            </div>

            <h1 className="text-5xl sm:text-6xl lg:text-7xl font-black text-white leading-[1.1] tracking-tight mb-6">
              {t("home.heroTitlePart1", "Fresh produce.")} <br />
              <span className="bg-gradient-to-r from-amber-300 via-amber-400 to-yellow-500 bg-clip-text text-transparent">
                {t("home.heroTitlePart2", "Fair price.")}
              </span>{" "}
              {t("home.heroTitlePart3", "Zero agents.")}
            </h1>

            <p className="text-slate-400 text-lg sm:text-xl leading-relaxed mb-8 max-w-xl">
              {t("home.heroDescription", "FasalConnect links farmers directly with enterprise buyers and regional markets across India—eliminating extra margins and ensuring transparent transactions.")}
            </p>

            <div className="flex flex-wrap gap-4 mb-12">
              <Link
                to="/register"
                className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-bold px-8 py-4 rounded-2xl shadow-xl shadow-emerald-900/40 transition-all hover:scale-105 active:scale-95 text-base"
              >
                {t("home.btnStartSelling", "Start Selling Today")}
              </Link>
              <Link
                to="/register?role=Buyer"
                className="bg-slate-900/80 border border-emerald-500/30 hover:border-emerald-400/60 text-white font-bold px-8 py-4 rounded-2xl transition-all hover:bg-slate-800/80 backdrop-blur-md text-base"
              >
                {t("home.btnFindProduce", "Find Fresh Produce")}
              </Link>
            </div>

            <div className="pt-6 border-t border-white/10 flex flex-wrap gap-6 text-xs font-semibold text-slate-400 uppercase tracking-wider">
              <span className="flex items-center gap-2">🔒 {t("home.trustRazorpay", "Razorpay Secured")}</span>
              <span className="flex items-center gap-2">🌐 {t("home.trustLanguages", "4 Languages")}</span>
              <span className="flex items-center gap-2">📱 {t("home.trustWhatsApp", "WhatsApp Support")}</span>
            </div>
          </div>

          <div className="lg:col-span-5">
            <Hero3DCard />
          </div>
        </div>
      </section>

      {/* Stats Bar */}
      <section className="relative z-10 border-y border-emerald-500/10 bg-slate-950/60 backdrop-blur-xl">
        <div className="max-w-7xl mx-auto px-6 py-12 grid grid-cols-2 lg:grid-cols-4 gap-8">
          {[
            { n: "+131%", key: "home.stats.income" },
            { n: "₹0", key: "home.stats.fees" },
            { n: "< 24h", key: "home.stats.delivery" },
            { n: "100%", key: "home.stats.payment" },
          ].map(({ n, key }) => (
            <div key={key} className="text-center">
              <div className="text-4xl lg:text-5xl font-black bg-gradient-to-r from-amber-300 to-amber-500 bg-clip-text text-transparent mb-1">
                {n}
              </div>
              <div className="text-slate-400 text-xs font-semibold uppercase tracking-wider max-w-[15ch] mx-auto">
                {t(key)}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* 3D Feature Grid */}
      <section className="relative z-10 max-w-7xl mx-auto px-6 py-28">
        <div className="text-center max-w-2xl mx-auto mb-16">
          <h2 className="text-3xl sm:text-5xl font-black text-white tracking-tight mb-4">
            {t("home.featuresHeading", "Built for Modern Agritech")}
          </h2>
          <p className="text-slate-400 text-base sm:text-lg">
            {t("home.featuresSubheading", "Engineered specifically to solve aggregation, pricing transparency, and regional logistics challenges across India.")}
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {FEATURES.map((f) => (
            <FeatureCard3D key={f.titleKey} {...f} />
          ))}
        </div>
      </section>

      {/* How it Works Section */}
      <section className="relative z-10 border-t border-emerald-500/10 bg-slate-950/40 backdrop-blur-md py-24">
        <div className="max-w-7xl mx-auto px-6">
          <h2 className="text-3xl sm:text-4xl font-black text-white text-center mb-16 tracking-tight">
            {t("home.howItWorksHeading", "How FasalConnect Works")}
          </h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="bg-slate-900/40 border border-emerald-500/20 rounded-3xl p-8 backdrop-blur-xl group hover:border-emerald-500/40 transition-all duration-300">
              <div className="text-xs font-black uppercase tracking-widest text-amber-400 bg-amber-400/10 border border-amber-400/20 inline-block px-3 py-1 rounded-full mb-6">
                {t("home.roleFarmer", "For Farmer / FPO")}
              </div>
              <ol className="space-y-6">
                {[1, 2, 3].map((stepNum) => (
                  <li key={stepNum} className="flex items-start gap-4">
                    <span className="flex-shrink-0 w-8 h-8 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 font-bold text-sm flex items-center justify-center">
                      {stepNum}
                    </span>
                    <span className="text-slate-300 font-medium text-sm leading-relaxed pt-1">
                      {t(`home.farmerStep${stepNum}`)}
                    </span>
                  </li>
                ))}
              </ol>
            </div>

            <div className="bg-slate-900/40 border border-emerald-500/20 rounded-3xl p-8 backdrop-blur-xl group hover:border-emerald-500/40 transition-all duration-300">
              <div className="text-xs font-black uppercase tracking-widest text-amber-400 bg-amber-400/10 border border-amber-400/20 inline-block px-3 py-1 rounded-full mb-6">
                {t("home.roleBuyer", "For Buyer")}
              </div>
              <ol className="space-y-6">
                {[1, 2, 3].map((stepNum) => (
                  <li key={stepNum} className="flex items-start gap-4">
                    <span className="flex-shrink-0 w-8 h-8 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 font-bold text-sm flex items-center justify-center">
                      {stepNum}
                    </span>
                    <span className="text-slate-300 font-medium text-sm leading-relaxed pt-1">
                      {t(`home.buyerStep${stepNum}`)}
                    </span>
                  </li>
                ))}
              </ol>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="relative z-10 max-w-7xl mx-auto px-6 py-24">
        <div className="relative rounded-3xl bg-gradient-to-r from-emerald-950/80 via-slate-900 to-emerald-950/80 border border-emerald-500/30 p-12 lg:p-16 text-center overflow-hidden shadow-2xl">
          <div className="absolute -top-24 -left-24 w-72 h-72 bg-emerald-500/20 rounded-full blur-3xl pointer-events-none" />
          
          <h2 className="text-3xl sm:text-5xl font-black text-white mb-4 tracking-tight">
            {t("home.ctaHeading", "Ready to Take Direct Control?")}
          </h2>
          <p className="text-slate-400 text-base sm:text-lg mb-8 max-w-xl mx-auto">
            {t("home.ctaSubheading", "Join thousands of independent farmers, FPOs, and business buyers streamlining agri-trade today.")}
          </p>

          <div className="flex flex-wrap gap-4 justify-center">
            <Link
              to="/register"
              className="bg-gradient-to-r from-emerald-500 to-emerald-600 hover:from-emerald-400 hover:to-emerald-500 text-white font-bold px-8 py-4 rounded-2xl shadow-xl shadow-emerald-900/50 transition-all hover:scale-105 active:scale-95"
            >
              {t("home.btnRegister", "Register Free")}
            </Link>
            <Link
              to="/login"
              className="bg-slate-900/80 border border-white/20 hover:border-white/40 text-white font-bold px-8 py-4 rounded-2xl transition-all"
            >
              {t("nav.signIn", "Sign In")}
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="relative z-10 border-t border-white/10 px-6 py-8 bg-slate-950">
        <div className="max-w-7xl mx-auto flex flex-col md:flex-row items-center justify-between gap-4 text-slate-500 text-xs font-medium">
          <span>© 2026 FasalConnect — Team Zenith, SIH</span>
          <div className="flex gap-6">
            <a href="https://wa.me/911234567890" target="_blank" rel="noopener noreferrer" className="hover:text-slate-300 transition-colors">
              {t("footer.whatsApp", "WhatsApp Support")}
            </a>
            <Link to="/login" className="hover:text-slate-300 transition-colors">{t("nav.signIn", "Sign in")}</Link>
            <Link to="/register" className="hover:text-slate-300 transition-colors">{t("home.btnRegister", "Register")}</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}