import { useEffect, useState } from "react";
import api from "../../services/api";
import { useAuth } from "../../hooks/useAuth";

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
