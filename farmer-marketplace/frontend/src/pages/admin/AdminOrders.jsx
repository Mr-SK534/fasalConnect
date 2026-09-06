import { useCallback, useEffect, useState } from "react";
import { toast } from "react-hot-toast";
import {
  getAdminOrders,
  overrideOrderStatus,
} from "../../services/adminService";

const statuses = [
  "Pending",
  "Confirmed",
  "InTransit",
  "Delivered",
  "Cancelled",
];
const statusStyles = {
  Pending: "bg-yellow-100 text-yellow-800",
  Confirmed: "bg-blue-100 text-blue-800",
  InTransit: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};
const unwrap = (data) =>
  Array.isArray(data) ? data : data?.items || data?.orders || [];
const metaOf = (data, page, size, count) => ({
  page: data?.page || page,
  pageSize: data?.pageSize || size,
  totalCount: data?.totalCount ?? count,
  totalPages:
    data?.totalPages ||
    Math.max(1, Math.ceil((data?.totalCount ?? count) / size)),
});
const summary = (order) =>
  order.items
    ?.map((item) => `${item.cropName} ${item.quantity}${item.unit || ""}`)
    .join(", ") || "-";

export default function AdminOrders() {
  const [orders, setOrders] = useState([]);
  const [filters, setFilters] = useState({
    status: "",
    dateFrom: "",
    dateTo: "",
  });
  const [page, setPage] = useState(1);
  const [meta, setMeta] = useState({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 1,
  });
  const [detail, setDetail] = useState(null);
  const [override, setOverride] = useState(null);
  const [overrideStatus, setOverrideStatus] = useState("Confirmed");
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await getAdminOrders({
        ...filters,
        status: filters.status || undefined,
        page,
        pageSize: 20,
      });
      setOrders(unwrap(data));
      setMeta(metaOf(data, page, 20, unwrap(data).length));
    } catch (requestError) {
      setError(
        requestError.response?.data?.message || "Could not load orders.",
      );
    } finally {
      setLoading(false);
    }
  }, [filters, page]);
  useEffect(() => {
    // Synchronize the table with the authenticated admin API.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);
  const submitOverride = async (event) => {
    event.preventDefault();
    try {
      await overrideOrderStatus(override.id, overrideStatus, note);
      toast.success("Order status updated");
      setOverride(null);
      setNote("");
      await load();
    } catch (requestError) {
      toast.error(
        requestError.response?.data?.message || "Could not override status.",
      );
    }
  };
  return (
    <div className="mx-auto max-w-7xl space-y-5">
      <header>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          Platform control
        </p>
        <h2 className="text-3xl font-black text-[#163820]">All Orders</h2>
      </header>
      <div className="grid gap-3 sm:grid-cols-3">
        <select
          value={filters.status}
          onChange={(event) => {
            setPage(1);
            setFilters({ ...filters, status: event.target.value });
          }}
          className="rounded-lg border border-gray-200 bg-white px-4 py-3"
        >
          <option value="">All statuses</option>
          {statuses.map((status) => (
            <option key={status}>{status}</option>
          ))}
        </select>
        <input
          type="date"
          value={filters.dateFrom}
          onChange={(event) => {
            setPage(1);
            setFilters({ ...filters, dateFrom: event.target.value });
          }}
          className="rounded-lg border border-gray-200 bg-white px-4 py-3"
        />
        <input
          type="date"
          value={filters.dateTo}
          onChange={(event) => {
            setPage(1);
            setFilters({ ...filters, dateTo: event.target.value });
          }}
          className="rounded-lg border border-gray-200 bg-white px-4 py-3"
        />
      </div>
      {error && (
        <p className="rounded-lg bg-red-50 p-4 text-red-700">{error}</p>
      )}
      <section className="overflow-x-auto rounded-xl border border-gray-100 bg-white shadow-sm">
        <table className="w-full border-collapse text-left text-sm">
          <thead className="bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              {[
                "Order",
                "Buyer",
                "Items",
                "Total",
                "Type",
                "Status",
                "Created",
                "Actions",
              ].map((heading) => (
                <th key={heading} className="p-3">
                  {heading}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan="8" className="p-8 text-center">
                  Loading orders...
                </td>
              </tr>
            ) : (
              orders.map((order) => (
                <tr
                  key={order.id}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="p-3 font-mono">
                    {String(order.id).slice(0, 8)}
                  </td>
                  <td className="p-3">
                    {order.buyerName}
                    <br />
                    <span className="text-xs text-slate-500">
                      {order.buyerPhone || "—"}
                    </span>
                  </td>
                  <td className="max-w-xs p-3">{summary(order)}</td>
                  <td className="p-3">
                    ₹{Number(order.totalAmount || 0).toLocaleString("en-IN")}
                    {order.isBulkOrder && (
                      <span className="ml-2 rounded-full bg-purple-100 px-2 py-1 text-xs font-semibold text-purple-700">
                        Bulk
                      </span>
                    )}
                  </td>
                  <td className="p-3">
                    <span className="rounded-full bg-blue-100 px-2 py-1 text-xs text-blue-700">
                      {order.deliveryType}
                    </span>
                  </td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-semibold ${statusStyles[order.status] || "bg-gray-100"}`}
                    >
                      {order.status}
                    </span>
                  </td>
                  <td className="p-3">
                    {new Date(order.createdAt).toLocaleDateString("en-IN")}
                  </td>
                  <td className="flex gap-2 p-3">
                    <button
                      type="button"
                      onClick={() => setDetail(order)}
                      className="rounded-lg bg-gray-100 px-3 py-1 text-sm"
                    >
                      View
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setOverride(order);
                        setOverrideStatus(order.status);
                      }}
                      className="rounded-lg bg-amber-100 px-3 py-1 text-sm text-amber-800"
                    >
                      Override
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </section>
      <div className="flex justify-between text-sm text-slate-600">
        <span>
          Showing {orders.length ? (meta.page - 1) * meta.pageSize + 1 : 0}-
          {Math.min(meta.page * meta.pageSize, meta.totalCount)} of{" "}
          {meta.totalCount}
        </span>
        <div className="flex gap-2">
          <button
            disabled={page <= 1}
            onClick={() => setPage(page - 1)}
            className="rounded-lg border px-3 py-1 disabled:opacity-40"
          >
            Previous
          </button>
          <span>
            Page {meta.page} of {meta.totalPages}
          </span>
          <button
            disabled={page >= meta.totalPages}
            onClick={() => setPage(page + 1)}
            className="rounded-lg border px-3 py-1 disabled:opacity-40"
          >
            Next
          </button>
        </div>
      </div>
      {detail && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl">
            <div className="flex justify-between">
              <h3 className="text-xl font-bold">Order details</h3>
              <button onClick={() => setDetail(null)}>×</button>
            </div>
            <p className="mt-4 text-sm">
              Buyer: {detail.buyerName} · {detail.buyerPhone || "No phone"}
            </p>
            <p className="text-sm">
              Address: {detail.deliveryAddress || "Pickup"}
            </p>
            <div className="mt-4 divide-y">
              {detail.items?.map((item) => (
                <div
                  key={item.id}
                  className="flex justify-between py-3 text-sm"
                >
                  <span>
                    {item.cropName} · {item.farmerName} · Qty {item.quantity} ·
                    ₹{item.priceAtOrderTime}
                  </span>
                  <strong>₹{item.subTotal}</strong>
                </div>
              ))}
            </div>
            <p className="mt-4 font-bold">Total: ₹{detail.totalAmount}</p>
          </div>
        </div>
      )}
      {override && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <form
            onSubmit={submitOverride}
            className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-2xl"
          >
            <h3 className="text-xl font-bold">Override status</h3>
            <select
              value={overrideStatus}
              onChange={(event) => setOverrideStatus(event.target.value)}
              className="mt-4 w-full rounded-lg border px-4 py-3"
            >
              {statuses.map((status) => (
                <option key={status}>{status}</option>
              ))}
            </select>
            <textarea
              required
              value={note}
              onChange={(event) => setNote(event.target.value)}
              rows="4"
              placeholder="Reason for override"
              className="mt-3 w-full rounded-lg border px-4 py-3"
            />
            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setOverride(null)}
                className="rounded-lg border px-4 py-2"
              >
                Cancel
              </button>
              <button className="rounded-lg bg-amber-500 px-4 py-2 font-medium text-white">
                Confirm Override
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
