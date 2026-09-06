import { useEffect, useState } from "react";
import api from "../../services/api";

export default function ManageLinkedFarmers() {
  const [farmers, setFarmers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get("/admin/users?role=Farmer")
      .then((response) =>
        setFarmers(Array.isArray(response.data) ? response.data : []),
      )
      .catch(() => setFarmers([]))
      .finally(() => setLoading(false));
  }, []);

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

      {/* Farmers Data Table Card */}
      <section className="overflow-hidden rounded-2xl border border-[#eadaaf] bg-white shadow-xs">
        {/* Table Header Row */}
        <div className="grid grid-cols-[1.3fr_1fr_1fr] border-b border-[#eadaaf] bg-[#fdfbf3] px-5 py-3 text-[11px] font-extrabold uppercase tracking-wider text-[#406836]">
          <span>Farmer</span>
          <span>Phone</span>
          <span>Profile</span>
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
              className="grid grid-cols-[1.3fr_1fr_1fr] items-center border-b border-[#eee5cc] px-5 py-3.5 last:border-0 hover:bg-[#fffef9] transition-colors"
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