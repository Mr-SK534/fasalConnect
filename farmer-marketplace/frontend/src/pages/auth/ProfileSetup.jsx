import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../hooks/useAuth";
import { useEffect } from "react";
import { ROLES, DASHBOARD_PATH_BY_ROLE } from "../../utils/roles";
import axiosClient from "../../services/api";

const INDIAN_STATES = [
  "Andhra Pradesh",
  "Arunachal Pradesh",
  "Assam",
  "Bihar",
  "Chhattisgarh",
  "Goa",
  "Gujarat",
  "Haryana",
  "Himachal Pradesh",
  "Jharkhand",
  "Karnataka",
  "Kerala",
  "Madhya Pradesh",
  "Maharashtra",
  "Manipur",
  "Meghalaya",
  "Mizoram",
  "Nagaland",
  "Odisha",
  "Punjab",
  "Rajasthan",
  "Sikkim",
  "Tamil Nadu",
  "Telangana",
  "Tripura",
  "Uttar Pradesh",
  "Uttarakhand",
  "West Bengal",
  "Andaman and Nicobar Islands",
  "Chandigarh",
  "Dadra and Nagar Haveli and Daman and Diu",
  "Delhi",
  "Jammu and Kashmir",
  "Ladakh",
  "Lakshadweep",
  "Puducherry",
];

