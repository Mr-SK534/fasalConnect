farmer-marketplace/
│
├── frontend/                                  # React (Vite)
│   ├── public/
│   │   ├── favicon.ico
│   │   └── logo.png
│   │
│   ├── src/
│   │   ├── assets/
│   │   │   ├── images/
│   │   │   └── icons/
│   │   │
│   │   ├── locales/                            # i18n translation files
│   │   │   ├── en.json
│   │   │   ├── hi.json
│   │   │   ├── mr.json
│   │   │   └── kn.json
│   │   ├── i18n.js                             # react-i18next config
│   │   │
│   │   ├── components/
│   │   │   ├── common/
│   │   │   │   ├── Navbar.jsx                  # includes language selector
│   │   │   │   ├── Footer.jsx
│   │   │   │   ├── Button.jsx
│   │   │   │   ├── Loader.jsx
│   │   │   │   ├── LanguageSelector.jsx
│   │   │   │   └── ProtectedRoute.jsx
│   │   │   ├── product/
│   │   │   │   ├── ProductCard.jsx
│   │   │   │   ├── ProductList.jsx
│   │   │   │   ├── ProductForm.jsx
│   │   │   │   └── DemandBadge.jsx
│   │   │   ├── cart/
│   │   │   │   ├── CartItem.jsx
│   │   │   │   ├── CartSummary.jsx
│   │   │   │   └── BulkOrderToggle.jsx          # bulk toggle on checkout
│   │   │   ├── order/
│   │   │   │   ├── OrderCard.jsx
│   │   │   │   └── OrderTracker.jsx
│   │   │   ├── payment/
│   │   │   │   └── RazorpayCheckout.jsx
│   │   │   ├── fpo/
│   │   │   │   └── LinkedFarmersList.jsx        # FPO Admin manages linked farmers
│   │   │   ├── map/
│   │   │   │   └── RouteMap.jsx
│   │   │   └── forecast/
│   │   │       └── ForecastChart.jsx
│   │   │
│   │   ├── pages/
│   │   │   ├── farmer/
│   │   │   │   ├── FarmerDashboard.jsx
│   │   │   │   ├── ListProduct.jsx
│   │   │   │   └── FarmerOrders.jsx
│   │   │   ├── fpo-admin/
│   │   │   │   ├── FPODashboard.jsx
│   │   │   │   └── ManageLinkedFarmers.jsx
│   │   │   ├── buyer/
│   │   │   │   ├── BrowseProducts.jsx
│   │   │   │   ├── Cart.jsx
│   │   │   │   ├── Checkout.jsx                 # includes bulk toggle
│   │   │   │   └── MyOrders.jsx
│   │   │   ├── admin/
│   │   │   │   └── RouteDashboard.jsx
│   │   │   ├── auth/
│   │   │   │   ├── Login.jsx
│   │   │   │   └── Register.jsx
│   │   │   └── Home.jsx
│   │   │
│   │   ├── routes/
│   │   │   └── AppRoutes.jsx
│   │   │
│   │   ├── context/
│   │   │   ├── AuthContext.jsx
│   │   │   ├── CartContext.jsx
│   │   │   └── LanguageContext.jsx
│   │   │
│   │   ├── hooks/
│   │   │   ├── useAuth.js
│   │   │   ├── useProducts.js
│   │   │   └── useOrders.js
│   │   │
│   │   ├── services/                            # Axios API calls
│   │   │   ├── api.js
│   │   │   ├── authService.js
│   │   │   ├── productService.js
│   │   │   ├── orderService.js
│   │   │   ├── forecastService.js
│   │   │   ├── routeService.js
│   │   │   └── paymentService.js
│   │   │
│   │   ├── styles/
│   │   │   ├── index.css                        # Tailwind directives
│   │   │   ├── globals.css
│   │   │   └── components/
│   │   │       └── map.css                      # Leaflet overrides
│   │   │
│   │   ├── utils/
│   │   │   ├── formatCurrency.js
│   │   │   ├── formatDate.js
│   │   │   └── validators.js
│   │   │
│   │   ├── App.jsx
│   │   ├── main.jsx
│   │   └── config.js
│   │
│   ├── .env / .env.example
│   ├── index.html
│   ├── package.json
│   ├── tailwind.config.js
│   └── vite.config.js
│
├── backend/                                     # FastAPI
│   ├── app/
│   │   ├── main.py
│   │   │
│   │   ├── core/
│   │   │   ├── config.py
│   │   │   ├── security.py                      # JWT + password hashing
│   │   │   └── dependencies.py                  # get_current_user, role guards
│   │   │
│   │   ├── db/
│   │   │   ├── base.py
│   │   │   ├── session.py
│   │   │   └── init_db.py
│   │   │
│   │   ├── models/                              # SQLAlchemy ORM
│   │   │   ├── user.py                          # includes fpo_id, role field
│   │   │   ├── product.py
│   │   │   ├── order.py
│   │   │   ├── order_item.py
│   │   │   ├── sales_history.py
│   │   │   ├── route.py
│   │   │   └── payment.py
│   │   │
│   │   ├── schemas/                             # Pydantic models
│   │   │   ├── user.py
│   │   │   ├── product.py
│   │   │   ├── order.py
│   │   │   ├── forecast.py
│   │   │   └── payment.py
│   │   │
│   │   ├── routers/
│   │   │   ├── auth.py
│   │   │   ├── products.py
│   │   │   ├── orders.py
│   │   │   ├── users.py
│   │   │   ├── fpo.py                           # manage linked farmers
│   │   │   ├── forecast.py
│   │   │   ├── routes.py
│   │   │   ├── payments.py                      # Razorpay order + webhook
│   │   │   └── whatsapp.py                      # notification webhook (if Scope B)
│   │   │
│   │   ├── services/                            # business logic
│   │   │   ├── demand_forecast.py               # Prophet wrapper
│   │   │   ├── route_optimizer.py               # OR-Tools VRP solver
│   │   │   ├── order_aggregator.py              # triggered by bulk toggle
│   │   │   ├── payment_service.py               # Razorpay + Route split payments
│   │   │   └── notification_service.py          # Twilio WhatsApp
│   │   │
│   │   └── utils/
│   │       ├── geo.py
│   │       └── validators.py
│   │
│   ├── alembic/                                  # DB migrations
│   │   ├── versions/
│   │   └── env.py
│   ├── alembic.ini
│   │
│   ├── tests/
│   │   ├── test_auth.py
│   │   ├── test_products.py
│   │   ├── test_orders.py
│   │   └── test_payments.py
│   │
│   ├── .env / .env.example
│   ├── requirements.txt
│   └── Dockerfile
│
├── data/
│   └── seed_sales_history.csv                    # for Prophet training
│
├── docs/
│   ├── architecture.md
│   ├── financial_model.md
│   ├── api_contract.md
│   └── figma_link.md
│
├── farmer_marketplace_calculator.py               # standalone financial model tool
├── docker-compose.yml
├── .gitignore
└── README.md
