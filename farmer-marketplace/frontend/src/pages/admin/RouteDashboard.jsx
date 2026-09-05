import { useEffect, useState } from "react";
import api from "../../services/api";

export default function RouteDashboard() {
  const [summary, setSummary] = useState(null);
  useEffect(() => {
    api
      .get("/admin/summary")
      .then((response) => setSummary(response.data))
      .catch(() => setSummary({}));
  }, []);
  const cards = [
    ["Farmers", summary?.totalFarmers],
    ["Buyers", summary?.totalBuyers],
    ["FPO admins", summary?.totalFpoAdmins],
    ["Products", summary?.totalProducts],
    ["Orders", summary?.totalOrders],
  ];
  return (
    <div className="mx-auto max-w-7xl space-y-8">
      <section className="rounded-2xl bg-[#174d35] p-8 text-white shadow-lg">
        <p className="text-base font-semibold text-[#f3c969]">
          Platform overview
        </p>
        <h2 className="mt-2 text-3xl font-bold">FasalConnect at a glance</h2>
        <p className="mt-3 max-w-2xl text-base leading-7 text-white/75">
          Monitor the marketplace community and the activity flowing through the
          platform.
        </p>
      </section>
      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        {cards.map(([label, value]) => (
          <article
            key={label}
            className="rounded-2xl border border-[#eadfbe] bg-white p-6 shadow-sm"
          >
            <p className="text-base font-semibold text-slate-500">{label}</p>
            <p className="mt-3 text-3xl font-bold text-[#174d35]">
              {value ?? "..."}
            </p>
          </article>
        ))}
      </section>
      <section className="rounded-2xl border border-[#eadfbe] bg-white p-7 shadow-sm">
        <h3 className="text-xl font-bold text-[#193b2a]">Platform health</h3>
        <p className="mt-2 text-base text-slate-600">
          Summary data is loaded from the protected admin API.
        </p>
      </section>
    </div>
  );
}
