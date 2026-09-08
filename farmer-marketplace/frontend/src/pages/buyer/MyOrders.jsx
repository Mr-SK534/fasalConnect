import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { getBuyerOrders } from "../../services/orderService";

const statusStyles = {
  Pending: "bg-yellow-100 text-yellow-800",
  Confirmed: "bg-blue-100 text-blue-800",
  InTransit: "bg-purple-100 text-purple-800",
  Delivered: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};

const formatDate = (value) =>
  value
    ? new Date(value).toLocaleDateString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "—";

export default function MyOrders() {
  const { user } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!user?.id) return;
    getBuyerOrders(user.id)
      .then((data) => setOrders(Array.isArray(data) ? data : []))
      .catch((requestError) =>
        setError(
          requestError.response?.data?.message || "Unable to load your orders.",
        ),
      )
      .finally(() => setLoading(false));
  }, [user?.id]);

  return (
    <main className="min-h-screen bg-gray-50 px-4 py-8 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-8 flex items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">My Orders</h1>
            <p className="mt-1 text-gray-500">Track your produce purchases.</p>
          </div>
          <Link
            to="/buyer/browse"
            className="rounded-lg bg-green-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-green-700"
          >
            Browse Products
          </Link>
        </div>
        {loading && (
          <p className="py-16 text-center text-gray-500">Loading orders...</p>
        )}
        {!loading && error && (
          <p className="rounded-lg bg-red-50 p-4 text-red-700">{error}</p>
        )}
        {!loading && !error && !orders.length && (
          <div className="rounded-xl bg-white p-12 text-center shadow-sm">
            <h2 className="text-xl font-semibold text-gray-800">
              No orders yet
            </h2>
            <p className="mt-2 text-gray-500">
              Your orders will appear here after checkout.
            </p>
          </div>
        )}
        <div className="space-y-5">
          {orders.map((order) => {
            const status = order.status || "Pending";
            return (
              <article
                key={order.id}
                className="rounded-xl bg-white p-5 shadow-sm sm:p-6"
              >
                <div className="flex flex-col justify-between gap-3 border-b border-gray-100 pb-4 sm:flex-row sm:items-center">
                  <div>
                    <p className="font-semibold text-gray-900">
                      Order #{order.id}
                    </p>
                    <p className="mt-1 text-sm text-gray-500">
                      Placed on {formatDate(order.createdAt)}
                    </p>
                  </div>
                  <span
                    className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[status] || "bg-gray-100 text-gray-700"}`}
                  >
                    {status}
                  </span>
                </div>
                <div className="divide-y divide-gray-100">
                  {order.items?.map((item) => (
                    <div
                      key={item.id}
                      className="flex items-center justify-between gap-4 py-4"
                    >
                      <div>
                        <p className="font-medium text-gray-800">
                          {item.cropName}
                        </p>
                        <p className="text-sm text-gray-500">
                          {item.farmerName} · Qty: {item.quantity}
                        </p>
                      </div>
                      <p className="font-semibold text-gray-800">
                        ₹{Number(item.subTotal || 0).toLocaleString("en-IN")}
                      </p>
                    </div>
                  ))}
                </div>
                <div className="flex justify-between border-t border-gray-100 pt-4 text-sm">
                  <span className="text-gray-500">Total amount</span>
                  <span className="text-lg font-bold text-green-700">
                    ₹{Number(order.totalAmount || 0).toLocaleString("en-IN")}
                  </span>
                </div>
              </article>
            );
          })}
        </div>
      </div>
    </main>
  );
}
