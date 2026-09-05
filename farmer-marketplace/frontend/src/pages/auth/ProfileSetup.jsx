// frontend/src/pages/auth/ProfileSetup.jsx

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../hooks/useAuth";
import { ROLES, DASHBOARD_PATH_BY_ROLE } from "../../utils/roles";
import axiosClient from "../../services/api";

const INDIAN_STATES = [
  "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chhattisgarh",
  "Goa", "Gujarat", "Haryana", "Himachal Pradesh", "Jharkhand", "Karnataka",
  "Kerala", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram",
  "Nagaland", "Odisha", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu",
  "Telangana", "Tripura", "Uttar Pradesh", "Uttarakhand", "West Bengal",
  "Andaman and Nicobar Islands", "Chandigarh", "Dadra and Nagar Haveli and Daman and Diu",
  "Delhi", "Jammu and Kashmir", "Ladakh", "Lakshadweep", "Puducherry",
];

export default function ProfileSetup() {
  const { user, updateUser } = useAuth();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();

  // Section A — Location
  const [village, setVillage] = useState("");
  const [district, setDistrict] = useState("");
  const [state, setState] = useState("");
  const [pincode, setPincode] = useState("");
  const [latitude, setLatitude] = useState(null);
  const [longitude, setLongitude] = useState(null);
  const [detecting, setDetecting] = useState(false);

  // Section B — Farmer/FPO
  const [crops, setCrops] = useState([]);
  const [cropInput, setCropInput] = useState("");
  const [bankAccount, setBankAccount] = useState("");
  const [ifsc, setIfsc] = useState("");
  const [accountHolder, setAccountHolder] = useState("");
  const [upiId, setUpiId] = useState("");

  // Section B — Buyer
  const [businessName, setBusinessName] = useState("");
  const [deliveryAddress, setDeliveryAddress] = useState("");
  const [gstNumber, setGstNumber] = useState("");

  // Section C — Language
  const [preferredLanguage, setPreferredLanguage] = useState(i18n.language || "en");

  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const isFarmer = user?.role === ROLES.FARMER || user?.role === ROLES.FPO_ADMIN;
  const isBuyer = user?.role === ROLES.BUYER;

  // Auto-detect location
  const handleAutoDetect = () => {
    if (!navigator.geolocation) {
      setError("Geolocation is not supported by your browser.");
      return;
    }
    setDetecting(true);
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const { latitude: lat, longitude: lng } = position.coords;
        setLatitude(lat);
        setLongitude(lng);
        try {
          const res = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`
          );
          const data = await res.json();
          const addr = data.address || {};
          setVillage(addr.village || addr.town || addr.city || addr.suburb || "");
          setDistrict(addr.county || addr.district || addr.state_district || "");
          setState(addr.state || "");
          setPincode(addr.postcode || "");
        } catch {
          setError("Could not reverse geocode location. Please fill in manually.");
        } finally {
          setDetecting(false);
        }
      },
      () => {
        setError("Location access denied. Please fill in manually.");
        setDetecting(false);
      }
    );
  };

  // Crop tag input
  const handleCropKeyDown = (e) => {
    if (e.key === "Enter" && cropInput.trim()) {
      e.preventDefault();
      if (!crops.includes(cropInput.trim())) {
        setCrops([...crops, cropInput.trim()]);
      }
      setCropInput("");
    }
  };

  const removeCrop = (crop) => {
    setCrops(crops.filter((c) => c !== crop));
  };

  const handleLanguageChange = (lng) => {
    setPreferredLanguage(lng);
    i18n.changeLanguage(lng);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);

    const payload = {
      village,
      district,
      state,
      pincode,
      latitude,
      longitude,
      preferredLanguage,
      // Farmer/FPO fields
      primaryCrops: isFarmer ? crops.join(",") : null,
      bankAccountNumber: isFarmer ? bankAccount : null,
      bankIfsc: isFarmer ? ifsc.toUpperCase() : null,
      accountHolderName: isFarmer ? accountHolder : null,
      upiId: isFarmer ? upiId : null,
      // Buyer fields
      businessName: isBuyer ? businessName : null,
      deliveryAddress: isBuyer ? deliveryAddress : null,
      gstNumber: isBuyer ? gstNumber.toUpperCase() : null,
    };

    try {
      const res = await axiosClient.put(`/users/${user.id}/profile`, payload);
      const updatedUser = res.data;
      updateUser(updatedUser);
      i18n.changeLanguage(preferredLanguage);
      localStorage.setItem("fasalconnect_lang", preferredLanguage);
      navigate(DASHBOARD_PATH_BY_ROLE[user.role] || "/");
    } catch (err) {
      const message =
        err.response?.data?.error?.message ||
        err.response?.data?.message ||
        "Failed to save profile. Please try again.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="min-h-screen w-full bg-cover bg-center relative flex items-center justify-center px-4 py-8"
      style={{ backgroundImage: `url('/hero.jpg')` }}
    >
      <div className="relative z-10 w-full max-w-lg bg-transparent backdrop-blur-none p-8 rounded-2xl shadow-2xl border border-white/5">

        {/* Language switcher */}
        <div className="flex justify-end mb-4">
          <select
            value={preferredLanguage}
            onChange={(e) => handleLanguageChange(e.target.value)}
            className="bg-white/20 border border-white/30 text-white rounded-lg px-3 py-1 text-sm backdrop-blur-sm focus:outline-none focus:ring-2 focus:ring-white/50"
          >
            <option value="en" className="text-gray-900">English</option>
            <option value="hi" className="text-gray-900">हिन्दी</option>
            <option value="bn" className="text-gray-900">বাংলা</option>
            <option value="mr" className="text-gray-900">मराठी</option>
          </select>
        </div>

        {/* Heading */}
        <div className="text-center mb-6">
          <div className="text-4xl mb-2">🌿</div>
          <h1 className="text-3xl font-bold text-white drop-shadow-lg">
            {t("profileSetup.heading", "Complete your profile")}
          </h1>
          <p className="text-white/90 text-sm mt-1 drop-shadow-lg">
            {t("profileSetup.subtitle", "Help us personalise your experience")}
          </p>
        </div>

        {error && (
          <div className="mb-4 text-sm text-red-100 bg-red-600/80 backdrop-blur-sm border border-red-400/50 rounded-lg px-3 py-2">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-5">

          {/* ── Section A: Location ── */}
          <div>
            <p className="text-white font-bold text-sm mb-3 drop-shadow uppercase tracking-wide">
              {t("profileSetup.locationSection", "📍 Your Location")}
            </p>

            <button
              type="button"
              onClick={handleAutoDetect}
              disabled={detecting}
              className="w-full mb-3 py-2 rounded-lg border border-white/40 bg-white/10 text-white text-sm font-semibold hover:bg-white/20 transition disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {detecting
                ? t("profileSetup.detecting", "Detecting...")
                : t("profileSetup.autoDetect", "📡 Auto-detect my location")}
            </button>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                  {t("profileSetup.village", "Village / Town")} *
                </label>
                <input
                  type="text"
                  required
                  value={village}
                  onChange={(e) => setVillage(e.target.value)}
                  className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                  {t("profileSetup.district", "District")} *
                </label>
                <input
                  type="text"
                  required
                  value={district}
                  onChange={(e) => setDistrict(e.target.value)}
                  className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                  {t("profileSetup.state", "State")} *
                </label>
                <select
                  required
                  value={state}
                  onChange={(e) => setState(e.target.value)}
                  className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                >
                  <option value="">Select state</option>
                  {INDIAN_STATES.map((s) => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                  {t("profileSetup.pincode", "Pincode")} *
                </label>
                <input
                  type="text"
                  required
                  maxLength={6}
                  value={pincode}
                  onChange={(e) => setPincode(e.target.value.replace(/\D/, ""))}
                  className="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                />
              </div>
            </div>
          </div>

          {/* ── Section B: Farmer / FPO ── */}
          {isFarmer && (
            <div>
              <p className="text-white font-bold text-sm mb-3 drop-shadow uppercase tracking-wide">
                {t("profileSetup.roleSection", "🌾 Farming Details")}
              </p>

              {/* Crop tag input */}
              <div className="mb-3">
                <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                  {t("profileSetup.primaryCrops", "Primary Crops")}
                </label>
                <div className="w-full px-3 py-2 bg-white/80 border border-white/40 rounded-lg flex flex-wrap gap-2 min-h-[48px]">
                  {crops.map((crop) => (
                    <span
                      key={crop}
                      className="flex items-center gap-1 bg-green-600 text-white text-xs px-2 py-1 rounded-full"
                    >
                      {crop}
                      <button type="button" onClick={() => removeCrop(crop)} className="hover:text-red-200">✕</button>
                    </span>
                  ))}
                  <input
                    type="text"
                    value={cropInput}
                    onChange={(e) => setCropInput(e.target.value)}
                    onKeyDown={handleCropKeyDown}
                    placeholder={t("profileSetup.cropPlaceholder", "Type crop & press Enter")}
                    className="flex-1 min-w-[120px] bg-transparent outline-none text-sm text-gray-800"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.bankAccount", "Bank Account No.")} *
                  </label>
                  <input
                    type="text"
                    required
                    value={bankAccount}
                    onChange={(e) => setBankAccount(e.target.value)}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.ifsc", "IFSC Code")} *
                  </label>
                  <input
                    type="text"
                    required
                    value={ifsc}
                    onChange={(e) => setIfsc(e.target.value.toUpperCase())}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.accountHolder", "Account Holder Name")} *
                  </label>
                  <input
                    type="text"
                    required
                    value={accountHolder}
                    onChange={(e) => setAccountHolder(e.target.value)}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.upiId", "UPI ID")}
                    <span className="font-normal opacity-70 ml-1">(optional)</span>
                  </label>
                  <input
                    type="text"
                    value={upiId}
                    onChange={(e) => setUpiId(e.target.value)}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </div>
              </div>
            </div>
          )}

          {/* ── Section B: Buyer ── */}
          {isBuyer && (
            <div>
              <p className="text-white font-bold text-sm mb-3 drop-shadow uppercase tracking-wide">
                {t("profileSetup.roleSection", "🛒 Buyer Details")}
              </p>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.businessName", "Business Name")}
                    <span className="font-normal opacity-70 ml-1">(optional)</span>
                  </label>
                  <input
                    type="text"
                    value={businessName}
                    onChange={(e) => setBusinessName(e.target.value)}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.deliveryAddress", "Delivery Address")} *
                  </label>
                  <textarea
                    required
                    rows={3}
                    value={deliveryAddress}
                    onChange={(e) => setDeliveryAddress(e.target.value)}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400 resize-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
                    {t("profileSetup.gstNumber", "GST Number")}
                    <span className="font-normal opacity-70 ml-1">(optional)</span>
                  </label>
                  <input
                    type="text"
                    value={gstNumber}
                    onChange={(e) => setGstNumber(e.target.value.toUpperCase())}
                    className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </div>
              </div>
            </div>
          )}

          {/* ── Section C: Preferred Language ── */}
          <div>
            <label className="block text-sm font-semibold text-white mb-1 drop-shadow">
              {t("profileSetup.preferredLanguage", "Preferred Language")}
            </label>
            <select
              value={preferredLanguage}
              onChange={(e) => handleLanguageChange(e.target.value)}
              className="w-full px-4 py-3 bg-white/80 border border-white/40 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-400"
            >
              <option value="en">English</option>
              <option value="hi">हिन्दी</option>
              <option value="bn">বাংলা</option>
              <option value="mr">मराठी</option>
            </select>
          </div>

          {/* Submit */}
          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-green-600 text-white py-3 rounded-lg font-bold hover:bg-green-700 disabled:opacity-50 shadow-xl transition transform hover:scale-[1.02]"
          >
            {isSubmitting
              ? t("profileSetup.saving", "Saving...")
              : t("profileSetup.saveButton", "Save & Continue")}
          </button>
        </form>
      </div>
    </div>
  );
}