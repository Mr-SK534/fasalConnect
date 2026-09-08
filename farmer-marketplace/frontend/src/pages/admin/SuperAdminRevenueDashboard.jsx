import { useEffect, useState } from "react";
import {
  FiDollarSign,
  FiCreditCard,
  FiTrendingUp,
  FiShield,
  FiCheckCircle,
  FiRefreshCw,
  FiArrowUpRight
} from "react-icons/fi";
import { getSummary } from "../../services/adminService";

export default function SuperAdminRevenueDashboard() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadData = (showSpinner = false) => {
    if (showSpinner) setLoading(true);
    setError("");
    getSummary()
      .then((data) => {
        setSummary(data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.response?.data?.message || "Failed to load SuperAdmin revenue data.");
        setLoading(false);
      });
  };

  useEffect(() => {
    loadData(true);
    const interval = setInterval(() => loadData(false), 5000);
    const handleFocus = () => loadData(false);
    window.addEventListener("focus", handleFocus);
    return () => {
      clearInterval(interval);
      window.removeEventListener("focus", handleFocus);
    };
  }, []);

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="flex items-center gap-3 text-[#1b5e20] font-semibold">
          <FiRefreshCw className="animate-spin text-2xl" />
          <span>Loading SuperAdmin Revenue & Bank Ledger...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-amber-200 pb-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="rounded-full bg-amber-100 px-3 py-0.5 text-xs font-bold uppercase tracking-wider text-amber-800 border border-amber-300">
              🏛️ Platform Revenue & Financial Overview
            </span>
            <span className="rounded-full bg-emerald-100 px-3 py-0.5 text-xs font-bold text-emerald-800 border border-emerald-300">
              Live Bank Settlement
            </span>
          </div>
          <h2 className="mt-2 text-3xl font-black tracking-tight text-[#163820]">
            Platform Revenue & Bank Wallet
          </h2>
          <p className="mt-1 text-sm text-slate-600">
            Monitor extra buyer markup collections, bank account details, and 100% listed price payouts to farmers.
          </p>
        </div>

        <button
          onClick={loadData}
          className="flex items-center gap-2 rounded-xl bg-white px-4 py-2 text-sm font-bold text-[#1b5e20] shadow-sm border border-[#eadaaf] hover:bg-[#faf4e1] transition"
        >
          <FiRefreshCw size={16} />
          <span>Refresh Data</span>
        </button>
      </div>

      {error && (
        <div className="rounded-xl bg-red-50 p-4 border border-red-200 text-sm font-medium text-red-700">
          {error}
        </div>
      )}

      {/* 🏦 SuperAdmin Registered Bank Account Card */}
      <section className="rounded-2xl border border-emerald-300 bg-gradient-to-r from-[#173a23] via-[#1b462a] to-[#0f2818] p-6 text-white shadow-xl space-y-6">
        <div className="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 pb-5">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-amber-400/20 text-amber-300 border border-amber-400/30">
              <FiCreditCard size={24} />
            </div>
            <div>
              <h3 className="text-xl font-extrabold text-white">SuperAdmin Primary Settlement Bank Account</h3>
              <p className="text-xs text-emerald-200/80">
                All platform commission and extra buyer markup is directly settled to this bank account upon order delivery.
              </p>
            </div>
          </div>

          <div className="text-right">
            <p className="text-xs font-bold uppercase tracking-wider text-emerald-300">Total Net SuperAdmin Revenue</p>
            <p className="text-3xl font-black text-amber-300">
              ₹{Number(summary?.totalSuperAdminRevenueRs || 0).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="rounded-xl bg-white/10 p-4 backdrop-blur-md border border-white/10">
            <p className="text-xs font-bold uppercase text-emerald-300 tracking-wider">Account Holder Name</p>
            <p className="mt-1 text-lg font-black text-white">{summary?.superAdminAccountHolderName || summary?.superAdminName || "SuperAdmin Account"}</p>
            <p className="text-xs text-emerald-200/80 mt-0.5">{summary?.superAdminEmail}</p>
          </div>

          <div className="rounded-xl bg-white/10 p-4 backdrop-blur-md border border-white/10">
            <p className="text-xs font-bold uppercase text-emerald-300 tracking-wider">Bank Account Number</p>
            <p className="mt-1 font-mono text-lg font-black text-amber-300 tracking-wide">{summary?.superAdminBankAccountNumber || "N/A"}</p>
            <p className="text-xs font-mono text-emerald-200/80 mt-0.5">IFSC: {summary?.superAdminBankIfsc || "N/A"}</p>
          </div>

          <div className="rounded-xl bg-white/10 p-4 backdrop-blur-md border border-white/10">
            <p className="text-xs font-bold uppercase text-emerald-300 tracking-wider">UPI Handle ID</p>
            <p className="mt-1 font-mono text-lg font-black text-white">{summary?.superAdminUpiId || "N/A"}</p>
            <p className="text-xs text-emerald-200/80 mt-0.5">Instant Real-time Auto Settlement</p>
          </div>

          <div className="rounded-xl bg-white/10 p-4 backdrop-blur-md border border-white/10">
            <p className="text-xs font-bold uppercase text-emerald-300 tracking-wider">100% Farmer Payout Disbursed</p>
            <p className="mt-1 text-lg font-black text-green-300">
              ₹{Number(summary?.totalFarmerPayoutsRs || 0).toLocaleString("en-IN", { minimumFractionDigits: 2 })}
            </p>
            <p className="text-xs text-emerald-200/80 mt-0.5">
              Gross Buyer Collected: ₹{Number(summary?.totalBuyerPaymentsRs || 0).toLocaleString("en-IN")}
            </p>
          </div>
        </div>
      </section>

      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        <div className="rounded-2xl border border-emerald-100 bg-white p-5 shadow-sm hover:shadow-md transition">
          <div className="flex items-center justify-between">
            <span className="text-xs font-extrabold uppercase tracking-wider text-slate-500">Gross Buyer Payments</span>
            <div className="rounded-xl bg-blue-50 p-2 text-blue-600">
              <FiDollarSign size={20} />
            </div>
          </div>
          <p className="mt-3 text-3xl font-black text-slate-900">
            ₹{Number(summary?.totalBuyerPaymentsRs || 0).toLocaleString("en-IN")}
          </p>
          <p className="mt-1 text-xs font-semibold text-slate-500">
            Total money processed through Escrow Wallet
          </p>
        </div>

        <div className="rounded-2xl border border-emerald-100 bg-white p-5 shadow-sm hover:shadow-md transition">
          <div className="flex items-center justify-between">
            <span className="text-xs font-extrabold uppercase tracking-wider text-slate-500">100% Farmer Payouts</span>
            <div className="rounded-xl bg-green-50 p-2 text-green-600">
              <FiCheckCircle size={20} />
            </div>
          </div>
          <p className="mt-3 text-3xl font-black text-emerald-700">
            ₹{Number(summary?.totalFarmerPayoutsRs || 0).toLocaleString("en-IN")}
          </p>
          <p className="mt-1 text-xs font-semibold text-slate-500">
            100% of listed ask price transferred to farmers/FPOs
          </p>
        </div>

        <div className="rounded-2xl border border-amber-200 bg-gradient-to-br from-amber-50 to-amber-100/50 p-5 shadow-sm hover:shadow-md transition">
          <div className="flex items-center justify-between">
            <span className="text-xs font-extrabold uppercase tracking-wider text-amber-900">SuperAdmin Revenue</span>
            <div className="rounded-xl bg-amber-200 p-2 text-amber-900">
              <FiTrendingUp size={20} />
            </div>
          </div>
          <p className="mt-3 text-3xl font-black text-amber-900">
            ₹{Number(summary?.totalSuperAdminRevenueRs || 0).toLocaleString("en-IN", { minimumFractionDigits: 2 })}
          </p>
          <p className="mt-1 text-xs font-semibold text-amber-800">
            Extra markup (8% commission + ₹0.50 logistics margin)
          </p>
        </div>
      </div>

      {/* Per-Order Payout & Markup Breakdown Table */}
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-6 shadow-sm space-y-4">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-lg font-bold text-slate-900">Order Payout Split & SuperAdmin Revenue Ledger</h3>
            <p className="text-xs text-slate-500">
              Detailed breakdown of farmer listed price payout vs extra buyer markup retained in SuperAdmin bank account.
            </p>
          </div>
          <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-bold text-slate-600">
            {summary?.superAdminPayoutBreakdown?.length || 0} Transactions
          </span>
        </div>

        {summary?.superAdminPayoutBreakdown && summary.superAdminPayoutBreakdown.length > 0 ? (
          <div className="overflow-x-auto rounded-xl border border-slate-200">
            <table className="w-full text-left text-sm border-collapse">
              <thead className="bg-slate-50 text-slate-600 font-bold uppercase text-[11px] tracking-wider border-b border-slate-200">
                <tr>
                  <th className="p-3">Order ID</th>
                  <th className="p-3">Farmer</th>
                  <th className="p-3">Buyer</th>
                  <th className="p-3 text-center">Delivered Qty</th>
                  <th className="p-3">Farmer Listed Price</th>
                  <th className="p-3 font-semibold text-emerald-800">Farmer Payout (100%)</th>
                  <th className="p-3">Extra Buyer Markup</th>
                  <th className="p-3 text-right font-black text-amber-900">SuperAdmin Revenue</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 font-medium text-slate-800">
                {summary.superAdminPayoutBreakdown.map((item) => (
                  <tr key={item.orderId} className="hover:bg-amber-50/30 transition">
                    <td className="p-3 font-mono font-bold text-emerald-900">{String(item.orderId).slice(0, 8)}</td>
                    <td className="p-3">{item.farmerName}</td>
                    <td className="p-3 text-slate-600">{item.buyerName}</td>
                    <td className="p-3 text-center font-mono">{item.deliveredKg} kg</td>
                    <td className="p-3 font-mono text-slate-600">₹{item.farmerListedPricePerKg}/kg</td>
                    <td className="p-3 font-mono text-emerald-700 font-bold">
                      ₹{item.farmerTotalPayoutRs.toFixed(2)}
                    </td>
                    <td className="p-3 font-mono text-slate-500">
                      +₹{item.extraBuyerMarkupPerKg.toFixed(2)}/kg
                    </td>
                    <td className="p-3 text-right font-mono font-black text-amber-900">
                      ₹{item.superAdminCollectedRevenueRs.toFixed(2)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="rounded-xl border border-dashed border-slate-200 p-8 text-center text-slate-400">
            <FiShield size={32} className="mx-auto mb-2 opacity-50" />
            <p className="font-semibold text-sm">No payout ledger records available yet.</p>
            <p className="text-xs">Completed delivered orders will automatically log revenue splits here.</p>
          </div>
        )}
      </section>
    </div>
  );
}
