#!/bin/bash
# ============================================================
# Farmer Marketplace — Project Structure Scaffolder
# Team Zenith — SIH Project
#
# Run this once to create the full folder + empty file structure.
# Usage:
#   chmod +x setup_project.sh
#   ./setup_project.sh
# ============================================================

set -e

ROOT="farmer-marketplace"
echo "Creating project root: $ROOT"
mkdir -p "$ROOT"
cd "$ROOT"

# ------------------------------------------------------------
# FRONTEND
# ------------------------------------------------------------
echo "Scaffolding frontend/..."

mkdir -p frontend/public
mkdir -p frontend/src/assets/images
mkdir -p frontend/src/assets/icons
mkdir -p frontend/src/locales
mkdir -p frontend/src/components/common
mkdir -p frontend/src/components/product
mkdir -p frontend/src/components/cart
mkdir -p frontend/src/components/order
mkdir -p frontend/src/components/payment
mkdir -p frontend/src/components/fpo
mkdir -p frontend/src/components/map
mkdir -p frontend/src/components/forecast
mkdir -p frontend/src/pages/farmer
mkdir -p frontend/src/pages/fpo-admin
mkdir -p frontend/src/pages/buyer
mkdir -p frontend/src/pages/admin
mkdir -p frontend/src/pages/auth
mkdir -p frontend/src/routes
mkdir -p frontend/src/context
mkdir -p frontend/src/hooks
mkdir -p frontend/src/services
mkdir -p frontend/src/styles/components
mkdir -p frontend/src/utils

# Locale files
touch frontend/src/locales/en.json
touch frontend/src/locales/hi.json
touch frontend/src/locales/mr.json
touch frontend/src/locales/kn.json
touch frontend/src/i18n.js

# Common components
touch frontend/src/components/common/Navbar.jsx
touch frontend/src/components/common/Footer.jsx
touch frontend/src/components/common/Button.jsx
touch frontend/src/components/common/Loader.jsx
touch frontend/src/components/common/LanguageSelector.jsx
touch frontend/src/components/common/ProtectedRoute.jsx

# Product components
touch frontend/src/components/product/ProductCard.jsx
touch frontend/src/components/product/ProductList.jsx
touch frontend/src/components/product/ProductForm.jsx
touch frontend/src/components/product/DemandBadge.jsx

# Cart components
touch frontend/src/components/cart/CartItem.jsx
touch frontend/src/components/cart/CartSummary.jsx
touch frontend/src/components/cart/BulkOrderToggle.jsx

# Order components
touch frontend/src/components/order/OrderCard.jsx
touch frontend/src/components/order/OrderTracker.jsx

# Payment components
touch frontend/src/components/payment/RazorpayCheckout.jsx

# FPO components
touch frontend/src/components/fpo/LinkedFarmersList.jsx

# Map components
touch frontend/src/components/map/RouteMap.jsx

# Forecast components
touch frontend/src/components/forecast/ForecastChart.jsx

# Pages — farmer
touch frontend/src/pages/farmer/FarmerDashboard.jsx
touch frontend/src/pages/farmer/ListProduct.jsx
touch frontend/src/pages/farmer/FarmerOrders.jsx

# Pages — fpo-admin
touch frontend/src/pages/fpo-admin/FPODashboard.jsx
touch frontend/src/pages/fpo-admin/ManageLinkedFarmers.jsx

# Pages — buyer
touch frontend/src/pages/buyer/BrowseProducts.jsx
touch frontend/src/pages/buyer/Cart.jsx
touch frontend/src/pages/buyer/Checkout.jsx
touch frontend/src/pages/buyer/MyOrders.jsx

# Pages — admin
touch frontend/src/pages/admin/RouteDashboard.jsx

# Pages — auth
touch frontend/src/pages/auth/Login.jsx
touch frontend/src/pages/auth/Register.jsx

# Pages — home
touch frontend/src/pages/Home.jsx

# Routes
touch frontend/src/routes/AppRoutes.jsx

# Context
touch frontend/src/context/AuthContext.jsx
touch frontend/src/context/CartContext.jsx
touch frontend/src/context/LanguageContext.jsx

# Hooks
touch frontend/src/hooks/useAuth.js
touch frontend/src/hooks/useProducts.js
touch frontend/src/hooks/useOrders.js

