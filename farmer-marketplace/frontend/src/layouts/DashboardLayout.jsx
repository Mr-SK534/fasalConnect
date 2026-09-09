import {
  FiBox,
  FiCalendar,
  FiCheckSquare,
  FiDollarSign,
  FiGrid,
  FiLogOut,
  FiSettings,
  FiShoppingCart,
  FiTrendingUp,
  FiUser,
  FiUsers,
} from "react-icons/fi";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { useCart } from "../hooks/useCart";
import { ROLES } from "../utils/roles";
import LanguageSelector from "../components/common/LanguageSelector";

const navigationByRole = {
  [ROLES.FARMER]: [
    {
      section: "MAIN",
      items: [
        { label: "Dashboard", to: "/farmer/dashboard", icon: FiGrid },
        { label: "My Products", to: "/farmer/list-product", icon: FiBox },
        { label: "Orders", to: "/farmer/orders", icon: FiCheckSquare },
        { label: "Earnings & Payouts", to: "/farmer/earnings", icon: FiDollarSign },
      ],
    },
    {
      section: "SMART",
      items: [
        { label: "Demand Forecast", to: "/farmer/forecast", icon: FiTrendingUp },
        { label: "Schedule", to: "/farmer/schedule", icon: FiCalendar },
      ],
    },
    {
      section: "ACCOUNT",
      items: [
        { label: "Profile", to: "/profile", icon: FiUser },
        { label: "Settings", to: "/farmer/settings", icon: FiSettings },
      ],
    },
  ],
  [ROLES.BUYER]: [
    {
      section: "MAIN",
      items: [
        { label: "Browse Products", to: "/buyer/browse", icon: FiGrid },
        { label: "Cart", to: "/buyer/cart", icon: FiBox },
        { label: "My Orders", to: "/buyer/orders", icon: FiCheckSquare },
        { label: "Demand Forecast", to: "/buyer/forecast", icon: FiTrendingUp },
      ],
    },
    {
      section: "ACCOUNT",
      items: [{ label: "Profile", to: "/profile", icon: FiUser }],
    },
  ],
  [ROLES.FPO_ADMIN]: [
    {
      section: "MAIN",
      items: [
        { label: "Dashboard", to: "/fpo-admin/dashboard", icon: FiGrid },
        { label: "My Products", to: "/fpo-admin/products", icon: FiBox },
        { label: "Orders", to: "/fpo-admin/orders", icon: FiCheckSquare },
        { label: "Linked Farmers", to: "/fpo-admin/farmers", icon: FiUsers },
        { label: "Network Earnings", to: "/fpo-admin/earnings", icon: FiDollarSign },
        { label: "Demand Forecast", to: "/fpo-admin/forecast", icon: FiTrendingUp },
      ],
    },
    {
      section: "ACCOUNT",
      items: [{ label: "Profile", to: "/profile", icon: FiUser }],
    },
  ],
  [ROLES.SUPER_ADMIN]: [
    {
      section: "SUPER ADMIN",
      items: [
        { label: "Dashboard", to: "/admin/dashboard", icon: FiGrid },
        { label: "Demand Forecast", to: "/admin/forecast", icon: FiTrendingUp },
        { label: "Platform Config", to: "/admin/config", icon: FiSettings },
        { label: "Revenue", to: "/admin/revenue", icon: FiDollarSign },
        { label: "Users", to: "/admin/users", icon: FiUsers },
        { label: "Orders", to: "/admin/orders", icon: FiCheckSquare },
        { label: "Payment Splits", to: "/admin/splits", icon: FiBox },
        {
          label: "Route Optimization",
          to: "/admin/routes",
          icon: FiTrendingUp,
        },
      ],
    },
    {
      section: "ACCOUNT",
      items: [{ label: "Profile", to: "/profile", icon: FiUser }],
    },
  ],
  [ROLES.PLATFORM_ADMIN]: [
    {
      section: "PLATFORM",
      items: [
        { label: "Dashboard", to: "/admin/dashboard", icon: FiGrid },
        { label: "Demand Forecast", to: "/admin/forecast", icon: FiTrendingUp },
        { label: "Platform Revenue", to: "/admin/revenue", icon: FiDollarSign },
        { label: "Users", to: "/admin/users", icon: FiUsers },
        { label: "Orders", to: "/admin/orders", icon: FiCheckSquare },
        { label: "Payment Splits", to: "/admin/splits", icon: FiBox },
        {
          label: "Route Optimization",
          to: "/admin/routes",
          icon: FiTrendingUp,
        },
      ],
    },
    {
      section: "ACCOUNT",
      items: [{ label: "Profile", to: "/profile", icon: FiUser }],
    },
  ],
};

// Share platform admin navigation across Admin and Manager roles (NO Platform Config or Revenue)
navigationByRole[ROLES.ADMIN] = navigationByRole[ROLES.PLATFORM_ADMIN];
navigationByRole[ROLES.MANAGER] = navigationByRole[ROLES.PLATFORM_ADMIN];

const roleLabels = {
  [ROLES.FARMER]: "Farmer",
  [ROLES.BUYER]: "Buyer",
  [ROLES.FPO_ADMIN]: "FPO Admin",
  [ROLES.PLATFORM_ADMIN]: "Platform Admin",
  [ROLES.SUPER_ADMIN]: "Super Admin",
  [ROLES.ADMIN]: "Admin",
  [ROLES.MANAGER]: "Manager",
};

function DashboardLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const { cartCount } = useCart();

  const sections = navigationByRole[user?.role] || [];
  const allItems = sections.flatMap((s) => s.items);
  const dashboardPath = allItems[0]?.to || "/login";
  const activeItem = allItems.find((item) => location.pathname === item.to);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-[#fcf9ee] text-slate-800 font-sans antialiased text-base leading-normal">
      {/* Sidebar Navigation */}
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-64 flex-col bg-gradient-to-b from-[#2e7d32] via-[#246b28] to-[#1b5e20] text-white shadow-xl md:flex">
        {/* Brand Header */}
        <div className="flex h-16 items-center border-b border-white/10 px-6">
          <NavLink
            to={dashboardPath}
            className="flex items-center gap-2 text-xl font-extrabold tracking-tight"
          >
            <span className="text-2xl">🌱</span>
            <span>
              Fasal<span className="text-[#f5d77f]">Connect</span>
            </span>
          </NavLink>
        </div>

        {/* Navigation Sections */}
        <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
          {sections.map((sec, idx) => (
            <div key={idx}>
              <p className="mb-2 px-3 text-[11px] font-extrabold uppercase tracking-wider text-[#a3e2a7]">
                {sec.section}
              </p>
              <nav
                className="space-y-1"
                aria-label={`Sidebar section ${sec.section}`}
              >
                {sec.items.map(({ label, to, icon: Icon }) => (
                  <NavLink
                    key={to}
                    to={to}
                    className={({ isActive }) =>
                      `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-all duration-200 ${
                        isActive
                          ? "bg-white text-[#1b5e20] shadow-sm translate-x-1"
                          : "text-white/90 hover:bg-white/10 hover:text-white"
                      }`
                    }
                  >
                    <Icon size={18} className="shrink-0" />
                    <span>{label}</span>
                  </NavLink>
                ))}
              </nav>
            </div>
          ))}
        </div>

        {/* Footer Brand Tag / Logout Button */}
        <div className="p-4 border-t border-white/10">
          <button
            type="button"
            onClick={handleLogout}
            className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold text-red-200 transition-all duration-200 hover:bg-red-500/20 hover:text-white"
          >
            <FiLogOut size={18} className="shrink-0" />
            <span>Log Out</span>
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="md:pl-64">
        {/* Sticky Navbar Header */}
        <header className="sticky top-0 z-20 border-b border-[#eadaaf] bg-[#fffef9]/95 px-6 py-4 shadow-xs backdrop-blur-md sm:px-8">
          <div className="flex items-center justify-between gap-4">
            <div>
              <p className="text-xs font-bold uppercase tracking-wider text-[#406836]">
                {roleLabels[user?.role] || "Dashboard"} &gt;
              </p>
              <h1 className="mt-0.5 text-2xl font-black tracking-tight text-[#163820]">
                {activeItem?.label || "Farmer Dashboard"}
              </h1>
            </div>

            {/* Profile & Language Controls */}
            <div className="flex items-center gap-3">
              <LanguageSelector />
              {user?.role === ROLES.BUYER && (
                <NavLink
                  to="/buyer/cart"
                  aria-label={`Cart with ${cartCount} items`}
                  className="relative rounded-xl border border-[#e5d8b6] bg-white/80 p-2.5 text-[#2e7d32] shadow-xs"
                >
                  <FiShoppingCart size={19} />
                  {cartCount > 0 && (
                    <span className="absolute -right-2 -top-2 min-w-5 rounded-full bg-[#2e7d32] px-1.5 py-0.5 text-center text-[10px] font-bold text-white">
                      {cartCount}
                    </span>
                  )}
                </NavLink>
              )}
              <div className="flex items-center gap-3 bg-white/80 border border-[#e5d8b6] px-3.5 py-1.5 rounded-xl shadow-xs">
                <div className="hidden text-right sm:block">
                  <p className="text-sm font-bold text-slate-900 leading-tight">
                    {user?.name || "Farmer Name"}
                  </p>
                  <p className="text-[11px] font-semibold uppercase text-slate-500 tracking-wider">
                    {roleLabels[user?.role] || "Verified Member"}
                  </p>
                </div>
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[#d2e8bf] text-base font-black text-[#174d35] ring-2 ring-[#a8d488]">
                  {(user?.name || "F").charAt(0).toUpperCase()}
                </div>
              </div>
            </div>
          </div>

          {/* Mobile Bottom Navigation */}
          <nav
            className="mt-3 flex gap-2 overflow-x-auto md:hidden pt-1"
            aria-label="Mobile dashboard navigation"
          >
            {allItems.map(({ label, to }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  `whitespace-nowrap rounded-lg px-3 py-1.5 text-sm font-semibold transition ${
                    isActive
                      ? "bg-[#2e7d32] text-white shadow-xs"
                      : "bg-[#f4e8c5] text-[#284a2f]"
                  }`
                }
              >
                {label}
              </NavLink>
            ))}
          </nav>
        </header>

        {/* Dashboard Main Viewport */}
        <main className="min-h-[calc(100vh-80px)] p-6 sm:p-8 text-sm font-normal text-slate-800">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default DashboardLayout;

