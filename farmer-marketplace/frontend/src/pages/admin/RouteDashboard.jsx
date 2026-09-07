import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  MapContainer,
  Marker,
  Polyline,
  Popup,
  TileLayer,
  useMap,
} from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import { toast } from "react-hot-toast";
import { getAdminOrders } from "../../services/adminService";
import {
  getBatchStatus,
  getRoute,
  optimizeRoute,
  runBatch,
} from "../../services/routeService";
import { useAuth } from "../../hooks/useAuth";

// ── Constants ─────────────────────────────────────────────────────────────────

const DEFAULT_HUB = { lat: 20.5937, lng: 78.9629 }; // geographic centre of India

// ── Helpers ───────────────────────────────────────────────────────────────────

const unwrap = (data) =>
  Array.isArray(data) ? data : data?.items || data?.orders || [];

const formatDate = (value) =>
  value
    ? new Date(value).toLocaleDateString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "-";

const formatDateTime = (value) =>
  value
    ? new Date(value).toLocaleString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
        hour: "numeric",
        minute: "2-digit",
        hour12: true,
      })
    : "-";

const shortId = (id) => String(id).slice(0, 8);

// ── Leaflet custom DivIcons ───────────────────────────────────────────────────

const makeIcon = (label, color) =>
  L.divIcon({
    html: `<div style="background:${color};color:white;border-radius:50%;width:30px;height:30px;display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:800;border:2px solid white;box-shadow:0 2px 6px rgba(0,0,0,.35)">${label}</div>`,
    className: "",
    iconSize: [30, 30],
    iconAnchor: [15, 15],
    popupAnchor: [0, -15],
  });

const ICON_HUB      = makeIcon("HUB", "#2563eb");
const ICON_PICKUP   = (seq) => makeIcon(`P${seq}`, "#16a34a");
const ICON_DELIVERY = (seq) => makeIcon(`D${seq}`, "#ea580c");

// ── Sub-components ────────────────────────────────────────────────────────────

function MapViewport({ hub, focusedStop, stops }) {
  const map = useMap();
  useEffect(() => {
    if (focusedStop) {
      map.setView([focusedStop.latitude, focusedStop.longitude], 14);
    } else if (stops && stops.length > 0) {
      const bounds = L.latLngBounds([
        [hub.lat, hub.lng],
        ...stops.map((s) => [s.latitude, s.longitude]),
      ]);
      map.fitBounds(bounds, { padding: [40, 40] });
    } else {
      map.setView([hub.lat, hub.lng], 10);
    }
  }, [focusedStop, hub, stops, map]);
  return null;
}

function BatchStatusCard({ onRunBatch }) {
  const [status, setStatus] = useState(null);
  const [running, setRunning] = useState(false);

  const load = useCallback(async () => {
    try {
      const data = await getBatchStatus();
      setStatus(data);
    } catch {
      // silently ignore — batch status is informational only
    }
  }, []);

  useEffect(() => {
    load();
    const interval = setInterval(load, 60_000); // refresh every minute
    return () => clearInterval(interval);
  }, [load]);

  const handleRunBatch = async () => {
    if (
      !window.confirm(
        "This will optimize all unrouted confirmed orders. Continue?",
      )
    )
      return;
    setRunning(true);
    try {
      const result = await runBatch();
      toast.success(
        `Batch complete — ${result.ordersRouted} orders, ${result.routesCreated} routes`,
      );
      await load();
      onRunBatch?.();
    } catch (err) {
      toast.error(err.response?.data?.message || "Batch run failed.");
    } finally {
      setRunning(false);
    }
  };

  return (
    <div className="mb-5 rounded-xl border border-blue-100 bg-blue-50 p-4">
      <p className="text-xs font-bold uppercase tracking-wider text-blue-700">
        Auto-batch scheduler
      </p>
      {status ? (
        <>
          <div className="mt-2 grid grid-cols-2 gap-2 text-xs text-slate-600">
            <div>
              <span className="font-semibold text-slate-700">Next batch</span>
              <br />
              {formatDateTime(status.nextBatchTime)}
            </div>
            <div>
              <span className="font-semibold text-slate-700">Last batch</span>
              <br />
              {status.lastBatchTime
                ? `${formatDateTime(status.lastBatchTime)} — ${status.lastBatchOrderCount} orders`
                : "Never run"}
            </div>
          </div>
          <p className="mt-2 text-xs text-slate-500">
            <span className="font-semibold text-orange-600">
              {status.unroutedConfirmedOrderCount}
            </span>{" "}
            unrouted confirmed orders waiting
          </p>
        </>
      ) : (
        <p className="mt-1 text-xs text-slate-400">Loading batch status…</p>
      )}
      <button
        type="button"
        disabled={running}
        onClick={handleRunBatch}
        className="mt-3 flex w-full items-center justify-center gap-2 rounded-lg bg-blue-600 py-2 text-xs font-bold text-white hover:bg-blue-700 disabled:opacity-60"
      >
        {running && (
          <span className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-white border-t-transparent" />
        )}
        {running ? "Running batch…" : "Run Batch Now"}
      </button>
    </div>
  );
}

