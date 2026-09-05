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
    <div className="mx-auto max-w-6xl text-xl">
      {/* Header Section */}
      <div className="mb-8">
        <p className="text-base font-extrabold uppercase tracking-widest text-[#406836]">
          FPO Network
        </p>
        <h2 className="mt-1 text-4xl font-black text-[#163820]">
          Linked Farmers
        </h2>
        <p className="mt-2 text-xl font-semibold text-slate-600">
          Farmers connected to your producer organisation.
        </p>
      </div>

      {/* Farmers Data Table Card */}
      <section className="overflow-hidden rounded-3xl border border-[#eadaaf] bg-white shadow-md">
        {/* Table Header Row */}
        <div className="grid grid-cols-[1.3fr_1fr_1fr] border-b border-[#eadaaf] bg-[#fdfbf3] px-8 py-5 text-lg font-black uppercase tracking-wider text-[#406836]">
          <span>Farmer</span>
          <span>Phone</span>
          <span>Profile</span>
        </div>

        {/* Loading State */}
        {loading ? (
          <p className="p-8 text-xl font-bold text-slate-500">
            Loading farmers...
          </p>
        ) : farmers.length ? (
          /* Farmers Row Items */
          farmers.map((farmer) => (
            <div
              key={farmer.id}
              className="grid grid-cols-[1.3fr_1fr_1fr] items-center border-b border-[#eee5cc] px-8 py-6 last:border-0 hover:bg-[#fffef9] transition-colors"
            >
              <div>
                <p className="text-2xl font-extrabold text-slate-900">
                  {farmer.name}
                </p>
                <p className="mt-1 text-lg font-semibold text-slate-500">
                  {farmer.email || "No email provided"}
                </p>
              </div>

              <p className="text-xl font-bold text-slate-700">
                {farmer.phone || "Not provided"}
              </p>

              <div>
                <span
                  className={`inline-block rounded-full px-4 py-1.5 text-base font-black uppercase tracking-wide ${
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
          <p className="p-12 text-center text-xl font-bold text-slate-500">
            No linked farmers found.
          </p>
        )}
      </section>
    </div>
  );
}