**Updated Feature List (Final)**

Farmer Features

Registration/Login (JWT, role: Farmer or FPO Admin)

Product Listing (crop, quantity, price, season, harvest date)

Demand Forecast Dashboard (Prophet-based)

Order Management (view/confirm/track)

Earnings Dashboard

FPO Aggregation — FPO Admin role manages listings/orders on behalf of multiple linked farmer accounts

Delivery Pickup Schedule

**Buyer Features (Consumer + Bulk, unified)**

Browse/Search Products (filter by crop, location, price, season)

Cart \& Checkout

Bulk toggle — a single flow where checking "Bulk Order" on checkout unlocks bulk pricing tiers and triggers the Order Aggregator (pools multiple farmers automatically if one farmer's stock can't fulfill the quantity) — no separate portal/UI needed, just conditional logic in the same checkout flow

Real Payment Integration (Razorpay, test mode)

Order Tracking \& History

**Admin/Platform Features**

Route Optimization Dashboard (Leaflet map, OR-Tools routes)

Platform-wide Order Monitoring

Analytics (revenue, commission, active users)

**Smart/Differentiated Features**

Demand Forecasting (Prophet/scikit-learn)

Route Optimization (Google OR-Tools)

Order Aggregator (now triggered by the bulk toggle rather than a separate role)

Cold Storage/Granary Visibility (lightweight, optional)

**Payments**

Razorpay Checkout (test mode)

Webhook-based payment verification

Split Payments (Razorpay Route) — handles payouts to multiple farmers when an FPO or aggregated bulk order is involved

**Accessibility \& Communication**

Multi-language Support — UI localization with a language switcher (English, Hindi, Bengali, other regional languages), prioritized on farmer-facing screens since farmers are the primary non-English-first users

WhatsApp Link/Notifications — click-to-chat WhatsApp link on product/order pages for quick buyer-farmer/support contact, plus WhatsApp-based order/payment status updates via Business API or wa.me deep links

**Auth \& Access Control**

Role-based JWT: Farmer, FPO Admin, Buyer (with bulk toggle), Platform Admin

Backend-enforced route protection



