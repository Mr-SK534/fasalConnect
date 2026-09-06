import { useCallback, useEffect, useState } from "react";
import { FiMessageCircle } from "react-icons/fi";
import { toast } from "react-hot-toast";
import api from "../../services/api";
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
const transitions = {
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

function OrderCard({ order, updating, onStatus }) {
  const action = transitions[order.status];
  return (
    <article className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex flex-wrap justify-between gap-3">
        <div>
          <p className="font-bold text-slate-900">Order #{order.id}</p>
          <p className="text-xs text-slate-500">
            {formatDate(order.createdAt)} · Buyer: {order.buyerName}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <span
            className={`rounded-full px-2 py-1 text-xs font-semibold ${statusStyles[order.status]}`}
          >
            {order.status}
          </span>
          {order.isBulkOrder && (
            <span className="rounded-full bg-purple-100 px-2 py-1 text-xs font-semibold text-purple-700">
              Aggregated across multiple farmers
            </span>
          )}
        </div>
      </div>
      <div className="mt-3 divide-y divide-slate-100">
        {order.items.map((item) => (
          <div key={item.id} className="flex justify-between py-2 text-sm">
            <span>
              {item.cropName} × {item.quantity} · ₹{item.priceAtOrderTime}
            </span>
            <strong>₹{Number(item.subTotal).toLocaleString("en-IN")}</strong>
          </div>
        ))}
      </div>
      <div className="mt-3 flex flex-wrap items-center justify-between gap-2 border-t pt-3">
        <strong>
          Farmer share: ₹{Number(order.totalAmount).toLocaleString("en-IN")}
        </strong>
        <div className="flex gap-2">
          {order.buyerPhone && (
            <a
              target="_blank"
              rel="noreferrer"
              href={`https://wa.me/${order.buyerPhone.replace(/\D/g, "")}`}
              className="inline-flex items-center gap-1 rounded-lg bg-green-50 px-3 py-2 text-xs font-bold text-green-700"
            >
              <FiMessageCircle /> WhatsApp
            </a>
          )}
          {action && (
            <button
              type="button"
              disabled={updating === order.id}
              onClick={() => onStatus(order.id, action[0])}
              className="rounded-lg bg-green-600 px-3 py-2 text-xs font-bold text-white hover:bg-green-700"
            >
              {updating === order.id ? "Updating..." : action[1]}
            </button>
          )}
        </div>
      </div>
    </article>
  );
}

export default function FPOOrders() {
  const { user } = useAuth();
  const [tab, setTab] = useState("mine");
  const [myOrders, setMyOrders] = useState([]);
  const [farmers, setFarmers] = useState([]);
  const [ordersByFarmer, setOrdersByFarmer] = useState({});
  const [updating, setUpdating] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    if (!user?.id) return;
    try {
      const [mine, farmerResponse] = await Promise.all([
        getFarmerOrders(user.id),
        api.get(`/fpo/${user.id}/farmers`),
      ]);
      const linked = Array.isArray(farmerResponse.data)
        ? farmerResponse.data
        : [];
      const entries = await Promise.all(
        linked.map(async (farmer) => [
          farmer.id,
          await getFarmerOrders(farmer.id),
        ]),
      );
      setMyOrders(mine);
      setFarmers(linked);
      setOrdersByFarmer(Object.fromEntries(entries));
    } catch (requestError) {
      setError(
        requestError.response?.data?.message || "Could not load FPO orders.",
      );
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    // Synchronize this view with the authenticated order API.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);
  const onStatus = async (id, status) => {
    setUpdating(id);
    try {
      await updateOrderStatus(id, status);
      await load();
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
    return <p className="rounded-lg bg-red-50 p-4 text-red-700">{error}</p>;
  return (
    <div className="mx-auto max-w-6xl space-y-5">
      <div>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          FPO fulfilment
        </p>
        <h2 className="text-2xl font-black text-[#163820]">Orders</h2>
      </div>
      <div className="flex gap-6 border-b border-slate-200">
        <button
          type="button"
          onClick={() => setTab("mine")}
          className={`border-b-2 px-2 py-3 text-sm font-bold ${tab === "mine" ? "border-green-600 text-green-700" : "border-transparent text-slate-500"}`}
        >
          My Orders
        </button>
        <button
          type="button"
          onClick={() => setTab("linked")}
          className={`border-b-2 px-2 py-3 text-sm font-bold ${tab === "linked" ? "border-green-600 text-green-700" : "border-transparent text-slate-500"}`}
        >
          Linked Farmers' Orders
        </button>
      </div>
      {tab === "mine" ? (
        <div className="space-y-4">
          {myOrders.length ? (
            myOrders.map((order) => (
              <OrderCard
                key={order.id}
                order={order}
                updating={updating}
                onStatus={onStatus}
              />
            ))
          ) : (
            <p className="rounded-xl bg-white p-10 text-center text-slate-500">
              No orders for your products.
            </p>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {farmers.map((farmer) => (
            <details
              key={farmer.id}
              className="rounded-xl border border-slate-200 bg-white"
            >
              <summary className="cursor-pointer list-none px-5 py-4 font-bold">
                {farmer.name}{" "}
                <span className="text-xs font-normal text-slate-500">
                  ({(ordersByFarmer[farmer.id] || []).length} orders)
                </span>
              </summary>
              <div className="space-y-4 border-t p-4">
                {(ordersByFarmer[farmer.id] || []).length ? (
                  ordersByFarmer[farmer.id].map((order) => (
                    <OrderCard
                      key={order.id}
                      order={order}
                      updating={updating}
                      onStatus={onStatus}
                    />
                  ))
                ) : (
                  <p className="text-center text-sm text-slate-500">
                    No orders.
                  </p>
                )}
              </div>
            </details>
          ))}
          {!farmers.length && (
            <p className="rounded-xl bg-white p-10 text-center text-slate-500">
              No linked farmers found.
            </p>
          )}
        </div>
      )}
    </div>
  );
}
