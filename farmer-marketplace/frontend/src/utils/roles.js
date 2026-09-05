// frontend/src/utils/roles.js
//
// Mirrors backend/FarmerMarketplace.Api/Models/User.cs -> enum UserRole
// IMPORTANT: it's "PlatformAdmin", not "Admin" — keep this file as the
// single source of truth so a typo doesn't silently break role checks
// scattered across the app.

export const ROLES = {
  FARMER: "Farmer",
  BUYER: "Buyer",
  FPO_ADMIN: "FpoAdmin",
  PLATFORM_ADMIN: "PlatformAdmin",
};

export const ROLE_OPTIONS = [
  { value: ROLES.FARMER, label: "Farmer" },
  { value: ROLES.BUYER, label: "Buyer" },
  { value: ROLES.FPO_ADMIN, label: "FPO Admin" },
  { value: ROLES.PLATFORM_ADMIN, label: "Platform Admin" },
];

export const DASHBOARD_PATH_BY_ROLE = {
  [ROLES.FARMER]: "/farmer/dashboard",
  [ROLES.BUYER]: "/buyer/browse",
  [ROLES.FPO_ADMIN]: "/fpo-admin/dashboard",
  [ROLES.PLATFORM_ADMIN]: "/admin/dashboard",
};