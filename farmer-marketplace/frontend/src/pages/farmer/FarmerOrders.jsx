export default function FarmerOrders() {
  return (
    <div className="mx-auto max-w-5xl py-8">
      <section className="rounded-3xl border border-[#eadaaf] bg-white p-12 text-center shadow-md sm:p-16">
        <div className="mx-auto flex h-24 w-24 items-center justify-center rounded-full bg-[#f4e8c5] text-5xl ring-4 ring-[#eadaaf]">
          📦
        </div>
        <h2 className="mt-6 text-4xl font-black text-[#163820]">
          Farmer Orders
        </h2>
        <p className="mx-auto mt-4 max-w-xl text-xl font-semibold leading-relaxed text-slate-600">
          Order management is ready for the backend order service. New incoming
          orders will appear here once that API is enabled.
        </p>
      </section>
    </div>
  );
}