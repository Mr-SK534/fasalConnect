import { useMemo, useState } from "react";

const formatCurrency = (value) =>
	new Intl.NumberFormat("en-IN", {
		style: "currency",
		currency: "INR",
		maximumFractionDigits: 0,
	}).format(value);

const readCart = () => {
	try {
		return JSON.parse(localStorage.getItem("cart") || "[]");
	} catch {
		return [];
	}
};

export default function Checkout() {
	const [items] = useState(readCart);
	const [paymentMethod, setPaymentMethod] = useState("cod");
	const [placed, setPlaced] = useState(false);
	const [form, setForm] = useState({
		name: "",
		phone: "",
		address: "",
		city: "",
		state: "",
		pincode: "",
	});

	const subtotal = useMemo(
		() =>
			items.reduce(
				(total, item) =>
					total + Number(item.price || item.amount || 0) * Number(item.quantity || 1),
				0,
			),
		[items],
	);
	const delivery = subtotal > 0 && subtotal < 500 ? 40 : 0;
	const total = subtotal + delivery;

	const update = (event) =>
		setForm({ ...form, [event.target.name]: event.target.value });

	const submit = (event) => {
		event.preventDefault();
		setPlaced(true);
		localStorage.removeItem("cart");
	};

	if (placed) {
		return (
			<main className="mx-auto max-w-3xl px-4 py-16 text-center">
				<div className="rounded-2xl bg-green-50 p-10">
					<div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-green-600 text-3xl text-white">✓</div>
					<h1 className="text-3xl font-bold text-gray-900">Order placed successfully!</h1>
					<p className="mt-2 text-gray-600">Thank you for supporting local farmers.</p>
					<a href="/buyer" className="mt-6 inline-block rounded-lg bg-green-600 px-6 py-3 font-medium text-white hover:bg-green-700">Continue shopping</a>
				</div>
			</main>
		);
	}

	return (
		<main className="mx-auto max-w-6xl px-4 py-8">
			<h1 className="mb-8 text-3xl font-bold text-gray-900">Checkout</h1>
			{items.length === 0 ? (
				<div className="rounded-xl border bg-white p-10 text-center text-gray-600">Your cart is empty.</div>
			) : (
				<form onSubmit={submit} className="grid gap-8 lg:grid-cols-[1fr_380px]">
					<section className="space-y-6">
						<div className="rounded-xl border bg-white p-6 shadow-sm">
							<h2 className="mb-5 text-xl font-semibold">Delivery details</h2>
							<div className="grid gap-4 sm:grid-cols-2">
								{[['name', 'Full name'], ['phone', 'Phone number'], ['city', 'City'], ['state', 'State'], ['pincode', 'PIN code']].map(([name, label]) => (
									<label key={name} className={name === 'name' || name === 'phone' ? '' : ''}>
										<span className="mb-1 block text-sm font-medium text-gray-700">{label}</span>
										<input required name={name} value={form[name]} onChange={update} className="w-full rounded-lg border px-3 py-2 outline-none focus:border-green-600" />
									</label>
								))}
								<label className="sm:col-span-2"><span className="mb-1 block text-sm font-medium text-gray-700">Address</span><textarea required name="address" value={form.address} onChange={update} rows="3" className="w-full rounded-lg border px-3 py-2 outline-none focus:border-green-600" /></label>
							</div>
						</div>
						<div className="rounded-xl border bg-white p-6 shadow-sm">
							<h2 className="mb-4 text-xl font-semibold">Payment method</h2>
							{[["cod", "Cash on delivery"], ["online", "Online payment"]].map(([value, label]) => (
								<label key={value} className="mb-3 flex cursor-pointer items-center gap-3 rounded-lg border p-3"><input type="radio" name="payment" value={value} checked={paymentMethod === value} onChange={(e) => setPaymentMethod(e.target.value)} />{label}</label>
							))}
						</div>
					</section>
					<aside className="h-fit rounded-xl border bg-white p-6 shadow-sm">
						<h2 className="mb-4 text-xl font-semibold">Order summary</h2>
						<div className="space-y-3 border-b pb-4">{items.map((item, index) => <div key={item._id || item.id || index} className="flex justify-between gap-3 text-sm"><span>{item.name || item.title} × {item.quantity || 1}</span><span>{formatCurrency(Number(item.price || item.amount || 0) * Number(item.quantity || 1))}</span></div>)}</div>
						<div className="mt-4 space-y-2 text-sm"><div className="flex justify-between"><span>Subtotal</span><span>{formatCurrency(subtotal)}</span></div><div className="flex justify-between"><span>Delivery</span><span>{delivery ? formatCurrency(delivery) : "FREE"}</span></div><div className="mt-3 flex justify-between border-t pt-3 text-lg font-bold"><span>Total</span><span>{formatCurrency(total)}</span></div></div>
						<button type="submit" className="mt-6 w-full rounded-lg bg-green-600 px-4 py-3 font-semibold text-white hover:bg-green-700">Place order</button>
					</aside>
				</form>
			)}
		</main>
	);
}
