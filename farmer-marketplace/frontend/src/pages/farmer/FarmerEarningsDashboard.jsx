import { useEffect, useState } from "react";
import { useAuth } from "../../hooks/useAuth";
import api from "../../services/api";
import { FiDollarSign, FiClock, FiCheckCircle, FiCreditCard, FiPackage, FiShoppingBag, FiLock } from "react-icons/fi";

export default function FarmerEarningsDashboard() {
  const { user } = useAuth();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchEarnings = async () => {
      if (!user?.id) return;
      try {
        setLoading(true);
        const res = await api.get(`/payments/farmer-earnings/${user.id}`);
        setData(res.data);
      } catch (err) {
        console.error("Failed to load farmer earnings", err);
      } finally {
        setLoading(false);
      }
    };

    fetchEarnings();
  }, [user?.id]);

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-[#2e7d32] border-t-transparent"></div>
      </div>
    );
  }

  const orderLedger = data?.orderLedger || [];
  const bankAccount = data?.bankAccountNumber || data?.bankInfo?.bankAccount || user?.bankAccountNumber || "";
  const bankIfsc = data?.bankIfsc || data?.bankInfo?.bankIfsc || user?.bankIfsc || "";
  const accountHolder = data?.accountHolderName || data?.bankInfo?.accountHolder || user?.accountHolderName || user?.name || "";
  const upiId = data?.upiId || data?.bankInfo?.upiId || user?.upiId || "";

  const totalEarnings = data?.totalEarningsRs ?? data?.totalGrossEarningsRs ?? 0;
  const heldInEscrow = data?.heldInEscrowRs ?? 0;
  const disbursedBankPayouts = data?.disbursedBankPayoutsRs ?? data?.disbursedPayoutsRs ?? 0;
  const deliveredOrdersCount = data?.deliveredOrdersCount ?? data?.totalFulfilledOrdersCount ?? 0;

  const hasVerifiedAccount = Boolean(bankAccount || upiId);

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header Banner */}
      <section className="rounded-2xl bg-gradient-to-r from-[#1b5e20] via-[#2e7d32] to-[#388e3c] p-6 text-white shadow-lg sm:p-8">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <div className="flex items-center gap-2 text-xs font-extrabold uppercase tracking-wider text-[#f5d77f]">
              <FiDollarSign className="text-lg" />
              Farmer Payout & Financial Hub
            </div>
            <h2 className="mt-1 text-2xl font-black sm:text-3xl tracking-tight">
              Your Earnings & Escrow Ledger
            </h2>
            <p className="mt-2 max-w-2xl text-sm leading-relaxed text-emerald-100 font-normal">
              You receive <span className="font-bold text-[#f5d77f]">100% of your listed asking price</span> per kg. Admin commissions & platform operational fees are charged transparently to buyers on top.
            </p>
          </div>
          <div className="rounded-xl bg-white/10 p-4 backdrop-blur-sm border border-white/20 text-right">
            <p className="text-xs uppercase font-extrabold text-emerald-200">Registered Account</p>
            <p className="text-base font-black text-white mt-1">
              {bankAccount
                ? `A/C: ${bankAccount}` 
                : upiId 
                  ? `UPI: ${upiId}` 
                  : "No Bank Details Added"}
            </p>
            <p className="text-xs text-emerald-200 mt-0.5">{bankIfsc ? `IFSC: ${bankIfsc}` : ""}</p>
          </div>
        </div>
      </section>

      {/* Overview Metric Cards */}
      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* Card 1: Total Gross Earnings */}
        <article className="rounded-2xl border border-emerald-200 bg-white p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">
              Total Net Earnings
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-emerald-100 text-emerald-700">
              <FiDollarSign className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-[#1b5e20]">
            ₹{Number(totalEarnings).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
          </p>
          <p className="mt-1 text-xs font-medium text-slate-500">
            100% Asking Price Payouts
          </p>
        </article>

        {/* Card 2: Held in Escrow */}
        <article className="rounded-2xl border border-amber-200 bg-amber-50/50 p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-amber-700">
              Held in Escrow
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-200 text-amber-800">
              <FiLock className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-amber-900">
            ₹{Number(heldInEscrow).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
          </p>
          <p className="mt-1 text-xs font-semibold text-amber-700">
            Protected until delivery
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
            ₹{Number(disbursedBankPayouts).toLocaleString("en-IN", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
          </p>
          <p className="mt-1 text-xs font-semibold text-blue-700">
            Transferred to Bank / UPI
          </p>
        </article>

        {/* Card 4: Fulfilled Orders */}
        <article className="rounded-2xl border border-slate-200 bg-white p-5 shadow-xs transition hover:shadow-md">
          <div className="flex items-center justify-between">
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">
              Fulfilled Items
            </p>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-100 text-slate-700">
              <FiPackage className="text-lg" />
            </div>
          </div>
          <p className="mt-2 text-3xl font-black text-slate-800">
            {deliveredOrdersCount}
          </p>
          <p className="mt-1 text-xs font-medium text-slate-500">
            Paid Produce Line Items
          </p>
        </article>
      </section>

      {/* Account Payout Profile Section */}
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-6 shadow-xs">
        <div className="flex items-center justify-between border-b border-slate-100 pb-4">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-100 text-emerald-800">
              <FiCreditCard className="text-xl" />
            </div>
            <div>
              <h3 className="text-base font-bold text-slate-900">
                Payout Bank Credentials
              </h3>
              <p className="text-xs text-slate-500">
                Escrow payouts are transferred directly to this account upon route completion.
              </p>
            </div>
          </div>
          <span className={`rounded-full px-3 py-1 text-xs font-bold ${
            hasVerifiedAccount
              ? "bg-emerald-100 text-emerald-800"
              : "bg-amber-100 text-amber-800"
          }`}>
            {hasVerifiedAccount ? "✓ Verified Account" : "⚠️ Update Required in Profile"}
          </span>
        </div>

        <div className="mt-4 grid gap-4 sm:grid-cols-2 md:grid-cols-4">
          <div className="rounded-xl bg-slate-50 p-3.5 border border-slate-100">
            <p className="text-[11px] font-semibold text-slate-500 uppercase">Account Holder</p>
            <p className="mt-1 text-sm font-bold text-slate-800">{accountHolder || "Not specified"}</p>
          </div>
          <div className="rounded-xl bg-slate-50 p-3.5 border border-slate-100">
            <p className="text-[11px] font-semibold text-slate-500 uppercase">Bank Account</p>
            <p className="mt-1 text-sm font-bold text-slate-800">{bankAccount || "Not specified"}</p>
          </div>
          <div className="rounded-xl bg-slate-50 p-3.5 border border-slate-100">
            <p className="text-[11px] font-semibold text-slate-500 uppercase">IFSC Code</p>
            <p className="mt-1 text-sm font-bold text-slate-800">{bankIfsc || "Not specified"}</p>
          </div>
          <div className="rounded-xl bg-slate-50 p-3.5 border border-slate-100">
            <p className="text-[11px] font-semibold text-slate-500 uppercase">UPI Handle</p>
            <p className="mt-1 text-sm font-bold text-slate-800">{upiId || "Not specified"}</p>
          </div>
        </div>
      </section>

      {/* Order Payout Ledger */}
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-xs sm:p-6">
        <div className="flex items-center justify-between pb-4">
          <div>
            <h3 className="text-lg font-bold text-[#163820]">
              Produce Payout Ledger
            </h3>
            <p className="text-xs text-slate-500">
              Breakdown of individual item sales and escrow release status.
            </p>
          </div>
          <span className="rounded-xl bg-emerald-50 px-3 py-1.5 text-xs font-bold text-emerald-800">
            {orderLedger.length} Line Items
          </span>
        </div>

        {orderLedger.length ? (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-600">
              <thead className="bg-[#f4e8c5]/40 text-slate-700 uppercase font-extrabold text-[11px]">
                <tr>
                  <th className="py-3 px-4 rounded-l-xl">Order #</th>
                  <th className="py-3 px-4">Date</th>
                  <th className="py-3 px-4">Produce Name</th>
                  <th className="py-3 px-4 text-right">Quantity (kg)</th>
                  <th className="py-3 px-4 text-right">Asking Price (₹/kg)</th>
                  <th className="py-3 px-4 text-right">Your Earnings (₹)</th>
                  <th className="py-3 px-4 text-center rounded-r-xl">Escrow Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 font-medium">
                {orderLedger.map((item, idx) => (
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
                    <td className="py-3.5 px-4 font-bold text-emerald-900">
                      {item.cropName || item.productName || "Produce"}
                    </td>
                    <td className="py-3.5 px-4 text-right font-semibold text-slate-800">
                      {item.quantityKg ?? item.quantityInKg ?? 0} kg
                    </td>
                    <td className="py-3.5 px-4 text-right text-slate-700 font-medium">
                      ₹{Number(item.listedPricePerKg ?? item.farmerAskingPricePerKg ?? 0).toFixed(2)}
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
          <div className="mt-4 rounded-xl border border-dashed border-[#eadaaf] bg-[#fdfbf3] p-8 text-center text-xs font-medium text-slate-500">
            <FiShoppingBag className="mx-auto text-3xl text-slate-400 mb-2" />
            No payout transactions yet. Once buyers place paid orders for your crops, earnings will be tracked here.
          </div>
        )}
      </section>
    </div>
  );
}
