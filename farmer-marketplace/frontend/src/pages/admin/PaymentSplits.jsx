import { useEffect, useState } from "react";
import { toast } from "react-hot-toast";
import { FiCheckCircle, FiClock, FiDollarSign, FiRefreshCw, FiArrowRight } from "react-icons/fi";
import { getAdminOrders, triggerPaymentSplit } from "../../services/adminService";

const unwrap = (data) =>
  Array.isArray(data) ? data : data?.items || data?.orders || data?.results || [];

export default function PaymentSplits() {
  const [orders, setOrders] = useState([]);
  const [selectedOrderId, setSelectedOrderId] = useState("");
  const [results, setResults] = useState([]);
  const [loadingOrders, setLoadingOrders] = useState(true);
  const [calculating, setCalculating] = useState(false);
  const [error, setError] = useState("");

  const loadOrders = () => {
    setLoadingOrders(true);
    getAdminOrders({ page: 1, pageSize: 100 })
      .then((data) => {
        const list = unwrap(data);
        setOrders(list);
        if (list.length > 0 && !selectedOrderId) {
          setSelectedOrderId(list[0].id);
        }
        setLoadingOrders(false);
      })
      .catch((err) => {
        console.error("Failed to load orders for payment split:", err);
        setLoadingOrders(false);
      });
  };

  useEffect(() => {
    loadOrders();
  }, []);

  const handleSplit = async (targetId) => {
    const idToUse = targetId || selectedOrderId;
    if (!idToUse) {
      toast.error("Please select or enter a valid Order ID");
      return;
    }

    setCalculating(true);
    setError("");
    setResults([]);

    try {
      const data = await triggerPaymentSplit(idToUse.trim());
      const splitList = Array.isArray(data) ? data : data?.items || data?.results || [];
      setResults(splitList);
      toast.success("Payment split calculated & disbursed successfully!");
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
          "Could not calculate split for this order.",
      );
    } finally {
      setCalculating(false);
    }
  };

  const selectedOrder = orders.find((o) => o.id === selectedOrderId);

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      {/* Header */}
      <header className="flex flex-wrap items-center justify-between gap-4 border-b border-gray-200 pb-4">
        <div>
          <p className="text-xs font-bold uppercase tracking-wider text-green-700">
            Platform Finance & Disbursal
          </p>
          <h2 className="text-3xl font-black text-[#163820]">Payment Splits</h2>
          <p className="mt-1 text-sm text-slate-500">
            Calculate and record farmer payment splits for platform marketplace orders.
          </p>
        </div>

        <button
          type="button"
          onClick={loadOrders}
          className="flex items-center gap-2 rounded-xl bg-white px-4 py-2 text-sm font-bold text-green-800 shadow-sm border border-green-200 hover:bg-green-50 transition"
        >
          <FiRefreshCw size={16} className={loadingOrders ? "animate-spin" : ""} />
          <span>Refresh Orders</span>
        </button>
      </header>

      {/* Guidance Card */}
      <section className="rounded-xl border border-blue-200 bg-blue-50/80 p-5 text-sm text-blue-900 shadow-xs leading-relaxed">
        <p className="font-semibold text-blue-950">💡 Automatic Double-Entry Ledger Settlement</p>
        <p className="mt-1 text-xs text-blue-800">
          Selecting an order below triggers the payment split calculation. 100% of the farmer's listed ask price is allocated to the farmer, while extra buyer markup is retained in the SuperAdmin platform bank account.
        </p>
      </section>

      {/* Main Order Selection & Manual Entry Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column: Order Selector & Input */}
        <div className="lg:col-span-1 space-y-4 rounded-xl bg-white p-5 shadow-sm border border-gray-100">
          <h3 className="text-base font-bold text-slate-900">Select Order to Split</h3>

          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-1">
              Select Recent Order
            </label>
            <select
              value={selectedOrderId}
              onChange={(e) => setSelectedOrderId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 p-2.5 text-sm font-medium text-slate-800 focus:border-green-500 focus:outline-none"
            >
              <option value="">-- Choose an Order --</option>
              {orders.map((o) => (
                <option key={o.id} value={o.id}>
                  Order #{String(o.id).slice(0, 8)} - {o.cropName || o.items?.[0]?.cropName || "Crop"} (₹{o.totalAmount})
                </option>
              ))}
            </select>
          </div>

          <div className="relative flex items-center justify-center py-2">
            <span className="bg-white px-2 text-xs font-bold text-slate-400 uppercase">Or Manual Entry</span>
            <div className="absolute inset-0 -z-10 flex items-center"><div className="w-full border-t border-gray-200" /></div>
          </div>

          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-500 mb-1">
              Manual Order GUID
            </label>
            <input
              type="text"
              value={selectedOrderId}
              onChange={(e) => setSelectedOrderId(e.target.value)}
              placeholder="e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6"
              className="w-full rounded-lg border border-gray-300 p-2.5 text-xs font-mono text-slate-800 focus:border-green-500 focus:outline-none"
            />
          </div>

          <button
            type="button"
            disabled={calculating || !selectedOrderId}
            onClick={() => handleSplit(selectedOrderId)}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#1b5e20] px-4 py-3 font-bold text-white shadow-md hover:bg-[#144718] transition disabled:opacity-50"
          >
            {calculating ? (
              <>
                <FiRefreshCw className="animate-spin" />
                <span>Calculating Split...</span>
              </>
            ) : (
              <>
                <FiDollarSign size={18} />
                <span>Trigger Payment Split</span>
              </>
            )}
          </button>
        </div>

        {/* Right Column: Order Details & Split Results */}
        <div className="lg:col-span-2 space-y-4">
          {selectedOrder && (
            <div className="rounded-xl border border-emerald-100 bg-emerald-50/50 p-4 text-sm text-slate-800 flex flex-wrap items-center justify-between gap-4">
              <div>
                <span className="text-xs font-bold uppercase text-emerald-800">Selected Order Summary</span>
                <h4 className="font-extrabold text-slate-900 mt-0.5">
                  Order #{String(selectedOrder.id).slice(0, 8)} ({selectedOrder.cropName || "Produce"})
                </h4>
                <p className="text-xs text-slate-600 mt-0.5">
                  Buyer: <span className="font-semibold text-slate-900">{selectedOrder.buyerName}</span> | Status: <span className="font-semibold text-green-700">{selectedOrder.status}</span>
                </p>
              </div>
              <div className="text-right">
                <p className="text-xs font-semibold text-slate-500">Order Total Paid</p>
                <p className="text-2xl font-black text-emerald-800">₹{Number(selectedOrder.totalAmount || 0).toLocaleString("en-IN")}</p>
              </div>
            </div>
          )}

          {error && (
            <p className="rounded-xl bg-red-50 p-4 text-sm font-medium text-red-700 border border-red-200">
              {error}
            </p>
          )}

          {results.length > 0 ? (
            <section className="overflow-x-auto rounded-xl bg-white shadow-sm border border-gray-200">
              <div className="p-4 border-b border-gray-100 flex items-center justify-between">
                <h3 className="font-bold text-slate-900">Farmer Disbursal Ledger</h3>
                <span className="rounded-full bg-green-100 px-3 py-0.5 text-xs font-bold text-green-800">
                  {results.length} Split Entry
                </span>
              </div>
              <table className="w-full border-collapse text-left text-sm">
                <thead className="bg-slate-50 text-xs uppercase text-slate-500 font-bold tracking-wider border-b border-gray-200">
                  <tr>
                    <th className="p-4">Farmer Name</th>
                    <th className="p-4">Amount Owed (100% Asking)</th>
                    <th className="p-4">Transfer Status</th>
                    <th className="p-4">Reference Transfer ID</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {results.map((item, idx) => (
                    <tr key={item.farmerId || idx} className="hover:bg-slate-50/50">
                      <td className="p-4 font-semibold text-slate-900">{item.farmerName || "Farmer Account"}</td>
                      <td className="p-4 font-mono font-bold text-green-700">
                        ₹{Number(item.amount || 0).toLocaleString("en-IN", { minimumFractionDigits: 2 })}
                      </td>
                      <td className="p-4">
                        <span
                          className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-xs font-bold ${
                            item.transferStatus === "Completed"
                              ? "bg-green-100 text-green-800"
                              : "bg-yellow-100 text-yellow-800"
                          }`}
                        >
                          <FiCheckCircle size={12} />
                          {item.transferStatus || "Completed"}
                        </span>
                      </td>
                      <td className="p-4 font-mono text-xs text-slate-600">
                        {item.razorpayTransferId || `trf_${String(item.farmerId).slice(0, 8)}`}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </section>
          ) : (
            <div className="rounded-xl border border-dashed border-gray-300 bg-white p-8 text-center text-slate-500">
              <FiDollarSign size={36} className="mx-auto mb-2 opacity-40 text-green-700" />
              <p className="font-bold text-slate-800">No payment split calculated yet for this order</p>
              <p className="text-xs text-slate-500 mt-1">Select an order from the left and click "Trigger Payment Split" to execute.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
