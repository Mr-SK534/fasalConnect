// frontend/src/components/map/RouteMap.jsx

import React, { useEffect, useMemo } from "react";
import { MapContainer, TileLayer, Marker, Popup, Polyline, useMap } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix Leaflet's default marker icon paths broken by Vite asset bundling
import iconUrl from "leaflet/dist/images/marker-icon.png";
import iconRetinaUrl from "leaflet/dist/images/marker-icon-2x.png";
import shadowUrl from "leaflet/dist/images/marker-shadow.png";

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconUrl,
  iconRetinaUrl,
  shadowUrl,
});

// Default vehicle color palette
const DEFAULT_VEHICLE_COLORS = [
  "#16a34a", // Vehicle 1: Green
  "#2563eb", // Vehicle 2: Blue
  "#9333ea", // Vehicle 3: Purple
  "#ea580c", // Vehicle 4: Orange
  "#0284c7", // Vehicle 5: Sky Blue
  "#d97706", // Vehicle 6: Amber
  "#dc2626", // Vehicle 7: Red
  "#059669", // Vehicle 8: Emerald
];

const makeIcon = (label, color, isUrgent = false) =>
  L.divIcon({
    html: `<div style="background:${isUrgent ? "#dc2626" : color};color:white;border-radius:50%;width:32px;height:32px;display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:800;border:2px solid white;box-shadow:0 3px 8px rgba(0,0,0,0.35);${
      isUrgent ? "animation: redPulse 1.5s infinite; outline: 3px solid #ef4444;" : ""
    }">${label}</div>`,
    className: "",
    iconSize: [32, 32],
    iconAnchor: [16, 16],
    popupAnchor: [0, -16],
  });

const ICON_DEPOT = makeIcon("HUB", "#2563eb");
const ICON_PICKUP = (seq, isUrgent) => makeIcon(`P${seq}`, "#ea580c", isUrgent);
const ICON_DELIVERY = (seq, isUrgent) => makeIcon(`D${seq}`, "#16a34a", isUrgent);

function MapBoundsFitter({ depot, stops }) {
  const map = useMap();

  useEffect(() => {
    if (!depot || !depot.lat || !depot.lng) return;

    if (stops && stops.length > 0) {
      const bounds = L.latLngBounds([
        [depot.lat, depot.lng],
        ...stops.map((s) => [s.lat || s.Lat, s.lng || s.Lng]),
      ]);
      map.fitBounds(bounds, { padding: [50, 50] });
    } else {
      map.setView([depot.lat, depot.lng], 12);
    }
  }, [depot, stops, map]);

  return null;
}

export default function RouteMap({
  stops = [],
  depot = { lat: 19.0760, lng: 72.8777 },
  vehicleColors = DEFAULT_VEHICLE_COLORS,
  className = "h-[450px] w-full rounded-xl overflow-hidden shadow-md border border-slate-200 relative",
}) {
  const stopsByVehicle = useMemo(() => {
    const map = {};
    stops.forEach((s) => {
      const vNum = s.vehicleNumber || s.VehicleNumber || 1;
      if (!map[vNum]) map[vNum] = [];
      map[vNum].push(s);
    });
    Object.keys(map).forEach((v) => {
      map[v].sort((a, b) => (a.sequence || a.Sequence) - (b.sequence || b.Sequence));
    });
    return map;
  }, [stops]);

  const polylines = useMemo(() => {
    return Object.entries(stopsByVehicle).map(([vehicleNumStr, vehicleStops]) => {
      const vIndex = parseInt(vehicleNumStr, 10) - 1;
      const color = vehicleColors[vIndex % vehicleColors.length];

      const coords = [
        [depot.lat, depot.lng],
        ...vehicleStops.map((s) => [s.lat || s.Lat, s.lng || s.Lng]),
        [depot.lat, depot.lng],
      ];

      return {
        vehicleNumber: parseInt(vehicleNumStr, 10),
        color,
        coords,
      };
    });
  }, [stopsByVehicle, depot, vehicleColors]);

  return (
    <div className={className}>
      <style>{`
        @keyframes redPulse {
          0% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.8); }
          70% { box-shadow: 0 0 0 12px rgba(239, 68, 68, 0); }
          100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
        }
      `}</style>
      <MapContainer
        center={[depot.lat, depot.lng]}
        zoom={11}
        scrollWheelZoom={true}
        className="h-full w-full"
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        <MapBoundsFitter depot={depot} stops={stops} />

        {/* Depot Marker */}
        <Marker position={[depot.lat, depot.lng]} icon={ICON_DEPOT}>
          <Popup>
            <div className="font-sans text-xs">
              <strong className="text-blue-700">Central Delivery Hub / Depot</strong>
              <br />
              Lat: {depot.lat}, Lng: {depot.lng}
            </div>
          </Popup>
        </Marker>

        {/* Stop Markers */}
        {stops.map((stop, idx) => {
          const lat = stop.lat || stop.Lat;
          const lng = stop.lng || stop.Lng;
          const type = (stop.type || stop.Type || "").toLowerCase();
          const seq = stop.sequence || stop.Sequence || idx + 1;
          const label = stop.label || stop.Label || "";
          const qty = stop.quantityAtStop || stop.QuantityAtStop || 0;
          const vNum = stop.vehicleNumber || stop.VehicleNumber || 1;
          const isUrgent = stop.isUrgent || stop.IsUrgent || false;
          const tier = stop.perishabilityTier || stop.PerishabilityTier || "Low";

          const isPickup = type === "pickup";
          const icon = isPickup ? ICON_PICKUP(seq, isUrgent) : ICON_DELIVERY(seq, isUrgent);

          return (
            <Marker key={`${vNum}-${seq}-${idx}`} position={[lat, lng]} icon={icon}>
              <Popup>
                <div className="font-sans text-xs space-y-1">
                  <div className="flex items-center gap-1 font-bold text-slate-800">
                    <span className={isPickup ? "text-orange-600" : "text-green-600"}>
                      [{isPickup ? "PICKUP" : "DELIVERY"}]
                    </span>
                    <span>Vehicle #{vNum} &bull; Stop #{seq}</span>
                  </div>
                  <p className="text-slate-600 font-medium">{label}</p>
                  <p className="text-slate-500">
                    Load: <span className="font-bold text-slate-800">{qty} kg</span> &bull; Perishability: <span className="font-bold">{tier}</span>
                  </p>
                  {isUrgent && (
                    <p className="text-red-600 font-bold text-[11px]">
                      ⚠️ ETA exceeds freshness window!
                    </p>
                  )}
                </div>
              </Popup>
            </Marker>
          );
        })}

        {/* Vehicle Route Polylines */}
        {polylines.map(({ vehicleNumber, color, coords }) => (
          <Polyline
            key={`polyline-v${vehicleNumber}`}
            positions={coords}
            pathOptions={{
              color,
              weight: 4,
              opacity: 0.85,
              dashArray: "6, 6",
            }}
          />
        ))}
      </MapContainer>
    </div>
  );
}
