import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import api from "../../services/api";

export default function FPODashboard() {
  const [summary, setSummary] = useState(null);
  useEffect(() => {
    api
      .get("/admin/summary")
      .then((response) => setSummary(response.data))
      .catch(() => setSummary({}));
  }, []);
  const cards = [
    ["Linked farmers", summary?.totalFarmers, "Farmers in your network"],
    ["Products", summary?.totalProducts, "Marketplace listings"],
    ["Pending orders", summary?.pendingOrders, "Needs attention"],
  ];
  return (
    <div className="mx-auto max-w-7xl space-y-8">
      <section className="rounded-2xl bg-[#174d35] p-8 text-white shadow-lg">
        <p className="text-base font-semibold text-[#f3c969]">FPO operations</p>
        <h2 className="mt-2 text-3xl font-bold">Grow your farmer network</h2>
        <p className="mt-3 max-w-2xl text-base leading-7 text-white/75">
          Keep your members supported and their harvests moving through the
          marketplace.
        </p>
      </section>
      <section className="grid gap-4 sm:grid-cols-3">
        {cards.map(([label, value, hint]) => (
          <article
            key={label}
            className="rounded-2xl border border-[#eadfbe] bg-white p-6 shadow-sm"
          >
            <p className="text-base font-semibold text-slate-500">{label}</p>
            <p className="mt-3 text-3xl font-bold text-[#174d35]">
              {value ?? "..."}
            </p>
            <p className="mt-2 text-sm text-slate-500">{hint}</p>
          </article>
        ))}
      </section>
      <section className="rounded-2xl border border-[#eadfbe] bg-white p-7 shadow-sm">
        <h3 className="text-xl font-bold text-[#193b2a]">Manage members</h3>
        <p className="mt-2 text-base text-slate-600">
          Review the farmers linked to your FPO and keep their details up to
          date.
        </p>
        <NavLink
          to="/fpo-admin/farmers"
          className="mt-5 inline-block rounded-lg bg-[#f3c969] px-5 py-3 text-base font-bold text-[#174d35]"
        >
          View linked farmers
        </NavLink>
      </section>
    </div>
  );
}
