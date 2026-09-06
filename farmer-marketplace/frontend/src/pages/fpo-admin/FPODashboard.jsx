import { useCallback, useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
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

export default function FPODashboard() {
  const { user } = useAuth();
  const [farmers, setFarmers] = useState([]);
  const [ordersByFarmer, setOrdersByFarmer] = useState({});
  const [earnings, setEarnings] = useState(null);
  const [openFarmers, setOpenFarmers] = useState({});
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(null);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    if (!user?.id) return;
    try {
      const [farmerResponse, earningsResponse] = await Promise.all([
        api.get(`/fpo/${user.id}/farmers`),
        api.get(`/fpo/${user.id}/earnings`),
      ]);
      const linkedFarmers = Array.isArray(farmerResponse.data)
        ? farmerResponse.data
        : [];
      setFarmers(linkedFarmers);
      const orderEntries = await Promise.all(
        linkedFarmers.map(async (farmer) => [
          farmer.id,
          await getFarmerOrders(farmer.id),
        ]),
      );
      setOrdersByFarmer(Object.fromEntries(orderEntries));
      setEarnings(earningsResponse.data);
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
          "Could not load FPO operations.",
      );
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    // The loader synchronizes this page with the authenticated order API.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);

  const changeStatus = async (orderId, status) => {
    setUpdating(orderId);
    try {
      await updateOrderStatus(orderId, status);
      setLoading(true);
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
    return (
      <p className="p-8 text-center text-slate-500">
        Loading FPO operations...
      </p>
    );
  if (error)
    return <p className="rounded-xl bg-red-50 p-4 text-red-700">{error}</p>;

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <section className="rounded-2xl bg-gradient-to-r from-[#2e7d32] to-[#1b5e20] p-6 text-white shadow-md">
        <p className="text-xs font-extrabold uppercase tracking-wider text-[#f5d77f]">
          FPO Operations
        </p>
        <h2 className="mt-1 text-3xl font-black">Farmer network orders</h2>
        <p className="mt-2 text-sm text-emerald-100">
          Monitor fulfilment and earnings across your linked farmers.
        </p>
      </section>
      <section className="grid gap-4 sm:grid-cols-3">
        {[
          ["Linked farmers", earnings?.linkedFarmers ?? farmers.length],
          ["Paid orders", earnings?.paidOrders ?? 0],
          [
            "Farmer earnings",
            `₹${Number(earnings?.totalEarnings || 0).toLocaleString("en-IN")}`,
          ],
        ].map(([label, value]) => (
          <article
            key={label}
            className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-sm"
          >
            <p className="text-xs font-bold uppercase tracking-wider text-slate-500">
              {label}
            </p>
            <p className="mt-2 text-3xl font-black text-green-700">{value}</p>
          </article>
        ))}
      </section>
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-sm">
        <div className="flex items-center justify-between gap-3">
          <div>
            <h3 className="text-xl font-bold text-[#163820]">Farmer orders</h3>
            <p className="text-sm text-slate-500">
              Each order is scoped to the farmer's own items and earnings.
            </p>
          </div>
          <NavLink
            to="/fpo-admin/farmers"
            className="rounded-lg bg-green-600 px-4 py-2 text-sm font-bold text-white hover:bg-green-700"
          >
            Manage Farmers
          </NavLink>
        </div>
        <div className="mt-5 space-y-3">
          {farmers.map((farmer) => (
            <details
              key={farmer.id}
              open={openFarmers[farmer.id]}
              onToggle={(event) =>
                setOpenFarmers({
                  ...openFarmers,
                  [farmer.id]: event.currentTarget.open,
                })
              }
              className="rounded-xl border border-slate-200"
            >
              <summary className="cursor-pointer list-none px-4 py-3 font-bold text-slate-800">
                {farmer.name}{" "}
                <span className="ml-2 text-xs font-normal text-slate-500">
                  ({(ordersByFarmer[farmer.id] || []).length} orders)
                </span>
              </summary>
              <div className="space-y-3 border-t p-4">
                {(ordersByFarmer[farmer.id] || []).length ? (
                  ordersByFarmer[farmer.id].map((order) => {
                    const action = nextStatus[order.status];
                    return (
                      <article
                        key={order.id}
                        className="rounded-lg bg-slate-50 p-4"
                      >
                        <div className="flex flex-wrap justify-between gap-2">
                          <div>
                            <p className="font-bold">Order #{order.id}</p>
                            <p className="text-xs text-slate-500">
                              {formatDate(order.createdAt)} · {order.buyerName}
                            </p>
                          </div>
                          <div className="flex items-center gap-2">
                            <span
                              className={`rounded-full px-2.5 py-1 text-xs font-bold ${statusStyles[order.status]}`}
                            >
                              {order.status}
                            </span>
                            {order.isBulkOrder && (
                              <span className="rounded-full bg-indigo-100 px-2.5 py-1 text-xs font-bold text-indigo-800">
                                Bulk · Aggregated across multiple farmers
                              </span>
                            )}
                          </div>
                        </div>
                        <div className="mt-3 space-y-1 text-sm">
                          {order.items.map((item) => (
                            <p key={item.id} className="flex justify-between">
                              <span>
                                {item.cropName} × {item.quantity}
                              </span>
                              <span>
                                ₹{Number(item.subTotal).toLocaleString("en-IN")}
                              </span>
                            </p>
                          ))}
                        </div>
                        <div className="mt-3 flex flex-wrap items-center justify-between gap-2 border-t pt-3 text-sm">
                          <strong>
                            Share: ₹
                            {Number(order.totalAmount).toLocaleString("en-IN")}
                          </strong>
                          {action && (
                            <button
                              type="button"
                              disabled={updating === order.id}
                              onClick={() => changeStatus(order.id, action[0])}
                              className="rounded-lg bg-green-600 px-3 py-2 text-xs font-bold text-white hover:bg-green-700"
                            >
                              {updating === order.id
                                ? "Updating..."
                                : action[1]}
                            </button>
                          )}
                        </div>
                      </article>
                    );
                  })
                ) : (
                  <p className="py-5 text-center text-sm text-slate-500">
                    No orders for this farmer.
                  </p>
                )}
              </div>
            </details>
          ))}
        </div>
      </section>
    </div>
  );
}
