import { useState } from "react";
import { toast } from "react-hot-toast";
import { triggerPaymentSplit } from "../../services/adminService";

export default function PaymentSplits() {
  const [orderId, setOrderId] = useState("");
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const submit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError("");
    try {
      const data = await triggerPaymentSplit(orderId.trim());
      setResults(Array.isArray(data) ? data : data?.items || []);
      toast.success("Payment split calculated");
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
          "Could not calculate split. The order may already be split or unpaid.",
      );
    } finally {
      setLoading(false);
    }
  };
  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <header>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          Platform finance
        </p>
        <h2 className="text-3xl font-black text-[#163820]">Payment Splits</h2>
        <p className="mt-1 text-sm text-slate-500">
          Trigger farmer payouts for completed orders.
        </p>
      </header>
      <section className="rounded-xl border border-blue-100 bg-blue-50 p-5 text-sm text-blue-800">
        Enter a completed order ID to calculate and record how much each farmer
        is owed. This records the split; actual bank transfer requires Razorpay
        Route setup.
      </section>
      <form
        onSubmit={submit}
        className="flex flex-col gap-3 rounded-xl bg-white p-5 shadow-sm sm:flex-row"
      >
        <input
          required
          value={orderId}
          onChange={(event) => setOrderId(event.target.value)}
          placeholder="Order ID (GUID)"
          className="flex-1 rounded-lg border border-gray-200 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400"
        />
        <button
          disabled={loading}
          className="rounded-lg bg-green-600 px-5 py-3 font-medium text-white hover:bg-green-700 disabled:opacity-60"
        >
          {loading ? "Calculating..." : "Calculate Split"}
        </button>
      </form>
      {error && (
        <p className="rounded-lg bg-red-50 p-4 text-red-700">{error}</p>
      )}
      {results.length > 0 && (
        <section className="overflow-x-auto rounded-xl bg-white shadow-sm">
          <table className="w-full border-collapse text-left text-sm">
            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="p-4">Farmer</th>
                <th className="p-4">Amount owed</th>
                <th className="p-4">Transfer status</th>
                <th className="p-4">Transfer ID</th>
              </tr>
            </thead>
            <tbody>
              {results.map((item) => (
                <tr key={item.farmerId} className="border-b border-gray-100">
                  <td className="p-4 font-semibold">{item.farmerName}</td>
                  <td className="p-4">
                    ₹{Number(item.amount || 0).toLocaleString("en-IN")}
                  </td>
                  <td className="p-4">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-semibold ${item.transferStatus === "Completed" ? "bg-green-100 text-green-700" : item.transferStatus === "Failed" ? "bg-red-100 text-red-700" : "bg-yellow-100 text-yellow-800"}`}
                    >
                      {item.transferStatus}
                    </span>
                  </td>
                  <td className="p-4 font-mono">
                    {item.razorpayTransferId || "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}
    </div>
  );
}
