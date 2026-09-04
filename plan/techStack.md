# Tech Stack — Farmer Marketplace
**Team Zenith · SIH Project**

---

## 🎨 Design
| Tool | Purpose |
|------|---------|
| Figma | UI/UX design, wireframes, prototypes |
| Canva | Graphics, presentations, marketing assets |

---

## 🖥️ Frontend
| Tech | Purpose |
|------|---------|
| React + Vite | Core framework + fast dev build tool |
| React Router | Client-side routing |
| Tailwind CSS v4 | Utility-first styling |
| shadcn/ui | Accessible, pre-built component library |
| Leaflet | Interactive maps (route optimization dashboard) |
| Recharts | Charts and analytics visualizations |
| react-i18next | Multi-language support (EN, HI, BN, regional) |

---

## ⚙️ Backend
| Tech | Purpose |
|------|---------|
| ASP.NET Core Web API | REST API framework |
| .NET 10 | Runtime |
| Single flat project | Controllers → Services → Data (no Clean Architecture layering) |

---

## 🗄️ Database
| Tech | Purpose |
|------|---------|
| PostgreSQL | Primary relational database |
| Entity Framework Core | ORM — migrations, queries, seeding |
| Npgsql | EF Core driver for PostgreSQL |

---

## 🔒 Auth & Security
| Tech | Purpose |
|------|---------|
| JWT (Microsoft.AspNetCore.Authentication.JwtBearer) | Role-based auth — Farmer, FPO Admin, Buyer, Platform Admin |
| BCrypt.Net-Next | Password hashing |

---

## 🧠 Smart Features
| Tech | Purpose |
|------|---------|
| ML.NET (SSA time-series) | Demand forecasting for crop categories |
| Google OR-Tools | Delivery route optimization |

---

## 💳 Payments
| Tech | Purpose |
|------|---------|
| Razorpay .NET SDK | Checkout integration (test mode) |
| Razorpay Route | Split payouts to multiple farmers for bulk/FPO orders |

---

## 💬 Notifications
| Tech | Purpose |
|------|---------|
| Twilio WhatsApp API | Order and payment status notifications (sandbox mode) |

---

## 📄 API Docs
| Tech | Purpose |
|------|---------|
| Swashbuckle | Swagger UI — built into ASP.NET Core template |

---

## 🚀 Deployment
| Layer | Platform |
|-------|---------|
| Frontend | Vercel / Netlify |
| Backend + DB | Railway / Render |