import { 
  FiBox, 
  FiCalendar, 
  FiCheckSquare, 
  FiGrid, 
  FiLogOut, 
  FiSettings, 
  FiTrendingUp, 
  FiUser, 
  FiUsers 
} from "react-icons/fi";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { ROLES } from "../utils/roles";

const navigationByRole = {
  [ROLES.FARMER]: [
    {
      section: "MAIN",
      items: [
        { label: "Dashboard", to: "/farmer/dashboard", icon: FiGrid },
        { label: "My Products", to: "/farmer/list-product", icon: FiBox },
        { label: "Orders", to: "/farmer/orders", icon: FiCheckSquare },
      ],
    },
    {
      section: "SMART",
      items: [
        { label: "Forecast", to: "/farmer/forecast", icon: FiTrendingUp },
        { label: "Schedule", to: "/farmer/schedule", icon: FiCalendar },
      ],
    },
    {
      section: "ACCOUNT",
      items: [
        { label: "Profile", to: "/farmer/profile", icon: FiUser },
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
      ],
    },
  ],
  [ROLES.FPO_ADMIN]: [
    {
      section: "MAIN",
      items: [
        { label: "Dashboard", to: "/fpo-admin/dashboard", icon: FiGrid },
        { label: "Linked Farmers", to: "/fpo-admin/farmers", icon: FiUsers },
      ],
    },
  ],
  [ROLES.PLATFORM_ADMIN]: [
    {
      section: "MAIN",
      items: [
        { label: "Dashboard", to: "/admin/dashboard", icon: FiGrid },
      ],
    },
  ],
};

const roleLabels = {
  [ROLES.FARMER]: "Farmer",
  [ROLES.BUYER]: "Buyer",
  [ROLES.FPO_ADMIN]: "FPO Admin",
  [ROLES.PLATFORM_ADMIN]: "Platform Admin",
};

function DashboardLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  
  const sections = navigationByRole[user?.role] || [];
  const allItems = sections.flatMap((s) => s.items);
  const dashboardPath = allItems[0]?.to || "/login";
  const activeItem = allItems.find((item) => location.pathname === item.to);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-[#fcf9ee] text-slate-900 font-sans antialiased text-xl leading-relaxed">
      {/* Sidebar Navigation */}
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-80 flex-col bg-gradient-to-b from-[#2e7d32] via-[#246b28] to-[#1b5e20] text-white shadow-2xl md:flex">
        {/* Brand Header */}
        <div className="flex h-24 items-center border-b border-white/10 px-8">
          <NavLink
            to={dashboardPath}
            className="flex items-center gap-2 text-3xl font-extrabold tracking-tight"
          >
            <span className="text-4xl">🌱</span>
            <span>Fasal<span className="text-[#f5d77f]">Connect</span></span>
          </NavLink>
        </div>

        {/* Navigation Sections */}
        <div className="flex-1 overflow-y-auto px-6 py-6 space-y-8">
          {sections.map((sec, idx) => (
            <div key={idx}>
              <p className="mb-3 px-3 text-sm font-extrabold uppercase tracking-widest text-[#a3e2a7]">
                {sec.section}
              </p>
              <nav className="space-y-1.5" aria-label={`Sidebar section ${sec.section}`}>
                {sec.items.map(({ label, to, icon: Icon }) => (
                  <NavLink
                    key={to}
                    to={to}
                    className={({ isActive }) =>
                      `flex items-center gap-3.5 rounded-2xl px-4 py-3.5 text-xl font-bold transition-all duration-200 ${
                        isActive
                          ? "bg-white text-[#1b5e20] shadow-md shadow-black/10 translate-x-1"
                          : "text-white/90 hover:bg-white/10 hover:text-white"
                      }`
                    }
                  >
                    <Icon size={24} className="shrink-0" />
                    <span>{label}</span>
                  </NavLink>
                ))}
              </nav>
            </div>
          ))}
        </div>

        {/* Footer Brand Tag / Logout Button */}
        <div className="p-6 border-t border-white/10">
          <button
            type="button"
            onClick={handleLogout}
            className="flex w-full items-center gap-3.5 rounded-2xl px-4 py-3.5 text-xl font-bold text-red-200 transition-all duration-200 hover:bg-red-500/20 hover:text-white"
          >
            <FiLogOut size={24} className="shrink-0" />
            <span>Log Out</span>
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="md:pl-80">
        {/* Sticky Navbar Header */}
        <header className="sticky top-0 z-20 border-b border-[#eadaaf] bg-[#fffef9]/95 px-6 py-6 shadow-sm backdrop-blur-md sm:px-10">
          <div className="flex items-center justify-between gap-6">
            <div>
              <p className="text-base font-extrabold uppercase tracking-widest text-[#406836]">
                {roleLabels[user?.role] || "Dashboard"} &gt;
              </p>
              <h1 className="mt-1 text-4xl font-black tracking-tight text-[#163820]">
                {activeItem?.label || "Farmer Dashboard"}
              </h1>
            </div>

            {/* Profile Bar */}
            <div className="flex items-center gap-4 bg-white/80 border border-[#e5d8b6] px-5 py-2.5 rounded-2xl shadow-sm">
              <div className="hidden text-right sm:block">
                <p className="text-xl font-bold text-slate-900 leading-tight">
                  {user?.name || "Farmer Name"}
                </p>
                <p className="text-sm font-bold uppercase text-slate-500 tracking-wider">
                  {roleLabels[user?.role] || "Verified Member"}
                </p>
              </div>
              <div className="flex h-14 w-14 items-center justify-center rounded-full bg-[#d2e8bf] text-2xl font-black text-[#174d35] ring-2 ring-[#a8d488]">
                {(user?.name || "F").charAt(0).toUpperCase()}
              </div>
            </div>
          </div>

          {/* Mobile Bottom Navigation */}
          <nav
            className="mt-4 flex gap-2 overflow-x-auto md:hidden pt-2"
            aria-label="Mobile dashboard navigation"
          >
            {allItems.map(({ label, to }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  `whitespace-nowrap rounded-xl px-4 py-3 text-lg font-bold transition ${
                    isActive
                      ? "bg-[#2e7d32] text-white shadow"
                      : "bg-[#f4e8c5] text-[#284a2f]"
                  }`
                }
              >
                {label}
              </NavLink>
            ))}
          </nav>
        </header>

        {/* Dashboard Main Viewport (Applies text-xl to all rendered nested route components) */}
        <main className="min-h-[calc(100vh-100px)] p-6 sm:p-10 text-xl font-medium text-slate-800">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default DashboardLayout;