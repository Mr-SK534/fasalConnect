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
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header Banner */}
      <section className="rounded-2xl bg-gradient-to-r from-[#2e7d32] via-[#246b28] to-[#1b5e20] p-6 text-white shadow-md sm:p-8">
        <p className="text-xs font-extrabold uppercase tracking-wider text-[#f5d77f]">
          FPO Operations
        </p>
        <h2 className="mt-1 text-2xl font-black sm:text-3xl tracking-tight">
          Grow Your Farmer Network
        </h2>
        <p className="mt-2 max-w-2xl text-sm leading-relaxed text-emerald-100 font-normal">
          Keep your members supported and their harvests moving through the
          marketplace.
        </p>
      </section>

      {/* Summary Cards */}
      <section className="grid gap-4 sm:grid-cols-3">
        {cards.map(([label, value, hint]) => (
          <article
            key={label}
            className="rounded-2xl border border-[#eadaaf] bg-white/90 p-5 shadow-xs backdrop-blur transition hover:shadow-md"
          >
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">
              {label}
            </p>
            <p className="mt-2 text-3xl font-black text-[#1b5e20]">
              {value ?? "..."}
            </p>
            <p className="mt-1 text-xs font-semibold text-slate-500">{hint}</p>
          </article>
        ))}
      </section>

      {/* Manage Members Box */}
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-xs sm:p-6">
        <h3 className="text-lg font-bold text-[#163820]">Manage Members</h3>
        <p className="mt-0.5 text-xs text-slate-500">
          Review the farmers linked to your FPO and keep their details up to
          date.
        </p>
        <NavLink
          to="/fpo-admin/farmers"
          className="mt-4 inline-block rounded-xl bg-[#f5d77f] px-4 py-2 text-xs font-bold text-[#1b5e20] shadow-2xs transition hover:bg-[#eac459] hover:shadow-xs"
        >
          View Linked Farmers →
        </NavLink>
      </section>
    </div>
  );
}