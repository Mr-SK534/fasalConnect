import { useMemo, useState } from 'react';

const products = [
	{ id: 1, name: 'Fresh Tomatoes', category: 'Vegetables', farmer: 'Ramesh Kumar', location: 'Punjab', price: 40, unit: 'kg', rating: 4.8, image: '🍅' },
	{ id: 2, name: 'Organic Potatoes', category: 'Vegetables', farmer: 'Sunita Devi', location: 'Uttar Pradesh', price: 32, unit: 'kg', rating: 4.7, image: '🥔' },
	{ id: 3, name: 'Alphonso Mangoes', category: 'Fruits', farmer: 'Mohan Patil', location: 'Maharashtra', price: 180, unit: 'kg', rating: 4.9, image: '🥭' },
	{ id: 4, name: 'Fresh Spinach', category: 'Leafy Greens', farmer: 'Anita Sharma', location: 'Haryana', price: 25, unit: 'bunch', rating: 4.6, image: '🥬' },
	{ id: 5, name: 'Basmati Rice', category: 'Grains', farmer: 'Jaspreet Singh', location: 'Haryana', price: 95, unit: 'kg', rating: 4.8, image: '🌾' },
	{ id: 6, name: 'Fresh Apples', category: 'Fruits', farmer: 'Karan Thakur', location: 'Himachal Pradesh', price: 140, unit: 'kg', rating: 4.7, image: '🍎' },
];

export default function BrowseProducts() {
	const [query, setQuery] = useState('');
	const [category, setCategory] = useState('All');
	const [sort, setSort] = useState('Recommended');
	const [cart, setCart] = useState([]);

	const categories = ['All', ...new Set(products.map((product) => product.category))];
	const visibleProducts = useMemo(() => {
		const result = products.filter((product) => {
			const matchesCategory = category === 'All' || product.category === category;
			const text = `${product.name} ${product.farmer} ${product.location}`.toLowerCase();
			return matchesCategory && text.includes(query.toLowerCase());
		});
		if (sort === 'Price: Low to High') return result.sort((a, b) => a.price - b.price);
		if (sort === 'Price: High to Low') return result.sort((a, b) => b.price - a.price);
		if (sort === 'Top Rated') return result.sort((a, b) => b.rating - a.rating);
		return result;
	}, [category, query, sort]);

	const addToCart = (product) => setCart((items) => [...items, product]);

	return (
		<main className="min-h-screen bg-slate-50 px-4 py-8 sm:px-8">
			<div className="mx-auto max-w-7xl">
				<div className="mb-8 flex flex-wrap items-end justify-between gap-4">
					<div>
						<p className="mb-2 text-sm font-semibold uppercase tracking-wider text-green-600">Farmer marketplace</p>
						<h1 className="text-3xl font-bold text-slate-900">Browse fresh products</h1>
						<p className="mt-2 text-slate-500">Buy directly from trusted local farmers.</p>
					</div>
					<button className="rounded-lg bg-green-600 px-4 py-2.5 font-semibold text-white shadow-sm hover:bg-green-700">
						Cart ({cart.length})
					</button>
				</div>

				<section className="mb-8 rounded-xl bg-white p-4 shadow-sm">
					<div className="flex flex-col gap-3 md:flex-row">
						<input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search products, farmers or locations..." className="flex-1 rounded-lg border border-slate-200 px-4 py-3 outline-none focus:border-green-500" />
						<select value={category} onChange={(event) => setCategory(event.target.value)} className="rounded-lg border border-slate-200 px-4 py-3 outline-none focus:border-green-500">
							{categories.map((item) => <option key={item}>{item}</option>)}
						</select>
						<select value={sort} onChange={(event) => setSort(event.target.value)} className="rounded-lg border border-slate-200 px-4 py-3 outline-none focus:border-green-500">
							{['Recommended', 'Top Rated', 'Price: Low to High', 'Price: High to Low'].map((item) => <option key={item}>{item}</option>)}
						</select>
					</div>
				</section>

				<p className="mb-4 text-sm text-slate-500">{visibleProducts.length} products available</p>
				{visibleProducts.length ? (
					<div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
						{visibleProducts.map((product) => (
							<article key={product.id} className="overflow-hidden rounded-xl bg-white shadow-sm transition hover:-translate-y-1 hover:shadow-md">
								<div className="flex h-40 items-center justify-center bg-green-50 text-7xl">{product.image}</div>
								<div className="p-5">
									<div className="mb-2 flex items-start justify-between gap-2"><h2 className="font-bold text-slate-900">{product.name}</h2><span className="text-sm text-amber-500">★ {product.rating}</span></div>
									<p className="text-sm text-slate-500">By {product.farmer} · {product.location}</p>
									<div className="mt-5 flex items-center justify-between"><p className="text-lg font-bold text-green-700">₹{product.price}<span className="text-sm font-normal text-slate-500">/{product.unit}</span></p><button onClick={() => addToCart(product)} className="rounded-lg bg-green-600 px-3 py-2 text-sm font-semibold text-white hover:bg-green-700">Add to cart</button></div>
								</div>
							</article>
						))}
					</div>
				) : <div className="rounded-xl bg-white py-16 text-center text-slate-500">No products found.</div>}
			</div>
		</main>
	);
}