// ── Main component ────────────────────────────────────────────────────────────

export default function RouteDashboard() {
  const { user } = useAuth();
  const profileHub =
    user?.latitude && user?.longitude
      ? { lat: user.latitude, lng: user.longitude }
      : DEFAULT_HUB;
  const [hub, setHub] = useState(profileHub);
  const [orders, setOrders] = useState([]);
  const [selectedIds, setSelectedIds] = useState([]);
  const [routes, setRoutes] = useState([]);
  const [loadingOrders, setLoadingOrders] = useState(true);
  const [optimizing, setOptimizing] = useState(false);
  const [error, setError] = useState("");
  const [pastRouteId, setPastRouteId] = useState("");
  const [focusedStop, setFocusedStop] = useState(null);
  const [roadGeometries, setRoadGeometries] = useState({});

  // Sync hub when user profile coordinates arrive from session restore
  useEffect(() => {
    if (user?.latitude && user?.longitude)
      setHub({ lat: user.latitude, lng: user.longitude });
  }, [user?.latitude, user?.longitude]);

  const loadOrders = useCallback(async () => {
    setLoadingOrders(true);
    try {
      const data = await getAdminOrders({ status: "Confirmed", page: 1, pageSize: 100 });
      setOrders(unwrap(data).filter((o) => o.deliveryType === "Delivery"));
    } catch (err) {
      setError(err.response?.data?.message || "Could not load confirmed delivery orders.");
    } finally {
      setLoadingOrders(false);
    }
  }, []);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadOrders();
  }, [loadOrders]);

  const orderById = useMemo(
    () => new Map(orders.map((o) => [String(o.id), o])),
    [orders],
  );

  const straightRoutePoints = useCallback(
    (rt) =>
      rt
        ? [
            [rt.deliveryHubLat, rt.deliveryHubLng],
            ...rt.stops.map((s) => [s.latitude, s.longitude]),
            [rt.deliveryHubLat, rt.deliveryHubLng],
          ]
        : [],
    [],
  );

  const loadRoadGeometries = async (nextRoutes) => {
    const newGeoms = { ...roadGeometries };
    await Promise.all(
      nextRoutes.map(async (rt) => {
        const points = straightRoutePoints(rt);
        try {
          const coords = points.map(([lat, lng]) => `${lng},${lat}`).join(";");
          const response = await fetch(
            `https://router.project-osrm.org/route/v1/driving/${coords}?overview=full&geometries=geojson&steps=false`,
          );
          if (!response.ok) throw new Error("OSRM route request failed");
          const data = await response.json();
          const geometry = data.routes?.[0]?.geometry?.coordinates || [];
          newGeoms[rt.routeId] = geometry.map(([lng, lat]) => [lat, lng]);
        } catch {
          newGeoms[rt.routeId] = points;
        }
      })
    );
    setRoadGeometries(newGeoms);
  };

  const toggleOrder = (id) =>
    setSelectedIds((cur) =>
      cur.includes(id) ? cur.filter((x) => x !== id) : [...cur, id],
    );

  const useMyLocation = () =>
    navigator.geolocation?.getCurrentPosition(
      ({ coords }) => setHub({ lat: coords.latitude, lng: coords.longitude }),
      () => setError("Location access was denied."),
    );

  const optimize = async () => {
    setError("");
    setOptimizing(true);
    try {
      const results = await optimizeRoute(selectedIds, hub);
      const routesArray = Array.isArray(results) ? results : [results];
      setRoutes(routesArray);
      setFocusedStop(null);
      await loadRoadGeometries(routesArray);
      toast.success(`Optimized ${routesArray.length} route(s)`);
    } catch (err) {
      setError(err.response?.data?.message || "Could not optimize route.");
    } finally {
      setOptimizing(false);
    }
  };

  const loadPastRoute = async (event) => {
    event.preventDefault();
    setError("");
    try {
      const result = await getRoute(pastRouteId.trim());
      setRoutes([result]);
      setHub({ lat: result.deliveryHubLat, lng: result.deliveryHubLng });
      setFocusedStop(null);
      await loadRoadGeometries([result]);
    } catch (err) {
      setError(err.response?.data?.message || "Route not found");
    }
  };

  const copyRouteId = async (id) => {
    await navigator.clipboard.writeText(id);
    toast.success("Route ID copied");
  };

  // All stops across all routes for map bounds
  const allStops = useMemo(() => routes.flatMap((r) => r.stops), [routes]);

  return (
    <div className="-m-6 flex h-[calc(100vh-80px)] min-h-[620px] flex-col bg-white lg:flex-row">
      {/* ── Left sidebar ── */}
      <aside className="w-full overflow-y-auto border-r border-gray-200 bg-white p-5 lg:w-[420px] lg:flex-shrink-0">
        <header className="mb-5">
          <p className="text-xs font-bold uppercase tracking-wider text-green-700">
            Platform logistics
          </p>
          <h2 className="mt-1 text-2xl font-black text-[#163820]">
            Route Optimization
          </h2>
          <p className="mt-1 text-sm text-slate-500">
            Plan efficient delivery routes for confirmed orders.
          </p>
        </header>

        {/* Batch status */}
        <BatchStatusCard onRunBatch={loadOrders} />

        {/* Hub location */}
        <section className="mb-5">
          <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-gray-500">
            Delivery Hub Location
          </h3>
          <div className="grid grid-cols-2 gap-2">
            <label className="text-xs font-medium text-slate-600">
              Latitude
              <input
                type="number"
                step="any"
                value={hub.lat}
                onChange={(e) => setHub({ ...hub, lat: Number(e.target.value) })}
                className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2"
              />
            </label>
            <label className="text-xs font-medium text-slate-600">
              Longitude
              <input
                type="number"
                step="any"
                value={hub.lng}
                onChange={(e) => setHub({ ...hub, lng: Number(e.target.value) })}
                className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2"
              />
            </label>
          </div>
          <button
            type="button"
            onClick={useMyLocation}
            className="mt-2 rounded-lg border border-green-200 px-3 py-2 text-sm font-medium text-green-700 hover:bg-green-50"
          >
            Use my location
          </button>
        </section>

        {/* Order selector */}
        <section className="mb-5">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
              Select Orders to Route
            </h3>
            <span className="text-xs font-semibold text-green-700">
              {selectedIds.length} selected
            </span>
          </div>
          <div className="mb-3 flex gap-2">
            <button
              type="button"
              onClick={() => setSelectedIds(orders.map((o) => o.id))}
              className="text-xs font-semibold text-green-700"
            >
              Select All
            </button>
            <button
              type="button"
              onClick={() => setSelectedIds([])}
              className="text-xs font-semibold text-slate-500"
            >
              Deselect All
            </button>
          </div>
          {loadingOrders ? (
            <p className="text-sm text-slate-500">Loading orders…</p>
          ) : orders.length ? (
            orders.map((order) => (
              <label
                key={order.id}
                className={`mb-2 flex items-start gap-3 rounded-lg border p-3 ${
                  selectedIds.includes(order.id)
                    ? "border-green-200 bg-green-50"
                    : "border-gray-100 hover:bg-gray-50"
                }`}
              >
                <input
                  type="checkbox"
                  checked={selectedIds.includes(order.id)}
                  onChange={() => toggleOrder(order.id)}
                  className="mt-1 accent-green-600"
                />
                <span className="min-w-0 text-sm">
                  <strong className="block">
                    #{shortId(order.id)} · {order.buyerName}
                  </strong>
                  <span className="block truncate text-xs text-slate-500">
                    {order.deliveryAddress || "No address"}
                  </span>
                  <span className="text-xs font-semibold text-slate-700">
                    ₹{Number(order.totalAmount || 0).toLocaleString("en-IN")}
                  </span>
                </span>
              </label>
            ))
          ) : (
            <p className="rounded-lg bg-gray-50 p-3 text-xs text-slate-500">
              No confirmed delivery orders available. Orders must be Confirmed
              status and Delivery type to be routed.
            </p>
          )}
        </section>

        {error && (
          <p className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
            {error}
          </p>
        )}

        <button
          type="button"
          disabled={!selectedIds.length || optimizing}
          onClick={optimize}
          className="w-full rounded-lg bg-green-600 py-3 font-semibold text-white hover:bg-green-700 disabled:opacity-50"
        >
          {optimizing ? "Optimizing route…" : "Optimize Route"}
        </button>

        {/* Route stops list */}
        {routes.length > 0 && (
          <section className="mt-6 flex flex-col gap-6">
            {routes.map((rt, i) => (
              <div key={rt.routeId}>
                <div className="flex items-center justify-between">
                  <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
                    Vehicle {i + 1} ({rt.stops.length} stops)
                  </h3>
                  <button
                    type="button"
                    onClick={() => copyRouteId(rt.routeId)}
                    className="text-xs font-semibold text-green-700"
                  >
                    Copy Route ID
                  </button>
                </div>
                <p className="mt-1 break-all font-mono text-[10px] text-slate-400">
                  {rt.routeId}
                </p>
                <div className="mt-2">
                  {rt.stops.map((stop) => {
                    const order = orderById.get(String(stop.orderId));
                    const isPickup = stop.stopType === "Pickup";
                    return (
                      <button
                        type="button"
                        key={`${stop.orderId}-${stop.stopSequence}`}
                        onClick={() => setFocusedStop(stop)}
                        className="flex w-full items-start gap-3 border-b border-gray-100 p-3 text-left hover:bg-gray-50"
                      >
                        <span
                          className={`flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white ${
                            isPickup ? "bg-green-600" : "bg-orange-500"
                          }`}
                        >
                          {stop.stopSequence}
                        </span>
                        <span className="min-w-0 text-sm">
                          <span className="flex items-center gap-1.5">
                            <span
                              className={`rounded px-1.5 py-0.5 text-[10px] font-bold ${
                                isPickup
                                  ? "bg-green-100 text-green-700"
                                  : "bg-orange-100 text-orange-700"
                              }`}
                            >
                              {isPickup ? "PICKUP" : "DELIVERY"}
                            </span>
                            <strong className="block truncate">
                              {isPickup
                                ? stop.farmerName || "Farmer"
                                : stop.address || order?.deliveryAddress || `Stop ${stop.stopSequence}`}
                            </strong>
                          </span>
                          <span className="mt-0.5 block text-xs text-slate-500">
                            Pickup {formatDate(stop.pickupDate)} · Delivery{" "}
                            {formatDate(stop.deliveryDate)} · ETA{" "}
                            {formatDateTime(stop.estimatedArrival)}
                          </span>
                        </span>
                      </button>
                    );
                  })}
                </div>
              </div>
            ))}
          </section>
        )}

        {/* Load past route */}
        <form onSubmit={loadPastRoute} className="mt-6 border-t border-gray-100 pt-5">
          <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-gray-500">
            Past Routes
          </h3>
          <div className="flex gap-2">
            <input
              required
              value={pastRouteId}
              onChange={(e) => setPastRouteId(e.target.value)}
              placeholder="Route ID"
              className="min-w-0 flex-1 rounded-lg border border-gray-200 px-3 py-2 text-sm"
            />
            <button className="rounded-lg bg-slate-800 px-3 py-2 text-sm font-medium text-white">
              Load
            </button>
          </div>
        </form>
      </aside>

      {/* ── Map panel ── */}
      <main className="min-h-[420px] flex-1">
        <MapContainer
          center={[hub.lat, hub.lng]}
          zoom={10}
          className="h-full min-h-[420px] w-full"
        >
          <TileLayer
            attribution="&copy; OpenStreetMap contributors"
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <MapViewport hub={hub} focusedStop={focusedStop} stops={allStops} />

          {/* Delivery Hub marker */}
          <Marker position={[hub.lat, hub.lng]} icon={ICON_HUB}>
            <Popup>
              <strong>Delivery Hub</strong>
            </Popup>
          </Marker>

          {/* Stop markers — P for pickup (green), D for delivery (orange) */}
          {allStops.map((stop) => {
            const order = orderById.get(String(stop.orderId));
            const isPickup = stop.stopType === "Pickup";
            const icon = isPickup
              ? ICON_PICKUP(stop.stopSequence)
              : ICON_DELIVERY(stop.stopSequence);
            return (
              <Marker
                key={`${stop.orderId}-${stop.stopSequence}`}
                position={[stop.latitude, stop.longitude]}
                icon={icon}
              >
                <Popup>
                  <strong>
                    {isPickup ? "Pickup" : "Delivery"} Stop #{stop.stopSequence}
                  </strong>
                  <br />
                  {isPickup
                    ? `Farmer: ${stop.farmerName || "—"}`
                    : stop.address || order?.deliveryAddress || "Route stop"}
                  <br />
                  Pickup: {formatDate(stop.pickupDate)}
                  <br />
                  Delivery: {formatDate(stop.deliveryDate)}
                  <br />
                  ETA: {formatDateTime(stop.estimatedArrival)}
                  <br />
                  Order: {shortId(stop.orderId)}
                </Popup>
              </Marker>
            );
          })}

          {routes.map((rt, i) => {
            const colors = ["#16a34a", "#2563eb", "#9333ea", "#ea580c", "#eab308"];
            const color = colors[i % colors.length];
            const geom = roadGeometries[rt.routeId] || straightRoutePoints(rt);
            return (
              <Polyline
                key={rt.routeId}
                positions={geom}
                pathOptions={{ color, weight: 4 }}
              />
            );
          })}
        </MapContainer>
      </main>
    </div>
  );
}
