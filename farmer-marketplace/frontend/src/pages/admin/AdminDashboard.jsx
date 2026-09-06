import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  FiClock,
  FiClipboard,
  FiPackage,
  FiShoppingCart,
  FiUsers,
} from "react-icons/fi";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { getAdminOrders, getSummary } from "../../services/adminService";

const statusColors = {
  Pending: "bg-red-100 text-red-700",
  Confirmed: "bg-blue-100 text-blue-700",
  InTransit: "bg-purple-100 text-purple-700",
  Delivered: "bg-green-100 text-green-700",
  Cancelled: "bg-gray-100 text-gray-700",
};
const statusList = [
  "Pending",
  "Confirmed",
  "InTransit",
  "Delivered",
  "Cancelled",
];
const cards = [
  ["Total Farmers", "totalFarmers", "bg-green-50 text-green-700", FiUsers],
  ["Total Buyers", "totalBuyers", "bg-blue-50 text-blue-700", FiShoppingCart],
  [
    "Total FPO Admins",
    "totalFpoAdmins",
    "bg-purple-50 text-purple-700",
    FiUsers,
  ],
  ["Total Products", "totalProducts", "bg-amber-50 text-amber-700", FiPackage],
  ["Total Orders", "totalOrders", "bg-indigo-50 text-indigo-700", FiClipboard],
  ["Pending Orders", "pendingOrders", "bg-red-50 text-red-700", FiClock],
];
const unwrap = (data) =>
  Array.isArray(data)
    ? data
    : data?.items || data?.orders || data?.results || [];
const formatDate = (value) =>
  value
    ? new Date(value).toLocaleDateString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "-";

export default function AdminDashboard() {
  const navigate = useNavigate();
  const [summary, setSummary] = useState(null);
  const [orders, setOrders] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([getSummary(), getAdminOrders({ page: 1, pageSize: 1000 })])
      .then(([summaryData, orderData]) => {
        setSummary(summaryData);
        setOrders(unwrap(orderData));
      })
      .catch((requestError) =>
        setError(
          requestError.response?.data?.message ||
            "Could not load admin dashboard.",
        ),
      );
  }, []);

  const statusData = useMemo(
    () =>
      statusList.map((status) => ({
        status,
        count: orders.filter((order) => order.status === status).length,
      })),
    [orders],
  );
  const revenueData = useMemo(() => {
    const months = {};
    orders
      .filter((order) => ["Confirmed", "Delivered"].includes(order.status))
      .forEach((order) => {
        const key = new Date(order.createdAt).toLocaleDateString("en-IN", {
          month: "short",
          year: "2-digit",
        });
        months[key] = (months[key] || 0) + Number(order.totalAmount || 0);
      });
    return Object.entries(months).map(([month, revenue]) => ({
      month,
      revenue,
    }));
  }, [orders]);

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <header>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          Platform control
        </p>
        <h2 className="mt-1 text-3xl font-black text-[#163820]">
          Admin Dashboard
        </h2>
        <p className="mt-1 text-sm text-slate-500">
          Monitor users, marketplace activity, and payments.
        </p>
      </header>
      {error && (
        <p className="rounded-lg bg-red-50 p-4 text-sm text-red-700">{error}</p>
      )}
      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {cards.map(([label, key, color, Icon]) => (
          <article
            key={key}
            className="rounded-xl border border-gray-100 bg-white p-5 shadow-sm"
          >
            <div
              className={`flex h-10 w-10 items-center justify-center rounded-lg ${color}`}
            >
              <Icon size={20} />
            </div>
            <p className="mt-4 text-xs font-bold uppercase tracking-wide text-slate-500">
              {label}
            </p>
            <p className="mt-1 text-3xl font-black text-slate-900">
              {summary?.[key] ?? "..."}
            </p>
          </article>
        ))}
      </section>
      <section className="grid gap-6 lg:grid-cols-2">
        <article className="rounded-xl border border-gray-100 bg-white p-5 shadow-sm">
          <h3 className="font-bold text-slate-900">Orders by status</h3>
          <div className="mt-4 h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={statusData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="status" />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="count" fill="#16a34a" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </article>
        <article className="rounded-xl border border-gray-100 bg-white p-5 shadow-sm">
          <h3 className="font-bold text-slate-900">Revenue over time</h3>
          <div className="mt-4 h-64">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={revenueData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="month" />
                <YAxis />
                <Tooltip
                  formatter={(value) =>
                    `₹${Number(value).toLocaleString("en-IN")}`
                  }
                />
                <Line
                  type="monotone"
                  dataKey="revenue"
                  stroke="#2563eb"
                  strokeWidth={3}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </article>
      </section>
      <section className="rounded-xl border border-gray-100 bg-white p-5 shadow-sm">
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-slate-900">Recent orders</h3>
          <button
            type="button"
            onClick={() => navigate("/admin/orders")}
            className="rounded-lg bg-green-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-green-700"
          >
            View All Orders
          </button>
        </div>
        <div className="mt-4 overflow-x-auto">
          <table className="w-full border-collapse text-left text-sm">
            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="p-3">Order</th>
                <th className="p-3">Buyer</th>
                <th className="p-3">Total</th>
                <th className="p-3">Status</th>
                <th className="p-3">Created</th>
              </tr>
            </thead>
            <tbody>
              {orders.slice(0, 5).map((order) => (
                <tr
                  key={order.id}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="p-3 font-mono">
                    {String(order.id).slice(0, 8)}
                  </td>
                  <td className="p-3">{order.buyerName || "-"}</td>
                  <td className="p-3">
                    ₹{Number(order.totalAmount || 0).toLocaleString("en-IN")}
                  </td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-semibold ${statusColors[order.status] || "bg-gray-100 text-gray-700"}`}
                    >
                      {order.status}
                    </span>
                  </td>
                  <td className="p-3">{formatDate(order.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
      <section className="flex flex-wrap gap-3">
        {[
          ["Manage Users", "/admin/users"],
          ["View All Orders", "/admin/orders"],
          ["Payment Splits", "/admin/splits"],
          ["Route Optimization", "/admin/routes"],
        ].map(([label, path]) => (
          <button
            key={path}
            type="button"
            onClick={() => navigate(path)}
            className="rounded-lg border border-green-200 bg-white px-4 py-2.5 text-sm font-medium text-green-700 hover:bg-green-50"
          >
            {label}
          </button>
        ))}
      </section>
    </div>
  );
}
