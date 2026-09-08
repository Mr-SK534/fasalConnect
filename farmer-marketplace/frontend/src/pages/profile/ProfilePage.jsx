import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "react-hot-toast";
import api from "../../services/api";
import { useAuth } from "../../hooks/useAuth";
import { ROLES } from "../../utils/roles";

const states = [
  "Andhra Pradesh",
  "Bihar",
  "Delhi",
  "Gujarat",
  "Haryana",
  "Karnataka",
  "Kerala",
  "Madhya Pradesh",
  "Maharashtra",
  "Odisha",
  "Punjab",
  "Rajasthan",
  "Tamil Nadu",
  "Telangana",
  "Uttar Pradesh",
  "Uttarakhand",
  "West Bengal",
];
const inputClass =
  "mt-1 w-full rounded-lg border border-gray-200 bg-white px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400";

export default function ProfilePage() {
  const { user, updateUser } = useAuth();
  const { i18n } = useTranslation();
  const [form, setForm] = useState({
    name: "",
    phone: "",
    preferredLanguage: "en",
    address: "",
    district: "",
    state: "",
    pincode: "",
    primaryCrops: "",
    bankAccountNumber: "",
    bankIfsc: "",
    accountHolderName: "",
    upiId: "",
    businessName: "",
    deliveryAddress: "",
    gstNumber: "",
    latitude: null,
    longitude: null,
  });
  const [cropInput, setCropInput] = useState("");
  const [detecting, setDetecting] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!user?.id) return;
    api
      .get(`/users/${user.id}/profile`)
      .then((response) => {
        const profile = response.data;
        setForm((current) => ({
          ...current,
          ...user,
          ...profile,
          primaryCrops: profile.primaryCrops || user.primaryCrops || "",
          preferredLanguage: user.preferredLanguage || i18n.language || "en",
        }));
      })
      .catch(() =>
        setForm((current) => ({
          ...current,
          ...user,
          preferredLanguage: user.preferredLanguage || i18n.language || "en",
        })),
      );
  }, [i18n.language, user]);

  const isFarmer =
    user?.role === ROLES.FARMER || user?.role === ROLES.FPO_ADMIN;
  const isAdminLike =
    user?.role === ROLES.SUPER_ADMIN ||
    user?.role === ROLES.PLATFORM_ADMIN ||
    user?.role === ROLES.ADMIN ||
    user?.role === ROLES.MANAGER;
  const showBankSection = isFarmer || isAdminLike;
  const isBuyer = user?.role === ROLES.BUYER;
  const update = (event) =>
    setForm({ ...form, [event.target.name]: event.target.value });
  const crops = form.primaryCrops
    ? form.primaryCrops
        .split(",")
        .map((crop) => crop.trim())
        .filter(Boolean)
    : [];

  const autoDetect = () => {
    if (!navigator.geolocation) {
      setError("Geolocation is not supported by your browser.");
      return;
    }
    setDetecting(true);
    navigator.geolocation.getCurrentPosition(
      async ({ coords }) => {
        try {
          const response = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${coords.latitude}&lon=${coords.longitude}`,
          );
          const data = await response.json();
          const address = data.address || {};
          
          setForm((current) => ({
            ...current,
            latitude: coords.latitude,
            longitude: coords.longitude,
            address: data.display_name || "",
            district:
              address.county ||
              address.district ||
              address.state_district ||
              "",
            state: address.state || "",
            pincode: address.postcode || "",
          }));
        } catch {
          setError("Could not detect the address. Please enter it manually.");
        } finally {
          setDetecting(false);
        }
      },
      () => {
        setError("Location access denied. Please enter it manually.");
        setDetecting(false);
      },
    );
  };

  useEffect(() => {
    // Forward geocoding logic based on form fields
    const { address, district, state, pincode } = form;
    if (!address && !district && !state && !pincode) return;

    const timeoutId = setTimeout(async () => {
      try {
        const queryParts = [address, district, state, pincode].filter(Boolean);
        const query = queryParts.join(", ");
        
        if (query.trim() === "") return;

        const response = await fetch(
          `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(
            query
          )}&countrycodes=in&limit=1`
        );
        const data = await response.json();
        
        if (data && data.length > 0) {
          setForm((current) => ({
            ...current,
            latitude: parseFloat(data[0].lat),
            longitude: parseFloat(data[0].lon),
          }));
        }
      } catch (error) {
        console.error("Geocoding failed", error);
      }
    }, 1500);

    return () => clearTimeout(timeoutId);
  }, [form.address, form.district, form.state, form.pincode]);

  const addCrop = (event) => {
    if (event.key === "Enter" && cropInput.trim()) {
      event.preventDefault();
      if (!crops.includes(cropInput.trim()))
        setForm({
          ...form,
          primaryCrops: [...crops, cropInput.trim()].join(","),
        });
      setCropInput("");
    }
  };
  const removeCrop = (crop) =>
    setForm({
      ...form,
      primaryCrops: crops.filter((item) => item !== crop).join(","),
    });

  const save = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      const response = await api.put(`/users/${user.id}/profile`, {
        ...form,
        preferredLanguage: form.preferredLanguage,
        primaryCrops: isFarmer ? form.primaryCrops : null,
        bankAccountNumber: showBankSection ? form.bankAccountNumber : null,
        bankIfsc: showBankSection ? form.bankIfsc : null,
        accountHolderName: showBankSection ? form.accountHolderName : null,
        upiId: showBankSection ? form.upiId : null,
        businessName: isBuyer ? form.businessName : null,
        deliveryAddress: isBuyer ? form.deliveryAddress : null,
        gstNumber: isBuyer ? form.gstNumber : null,
      });
      updateUser(response.data);
      toast.success("Profile saved");
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
          requestError.response?.data?.title ||
          "Could not save profile.",
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={save} className="mx-auto max-w-4xl space-y-6">
      <div>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          Account
        </p>
        <h2 className="text-2xl font-black text-[#163820]">My Profile</h2>
      </div>
      {error && (
        <p className="rounded-lg bg-red-50 p-4 text-sm text-red-700">{error}</p>
      )}
      <section className="rounded-xl bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-lg font-bold">Basic information</h3>
        <div className="grid gap-4 sm:grid-cols-2">
          <label>
            Full name
            <input
              required
              name="name"
              value={form.name || ""}
              onChange={update}
              className={inputClass}
            />
          </label>
          <label>
            Phone
            <input
              required
              name="phone"
              value={form.phone || ""}
              onChange={update}
              className={inputClass}
            />
          </label>
          <label>
            Email
            <input
              readOnly
              value={user?.email || ""}
              className={`${inputClass} bg-gray-50`}
            />
          </label>
          <label>
            Role
            <span className="mt-1 block rounded-lg bg-green-50 px-4 py-3 font-semibold text-green-700">
              {user?.role}
            </span>
          </label>
          <label>
            Preferred language
            <select
              name="preferredLanguage"
              value={form.preferredLanguage}
              onChange={(event) => {
                update(event);
                i18n.changeLanguage(event.target.value);
              }}
              className={inputClass}
            >
              <option value="en">English</option>
              <option value="hi">Hindi</option>
              <option value="mr">Marathi</option>
              <option value="bn">Bengali</option>
            </select>
          </label>
        </div>
      </section>
      <section className="rounded-xl bg-white p-6 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h3 className="text-lg font-bold">Location</h3>
          <button
            type="button"
            onClick={autoDetect}
            disabled={detecting}
            className="rounded-lg bg-green-600 px-4 py-2 text-sm font-bold text-white hover:bg-green-700"
          >
            {detecting ? "Detecting..." : "Auto-detect location"}
          </button>
        </div>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          <label className="sm:col-span-2">
            Address
            <input
              name="address"
              value={form.address || ""}
              onChange={update}
              placeholder="e.g. MI Road, Jaipur"
              className={inputClass}
            />
          </label>
          <label>
            District
            <input
              name="district"
              value={form.district || ""}
              onChange={update}
              className={inputClass}
            />
          </label>
          <label>
            State
            <select
              name="state"
              value={form.state || ""}
              onChange={update}
              className={inputClass}
            >
              <option value="">Select state</option>
              {states.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label>
            Pincode
            <input
              name="pincode"
              value={form.pincode || ""}
              onChange={update}
              className={inputClass}
            />
          </label>
        </div>
      </section>
      {showBankSection && (
        <section className="rounded-xl bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-bold">
            {isFarmer ? "Farmer / FPO details" : "Bank & Payout Details"}
          </h3>
          {isFarmer && (
            <>
              <label>
                Primary crops
                <input
                  value={cropInput}
                  onChange={(event) => setCropInput(event.target.value)}
                  onKeyDown={addCrop}
                  placeholder="Type a crop and press Enter"
                  className={inputClass}
                />
              </label>
              <div className="mt-2 flex flex-wrap gap-2">
                {crops.map((crop) => (
                  <button
                    type="button"
                    key={crop}
                    onClick={() => removeCrop(crop)}
                    className="rounded-full bg-green-100 px-3 py-1 text-xs font-semibold text-green-800"
                  >
                    {crop} ×
                  </button>
                ))}
              </div>
            </>
          )}
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            {[
              ["bankAccountNumber", "Bank account number"],
              ["bankIfsc", "IFSC code"],
              ["accountHolderName", "Account holder name"],
              ["upiId", "UPI ID"],
            ].map(([name, label]) => (
              <label key={name}>
                {label}
                <input
                  name={name}
                  value={form[name] || ""}
                  onChange={update}
                  className={inputClass}
                />
              </label>
            ))}
          </div>
        </section>
      )}
      {isBuyer && (
        <section className="rounded-xl bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-bold">Buyer details</h3>
          <div className="grid gap-4">
            <label>
              Business name
              <input
                name="businessName"
                value={form.businessName || ""}
                onChange={update}
                className={inputClass}
              />
            </label>
            <label>
              Delivery address
              <textarea
                name="deliveryAddress"
                value={form.deliveryAddress || ""}
                onChange={update}
                rows="3"
                className={inputClass}
              />
            </label>
            <label>
              GST number
              <input
                name="gstNumber"
                value={form.gstNumber || ""}
                onChange={update}
                className={inputClass}
              />
            </label>
          </div>
        </section>
      )}
      <button
        type="submit"
        disabled={saving}
        className="rounded-lg bg-green-600 px-6 py-3 font-bold text-white hover:bg-green-700 disabled:opacity-60"
      >
        {saving ? "Saving..." : "Save Profile"}
      </button>
    </form>
  );
}
