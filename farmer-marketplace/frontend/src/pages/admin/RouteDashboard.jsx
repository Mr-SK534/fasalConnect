import { useCallback, useEffect, useMemo, useState } from "react";
import {
  CircleMarker,
  MapContainer,
  Polyline,
  Popup,
  TileLayer,
  useMap,
} from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { toast } from "react-hot-toast";
import { getAdminOrders } from "../../services/adminService";
import { getRoute, optimizeRoute } from "../../services/routeService";

const DEFAULT_HUB = { lat: 19.076, lng: 72.8777 };
const unwrap = (data) =>
  Array.isArray(data) ? data : data?.items || data?.orders || [];
const formatTime = (value) =>
  value
    ? new Date(value).toLocaleTimeString("en-IN", {
        hour: "numeric",
        minute: "2-digit",
      })
    : "-";
const formatDate = (value) =>
  value
    ? new Date(value).toLocaleDateString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "-";
const shortId = (id) => String(id).slice(0, 8);

function MapViewport({ hub, focusedStop }) {
  const map = useMap();
  useEffect(() => {
    if (focusedStop)
      map.setView([focusedStop.latitude, focusedStop.longitude], 14);
    else map.setView([hub.lat, hub.lng], 10);
  }, [focusedStop, hub, map]);
  return null;
}

