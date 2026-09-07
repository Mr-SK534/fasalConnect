import { useEffect, useState } from "react";
import api from "../../services/api";
import { useAuth } from "../../hooks/useAuth";
import { toast } from "react-hot-toast";

const getApiError = (error, fallback) => {
  const data = error.response?.data;
  const validationMessage = data?.errors
    ? Object.values(data.errors).flat().join(" ")
    : null;
  return validationMessage || data?.message || data?.title || fallback;
};

export default function ManageLinkedFarmers() {
  const { user } = useAuth();
  const [farmers, setFarmers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [farmerEmail, setFarmerEmail] = useState("");
  const [actionError, setActionError] = useState("");
  const [mutating, setMutating] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createForm, setCreateForm] = useState({
    name: "",
    phone: "",
    email: "",
    password: "",
    address: "",
    district: "",
    state: "",
    region: "",
    latitude: "",
    longitude: "",
  });
  const [createError, setCreateError] = useState("");

  useEffect(() => {
    if (!showCreateModal) return;
    const { address, district, state } = createForm;
    if (!address && !district && !state) return;
    const timeout = setTimeout(async () => {
      try {
        const query = [address, district, state].filter(Boolean).join(", ");
        if (!query) return;
        const res = await fetch(
          `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=1`,
        );
        const data = await res.json();
        if (data && data.length > 0) {
          setCreateForm((prev) => ({
            ...prev,
            latitude: parseFloat(data[0].lat),
            longitude: parseFloat(data[0].lon),
          }));
        }
      } catch (err) {
        console.error("Geocoding error", err);
      }
    }, 1200);
    return () => clearTimeout(timeout);
  }, [createForm.address, createForm.district, createForm.state, showCreateModal]);

  useEffect(() => {
    if (!user?.id) return;
    api
      .get(`/fpo/${user.id}/farmers`)
      .then((response) =>
        setFarmers(Array.isArray(response.data) ? response.data : []),
      )
      .catch((err) => {
        setFarmers([]);
        setActionError(getApiError(err, "Could not load linked farmers."));
      })
      .finally(() => setLoading(false));
  }, [user?.id]);

  const refreshFarmers = async () => {
    const response = await api.get(`/fpo/${user.id}/farmers`);
    setFarmers(Array.isArray(response.data) ? response.data : []);
  };

  const linkFarmer = async (event) => {
    event.preventDefault();
    if (!farmerEmail.trim()) return;
    setActionError("");
    setMutating(true);
    try {
      await api.post(`/fpo/${user.id}/farmers`, {
        farmerEmail: farmerEmail.trim(),
      });
      setFarmerEmail("");
      await refreshFarmers();
    } catch (err) {
      setActionError(getApiError(err, "Could not link this farmer."));
    } finally {
      setMutating(false);
    }
  };

  const unlinkFarmer = async (farmer) => {
    if (!window.confirm(`Unlink ${farmer.name || "this farmer"}?`)) return;
    setActionError("");
    setMutating(true);
    try {
      await api.delete(`/fpo/${user.id}/farmers/${farmer.id}`);
      await refreshFarmers();
    } catch (err) {
      setActionError(getApiError(err, "Could not unlink this farmer."));
    } finally {
      setMutating(false);
    }
  };

  const createFarmer = async (event) => {
    event.preventDefault();
    setCreateError("");
    setMutating(true);
    try {
      await api.post(`/fpo/${user.id}/farmers/create`, {
        ...createForm,
        address: createForm.address || undefined,
        district: createForm.district || undefined,
        state: createForm.state || undefined,
        region: createForm.region || undefined,
        email: createForm.email || undefined,
        latitude: createForm.latitude || undefined,
        longitude: createForm.longitude || undefined,
      });
      setCreateForm({
        name: "",
        phone: "",
        email: "",
        password: "",
        address: "",
        district: "",
        state: "",
        region: "",
        latitude: "",
        longitude: "",
      });
      setShowCreateModal(false);
      await refreshFarmers();
      toast.success("Farmer account created");
    } catch (err) {
      setCreateError(getApiError(err, "Could not create farmer account."));
    } finally {
      setMutating(false);
    }
  };

  return (
    <div className="mx-auto max-w-6xl space-y-5 text-sm">
      {/* Header Section */}
      <div>
        <p className="text-xs font-bold uppercase tracking-wider text-[#406836]">
          FPO Network
        </p>
        <h2 className="mt-0.5 text-2xl font-black text-[#163820]">
          Linked Farmers
        </h2>
        <p className="mt-1 text-xs text-slate-500">
          Farmers connected to your producer organisation.
        </p>
        <button
          type="button"
          onClick={() => {
            setCreateError("");
            setShowCreateModal(true);
          }}
          className="mt-4 rounded-lg bg-green-600 px-4 py-2.5 text-xs font-bold text-white hover:bg-green-700"
        >
          + Add Farmer
        </button>
      </div>

      <form
        onSubmit={linkFarmer}
        className="flex flex-col gap-3 rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-xs sm:flex-row sm:items-end"
      >
        <label className="flex-1 text-xs font-bold text-slate-800">
          Link farmer by email
          <input
            required
            type="email"
            value={farmerEmail}
            onChange={(event) => setFarmerEmail(event.target.value)}
            placeholder="farmer@example.com"
            className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
          />
        </label>
        <button
          type="submit"
          disabled={mutating || !user?.id}
          className="rounded-xl bg-[#2e7d32] px-5 py-2.5 text-xs font-bold text-white transition hover:bg-[#246b28] disabled:opacity-60"
        >
          {mutating ? "Working..." : "Link Farmer"}
        </button>
      </form>

      {actionError && (
        <p className="rounded-xl bg-red-50 p-3.5 text-xs font-semibold text-red-700 ring-1 ring-red-200">
          {actionError}
        </p>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4 backdrop-blur-sm">
          <form
            onSubmit={createFarmer}
            className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl bg-white p-6 shadow-xl"
          >
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-bold text-slate-900">
                Create Farmer Account
              </h3>
              <button
                type="button"
                onClick={() => setShowCreateModal(false)}
                className="text-2xl text-slate-400"
              >
                ×
              </button>
            </div>
            {createError && (
              <p className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
                {createError}
              </p>
            )}
            <div className="mt-5 grid gap-4 sm:grid-cols-2">
              {[
                ["name", "Full name", true],
                ["phone", "Phone", true],
                ["email", "Email", false],
                ["password", "Password", true],
                ["address", "Address", false],
                ["district", "District", false],
                ["state", "State", false],
                ["region", "Region", false],
              ].map(([name, label, required]) => (
                <label
                  key={name}
                  className="text-sm font-medium text-slate-700"
                >
                  {label}
                  <input
                    required={required}
                    minLength={name === "password" ? 6 : undefined}
                    type={
                      name === "password"
                        ? "password"
                        : name === "email"
                          ? "email"
                          : "text"
                    }
                    value={createForm[name]}
                    onChange={(event) =>
                      setCreateForm({
                        ...createForm,
                        [name]: event.target.value,
                      })
                    }
                    className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </label>
              ))}
            </div>
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setShowCreateModal(false)}
                className="rounded-lg border border-gray-200 px-4 py-2.5 text-sm font-semibold text-slate-600"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={mutating}
                className="rounded-lg bg-green-600 px-5 py-2.5 text-sm font-bold text-white hover:bg-green-700 disabled:opacity-60"
              >
                {mutating ? "Creating..." : "Create Farmer"}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Farmers Data Table Card */}
      <section className="overflow-hidden rounded-2xl border border-[#eadaaf] bg-white shadow-xs">
        {/* Table Header Row */}
        <div className="grid grid-cols-[1.3fr_1fr_1fr_auto] border-b border-[#eadaaf] bg-[#fdfbf3] px-5 py-3 text-[11px] font-extrabold uppercase tracking-wider text-[#406836]">
          <span>Farmer</span>
          <span>Phone</span>
          <span>Profile</span>
          <span>Actions</span>
        </div>

        {/* Loading State */}
        {loading ? (
          <p className="p-6 text-xs font-semibold text-slate-500">
            Loading farmers...
          </p>
        ) : farmers.length ? (
          /* Farmers Row Items */
          farmers.map((farmer) => (
            <div
              key={farmer.id}
              className="grid grid-cols-[1.3fr_1fr_1fr_auto] items-center border-b border-[#eee5cc] px-5 py-3.5 last:border-0 hover:bg-[#fffef9] transition-colors"
            >
              <div>
                <p className="text-sm font-bold text-slate-900">
                  {farmer.name}
                </p>
                <p className="text-xs font-normal text-slate-500">
                  {farmer.email || "No email provided"}
                </p>
              </div>

              <p className="text-xs font-semibold text-slate-700">
                {farmer.phone || "Not provided"}
              </p>

              <div>
                <span
                  className={`inline-block rounded-full px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wide ${
                    farmer.isProfileComplete
                      ? "bg-[#d2e8bf] text-[#174d35] ring-1 ring-[#a8d488]"
                      : "bg-[#f4e8c5] text-[#745512] ring-1 ring-[#e2cd93]"
                  }`}
                >
                  {farmer.isProfileComplete ? "Complete" : "Incomplete"}
                </span>
              </div>

              <button
                type="button"
                disabled={mutating}
                onClick={() => unlinkFarmer(farmer)}
                className="text-left text-xs font-bold text-red-700 hover:text-red-900 disabled:opacity-50"
              >
                Unlink
              </button>
            </div>
          ))
        ) : (
          /* Empty State */
          <p className="p-8 text-center text-xs font-semibold text-slate-500">
            No linked farmers found.
          </p>
        )}
      </section>
    </div>
  );
}