export default function ProfileSetup() {
  const { user, updateUser } = useAuth();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();

  // Section A — Location
  const [location, setLocation] = useState("");
  const [district, setDistrict] = useState("");
  const [state, setState] = useState("");
  const [region, setRegion] = useState("");
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
  const [preferredLanguage, setPreferredLanguage] = useState(
    i18n.language || "en",
  );

  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const isFarmer =
    user?.role === ROLES.FARMER || user?.role === ROLES.FPO_ADMIN;
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
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`,
          );
          const data = await res.json();
          const addr = data.address || {};
          setLocation(data.display_name || "");
          setDistrict(
            addr.county || addr.district || addr.state_district || "",
          );
          setState(addr.state || "");
          setPincode(addr.postcode || "");
        } catch {
          setError(
            "Could not reverse geocode location. Please fill in manually.",
          );
        } finally {
          setDetecting(false);
        }
      },
      () => {
        setError("Location access denied. Please fill in manually.");
        setDetecting(false);
      },
    );
  };

  // Forward geocoding when address inputs change
  useEffect(() => {
    if (!location && !district && !state && !pincode) return;
    const timeout = setTimeout(async () => {
      try {
        const query = [location, district, state, pincode]
          .filter(Boolean)
          .join(", ");
        if (!query) return;
        const res = await fetch(
          `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=1`,
        );
        const data = await res.json();
        if (data && data.length > 0) {
          setLatitude(parseFloat(data[0].lat));
          setLongitude(parseFloat(data[0].lon));
        }
      } catch (err) {
        console.error("Geocoding error", err);
      }
    }, 1200);
    return () => clearTimeout(timeout);
  }, [location, district, state, pincode]);

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

    const pendingCrop = cropInput.trim();
    const selectedCrops =
      pendingCrop && !crops.includes(pendingCrop)
        ? [...crops, pendingCrop]
        : crops;

    const payload = {
      address: location,
      district,
      state,
      region,
      pincode,
      latitude,
      longitude,
      preferredLanguage,
      // Farmer/FPO fields
      primaryCrops: isFarmer ? selectedCrops.join(",") : null,
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
      className="min-h-screen w-full bg-cover bg-center relative flex items-center justify-center px-4 py-6"
      style={{ backgroundImage: `url('/hero.jpg')` }}
    >
      <div className="relative z-10 w-full max-w-xl bg-transparent backdrop-blur-none p-5 sm:p-7 rounded-2xl shadow-xl border border-white/5 my-auto">
        {/* Language switcher */}
        <div className="flex justify-end mb-2">
          <select
            value={preferredLanguage}
            onChange={(e) => handleLanguageChange(e.target.value)}
            className="bg-white/20 border border-white/40 text-white rounded-lg px-2.5 py-1 text-sm font-bold backdrop-blur-xs focus:outline-none focus:ring-2 focus:ring-white/50 cursor-pointer"
          >
            <option value="en" className="text-slate-900 font-semibold">
              English
            </option>
            <option value="hi" className="text-slate-900 font-semibold">
              हिन्दी
            </option>
            <option value="bn" className="text-slate-900 font-semibold">
              বাংলা
            </option>
            <option value="mr" className="text-slate-900 font-semibold">
              मराठी
            </option>
          </select>
        </div>

        {/* Heading */}
        <div className="text-center mb-4">
          <div className="text-3xl mb-1">🌿</div>
          <h1 className="text-2xl font-black text-white drop-shadow-md tracking-tight leading-tight">
            {t("profileSetup.heading", "Complete Your Profile")}
          </h1>
          <p className="text-white/95 text-sm font-medium mt-0.5 drop-shadow-xs">
            {t("profileSetup.subtitle", "Help us personalise your experience")}
          </p>
        </div>

        {error && (
          <div className="mb-3 text-sm font-semibold text-red-100 bg-red-600/80 backdrop-blur-xs border border-red-400/50 rounded-xl px-3.5 py-2 shadow-xs">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4 text-sm">
          {/* ── Section A: Location ── */}
          <div className="space-y-2">
            <p className="text-white font-bold text-sm drop-shadow-xs uppercase tracking-wider border-b border-white/20 pb-1">
              {t("profileSetup.locationSection", "📍 Your Location")}
            </p>

            <button
              type="button"
              onClick={handleAutoDetect}
              disabled={detecting}
              className="w-full py-2 rounded-xl border border-white/40 bg-white/15 text-white text-sm font-bold hover:bg-white/25 transition disabled:opacity-50 flex items-center justify-center gap-2 shadow-xs"
            >
              {detecting
                ? t("profileSetup.detecting", "Detecting...")
                : t("profileSetup.autoDetect", "📡 Auto-detect my location")}
            </button>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
              <label className="sm:col-span-2">
                <span className="block font-bold text-white mb-1 drop-shadow-xs">Address *</span>
                <input
                  type="text"
                  required
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  placeholder="e.g. MI Road, Jaipur"
                  className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 backdrop-blur-xs border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                />
              </label>
              <div>
                <label className="block font-bold text-white mb-1 drop-shadow-xs">
                  {t("profileSetup.district", "District")} *
                </label>
                <input
                  type="text"
                  required
                  value={district}
                  onChange={(e) => setDistrict(e.target.value)}
                  className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 backdrop-blur-xs border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                />
              </div>
              <div>
                <label className="block font-bold text-white mb-1 drop-shadow-xs">
                  {t("profileSetup.state", "State")} *
                </label>
                <select
                  required
                  value={state}
                  onChange={(e) => setState(e.target.value)}
                  className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 backdrop-blur-xs border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                >
                  <option value="">Select state</option>
                  {INDIAN_STATES.map((s) => (
                    <option key={s} value={s}>
                      {s}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block font-bold text-white mb-1 drop-shadow-xs">
                  {t("profileSetup.region", "Region")}
                  <span className="font-normal text-white/90 ml-1 text-xs">
                    (optional)
                  </span>
                </label>
                <input
                  type="text"
                  value={region}
                  onChange={(e) => setRegion(e.target.value)}
                  placeholder="e.g. North, South"
                  className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 backdrop-blur-xs border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                />
              </div>
              <div>
                <label className="block font-bold text-white mb-1 drop-shadow-xs">
                  {t("profileSetup.pincode", "Pincode")} *
                </label>
                <input
                  type="text"
                  required
                  maxLength={6}
                  value={pincode}
                  onChange={(e) => setPincode(e.target.value.replace(/\D/, ""))}
                  className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 backdrop-blur-xs border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                />
              </div>
            </div>
          </div>

          {/* ── Section B: Farmer / FPO ── */}
          {isFarmer && (
            <div className="space-y-2">
              <p className="text-white font-bold text-sm drop-shadow-xs uppercase tracking-wider border-b border-white/20 pb-1">
                {t("profileSetup.roleSection", "🌾 Farming Details")}
              </p>

              {/* Crop tag input */}
              <div>
                <label className="block font-bold text-white mb-1 drop-shadow-xs">
                  {t("profileSetup.primaryCrops", "Primary Crops")}
                </label>
                <div className="w-full px-3 py-1.5 bg-white/80 border border-white/40 rounded-xl flex flex-wrap gap-1.5 min-h-[38px] items-center shadow-xs">
                  {crops.map((crop) => (
                    <span
                      key={crop}
                      className="flex items-center gap-1 bg-[#2e7d32] text-white text-xs font-bold px-2 py-0.5 rounded-md shadow-2xs"
                    >
                      {crop}
                      <button
                        type="button"
                        onClick={() => removeCrop(crop)}
                        className="hover:text-red-300 font-extrabold text-xs"
                      >
                        ✕
                      </button>
                    </span>
                  ))}
                  <input
                    type="text"
                    value={cropInput}
                    onChange={(e) => setCropInput(e.target.value)}
                    onKeyDown={handleCropKeyDown}
                    placeholder={t(
                      "profileSetup.cropPlaceholder",
                      "Type crop & press Enter",
                    )}
                    className="flex-1 min-w-[140px] bg-transparent outline-none text-sm font-medium text-slate-900 placeholder:text-slate-400"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.bankAccount", "Bank Account No.")} *
                  </label>
                  <input
                    type="text"
                    required
                    value={bankAccount}
                    onChange={(e) => setBankAccount(e.target.value)}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                  />
                </div>
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.ifsc", "IFSC Code")} *
                  </label>
                  <input
                    type="text"
                    required
                    value={ifsc}
                    onChange={(e) => setIfsc(e.target.value.toUpperCase())}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs uppercase"
                  />
                </div>
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.accountHolder", "Account Holder Name")} *
                  </label>
                  <input
                    type="text"
                    required
                    value={accountHolder}
                    onChange={(e) => setAccountHolder(e.target.value)}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                  />
                </div>
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.upiId", "UPI ID")}
                    <span className="font-normal text-white/90 ml-1 text-xs">
                      (optional)
                    </span>
                  </label>
                  <input
                    type="text"
                    value={upiId}
                    onChange={(e) => setUpiId(e.target.value)}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                  />
                </div>
              </div>
            </div>
          )}

          {/* ── Section B: Buyer ── */}
          {isBuyer && (
            <div className="space-y-2">
              <p className="text-white font-bold text-sm drop-shadow-xs uppercase tracking-wider border-b border-white/20 pb-1">
                {t("profileSetup.roleSection", "🛒 Buyer Details")}
              </p>
              <div className="space-y-2.5">
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.businessName", "Business Name")}
                    <span className="font-normal text-white/90 ml-1 text-xs">
                      (optional)
                    </span>
                  </label>
                  <input
                    type="text"
                    value={businessName}
                    onChange={(e) => setBusinessName(e.target.value)}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
                  />
                </div>
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.deliveryAddress", "Delivery Address")} *
                  </label>
                  <textarea
                    required
                    rows={2}
                    value={deliveryAddress}
                    onChange={(e) => setDeliveryAddress(e.target.value)}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs resize-none"
                  />
                </div>
                <div>
                  <label className="block font-bold text-white mb-1 drop-shadow-xs">
                    {t("profileSetup.gstNumber", "GST Number")}
                    <span className="font-normal text-white/90 ml-1 text-xs">
                      (optional)
                    </span>
                  </label>
                  <input
                    type="text"
                    value={gstNumber}
                    onChange={(e) => setGstNumber(e.target.value.toUpperCase())}
                    className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs uppercase"
                  />
                </div>
              </div>
            </div>
          )}

          {/* ── Section C: Preferred Language ── */}
          <div>
            <label className="block font-bold text-white mb-1 drop-shadow-xs">
              {t("profileSetup.preferredLanguage", "Preferred Language")}
            </label>
            <select
              value={preferredLanguage}
              onChange={(e) => handleLanguageChange(e.target.value)}
              className="w-full px-3 py-2 text-sm font-medium text-slate-900 bg-white/80 border border-white/40 rounded-xl focus:outline-none focus:ring-2 focus:ring-green-400/50 shadow-xs"
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
            className="w-full bg-[#2e7d32] text-white py-2.5 rounded-xl text-sm font-bold hover:bg-[#246b28] disabled:opacity-50 shadow-xs transition mt-3"
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