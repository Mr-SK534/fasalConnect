import { useCallback, useEffect, useState } from "react";
import { FiMessageCircle } from "react-icons/fi";
import { toast } from "react-hot-toast";
import { useAuth } from "../../hooks/useAuth";
import {
  getFarmerOrders,
  updateOrderStatus,
} from "../../services/orderService";

const statusStyles = {
  Pending: "bg-yellow-100 text-yellow-800",
  Confirmed: "bg-blue-100 text-blue-800",
  InTransit: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};
const nextStatus = {
  Pending: ["Confirmed", "Confirm Order"],
  Confirmed: ["InTransit", "Mark In Transit"],
  InTransit: ["Delivered", "Mark Delivered"],
};

const formatDate = (value) =>
  new Date(value).toLocaleDateString("en-IN", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });

export default function FarmerOrders() {
  const { user } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(null);
  const [error, setError] = useState("");

  const loadOrders = useCallback(() => {
    if (!user?.id) return Promise.resolve();
    return getFarmerOrders(user.id)
      .then(setOrders)
      .catch((requestError) =>
        setError(
          requestError.response?.data?.message || "Could not load orders.",
        ),
      )
      .finally(() => setLoading(false));
  }, [user]);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  const changeStatus = async (orderId, status) => {
    setUpdating(orderId);
    try {
      await updateOrderStatus(orderId, status);
      setLoading(true);
      await loadOrders();
      toast.success(`Order marked ${status}`);
    } catch (requestError) {
      toast.error(
        requestError.response?.data?.message || "Could not update order.",
      );
    } finally {
      setUpdating(null);
    }
  };

  if (loading)
    return <p className="p-8 text-center text-slate-500">Loading orders...</p>;
  if (error)
    return <p className="rounded-xl bg-red-50 p-4 text-red-700">{error}</p>;
  if (!orders.length)
    return (
      <div className="mx-auto max-w-4xl rounded-2xl border border-dashed border-green-200 bg-white p-12 text-center">
        <div className="text-5xl">📦</div>
        <h2 className="mt-4 text-2xl font-bold text-[#163820]">
          No orders yet
        </h2>
        <p className="mt-2 text-slate-500">
          Incoming buyer orders will appear here.
        </p>
      </div>
    );

  return (
    <div className="mx-auto max-w-5xl space-y-5">
      <div>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          Marketplace fulfilment
        </p>
        <h2 className="text-2xl font-black text-[#163820]">Farmer Orders</h2>
      </div>
      {orders.map((order) => {
        const action = nextStatus[order.status];
        return (
          <article
            key={order.id}
            className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-sm"
          >
            <div className="flex flex-wrap items-start justify-between gap-3 border-b border-slate-100 pb-4">
              <div>
                <h3 className="font-bold text-slate-900">Order #{order.id}</h3>
                <p className="text-xs text-slate-500">
                  {formatDate(order.createdAt)} · Buyer: {order.buyerName}
                </p>
              </div>
              <span
                className={`rounded-full px-3 py-1 text-xs font-bold ${statusStyles[order.status] || "bg-gray-100 text-gray-700"}`}
              >
                {order.status}
              </span>
            </div>
            <div className="divide-y divide-slate-100">
              {order.items.map((item) => (
                <div
                  key={item.id}
                  className="flex justify-between gap-3 py-3 text-sm"
                >
                  <span>
                    {item.cropName} · Qty {item.quantity}
                  </span>
                  <span className="font-semibold">
                    ₹{Number(item.subTotal).toLocaleString("en-IN")}
                  </span>
                </div>
              ))}
            </div>
            <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-4">
              <strong>
                Total share: ₹
                {Number(order.totalAmount).toLocaleString("en-IN")}
              </strong>
              <div className="flex gap-2">
                {order.buyerPhone && (
                  <a
                    target="_blank"
                    rel="noreferrer"
                    href={`https://wa.me/${order.buyerPhone.replace(/\D/g, "")}`}
                    className="inline-flex items-center gap-2 rounded-lg bg-green-50 px-3 py-2 text-xs font-bold text-green-700"
                  >
                    <FiMessageCircle /> WhatsApp
                  </a>
                )}
                {action && (
                  <button
                    disabled={updating === order.id}
                    onClick={() => changeStatus(order.id, action[0])}
                    className="rounded-lg bg-green-600 px-3 py-2 text-xs font-bold text-white hover:bg-green-700"
                  >
                    {updating === order.id ? "Updating..." : action[1]}
                  </button>
                )}
              </div>
            </div>
          </article>
        );
      })}
    </div>
  );
}
