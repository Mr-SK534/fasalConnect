import { useEffect, useMemo, useState } from "react";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api";

const statusStyles = {
	delivered: "bg-green-100 text-green-700",
	completed: "bg-green-100 text-green-700",
	shipped: "bg-blue-100 text-blue-700",
	processing: "bg-yellow-100 text-yellow-700",
	pending: "bg-orange-100 text-orange-700",
	cancelled: "bg-red-100 text-red-700",
};

const formatDate = (date) => {
	if (!date) return "—";
	return new Date(date).toLocaleDateString("en-IN", {
		day: "numeric",
		month: "short",
		year: "numeric",
	});
};

const formatStatus = (status = "pending") =>
	status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();

export default function MyOrders() {
	const [orders, setOrders] = useState([]);
	const [filter, setFilter] = useState("all");
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState("");

	useEffect(() => {
		const loadOrders = async () => {
			try {
				const token = localStorage.getItem("token");
				const response = await fetch(`${API_URL}/orders/my-orders`, {
					headers: token ? { Authorization: `Bearer ${token}` } : {},
				});

				if (!response.ok) throw new Error("Unable to load your orders.");
				const data = await response.json();
				setOrders(Array.isArray(data) ? data : data.orders || []);
			} catch (err) {
				setError(err.message || "Unable to load your orders.");
			} finally {
				setLoading(false);
			}
		};

		loadOrders();
	}, []);

	const visibleOrders = useMemo(
		() =>
			filter === "all"
				? orders
				: orders.filter((order) => (order.status || "pending").toLowerCase() === filter),
		[filter, orders]
	);

	return (
		<main className="min-h-screen bg-gray-50 px-4 py-8 sm:px-6 lg:px-8">
			<div className="mx-auto max-w-6xl">
				<div className="mb-8 flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
					<div>
						<h1 className="text-3xl font-bold text-gray-900">My Orders</h1>
						<p className="mt-1 text-gray-500">Track and manage your purchases</p>
					</div>
					<select
						value={filter}
						onChange={(event) => setFilter(event.target.value)}
						className="rounded-lg border border-gray-200 bg-white px-4 py-2.5 text-sm text-gray-700 outline-none focus:border-green-500"
						aria-label="Filter orders"
					>
						<option value="all">All orders</option>
						<option value="pending">Pending</option>
						<option value="processing">Processing</option>
						<option value="shipped">Shipped</option>
						<option value="delivered">Delivered</option>
						<option value="cancelled">Cancelled</option>
					</select>
				</div>

				{loading && <p className="py-16 text-center text-gray-500">Loading orders...</p>}
				{!loading && error && <p className="rounded-lg bg-red-50 p-4 text-red-600">{error}</p>}
				{!loading && !error && visibleOrders.length === 0 && (
					<div className="rounded-xl bg-white p-12 text-center shadow-sm">
						<div className="mb-3 text-5xl">🛒</div>
						<h2 className="text-xl font-semibold text-gray-800">No orders found</h2>
						<p className="mt-2 text-gray-500">Your orders will appear here once you place one.</p>
					</div>
				)}

				<div className="space-y-5">
					{visibleOrders.map((order) => {
						const status = (order.status || "pending").toLowerCase();
						const items = order.items || order.orderItems || [];
						return (
							<article key={order._id || order.id} className="rounded-xl bg-white p-5 shadow-sm sm:p-6">
								<div className="flex flex-col justify-between gap-3 border-b border-gray-100 pb-4 sm:flex-row sm:items-center">
									<div>
										<p className="font-semibold text-gray-900">Order #{order.orderNumber || order._id || order.id}</p>
										<p className="mt-1 text-sm text-gray-500">Placed on {formatDate(order.createdAt || order.date)}</p>
									</div>
									<span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[status] || "bg-gray-100 text-gray-700"}`}>
										{formatStatus(status)}
									</span>
								</div>
								<div className="divide-y divide-gray-100">
									{items.map((item, index) => (
										<div key={item._id || item.id || index} className="flex items-center gap-4 py-4">
											<div className="flex h-14 w-14 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-green-50 text-2xl">
												{item.product?.image || item.image ? <img src={item.product?.image || item.image} alt={item.product?.name || item.name || "Product"} className="h-full w-full object-cover" /> : "🌾"}
											</div>
											<div className="min-w-0 flex-1">
												<p className="truncate font-medium text-gray-800">{item.product?.name || item.name || "Farm product"}</p>
												<p className="text-sm text-gray-500">Qty: {item.quantity || 1}</p>
											</div>
											<p className="font-semibold text-gray-800">₹{Number(item.price || 0).toLocaleString("en-IN")}</p>
										</div>
									))}
								</div>
								<div className="flex justify-between border-t border-gray-100 pt-4 text-sm">
									<span className="text-gray-500">Total amount</span>
									<span className="text-lg font-bold text-green-700">₹{Number(order.totalAmount || order.total || 0).toLocaleString("en-IN")}</span>
								</div>
							</article>
						);
					})}
				</div>
			</div>
		</main>
	);
}
