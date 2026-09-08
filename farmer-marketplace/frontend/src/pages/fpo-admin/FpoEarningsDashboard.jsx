import { useEffect, useState } from "react";
import { useAuth } from "../../hooks/useAuth";
import api from "../../services/api";
import { FiDollarSign, FiUsers, FiLock, FiCheckCircle, FiPackage, FiShoppingBag, FiLayers, FiList } from "react-icons/fi";

export default function FpoEarningsDashboard() {
  const { user } = useAuth();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("farmers"); // "farmers" | "ledger"

  useEffect(() => {
    const fetchFpoEarnings = async () => {
      if (!user?.id) return;
      try {
        setLoading(true);
        const res = await api.get(`/fpo/${user.id}/earnings-detail`);
        setData(res.data);
      } catch (err) {
        console.error("Failed to load FPO earnings", err);
      } finally {
        setLoading(false);
      }
    };

    fetchFpoEarnings();
  }, [user?.id]);

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-[#2e7d32] border-t-transparent"></div>
      </div>
    );
  }

  const farmerBreakdown = data?.farmerBreakdown || [];
  const networkOrderLedger = data?.networkOrderLedger || [];

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header Banner */}
      <section className="rounded-2xl bg-gradient-to-r from-[#1b5e20] via-[#246b28] to-[#2e7d32] p-6 text-white shadow-lg sm:p-8">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <div className="flex items-center gap-2 text-xs font-extrabold uppercase tracking-wider text-[#f5d77f]">
              <FiLayers className="text-lg" />
              FPO Collective Financial Overview
            </div>
            <h2 className="mt-1 text-2xl font-black sm:text-3xl tracking-tight">
              {data?.fpoName || "FPO Network"} Earnings & Payout Ledger
            </h2>
            <p className="mt-2 max-w-2xl text-sm leading-relaxed text-emerald-100 font-normal">
              Track aggregate revenue for all linked farmers. Farmers receive <span className="font-bold text-[#f5d77f]">100% of their listed asking price</span> with transparent buyer-side fees.
            </p>
          </div>
          <div className="rounded-xl bg-white/10 p-4 backdrop-blur-sm border border-white/20 text-right">
            <p className="text-xs uppercase font-extrabold text-emerald-200">Network Reach</p>
            <p className="text-2xl font-black text-white mt-1">
              {data?.activeFarmersCount || 0} / {data?.linkedFarmersCount || 0}
            </p>
            <p className="text-xs text-emerald-200 mt-0.5">Active Earning Farmers</p>
          </div>
        </div>
      </section>

      {/* Overview Metric Cards */}
      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* Card 1: Total Network Gross Earnings */}
        <article className="rounded-2xl border border-emerald-200 bg-white p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">
              Total Network Earnings
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-emerald-100 text-emerald-700">
              <FiDollarSign className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-[#1b5e20]">
            ₹{Number(data?.totalNetworkEarningsRs || 0).toLocaleString("en-IN")}
          </p>
          <p className="mt-1 text-xs font-medium text-slate-500">
            Combined Farmer Asking Price Payouts
          </p>
        </article>

        {/* Card 2: Network Held in Escrow */}
        <article className="rounded-2xl border border-amber-200 bg-amber-50/50 p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-amber-700">
              Network Escrow Held
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-200 text-amber-800">
              <FiLock className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-amber-900">
            ₹{Number(data?.heldInEscrowRs || 0).toLocaleString("en-IN")}
          </p>
          <p className="mt-1 text-xs font-semibold text-amber-700">
            Pending Delivery Completion
          </p>
        </article>

        {/* Card 3: Disbursed Payouts */}
        <article className="rounded-2xl border border-blue-200 bg-blue-50/50 p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-blue-700">
              Disbursed Payouts
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-200 text-blue-800">
              <FiCheckCircle className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-blue-900">
            ₹{Number(data?.disbursedPayoutsRs || 0).toLocaleString("en-IN")}
          </p>
          <p className="mt-1 text-xs font-semibold text-blue-700">
            Transferred to Farmer Accounts
          </p>
        </article>

        {/* Card 4: Fulfilled Orders Count */}
        <article className="rounded-2xl border border-slate-200 bg-white p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">
              Fulfilled Orders
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-100 text-slate-700">
              <FiPackage className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-slate-800">
            {data?.totalFulfilledOrdersCount || 0}
          </p>
          <p className="mt-1 text-xs font-medium text-slate-500">
            Across {data?.linkedFarmersCount || 0} Linked Farmers
          </p>
        </article>
      </section>

      {/* Tabs Switcher */}
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-xs sm:p-6">
        <div className="flex flex-wrap items-center justify-between border-b border-slate-100 pb-4 gap-4">
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setActiveTab("farmers")}
              className={`flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition ${
                activeTab === "farmers"
                  ? "bg-[#1b5e20] text-white shadow-xs"
                  : "bg-slate-100 text-slate-600 hover:bg-slate-200"
              }`}
            >
              <FiUsers className="text-sm" />
              Farmer Earnings Breakdown ({farmerBreakdown.length})
            </button>
            <button
              type="button"
              onClick={() => setActiveTab("ledger")}
              className={`flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition ${
                activeTab === "ledger"
                  ? "bg-[#1b5e20] text-white shadow-xs"
                  : "bg-slate-100 text-slate-600 hover:bg-slate-200"
              }`}
            >
              <FiList className="text-sm" />
              Network Order Ledger ({networkOrderLedger.length})
            </button>
          </div>
        </div>

        {/* Tab 1: Farmer Earnings Breakdown Table */}
        {activeTab === "farmers" && (
          <div className="mt-4">
            {farmerBreakdown.length ? (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-xs text-slate-600">
                  <thead className="bg-[#f4e8c5]/40 text-slate-700 uppercase font-extrabold text-[11px]">
                    <tr>
                      <th className="py-3 px-4 rounded-l-xl">Farmer Name</th>
                      <th className="py-3 px-4">Contact Phone</th>
                      <th className="py-3 px-4 text-center">Orders</th>
                      <th className="py-3 px-4 text-right">Total Quantity (kg)</th>
                      <th className="py-3 px-4 text-right">Gross Earnings (₹)</th>
                      <th className="py-3 px-4">Bank Account</th>
                      <th className="py-3 px-4 rounded-r-xl">UPI ID</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 font-medium">
                    {farmerBreakdown.map((f) => (
                      <tr key={f.farmerId} className="hover:bg-emerald-50/30 transition-colors">
                        <td className="py-3.5 px-4 font-bold text-slate-900">
                          {f.farmerName}
                        </td>
                        <td className="py-3.5 px-4 text-slate-600">
                          {f.phone}
                        </td>
                        <td className="py-3.5 px-4 text-center">
                          <span className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-bold ${
                            f.totalOrders > 0 ? "bg-emerald-100 text-emerald-800" : "bg-slate-100 text-slate-500"
                          }`}>
                            {f.totalOrders}
                          </span>
                        </td>
                        <td className="py-3.5 px-4 text-right font-semibold text-slate-800">
                          {f.totalQuantityKg} kg
                        </td>
                        <td className="py-3.5 px-4 text-right font-black text-[#1b5e20] text-sm">
                          ₹{Number(f.totalGrossEarningsRs).toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                        </td>
                        <td className="py-3.5 px-4 font-mono text-slate-700 text-[11px]">
                          {f.bankAccount}
                        </td>
                        <td className="py-3.5 px-4 font-mono text-slate-700 text-[11px]">
                          {f.upiId}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="rounded-xl border border-dashed border-[#eadaaf] bg-[#fdfbf3] p-8 text-center text-xs font-medium text-slate-500">
                <FiUsers className="mx-auto text-3xl text-slate-400 mb-2" />
                No farmers linked to this FPO yet.
              </div>
            )}
          </div>
        )}

        {/* Tab 2: Network Order Ledger */}
        {activeTab === "ledger" && (
          <div className="mt-4">
            {networkOrderLedger.length ? (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-xs text-slate-600">
                  <thead className="bg-[#f4e8c5]/40 text-slate-700 uppercase font-extrabold text-[11px]">
                    <tr>
                      <th className="py-3 px-4 rounded-l-xl">Order #</th>
                      <th className="py-3 px-4">Date</th>
                      <th className="py-3 px-4">Farmer</th>
                      <th className="py-3 px-4">Produce</th>
                      <th className="py-3 px-4 text-right">Quantity</th>
                      <th className="py-3 px-4 text-right">Asking Price</th>
                      <th className="py-3 px-4 text-right">Farmer Payout (₹)</th>
                      <th className="py-3 px-4 text-center rounded-r-xl">Escrow Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 font-medium">
                    {networkOrderLedger.map((item, idx) => (
                      <tr key={item.orderItemId || item.orderId || idx} className="hover:bg-emerald-50/30 transition-colors">
                        <td className="py-3.5 px-4 font-bold text-slate-900">
                          #{item.orderNumber || (item.orderId ? item.orderId.substring(0, 8).toUpperCase() : `ORD-${idx+1}`)}
                        </td>
                        <td className="py-3.5 px-4 text-slate-500 whitespace-nowrap">
                          {new Date(item.createdAt || item.orderDate || Date.now()).toLocaleDateString("en-IN", {
                            day: "numeric",
                            month: "short",
                            year: "numeric"
                          })}
                        </td>
                        <td className="py-3.5 px-4 font-semibold text-slate-800">
                          {item.farmerName}
                        </td>
                        <td className="py-3.5 px-4 font-bold text-emerald-900">
                          {item.cropName || item.productName || "Produce"}
                        </td>
                        <td className="py-3.5 px-4 text-right font-semibold text-slate-800">
                          {item.quantityKg ?? item.quantityInKg ?? 0} kg
                        </td>
                        <td className="py-3.5 px-4 text-right text-slate-700">
                          ₹{Number(item.listedPricePerKg ?? item.farmerAskingPricePerKg ?? 0).toFixed(2)} / kg
                        </td>
                        <td className="py-3.5 px-4 text-right font-black text-[#1b5e20] text-sm">
                          ₹{Number(item.totalFarmerEarningsRs ?? item.netFarmerEarnings ?? 0).toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                        </td>
                        <td className="py-3.5 px-4 text-center">
                          <span className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold ${
                            item.escrowStatus === "Disbursed"
                              ? "bg-blue-100 text-blue-800"
                              : "bg-amber-100 text-amber-800"
                          }`}>
                            {item.escrowStatus === "Disbursed" ? (
                              <>
                                <FiCheckCircle className="text-xs" /> Disbursed
                              </>
                            ) : (
                              <>
                                <FiLock className="text-xs" /> Escrow Held
                              </>
                            )}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="rounded-xl border border-dashed border-[#eadaaf] bg-[#fdfbf3] p-8 text-center text-xs font-medium text-slate-500">
                <FiShoppingBag className="mx-auto text-3xl text-slate-400 mb-2" />
                No network orders recorded yet.
              </div>
            )}
          </div>
        )}
      </section>
    </div>
  );
}
