# 🌱 FasalConnect - System Architecture & Project Documentation

## 1. Core Problem Statement & Mission

### Problem Statement
> **"Multiple intermediaries reduce farmers' earnings and increase consumer prices."**

### Traditional Agricultural Supply Chain Inefficiencies
In the conventional agricultural supply chain in India, produce passes through 5 to 7 layers of intermediaries before reaching the consumer:

```
[ Farmer ] ➔ [ Village Trader ] ➔ [ Mandi Commission Agent (APMC) ] ➔ [ Wholesaler ] ➔ [ Sub-Wholesaler ] ➔ [ Retailer ] ➔ [ Consumer ]
```

#### Negative Impacts of Intermediaries:
1. **Erosion of Farmer Income**: Intermediary margins, illegal deductions, and unstandardized weighing reduce farmer realization to just **20%–30%** of the final consumer price.
2. **Inflated Consumer Costs**: Stacked middleman markups drastically increase prices paid by end buyers.
3. **Payment Delays & Opacity**: Farmers face delayed payments (weeks to months) and lack visibility into market prices.
4. **Logistics Wastage**: Uncoordinated, multi-stage transportation leads to crop spoilage and excessive freight overhead.

---

## 2. The FasalConnect Solution

**FasalConnect** eliminates unnecessary supply chain middlemen by providing a direct-to-buyer digital marketplace powered by an escrow payment ledger and an automated route optimization engine:

```
                                  ┌───────────────────────────────┐
                                  │      FasalConnect Engine      │
[ Farmer / FPO ] ───────────────► │ 100% Ask Price Payout Escrow  │ ───────────────► [ Buyer ]
                                  │  Automated Route Optimization │
                                  └───────────────────────────────┘
```

### Key Solution Innovations
- **100% Asking Price Guarantee**: Farmers set their own asking price per kilogram and receive **100% of that listed price** ($\text{Delivered Qty} \times \text{Farmer Price}$). Middleman deductions are completely eliminated.
- **Transparent Buyer Pricing**: Platform operational fees (e.g. 8% commission + logistics cost) are added transparently at checkout so buyers see exactly where every rupee goes.
- **Escrow Payment Protection**: Buyer payments are locked in an Escrow Wallet upon checkout and disbursed directly into farmer/FPO bank accounts upon verified delivery.
- **AI-Driven Route Optimization**: Clusters orders and optimizes logistics routes using Haversine distance and Traveling Salesperson (TSP) algorithms, lowering transportation costs.

---

## 3. User Roles & Capabilities

| User Role | Solution Impact | System Capabilities & Dashboard Routes |
| :--- | :--- | :--- |
| **Farmer** | Direct market access, 100% asking price payout guarantee, zero middleman cuts. | List produce catalogue, track order fulfillment, view live earnings & bank payouts (`/farmer/earnings`), yield forecasting & crop scheduling. |
| **FPO Admin** | Empowers farmer collectives to aggregate produce and gain bulk bargaining power. | Link & create network farmers, manage collective catalogue, view aggregate network earnings & farmer payout breakdowns (`/fpo-admin/earnings`). |
| **Buyer** | Pays fair, transparent prices directly to farmers with quality and delivery assurance. | Browse fresh produce, filter by crop & region, select Delivery vs. Pickup, pay via Razorpay/Escrow, track order status. |
| **Platform Admin** | Maintains ecosystem health, monitors logistics, audits financial splits. | Manage users, monitor orders, audit payment split ledgers (`/admin/splits`), optimize logistics routes (`/admin/routes`). |
| **Super Admin** | Configures platform parameters dynamically to ensure sustainability. | Dynamic platform fee configuration (`/admin/config`), live platform revenue & bank wallet settlement ledger (`/admin/revenue`). |

---

## 4. Financial Architecture & Escrow Engine

```
[ Buyer Checkout ]
       │
       ▼ (Pays Total = Farmer Ask + Commission + Logistics + Gateway Fee)
[ Escrow Wallet (Status: Held) ]
       │
       ├── Order Picked Up & Delivered (Route Execution)
       │
       ▼ (Delivery Confirmed / Escrow Released)
┌───────────────────────────────────────┬──────────────────────────────────────────┐
│  Farmer Payout (100% Asking Price)   │  SuperAdmin Bank Settlement (Extra Markup)
│  Transferred to Farmer Bank / UPI    │  Commission (8%) + Logistics Margin (₹0.50)
└───────────────────────────────────────┴──────────────────────────────────────────┘
```

