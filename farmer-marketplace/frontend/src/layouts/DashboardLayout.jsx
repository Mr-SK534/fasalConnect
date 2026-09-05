import { FiBox, FiClipboard, FiGrid, FiLogOut, FiUsers } from "react-icons/fi";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { ROLES } from "../utils/roles";

const navigationByRole = {
  [ROLES.FARMER]: [
    { label: "Dashboard", to: "/farmer/dashboard", icon: FiGrid },
    { label: "List product", to: "/farmer/list-product", icon: FiBox },
    { label: "Orders", to: "/farmer/orders", icon: FiClipboard },
  ],
  [ROLES.BUYER]: [
    { label: "Browse products", to: "/buyer/browse", icon: FiGrid },
    { label: "Cart", to: "/buyer/cart", icon: FiBox },
    { label: "My orders", to: "/buyer/orders", icon: FiClipboard },
  ],
  [ROLES.FPO_ADMIN]: [
    { label: "Dashboard", to: "/fpo-admin/dashboard", icon: FiGrid },
    { label: "Linked farmers", to: "/fpo-admin/farmers", icon: FiUsers },
  ],
  [ROLES.PLATFORM_ADMIN]: [
    { label: "Dashboard", to: "/admin/dashboard", icon: FiGrid },
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
  const navigation = navigationByRole[user?.role] || [];
  const dashboardPath = navigation[0]?.to || "/login";
  const activeItem = navigation.find((item) => location.pathname === item.to);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-[#fffaf0] text-slate-800">
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-72 flex-col bg-[#174d35] text-white shadow-xl md:flex">
        <div className="flex h-20 items-center border-b border-white/15 px-7">
          <NavLink
            to={dashboardPath}
            className="text-2xl font-bold tracking-tight"
          >
            Fasal<span className="text-[#f3c969]">Connect</span>
          </NavLink>
        </div>
        <div className="px-7 py-6">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[#b9d6a2]">
            Workspace
          </p>
          <p className="mt-2 text-lg font-semibold">
            {roleLabels[user?.role] || "Marketplace"}
          </p>
        </div>
        <nav
          className="flex-1 space-y-2 px-4"
          aria-label="Dashboard navigation"
        >
          {navigation.map(({ label, to, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-xl px-4 py-3 text-base font-semibold transition ${isActive ? "bg-[#f3c969] text-[#174d35] shadow-sm" : "text-white/80 hover:bg-white/10 hover:text-white"}`
              }
            >
              <Icon size={20} />
              {label}
            </NavLink>
          ))}
        </nav>
        <button
          type="button"
          onClick={handleLogout}
          className="mx-4 mb-6 flex items-center gap-3 rounded-xl px-4 py-3 text-base font-semibold text-white/80 transition hover:bg-red-500/20 hover:text-white"
        >
          <FiLogOut size={20} />
          Log out
        </button>
      </aside>

      <div className="md:pl-72">
        <header className="sticky top-0 z-20 border-b border-[#eadfbe] bg-[#fffdf7]/95 px-5 py-4 shadow-sm backdrop-blur sm:px-8">
          <div className="flex items-center justify-between gap-4">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[#668357]">
                FasalConnect
              </p>
              <h1 className="mt-1 text-2xl font-bold text-[#193b2a]">
                {activeItem?.label || "Dashboard"}
              </h1>
            </div>
            <div className="flex items-center gap-3">
              <div className="hidden text-right sm:block">
                <p className="text-base font-semibold text-slate-800">
                  {user?.name || "Account"}
                </p>
                <p className="text-sm text-slate-500">
                  {roleLabels[user?.role]}
                </p>
              </div>
              <div className="flex h-11 w-11 items-center justify-center rounded-full bg-[#d9ebc9] text-lg font-bold text-[#174d35]">
                {(user?.name || "A").charAt(0).toUpperCase()}
              </div>
            </div>
          </div>
          <nav
            className="mt-4 flex gap-2 overflow-x-auto md:hidden"
            aria-label="Mobile dashboard navigation"
          >
            {navigation.map(({ label, to }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  `whitespace-nowrap rounded-lg px-3 py-2 text-sm font-semibold ${isActive ? "bg-[#174d35] text-white" : "bg-[#f4e8c5] text-[#31553d]"}`
                }
              >
                {label}
              </NavLink>
            ))}
          </nav>
        </header>
        <main className="min-h-[calc(100vh-88px)] p-5 sm:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default DashboardLayout;
