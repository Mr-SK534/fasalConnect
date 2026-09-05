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
    <div className="mx-auto max-w-6xl">
      <div className="mb-8">
        <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[#668357]">
          FPO network
        </p>
        <h2 className="mt-2 text-3xl font-bold text-[#193b2a]">
          Linked farmers
        </h2>
        <p className="mt-2 text-base text-slate-600">
          Farmers connected to your producer organisation.
        </p>
      </div>
      <section className="overflow-hidden rounded-2xl border border-[#eadfbe] bg-white shadow-sm">
        <div className="grid grid-cols-[1.3fr_1fr_1fr] border-b border-[#eadfbe] bg-[#fffaf0] px-6 py-4 text-sm font-bold uppercase tracking-wide text-[#668357]">
          <span>Farmer</span>
          <span>Phone</span>
          <span>Profile</span>
        </div>
        {loading ? (
          <p className="p-6 text-base text-slate-500">Loading farmers...</p>
        ) : farmers.length ? (
          farmers.map((farmer) => (
            <div
              key={farmer.id}
              className="grid grid-cols-[1.3fr_1fr_1fr] items-center border-b border-[#f0e7cd] px-6 py-5 last:border-0"
            >
              <div>
                <p className="text-lg font-semibold text-slate-800">
                  {farmer.name}
                </p>
                <p className="text-sm text-slate-500">
                  {farmer.email || "No email provided"}
                </p>
              </div>
              <p className="text-base text-slate-600">
                {farmer.phone || "Not provided"}
              </p>
              <span
                className={`w-fit rounded-full px-3 py-1 text-sm font-semibold ${farmer.isProfileComplete ? "bg-[#d9ebc9] text-[#28613e]" : "bg-[#f4e8c5] text-[#80631b]"}`}
              >
                {farmer.isProfileComplete ? "Complete" : "Incomplete"}
              </span>
            </div>
          ))
        ) : (
          <p className="p-8 text-center text-base text-slate-500">
            No linked farmers found.
          </p>
        )}
      </section>
    </div>
  );
}