### Price & Fee Calculation Formula
When a buyer purchases produce on FasalConnect:
$$\text{Commission per kg} = \text{Farmer Asking Price} \times \text{Commission \% (e.g. 8\%)}$$
$$\text{Logistics Charge per kg} = \text{Logistics Partner Rate (e.g. ₹2.00)} + \text{Platform Margin (e.g. ₹0.50)}$$
$$\text{Consumer Price per kg} = \text{Farmer Asking Price} + \text{Commission per kg} + \text{Logistics Charge per kg}$$
$$\text{Buyer Checkout Total} = (\text{Consumer Price per kg} \times \text{Quantity}) + \text{Gateway Fee (2\%)}$$

### Distribution Breakdown Upon Delivery Confirmation
- **Farmer Share**: $100\%$ of Listed Asking Price $\times$ Quantity Delivered (kg) $\to$ Transferred directly to Farmer's Bank Account / UPI.
- **SuperAdmin Share**: $(\text{Commission per kg} + \text{Logistics Platform Margin}) \times \text{Quantity Delivered}$ $\to$ Transferred to SuperAdmin Bank Account.
- **Logistics Partner Share**: $\text{Logistics Partner Rate} \times \text{Quantity Delivered}$.

---

## 5. Route Optimization Engine & Logic

To solve the logistics cost escalation caused by fragmented middleman transport, FasalConnect integrates an automated **Route Optimization Engine**.

```
[ Unrouted Confirmed Orders ]
            │
            ▼
[ Filter & Geo-code Coordinates ] (Pickup Lat/Lng & Delivery Lat/Lng)
            │
            ▼
[ Haversine Distance Matrix ] ──► Calculates actual km distance between all stops
            │
            ▼
[ Vehicle Capacity Clustering ] ──► Groups stops into routes within max weight/stop limits
            │
            ▼
[ Nearest-Neighbor TSP Solver ] ──► Orders stops (Hub ➔ Farmer 1 ➔ Farmer 2 ➔ Buyer 1 ➔ Buyer 2)
            │
            ▼
[ Route Generation & ETA Calculation ] ──► Assigns Vehicle #, Stop Sequence & ETAs
```

### Core Algorithms & Logic Steps

1. **Order Filtering & Geo-Coding**:
   - Fetches all orders with `Status = Confirmed`, `DeliveryType = Delivery`, and `RouteId = NULL`.
2. **Haversine Distance Metric**:
   - Calculates exact geographical distances over Earth's spherical surface between coordinates $(lat_1, lng_1)$ and $(lat_2, lng_2)$:
   $$d = 2 R \arcsin \left( \sqrt{ \sin^2\left(\frac{\Delta lat}{2}\right) + \cos(lat_1) \cos(lat_2) \sin^2\left(\frac{\Delta lng}{2}\right) } \right)$$
   where $R = 6371\text{ km}$.

3. **Clustering & Vehicle Capacity Limits**:
   - Groups orders based on proximity to a central collection hub.
   - Enforces payload constraints (e.g. max 500 kg per vehicle, max 10 stops per route).

4. **Nearest-Neighbor Traveling Salesperson (TSP) Heuristic**:
   - Determines the most efficient stop sequence ($1 \to 2 \to 3 \dots \to N$).
   - Strategy: Origin Hub $\to$ Nearest Farmer Pickup locations $\to$ Nearest Buyer Delivery locations.

5. **ETA & Vehicle Assignment**:
   - Calculates Estimated Time of Arrival (ETA) for each stop assuming average transit speeds (30 km/h) + dwell times.
   - Assigns vehicle numbers and sequence indices (`StopSequence = 1, 2, 3...`).

---

## 6. Technology Stack

### Backend
- **Framework**: ASP.NET Core 10 Web API (C#)
- **Data Access**: Entity Framework Core with DbContext ORM
- **Database**: PostgreSQL / SQLite with automatic migrations
- **Security & Auth**: JWT Bearer Authentication & Password Hashing
- **Payment & Escrow**: Razorpay Gateway Integration & Double-Entry Ledger

### Frontend
- **Framework**: React (Vite) & JavaScript (ESNext)
- **Styling**: Vanilla CSS & TailwindCSS (Glassmorphic Emerald/Gold theme)
- **Icons**: React Feather / Lucide Icons (`react-icons/fi`)
- **Routing**: React Router v6 with Role-Based Protected Route Guards
- **Notifications**: React Hot Toast

---

## 7. Verification & Status
- **Backend Build (`dotnet build`)**: Compiled with **0 Errors**.
- **Frontend Build (`npm run build`)**: Compiled cleanly with Vite.
- **Backend API Server**: Active and listening.
