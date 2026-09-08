// frontend/src/pages/forecast/DemandForecastPage.jsx

import React, { useState, useEffect } from "react";
import { useAuth } from "../../hooks/useAuth";
import { ROLES } from "../../utils/roles";
import {
  getCropForecast,
  getFarmerForecast,
  getFarmersList,
  getTopCropsSummary,
  getAvailableCrops,
} from "../../services/forecastService";
import ForecastChart from "../../components/forecast/ForecastChart";
import {
  FiTrendingUp,
  FiUser,
  FiGrid,
  FiSearch,
  FiCalendar,
  FiLayers,
  FiPlus,
  FiCheckCircle,
} from "react-icons/fi";

export default function DemandForecastPage() {
  const { user } = useAuth();
  const isAdmin =
    user?.role === ROLES.SUPER_ADMIN ||
    user?.role === ROLES.PLATFORM_ADMIN ||
    user?.role === ROLES.ADMIN ||
    user?.role === ROLES.MANAGER;

  const isFpo = user?.role === ROLES.FPO_ADMIN;

  // Active tab: 'crop' or 'farmer'
  const [activeTab, setActiveTab] = useState(isAdmin ? "farmer" : "crop");

  // Dynamic Crops List from API
  const [availableCrops, setAvailableCrops] = useState([
    "Red Onion", "Tomato", "Potato", "Wheat", "Rice", "Soybean", "Cotton",
    "Garlic", "Chilli", "Maize", "Sugarcane", "Mango", "Ginger", "Turmeric", "Mustard"
  ]);

  // Crop Forecast states
  const [selectedCrop, setSelectedCrop] = useState("Red Onion");
  const [customCropInput, setCustomCropInput] = useState("");
  const [selectedRegion, setSelectedRegion] = useState("");
  const [horizonDays, setHorizonDays] = useState(30);
  const [cropForecast, setCropForecast] = useState(null);
  const [loadingCrop, setLoadingCrop] = useState(true);

  // Farmer Selector states
  const [farmersList, setFarmersList] = useState([]);
  const [selectedFarmerId, setSelectedFarmerId] = useState("");
  const [farmerSearch, setFarmerSearch] = useState("");
  const [farmerForecast, setFarmerForecast] = useState(null);
  const [loadingFarmer, setLoadingFarmer] = useState(false);

  // Top Crops Summary Table
  const [cropsSummary, setCropsSummary] = useState([]);
  const [loadingSummary, setLoadingSummary] = useState(true);

  // Load dynamically available crops from database
  useEffect(() => {
    getAvailableCrops()
      .then((crops) => {
        if (crops && crops.length > 0) {
          setAvailableCrops(crops);
        }
      })
      .catch(() => {});
  }, []);

  // Load Crop Forecast
  useEffect(() => {
    if (!selectedCrop) return;
    setLoadingCrop(true);
    getCropForecast(selectedCrop, selectedRegion, horizonDays)
      .then((data) => {
        setCropForecast(data);
        setLoadingCrop(false);
      })
      .catch((err) => {
        console.error("Failed to load crop forecast:", err);
        setLoadingCrop(false);
      });
  }, [selectedCrop, selectedRegion, horizonDays]);

  // Load Farmers List for Admin / SuperAdmin / FPO
  useEffect(() => {
    if (isAdmin || isFpo) {
      getFarmersList(farmerSearch)
        .then((data) => {
          setFarmersList(data || []);
          if (data && data.length > 0 && !selectedFarmerId) {
            setSelectedFarmerId(data[0].id);
          }
        })
        .catch((err) => console.error("Failed to load farmers list:", err));
    }
  }, [isAdmin, isFpo, farmerSearch]);

  // Load Farmer-Specific Forecast
  useEffect(() => {
    if (selectedFarmerId && (isAdmin || isFpo || activeTab === "farmer")) {
      setLoadingFarmer(true);
      getFarmerForecast(selectedFarmerId, horizonDays)
        .then((data) => {
          setFarmerForecast(data);
          setLoadingFarmer(false);
        })
        .catch((err) => {
          console.error("Failed to load farmer forecast:", err);
          setLoadingFarmer(false);
        });
    }
  }, [selectedFarmerId, horizonDays, isAdmin, isFpo, activeTab]);

  // Load Top Crops Summary
  useEffect(() => {
    getTopCropsSummary(14)
      .then((data) => {
        setCropsSummary(data || []);
        setLoadingSummary(false);
      })
      .catch(() => setLoadingSummary(false));
  }, []);

  // Handle custom crop submission
  const handleCustomCropSubmit = (e) => {
    e.preventDefault();
    if (customCropInput.trim()) {
      const crop = customCropInput.trim();
      if (!availableCrops.includes(crop)) {
        setAvailableCrops([crop, ...availableCrops]);
      }
      setSelectedCrop(crop);
      setCustomCropInput("");
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-8">
      {/* Hero Banner */}
      <div className="relative overflow-hidden rounded-3xl bg-gradient-to-r from-[#1b5e20] via-[#2e7d32] to-[#388e3c] p-8 text-white shadow-xl">
        <div className="relative z-10 max-w-3xl">
          <div className="flex items-center gap-3">
            <span className="rounded-xl bg-white/20 p-2.5 backdrop-blur-md">
              <FiTrendingUp size={28} className="text-[#f5d77f]" />
            </span>
            <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold uppercase tracking-wider text-[#f5d77f]">
              Universal Multi-Crop AI Forecast Engine
            </span>
          </div>
          <h1 className="mt-4 text-3xl font-black tracking-tight sm:text-4xl">
            Agricultural Demand & Multi-Crop Intelligence
          </h1>
          <p className="mt-2 text-sm font-medium text-emerald-100 leading-relaxed">
            Predict demand for <b>ANY crop</b> (Garlic, Chilli, Sugarcane, Mango, Ginger, Wheat, Rice, etc.). Powered by ML.NET SSA time-series modeling & Category Transfer Learning.
          </p>
        </div>
      </div>

      {/* Role View Switcher Tabs (For Admin / SuperAdmin / FPO) */}
      {(isAdmin || isFpo) && (
        <div className="flex flex-wrap items-center justify-between gap-4 border-b border-[#e5d8b6] pb-2">
          <div className="flex gap-2">
            <button
              onClick={() => setActiveTab("farmer")}
              className={`flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-bold transition-all ${
                activeTab === "farmer"
                  ? "bg-[#2e7d32] text-white shadow-md"
                  : "bg-white text-slate-700 hover:bg-slate-100"
              }`}
            >
              <FiUser size={18} />
              <span>Farmer-Specific Forecast (Admin View)</span>
            </button>

            <button
              onClick={() => setActiveTab("crop")}
              className={`flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-bold transition-all ${
                activeTab === "crop"
                  ? "bg-[#2e7d32] text-white shadow-md"
                  : "bg-white text-slate-700 hover:bg-slate-100"
              }`}
            >
              <FiGrid size={18} />
              <span>Universal Crop Analytics</span>
            </button>
          </div>

          <div className="text-xs font-bold text-[#2e7d32]">
            Role Scope: {user?.role || "Administrator"} Access Enabled
          </div>
        </div>
      )}

      {/* SECTION: Farmer-Specific Forecast (SuperAdmin / Admin / FPO View) */}
      {(isAdmin || isFpo) && activeTab === "farmer" && (
        <div className="space-y-6">
          {/* Farmer Dropdown Selection Control */}
          <div className="rounded-2xl border border-[#e5d8b6] bg-white p-6 shadow-sm space-y-4">
            <div className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <h3 className="text-lg font-black text-slate-900 flex items-center gap-2">
                  <FiUser className="text-[#2e7d32]" /> Select Farmer to Inspect Personal Forecast
                </h3>
                <p className="text-xs text-slate-500">
                  SuperAdmin & Admin privilege: View crop yield projections and 30-day demand curves for any registered farmer's actual listed produce.
                </p>
              </div>

              {/* Horizon Selector */}
              <div className="flex items-center gap-2 bg-slate-100 p-1.5 rounded-xl text-xs font-bold">
                <FiCalendar className="text-slate-500" />
                <span>Horizon:</span>
                {[7, 14, 30, 60].map((d) => (
                  <button
                    key={d}
                    onClick={() => setHorizonDays(d)}
                    className={`rounded-lg px-2.5 py-1 transition ${
                      horizonDays === d ? "bg-[#2e7d32] text-white" : "text-slate-600 hover:bg-slate-200"
                    }`}
                  >
                    {d} Days
                  </button>
                ))}
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {/* Farmer Dropdown Select */}
              <div className="md:col-span-2">
                <label className="mb-1 block text-xs font-extrabold uppercase tracking-wider text-slate-700">
                  Select Registered Farmer
                </label>
                <select
                  value={selectedFarmerId}
                  onChange={(e) => setSelectedFarmerId(e.target.value)}
                  className="w-full rounded-xl border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 shadow-xs focus:border-[#2e7d32] focus:outline-hidden"
                >
                  {farmersList.map((f) => (
                    <option key={f.id} value={f.id}>
                      {f.name} — {f.location} ({f.primaryCrops})
                    </option>
                  ))}
                </select>
              </div>

              {/* Quick Search Input */}
              <div>
                <label className="mb-1 block text-xs font-extrabold uppercase tracking-wider text-slate-700">
                  Filter by Name / District
                </label>
                <div className="relative">
                  <FiSearch className="absolute left-3.5 top-3 text-slate-400" />
                  <input
                    type="text"
                    placeholder="Search farmer..."
                    value={farmerSearch}
                    onChange={(e) => setFarmerSearch(e.target.value)}
                    className="w-full rounded-xl border border-slate-300 bg-white pl-10 pr-4 py-2 text-sm font-medium text-slate-800 shadow-xs focus:border-[#2e7d32] focus:outline-hidden"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Display Farmer Forecast Data */}
          {loadingFarmer ? (
            <div className="p-12 text-center text-slate-500">Loading farmer forecast model...</div>
          ) : farmerForecast ? (
            <div className="space-y-6">
              {/* Farmer Profile Summary Card */}
              <div className="rounded-2xl border border-emerald-200 bg-gradient-to-br from-emerald-50 to-teal-50 p-6 shadow-sm flex flex-wrap items-center justify-between gap-4">
                <div>
                  <span className="text-xs font-extrabold uppercase tracking-wider text-emerald-800">
                    Farmer Forecast Profile
                  </span>
                  <h2 className="text-2xl font-black text-slate-900 mt-1">
                    {farmerForecast.farmerName}
                  </h2>
                  <p className="text-xs font-bold text-slate-600">
                    Location: {farmerForecast.location} • Crops Grown: {farmerForecast.primaryCrops?.join(", ")}
                  </p>
                </div>

                <div className="rounded-xl bg-white p-4 shadow-xs border border-emerald-200 text-right">
                  <p className="text-xs font-bold uppercase text-slate-500">Combined 30-Day Demand</p>
                  <p className="text-2xl font-black text-[#1b5e20]">
                    {farmerForecast.combined30DayDemandKg?.toLocaleString()} kg
                  </p>
                </div>
              </div>

              {/* Render charts for each crop grown by this farmer */}
              {farmerForecast.cropForecasts?.map((cForecast, idx) => (
                <ForecastChart key={idx} forecastData={cForecast} loading={false} />
              ))}
            </div>
          ) : (
            <div className="p-8 text-center text-slate-500">Select a farmer above to display demand predictions.</div>
          )}
        </div>
      )}

      {/* SECTION: Universal Crop Demand Analytics View (Farmer / All Roles) */}
      {(activeTab === "crop" || (!isAdmin && !isFpo)) && (
        <div className="space-y-6">
          {/* Universal Crop Search & Custom Type-In Bar */}
          <div className="rounded-2xl border border-[#e5d8b6] bg-white p-6 shadow-sm space-y-4">
            <div className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <h3 className="text-lg font-black text-slate-900 flex items-center gap-2">
                  <FiGrid className="text-[#2e7d32]" /> Universal Crop Search & Custom Forecast
                </h3>
                <p className="text-xs text-slate-500">
                  Select from active marketplace crops or type <b>ANY custom crop name</b> (e.g. Garlic, Chilli, Ginger, Sugarcane, Mango, Turmeric).
                </p>
              </div>

              {/* Horizon Selector */}
              <div className="flex items-center gap-2 bg-slate-100 p-1.5 rounded-xl text-xs font-bold">
                <FiCalendar className="text-slate-500" />
                <span>Horizon:</span>
                {[7, 14, 30, 60].map((d) => (
                  <button
                    key={d}
                    onClick={() => setHorizonDays(d)}
                    className={`rounded-lg px-2.5 py-1 transition ${
                      horizonDays === d ? "bg-[#2e7d32] text-white" : "text-slate-600 hover:bg-slate-200"
                    }`}
                  >
                    {d} Days
                  </button>
                ))}
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {/* Dynamic Crop Select Dropdown */}
              <div>
                <label className="mb-1 block text-xs font-extrabold uppercase tracking-wider text-slate-700">
                  Select Available Crop ({availableCrops.length} Registered)
                </label>
                <select
                  value={selectedCrop}
                  onChange={(e) => setSelectedCrop(e.target.value)}
                  className="w-full rounded-xl border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 shadow-xs focus:border-[#2e7d32] focus:outline-hidden"
                >
                  {availableCrops.map((c) => (
                    <option key={c} value={c}>
                      🌱 {c}
                    </option>
                  ))}
                </select>
              </div>

              {/* Type Any Custom Crop Form */}
              <form onSubmit={handleCustomCropSubmit} className="md:col-span-2">
                <label className="mb-1 block text-xs font-extrabold uppercase tracking-wider text-slate-700">
                  Type Custom / Unlisted Crop Name
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    placeholder="Type ANY crop name (e.g. Garlic, Chilli, Ginger, Turmeric, Dragon Fruit)..."
                    value={customCropInput}
                    onChange={(e) => setCustomCropInput(e.target.value)}
                    className="flex-1 rounded-xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 shadow-xs focus:border-[#2e7d32] focus:outline-hidden"
                  />
                  <button
                    type="submit"
                    className="flex items-center gap-1.5 rounded-xl bg-[#2e7d32] px-4 py-2 text-sm font-bold text-white shadow-md hover:bg-[#246b28] transition"
                  >
                    <FiPlus /> Forecast Crop
                  </button>
                </div>
              </form>
            </div>

            {/* Quick Crop Pills */}
            <div>
              <p className="mb-2 text-xs font-bold uppercase tracking-wider text-slate-500">Popular Crops:</p>
              <div className="flex flex-wrap gap-2">
                {["Red Onion", "Tomato", "Potato", "Wheat", "Rice", "Garlic", "Chilli", "Ginger", "Sugarcane", "Mango", "Soybean"].map((crop) => (
                  <button
                    key={crop}
                    onClick={() => setSelectedCrop(crop)}
                    className={`rounded-xl px-3 py-1.5 text-xs font-bold transition-all ${
                      selectedCrop.toLowerCase() === crop.toLowerCase()
                        ? "bg-[#2e7d32] text-white shadow-xs"
                        : "bg-slate-100 text-slate-700 hover:bg-slate-200"
                    }`}
                  >
                    {crop}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Recharts Forecast Component */}
          <ForecastChart forecastData={cropForecast} loading={loadingCrop} />
        </div>
      )}

      {/* SECTION: Platform-Wide Top Demanded Crops Leaderboard */}
      <div className="rounded-2xl border border-[#e5d8b6] bg-white p-6 shadow-md space-y-4">
        <div className="flex items-center justify-between border-b border-slate-100 pb-3">
          <div>
            <h3 className="text-lg font-black text-slate-900 flex items-center gap-2">
              <FiLayers className="text-[#2e7d32]" /> Marketplace Crop Demand Summary (14-Day Projections)
            </h3>
            <p className="text-xs text-slate-500">
              Aggregated AI forecasts across primary agricultural commodities
            </p>
          </div>
        </div>

        {loadingSummary ? (
          <div className="p-6 text-center text-slate-400">Loading demand leaderboard...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm font-medium">
              <thead className="bg-slate-50 text-xs font-extrabold uppercase tracking-wider text-slate-600 border-b border-slate-200">
                <tr>
                  <th className="px-4 py-3">Crop Name</th>
                  <th className="px-4 py-3">Category</th>
                  <th className="px-4 py-3">Current Weekly Demand</th>
                  <th className="px-4 py-3">Projected Weekly Demand</th>
                  <th className="px-4 py-3">Trend</th>
                  <th className="px-4 py-3">Market Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {cropsSummary.map((item, idx) => (
                  <tr key={idx} className="hover:bg-slate-50/80 transition">
                    <td className="px-4 py-3 font-bold text-slate-900 flex items-center gap-2">
                      <span>🌱</span> {item.cropName}
                    </td>
                    <td className="px-4 py-3 text-slate-600">{item.category}</td>
                    <td className="px-4 py-3 font-semibold text-slate-700">
                      {item.currentWeeklyDemandKg?.toLocaleString()} kg
                    </td>
                    <td className="px-4 py-3 font-extrabold text-[#1b5e20]">
                      {item.projectedWeeklyDemandKg?.toLocaleString()} kg
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-bold ${
                          item.trendPercentage >= 0
                            ? "bg-emerald-100 text-emerald-800"
                            : "bg-amber-100 text-amber-800"
                        }`}
                      >
                        {item.trendPercentage >= 0 ? "+" : ""}
                        {item.trendPercentage}%
                      </span>
                    </td>
                    <td className="px-4 py-3 text-xs text-slate-600 font-medium">
                      {item.recommendedAction}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
