export default function FarmerOrders() {
  return (
    <div className="mx-auto max-w-5xl">
      <section className="rounded-2xl border border-[#eadfbe] bg-white p-8 text-center shadow-sm">
        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-[#f4e8c5] text-3xl">
          📦
        </div>
        <h2 className="mt-5 text-2xl font-bold text-[#193b2a]">
          Farmer orders
        </h2>
        <p className="mx-auto mt-3 max-w-lg text-base leading-7 text-slate-600">
          Order management is ready for the backend order service. New incoming
          orders will appear here once that API is enabled.
        </p>
      </section>
    </div>
  );
}