export default function RouteDashboard() {
  const [hub, setHub] = useState(DEFAULT_HUB);
  const [orders, setOrders] = useState([]);
  const [selectedIds, setSelectedIds] = useState([]);
  const [route, setRoute] = useState(null);
  const [loadingOrders, setLoadingOrders] = useState(true);
  const [optimizing, setOptimizing] = useState(false);
  const [error, setError] = useState("");
  const [pastRouteId, setPastRouteId] = useState("");
  const [focusedStop, setFocusedStop] = useState(null);
  const [roadGeometry, setRoadGeometry] = useState([]);

  const loadOrders = useCallback(async () => {
    setLoadingOrders(true);
    try {
      const data = await getAdminOrders({
        status: "Confirmed",
        page: 1,
        pageSize: 100,
      });
      setOrders(
        unwrap(data).filter((order) => order.deliveryType === "Delivery"),
      );
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
          "Could not load confirmed delivery orders.",
      );
    } finally {
      setLoadingOrders(false);
    }
  }, []);

  useEffect(() => {
    // Synchronize the route selector with the authenticated admin API.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadOrders();
  }, [loadOrders]);

  const orderById = useMemo(
    () => new Map(orders.map((order) => [String(order.id), order])),
    [orders],
  );
  const straightRoutePoints = useMemo(
    () =>
      route
        ? [
            [route.deliveryHubLat, route.deliveryHubLng],
            ...route.stops.map((stop) => [stop.latitude, stop.longitude]),
            [route.deliveryHubLat, route.deliveryHubLng],
          ]
        : [],
    [route],
  );

  const loadRoadGeometry = async (nextRoute) => {
    const points = [
      [nextRoute.deliveryHubLat, nextRoute.deliveryHubLng],
      ...nextRoute.stops.map((stop) => [stop.latitude, stop.longitude]),
      [nextRoute.deliveryHubLat, nextRoute.deliveryHubLng],
    ];
    try {
      const coordinates = points.map(([lat, lng]) => `${lng},${lat}`).join(";");
      const response = await fetch(
        `https://router.project-osrm.org/route/v1/driving/${coordinates}?overview=full&geometries=geojson&steps=false`,
      );
      if (!response.ok) throw new Error("OSRM route request failed");
      const data = await response.json();
      const geometry = data.routes?.[0]?.geometry?.coordinates || [];
      setRoadGeometry(geometry.map(([lng, lat]) => [lat, lng]));
    } catch {
      setRoadGeometry(points);
      toast("Road geometry unavailable; showing a direct route line");
    }
  };

  const toggleOrder = (id) =>
    setSelectedIds((current) =>
      current.includes(id)
        ? current.filter((item) => item !== id)
        : [...current, id],
    );
  const useLocation = () =>
    navigator.geolocation?.getCurrentPosition(
      ({ coords }) => setHub({ lat: coords.latitude, lng: coords.longitude }),
      () => setError("Location access was denied."),
    );

  const optimize = async () => {
    setError("");
    setOptimizing(true);
    try {
      const result = await optimizeRoute(selectedIds, hub);
      setRoute(result);
      await loadRoadGeometry(result);
      setFocusedStop(null);
      toast.success("Route optimized");
    } catch (requestError) {
      setError(
        requestError.response?.data?.message || "Could not optimize route.",
      );
    } finally {
      setOptimizing(false);
    }
  };

  const loadPastRoute = async (event) => {
    event.preventDefault();
    setError("");
    try {
      const result = await getRoute(pastRouteId.trim());
      setRoute(result);
      setHub({ lat: result.deliveryHubLat, lng: result.deliveryHubLng });
      await loadRoadGeometry(result);
      setFocusedStop(null);
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Route not found");
    }
  };

  const copyRouteId = async () => {
    await navigator.clipboard.writeText(route.routeId);
    toast.success("Route ID copied");
  };

  return (
    <div className="-m-6 flex h-[calc(100vh-80px)] min-h-[620px] flex-col bg-white lg:flex-row">
      <aside className="w-full overflow-y-auto border-r border-gray-200 bg-white p-5 lg:w-[400px] lg:flex-shrink-0">
        <header className="mb-6">
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
        <section className="mb-6">
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
                onChange={(event) =>
                  setHub({ ...hub, lat: Number(event.target.value) })
                }
                className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2"
              />
            </label>
            <label className="text-xs font-medium text-slate-600">
              Longitude
              <input
                type="number"
                step="any"
                value={hub.lng}
                onChange={(event) =>
                  setHub({ ...hub, lng: Number(event.target.value) })
                }
                className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2"
              />
            </label>
          </div>
          <button
            type="button"
            onClick={useLocation}
            className="mt-3 rounded-lg border border-green-200 px-3 py-2 text-sm font-medium text-green-700 hover:bg-green-50"
          >
            Use my location
          </button>
        </section>
        <section className="mb-6">
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
              onClick={() => setSelectedIds(orders.map((order) => order.id))}
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
            <p className="text-sm text-slate-500">Loading orders...</p>
          ) : orders.length ? (
            orders.map((order) => (
              <label
                key={order.id}
                className={`mb-2 flex items-start gap-3 rounded-lg border p-3 ${selectedIds.includes(order.id) ? "border-green-200 bg-green-50" : "border-gray-100 hover:bg-gray-50"}`}
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
          {optimizing ? "Optimizing route..." : "Optimize Route"}
        </button>
        {route && (
          <section className="mt-6">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
                Route Stops ({route.stops.length})
              </h3>
              <button
                type="button"
                onClick={copyRouteId}
                className="text-xs font-semibold text-green-700"
              >
                Copy Route ID
              </button>
            </div>
            <p className="mt-1 break-all font-mono text-[10px] text-slate-400">
              {route.routeId}
            </p>
            <div className="mt-2">
              {route.stops.map((stop) => {
                const order = orderById.get(String(stop.orderId));
                return (
                  <button
                    type="button"
                    key={`${stop.orderId}-${stop.stopSequence}`}
                    onClick={() => setFocusedStop(stop)}
                    className="flex w-full items-start gap-3 border-b border-gray-100 p-3 text-left hover:bg-gray-50"
                  >
                    <span className="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full bg-green-600 text-xs font-bold text-white">
                      {stop.stopSequence}
                    </span>
                    <span className="text-sm">
                      <strong className="block">
                        {stop.stopType || "Delivery"}:{" "}
                        {stop.address ||
                          order?.deliveryAddress ||
                          `Stop ${stop.stopSequence}`}
                      </strong>
                      <span className="text-xs text-slate-500">
                        Pickup {formatDate(stop.pickupDate)} · Delivery{" "}
                        {formatDate(stop.deliveryDate)} · ETA{" "}
                        {formatTime(stop.estimatedArrival)} · Order{" "}
                        {shortId(stop.orderId)}
                      </span>
                    </span>
                  </button>
                );
              })}
            </div>
          </section>
        )}
        <form
          onSubmit={loadPastRoute}
          className="mt-6 border-t border-gray-100 pt-5"
        >
          <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-gray-500">
            Past Routes
          </h3>
          <div className="flex gap-2">
            <input
              required
              value={pastRouteId}
              onChange={(event) => setPastRouteId(event.target.value)}
              placeholder="Route ID"
              className="min-w-0 flex-1 rounded-lg border border-gray-200 px-3 py-2 text-sm"
            />
            <button className="rounded-lg bg-slate-800 px-3 py-2 text-sm font-medium text-white">
              Load
            </button>
          </div>
        </form>
      </aside>
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
          <MapViewport hub={hub} focusedStop={focusedStop} />
          <CircleMarker
            center={[hub.lat, hub.lng]}
            radius={10}
            pathOptions={{
              color: "#2563eb",
              fillColor: "#2563eb",
              fillOpacity: 0.9,
            }}
          >
            <Popup>Delivery Hub</Popup>
          </CircleMarker>
          {route?.stops.map((stop) => {
            const order = orderById.get(String(stop.orderId));
            return (
              <CircleMarker
                key={`${stop.orderId}-${stop.stopSequence}`}
                center={[stop.latitude, stop.longitude]}
                radius={9}
                pathOptions={{
                  color: "#15803d",
                  fillColor: "#16a34a",
                  fillOpacity: 0.95,
                }}
              >
                <Popup>
                  <strong>
                    {stop.stopType || "Delivery"} Stop #{stop.stopSequence}
                  </strong>
                  <br />
                  {stop.address || order?.deliveryAddress || "Route stop"}
                  <br />
                  Pickup: {formatDate(stop.pickupDate)}
                  <br />
                  Delivery: {formatDate(stop.deliveryDate)}
                  <br />
                  ETA: {formatTime(stop.estimatedArrival)}
                  <br />
                  Order: {shortId(stop.orderId)}
                </Popup>
              </CircleMarker>
            );
          })}
          <Polyline
            positions={roadGeometry.length ? roadGeometry : straightRoutePoints}
            pathOptions={{ color: "#16a34a", weight: 4 }}
          />
        </MapContainer>
      </main>
    </div>
  );
}
