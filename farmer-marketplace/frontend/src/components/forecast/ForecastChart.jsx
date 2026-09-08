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
    isCategoryTransferModel,
    categoryModelNote,
    historicalPoints = [],
    forecastPoints = [],
  } = forecastData;

  // Combine historical and forecast data points for continuous chart timeline
  const historicalFormatted = historicalPoints.map((h) => ({
    date: new Date(h.date).toLocaleDateString("en-IN", { month: "short", day: "numeric" }),
    HistoricalDemand: Math.round(h.quantitySoldKg),
    PricePerKg: h.avgPricePerKg,
  }));

  const forecastFormatted = forecastPoints.map((f) => ({
    date: new Date(f.date).toLocaleDateString("en-IN", { month: "short", day: "numeric" }),
    PredictedDemand: Math.round(f.forecastedQuantityKg),
    LowerBound: Math.round(f.lowerBoundKg),
    UpperBound: Math.round(f.upperBoundKg),
    ConfidenceRange: [Math.round(f.lowerBoundKg), Math.round(f.upperBoundKg)],
  }));

  const chartData = [...historicalFormatted, ...forecastFormatted];

  const isRising = trend?.toLowerCase().includes("rising");
  const isFalling = trend?.toLowerCase().includes("falling");

  return (
    <div className="space-y-6 rounded-2xl border border-[#e5d8b6] bg-white p-6 shadow-md">
      {/* Header Bar */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-100 pb-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="text-2xl">🌾</span>
            <h3 className="text-xl font-black text-slate-900 tracking-tight">
              {cropName} <span className="text-sm font-normal text-slate-500">({region})</span>
            </h3>
          </div>
          <p className="mt-1 text-xs text-slate-500">
            ML.NET SSA Singular Spectrum Analysis • {forecastHorizonDays}-Day Demand Horizon (95% Confidence Interval)
          </p>
        </div>

        <div className="flex items-center gap-3">
          {/* Trend Badge */}
          <div
            className={`flex items-center gap-1.5 rounded-full px-3.5 py-1.5 text-xs font-extrabold uppercase tracking-wide ${
              isRising
                ? "bg-emerald-100 text-emerald-800 border border-emerald-300"
                : isFalling
                ? "bg-amber-100 text-amber-800 border border-amber-300"
                : "bg-blue-100 text-blue-800 border border-blue-300"
            }`}
          >
            {isRising ? (
              <FiTrendingUp size={16} />
            ) : isFalling ? (
              <FiTrendingDown size={16} />
            ) : (
              <FiAlertCircle size={16} />
            )}
            <span>{trend}</span>
          </div>

          {/* Projected Volume Pill */}
          <div className="rounded-xl bg-[#2e7d32]/10 border border-[#2e7d32]/20 px-3.5 py-1.5 text-xs font-bold text-[#1b5e20]">
            30-Day Demand: <span className="font-extrabold">{totalProjectedDemandKg.toLocaleString()} kg</span>
          </div>
        </div>
      </div>

      {/* AI Transfer Model Banner */}
      {isCategoryTransferModel && (
        <div className="flex items-center gap-3 rounded-xl bg-indigo-50 border border-indigo-200 p-3.5 text-xs text-indigo-900 font-semibold shadow-xs">
          <span className="text-base">🤖</span>
          <div>
            <span className="font-extrabold uppercase tracking-wide text-indigo-800">AI Category Transfer Learning Model Applied</span>
            <p className="mt-0.5 font-medium text-indigo-700">{categoryModelNote}</p>
          </div>
        </div>
      )}

      {/* Main Area Chart */}
      <div className="h-80 w-full pt-2">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={chartData} margin={{ top: 10, right: 20, left: 0, bottom: 0 }}>
            <defs>
              <linearGradient id="historicalGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.4} />
                <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
              </linearGradient>
              <linearGradient id="forecastGrad" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#2e7d32" stopOpacity={0.4} />
                <stop offset="95%" stopColor="#2e7d32" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
            <XAxis dataKey="date" stroke="#64748b" fontSize={11} tickLine={false} />
            <YAxis stroke="#64748b" fontSize={11} unit=" kg" tickLine={false} />
            <Tooltip
              contentStyle={{
                backgroundColor: "#0f172a",
                borderColor: "#334155",
                borderRadius: "12px",
                color: "#fff",
                fontSize: "12px",
              }}
            />
            <Legend wrapperStyle={{ paddingTop: "12px", fontSize: "12px" }} />
            <Area
              type="monotone"
              dataKey="HistoricalDemand"
              name="Historical Sales (kg)"
              stroke="#3b82f6"
              fill="url(#historicalGrad)"
              strokeWidth={2.5}
            />
            <Area
              type="monotone"
              dataKey="PredictedDemand"
              name="AI Predicted Demand (kg)"
              stroke="#2e7d32"
              fill="url(#forecastGrad)"
              strokeWidth={2.5}
              strokeDasharray="4 4"
            />
            <Area
              type="monotone"
              dataKey="UpperBound"
              name="95% Upper Bound (kg)"
              stroke="#a7f3d0"
              fill="transparent"
              strokeDasharray="2 2"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>

      {/* Actionable Advisories */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 pt-2">
        <div className="flex items-start gap-3 rounded-xl bg-emerald-50 border border-emerald-200 p-4">
          <FiSun className="mt-0.5 shrink-0 text-emerald-700" size={20} />
          <div>
            <h4 className="text-xs font-extrabold uppercase tracking-wider text-emerald-900">
              Harvest & Scheduling Advisory
            </h4>
            <p className="mt-1 text-xs text-emerald-800 font-medium leading-relaxed">
              {harvestAdvisory}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3 rounded-xl bg-amber-50 border border-amber-200 p-4">
          <FiDollarSign className="mt-0.5 shrink-0 text-amber-700" size={20} />
          <div>
            <h4 className="text-xs font-extrabold uppercase tracking-wider text-amber-900">
              Optimal Price Realization Window
            </h4>
            <p className="mt-1 text-xs text-amber-800 font-medium leading-relaxed">
              {priceAdvisory}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
