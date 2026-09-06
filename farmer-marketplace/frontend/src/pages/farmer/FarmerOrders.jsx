export default function FarmerOrders() {
  return (
    <div className="mx-auto max-w-4xl py-4 space-y-5 text-sm">
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-8 text-center shadow-xs sm:p-10">
        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-[#f4e8c5] text-3xl ring-2 ring-[#eadaaf]">
          📦
        </div>
        <h2 className="mt-4 text-2xl font-black text-[#163820]">
          Farmer Orders
        </h2>
        <p className="mx-auto mt-2 max-w-md text-xs font-normal leading-relaxed text-slate-500">
          Order management is ready for the backend order service. New incoming
          orders will appear here once that API is enabled.
        </p>
      </section>
    </div>
  );
}