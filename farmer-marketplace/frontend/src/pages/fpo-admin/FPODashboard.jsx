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
    <div className="mx-auto max-w-7xl space-y-10 text-xl font-medium">
      {/* Header Banner */}
      <section className="rounded-3xl bg-gradient-to-r from-[#2e7d32] via-[#246b28] to-[#1b5e20] p-8 text-white shadow-xl sm:p-12">
        <p className="text-xl font-extrabold uppercase tracking-widest text-[#f5d77f]">
          FPO Operations
        </p>
        <h2 className="mt-3 text-4xl font-black sm:text-5xl">
          Grow Your Farmer Network
        </h2>
        <p className="mt-4 max-w-3xl text-xl leading-relaxed text-emerald-100">
          Keep your members supported and their harvests moving through the
          marketplace.
        </p>
      </section>

      {/* Summary Cards */}
      <section className="grid gap-6 sm:grid-cols-3">
        {cards.map(([label, value, hint]) => (
          <article
            key={label}
            className="rounded-3xl border border-[#eadaaf] bg-white/90 p-8 shadow-md backdrop-blur transition hover:shadow-lg"
          >
            <p className="text-lg font-extrabold uppercase tracking-wider text-slate-500">
              {label}
            </p>
            <p className="mt-4 text-5xl font-black text-[#1b5e20]">
              {value ?? "..."}
            </p>
            <p className="mt-3 text-lg font-bold text-slate-600">{hint}</p>
          </article>
        ))}
      </section>

      {/* Manage Members Box */}
      <section className="rounded-3xl border border-[#eadaaf] bg-white p-8 shadow-md sm:p-10">
        <h3 className="text-3xl font-black text-[#163820]">Manage Members</h3>
        <p className="mt-2 text-xl font-semibold text-slate-600">
          Review the farmers linked to your FPO and keep their details up to
          date.
        </p>
        <NavLink
          to="/fpo-admin/farmers"
          className="mt-6 inline-block rounded-2xl bg-[#f5d77f] px-8 py-4 text-xl font-extrabold text-[#1b5e20] shadow-sm transition hover:bg-[#eac459] hover:shadow"
        >
          View Linked Farmers →
        </NavLink>
      </section>
    </div>
  );
}