# Services
touch frontend/src/services/api.js
touch frontend/src/services/authService.js
touch frontend/src/services/productService.js
touch frontend/src/services/orderService.js
touch frontend/src/services/forecastService.js
touch frontend/src/services/routeService.js
touch frontend/src/services/paymentService.js

# Styles
touch frontend/src/styles/index.css
touch frontend/src/styles/globals.css
touch frontend/src/styles/components/map.css

# Utils
touch frontend/src/utils/formatCurrency.js
touch frontend/src/utils/formatDate.js
touch frontend/src/utils/validators.js

# Root frontend files
touch frontend/src/App.jsx
touch frontend/src/main.jsx
touch frontend/src/config.js
touch frontend/.env
touch frontend/.env.example
touch frontend/index.html
touch frontend/package.json
touch frontend/tailwind.config.js
touch frontend/vite.config.js
touch frontend/public/favicon.ico
touch frontend/public/logo.png

# ------------------------------------------------------------
# BACKEND
# ------------------------------------------------------------
echo "Scaffolding backend/..."

mkdir -p backend/app/core
mkdir -p backend/app/db
mkdir -p backend/app/models
mkdir -p backend/app/schemas
mkdir -p backend/app/routers
mkdir -p backend/app/services
mkdir -p backend/app/utils
mkdir -p backend/alembic/versions
mkdir -p backend/tests

# App entrypoint
touch backend/app/__init__.py
touch backend/app/main.py

# Core
touch backend/app/core/config.py
touch backend/app/core/security.py
touch backend/app/core/dependencies.py

# DB
touch backend/app/db/base.py
touch backend/app/db/session.py
touch backend/app/db/init_db.py

# Models
touch backend/app/models/__init__.py
touch backend/app/models/user.py
touch backend/app/models/product.py
touch backend/app/models/order.py
touch backend/app/models/order_item.py
touch backend/app/models/sales_history.py
touch backend/app/models/route.py
touch backend/app/models/payment.py

# Schemas
touch backend/app/schemas/__init__.py
touch backend/app/schemas/user.py
touch backend/app/schemas/product.py
touch backend/app/schemas/order.py
touch backend/app/schemas/forecast.py
touch backend/app/schemas/payment.py

# Routers
touch backend/app/routers/__init__.py
touch backend/app/routers/auth.py
touch backend/app/routers/products.py
touch backend/app/routers/orders.py
touch backend/app/routers/users.py
touch backend/app/routers/fpo.py
touch backend/app/routers/forecast.py
touch backend/app/routers/routes.py
touch backend/app/routers/payments.py
touch backend/app/routers/whatsapp.py

# Services
touch backend/app/services/__init__.py
touch backend/app/services/demand_forecast.py
touch backend/app/services/route_optimizer.py
touch backend/app/services/order_aggregator.py
touch backend/app/services/payment_service.py
touch backend/app/services/notification_service.py

# Utils
touch backend/app/utils/__init__.py
touch backend/app/utils/geo.py
touch backend/app/utils/validators.py

# Alembic
touch backend/alembic/env.py
touch backend/alembic.ini

# Tests
touch backend/tests/__init__.py
touch backend/tests/test_auth.py
touch backend/tests/test_products.py
touch backend/tests/test_orders.py
touch backend/tests/test_payments.py

# Root backend files
touch backend/.env
touch backend/.env.example
touch backend/requirements.txt
touch backend/Dockerfile

# ------------------------------------------------------------
# DATA, DOCS, ROOT FILES
# ------------------------------------------------------------
echo "Scaffolding data/, docs/, and root files..."

mkdir -p data
touch data/seed_sales_history.csv

mkdir -p docs
touch docs/architecture.md
touch docs/financial_model.md
touch docs/api_contract.md
touch docs/figma_link.md

touch farmer_marketplace_calculator.py
touch docker-compose.yml
touch .gitignore
touch README.md

echo ""
echo "============================================================"
echo "Project structure created successfully inside: $ROOT/"
echo "============================================================"
echo ""
echo "Next steps:"
echo "  cd $ROOT/frontend && npm create vite@latest . -- --template react"
echo "  cd $ROOT/backend  && python -m venv venv && source venv/bin/activate"
echo "============================================================"
