// frontend/src/routes/AppRoutes.jsx

import { Navigate, Routes, Route } from "react-router-dom";

// Layout
import DashboardLayout from "../layouts/DashboardLayout";

// Auth guard + roles
import ProtectedRoute from "../components/common/ProtectedRoute";
import { ROLES } from "../utils/roles";

// Public pages
import Home from "../pages/Home";
import Login from "../pages/auth/Login";
import Register from "../pages/auth/Register";
import ProfileSetup from "../pages/auth/ProfileSetup";

// Farmer pages
import FarmerDashboard from "../pages/farmer/FarmerDashboard";
import ListProduct from "../pages/farmer/ListProduct";
import FarmerOrders from "../pages/farmer/FarmerOrders";

// Buyer pages
import BrowseProducts from "../pages/buyer/BrowseProducts";
import Cart from "../pages/buyer/Cart";
import Checkout from "../pages/buyer/Checkout";
import MyOrders from "../pages/buyer/MyOrders";

// FPO Admin pages
import FPODashboard from "../pages/fpo-admin/FPODashboard";
import ManageLinkedFarmers from "../pages/fpo-admin/ManageLinkedFarmers";

// Platform Admin pages
import RouteDashboard from "../pages/admin/RouteDashboard";

import { useAuth } from "../hooks/useAuth";

// Guard that checks isProfileComplete — redirects to /profile-setup if not done
function ProfileCompleteRoute({ children }) {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return <div className="p-8 text-center text-gray-500">Loading...</div>;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (!user.isProfileComplete) {
    return <Navigate to="/profile-setup" replace />;
  }

  return children;
}

export default function AppRoutes() {
  return (
    <Routes>
      {/* ---------- Public routes ---------- */}
      <Route path="/" element={<Home />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* ---------- Profile setup ---------- */}
      {/* Protected (must be logged in) but NOT gated by isProfileComplete */}
      <Route
        path="/profile-setup"
        element={
          <ProtectedRoute>
            <ProfileSetup />
          </ProtectedRoute>
        }
      />

      {/* ---------- Farmer routes ---------- */}
      <Route
        path="/farmer"
        element={
          <ProtectedRoute allowedRoles={[ROLES.FARMER]}>
            <ProfileCompleteRoute>
              <DashboardLayout />
            </ProfileCompleteRoute>
          </ProtectedRoute>
        }
      >
        <Route path="dashboard" element={<FarmerDashboard />} />
        <Route path="list-product" element={<ListProduct />} />
        <Route path="orders" element={<FarmerOrders />} />
      </Route>

      {/* ---------- Buyer routes ---------- */}
      <Route
        path="/buyer"
        element={
          <ProtectedRoute allowedRoles={[ROLES.BUYER]}>
            <ProfileCompleteRoute>
              <DashboardLayout />
            </ProfileCompleteRoute>
          </ProtectedRoute>
        }
      >
        <Route path="browse" element={<BrowseProducts />} />
        <Route path="cart" element={<Cart />} />
        <Route path="checkout" element={<Checkout />} />
        <Route path="orders" element={<MyOrders />} />
      </Route>

      {/* ---------- FPO Admin routes ---------- */}
      <Route
        path="/fpo-admin"
        element={
          <ProtectedRoute allowedRoles={[ROLES.FPO_ADMIN]}>
            <ProfileCompleteRoute>
              <DashboardLayout />
            </ProfileCompleteRoute>
          </ProtectedRoute>
        }
      >
        <Route path="dashboard" element={<FPODashboard />} />
        <Route path="farmers" element={<ManageLinkedFarmers />} />
      </Route>

      {/* ---------- Platform Admin routes ---------- */}
      <Route
        path="/admin"
        element={
          <ProtectedRoute allowedRoles={[ROLES.PLATFORM_ADMIN]}>
            <ProfileCompleteRoute>
              <DashboardLayout />
            </ProfileCompleteRoute>
          </ProtectedRoute>
        }
      >
        <Route path="dashboard" element={<RouteDashboard />} />
      </Route>

      {/* ---------- Fallback ---------- */}
      <Route path="*" element={<Home />} />
    </Routes>
  );
}