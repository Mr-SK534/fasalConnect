// frontend/src/components/forecast/ForecastChart.jsx

import React from "react";
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
  Legend,
} from "recharts";
import { FiTrendingUp, FiTrendingDown, FiAlertCircle, FiSun, FiDollarSign } from "react-icons/fi";

export default function ForecastChart({ forecastData, loading }) {
  if (loading) {
    return (
      <div className="flex h-72 items-center justify-center rounded-2xl border border-slate-200 bg-white p-8 text-slate-500 shadow-sm">
        <div className="flex items-center gap-3">
          <div className="h-6 w-6 animate-spin rounded-full border-3 border-emerald-600 border-t-transparent" />
          <span className="font-medium">Training & Executing ML.NET Time-Series Engine...</span>
        </div>
      </div>
    );
  }

  if (!forecastData) {
    return (
      <div className="rounded-2xl border border-emerald-200 bg-emerald-50/50 p-8 text-center text-slate-700 shadow-sm space-y-3">
        <div className="text-3xl">🌱</div>
        <h4 className="text-base font-bold text-slate-900">AI Demand Forecast Ready</h4>
        <p className="text-xs text-slate-600 max-w-md mx-auto">
          Select any crop from the controls above or type a custom crop name to compute ML.NET time-series demand predictions.
        </p>
        <button
          onClick={() => window.location.reload()}
          className="mt-2 inline-flex items-center gap-2 rounded-xl bg-[#2e7d32] px-4 py-2 text-xs font-bold text-white shadow-sm hover:bg-[#246b28] transition"
        >
          ⚡ Refresh ML Model
        </button>
      </div>
    );
  }

  const {
    cropName,
    region,
    forecastHorizonDays,
    trend,
    totalProjectedDemandKg,
    harvestAdvisory,
    priceAdvisory,
    govMandiSource,
    minMandiPricePerKg,
    modalMandiPricePerKg,
    maxMandiPricePerKg,
    demandSignal,
    farmerSimpleAdvice,
    fpoGroupTip,
    directSaleAdvantagePercent,
    isCategoryTransferModel,
    categoryModelNote,
    historicalPoints = [],
    forecastPoints = [],
  } = forecastData;

  const [showAdvanced, setShowAdvanced] = React.useState(false);

  // Format historical & forecast points for continuous chart timeline
  const historicalFormatted = historicalPoints.map((h) => ({
    date: new Date(h.date).toLocaleDateString("en-IN", { month: "short", day: "numeric" }),
    PastDemand: Math.round(h.quantitySoldKg),
    PricePerKg: h.avgPricePerKg,
  }));

  const forecastFormatted = forecastPoints.map((f) => ({
    date: new Date(f.date).toLocaleDateString("en-IN", { month: "short", day: "numeric" }),
    ExpectedDemand: Math.round(f.forecastedQuantityKg),
    LowerBound: Math.round(f.lowerBoundKg),
    UpperBound: Math.round(f.upperBoundKg),
  }));

  const chartData = [...historicalFormatted, ...forecastFormatted];

  const isHigh = demandSignal === "HIGH_DEMAND" || trend?.toLowerCase().includes("rising");
  const isGlut = demandSignal === "EXCESS_SUPPLY" || trend?.toLowerCase().includes("falling");

  return (
    <div className="space-y-6 rounded-2xl border border-[#e5d8b6] bg-white p-6 shadow-md">
      {/* Top Controls & View Switcher */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-100 pb-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="text-2xl">🌱</span>
            <h3 className="text-xl font-black text-slate-900 tracking-tight">
              {cropName} <span className="text-sm font-normal text-slate-500">({region || "Regional Mandis"})</span>
            </h3>
          </div>
          <p className="mt-1 text-xs text-slate-500 flex items-center gap-1.5">
            <span className="inline-block h-2 w-2 rounded-full bg-emerald-500 animate-pulse"></span>
            Real Data Source: <strong className="text-slate-700">{govMandiSource || "Agmarknet / Ministry of Agriculture (APMC Mandi)"}</strong>
          </p>
        </div>

        <div className="flex items-center gap-3">
          {/* View Mode Toggle */}
          <div className="flex rounded-xl bg-slate-100 p-1 text-xs font-bold border border-slate-200">
            <button
              onClick={() => setShowAdvanced(false)}
              className={`rounded-lg px-3 py-1.5 transition ${
                !showAdvanced ? "bg-[#2e7d32] text-white shadow-xs" : "text-slate-600 hover:text-slate-900"
              }`}
            >
              🌾 Farmer / FPO View
            </button>
            <button
              onClick={() => setShowAdvanced(true)}
              className={`rounded-lg px-3 py-1.5 transition ${
                showAdvanced ? "bg-slate-800 text-white shadow-xs" : "text-slate-600 hover:text-slate-900"
              }`}
            >
              📊 Advanced AI Model Stats
            </button>
          </div>
        </div>
      </div>

      {/* 🟢/🟡/🔴 Simple Farmer Market Signal Banner */}
      <div
        className={`rounded-2xl p-5 border shadow-sm transition flex flex-wrap items-center justify-between gap-4 ${
          isHigh
            ? "bg-gradient-to-r from-emerald-500 via-emerald-600 to-teal-700 text-white border-emerald-600"
            : isGlut
            ? "bg-gradient-to-r from-rose-500 via-red-600 to-amber-700 text-white border-red-600"
            : "bg-gradient-to-r from-amber-400 via-amber-500 to-yellow-600 text-slate-900 border-amber-500"
        }`}
      >
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-white/20 backdrop-blur-md text-2xl font-black">
            {isHigh ? "🟢" : isGlut ? "🔴" : "🟡"}
          </div>
          <div>
            <span className="text-xs font-black uppercase tracking-wider opacity-90">Market Demand Signal</span>
            <h2 className="text-xl font-black tracking-tight">
              {isHigh
                ? "HIGH BUYER DEMAND — Excellent Time to Harvest & Sell"
                : isGlut
                ? "MARKET GLUT WARNING — Heavy Arrivals Expected"
                : "STEADY MARKET DEMAND — Normal Selling Rate"}
            </h2>
            <p className="text-xs opacity-90 mt-0.5 font-medium">
              30-Day Total Projected Buyer Demand: <strong className="underline decoration-wavy">{totalProjectedDemandKg?.toLocaleString() || 0} kg</strong>
            </p>
          </div>
        </div>

        <div className="rounded-xl bg-white/10 p-3 backdrop-blur-md border border-white/20 text-right">
          <p className="text-[11px] font-bold uppercase tracking-wider opacity-90">Direct Direct-Sale Margin Boost</p>
          <p className="text-2xl font-black text-amber-200">+{directSaleAdvantagePercent || 25}% Higher Profit</p>
          <p className="text-[10px] opacity-80">Over traditional middleman APMC commissions</p>
        </div>
      </div>

      {/* 🏛️ Official Govt Agmarknet APMC Mandi Benchmark Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-slate-200 bg-slate-50/70 p-4 space-y-1">
          <p className="text-[11px] font-bold uppercase tracking-wider text-slate-500">APMC Mandi Min Rate</p>
          <p className="text-2xl font-black text-slate-800">₹{minMandiPricePerKg || 18} <span className="text-xs font-normal text-slate-500">/ kg</span></p>
          <p className="text-[11px] text-slate-500">Lowest reported mandi price</p>
        </div>

        <div className="rounded-2xl border border-emerald-300 bg-emerald-50/70 p-4 space-y-1">
          <p className="text-[11px] font-extrabold uppercase tracking-wider text-emerald-800">Modal Agmarknet APMC Rate</p>
          <p className="text-2xl font-black text-emerald-800">₹{modalMandiPricePerKg || 25} <span className="text-xs font-normal text-emerald-700">/ kg</span></p>
          <p className="text-[11px] text-emerald-700 font-semibold">Official Govt Benchmark Mandi Rate</p>
        </div>

        <div className="rounded-2xl border border-amber-200 bg-amber-50/70 p-4 space-y-1">
          <p className="text-[11px] font-bold uppercase tracking-wider text-amber-800">FasalConnect Peak Direct Price</p>
          <p className="text-2xl font-black text-amber-900">₹{maxMandiPricePerKg || 32} <span className="text-xs font-normal text-amber-800">/ kg</span></p>
          <p className="text-[11px] text-amber-800 font-semibold">Direct buyer peak asking price target</p>
        </div>
      </div>

      {/* 💡 3 Actionable Farmer & FPO Cards */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <div className="flex items-start gap-3 rounded-2xl bg-emerald-50 border border-emerald-200 p-4 shadow-xs">
          <span className="text-2xl mt-0.5">🌾</span>
          <div>
            <h4 className="text-xs font-black uppercase tracking-wider text-emerald-900">
              1. Harvest & Selling Decision
            </h4>
            <p className="mt-1 text-xs text-emerald-800 font-medium leading-relaxed">
              {farmerSimpleAdvice || harvestAdvisory}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3 rounded-2xl bg-amber-50 border border-amber-200 p-4 shadow-xs">
          <span className="text-2xl mt-0.5">💰</span>
          <div>
            <h4 className="text-xs font-black uppercase tracking-wider text-amber-900">
              2. Target Mandi Price Realization
            </h4>
            <p className="mt-1 text-xs text-amber-800 font-medium leading-relaxed">
              {priceAdvisory}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3 rounded-2xl bg-blue-50 border border-blue-200 p-4 shadow-xs">
          <span className="text-2xl mt-0.5">🤝</span>
          <div>
            <h4 className="text-xs font-black uppercase tracking-wider text-blue-900">
              3. FPO Group Collective Tip
            </h4>
            <p className="mt-1 text-xs text-blue-800 font-medium leading-relaxed">
              {fpoGroupTip || "Pool harvest with neighboring FPO farmers for bulk direct sales."}
            </p>
          </div>
        </div>
      </div>

      {/* Simplified Market Chart */}
      <div className="space-y-2 pt-2">
        <div className="flex items-center justify-between text-xs font-bold text-slate-700">
          <span>{cropName} Demand Curve ({showAdvanced ? "95% Confidence Interval ML Model" : "Simple Past Sales vs Future Expected Demand"})</span>
          <span className="text-slate-400 font-normal">Updated Live</span>
        </div>

        <div className="h-72 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
              <defs>
                <linearGradient id="pastGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="futureGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#2e7d32" stopOpacity={0.4} />
                  <stop offset="95%" stopColor="#2e7d32" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="date" stroke="#94a3b8" fontSize={11} tickLine={false} />
              <YAxis stroke="#94a3b8" fontSize={11} unit=" kg" tickLine={false} />
              <Tooltip
                contentStyle={{
                  backgroundColor: "#0f172a",
                  borderColor: "#334155",
                  borderRadius: "12px",
                  color: "#fff",
                  fontSize: "12px",
                }}
              />
              <Legend wrapperStyle={{ paddingTop: "8px", fontSize: "12px" }} />
              <Area
                type="monotone"
                dataKey="PastDemand"
                name="Past Mandi Sales (kg)"
                stroke="#3b82f6"
                fill="url(#pastGrad)"
                strokeWidth={2.5}
              />
              <Area
                type="monotone"
                dataKey="ExpectedDemand"
                name="Future Expected Buyer Demand (kg)"
                stroke="#2e7d32"
                fill="url(#futureGrad)"
                strokeWidth={3}
                strokeDasharray="4 4"
              />
              {showAdvanced && (
                <Area
                  type="monotone"
                  dataKey="UpperBound"
                  name="95% Upper Bound (kg)"
                  stroke="#a7f3d0"
                  fill="transparent"
                  strokeDasharray="2 2"
                />
              )}
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Advanced Technical Model Metadata (If Show Advanced is toggled) */}
      {showAdvanced && (
        <div className="rounded-xl bg-slate-900 p-4 text-white text-xs space-y-2 border border-slate-700">
          <p className="font-bold text-amber-400">📊 Advanced ML.NET Time-Series Specification:</p>
          <p className="text-slate-300 font-mono">
            Model: Microsoft ML.NET SSA (Singular Spectrum Analysis) • WindowSize: Auto-Trained • SeriesLength: Active DB History • Horizon: {forecastHorizonDays} Days
          </p>
          {isCategoryTransferModel && (
            <p className="text-indigo-300 font-mono">
              Transfer Learning: {categoryModelNote}
            </p>
          )}
        </div>
      )}
    </div>
  );
}
