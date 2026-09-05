import { Link } from "react-router-dom";

export default function Home() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-green-50 px-6">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-green-800">FasalConnect</h1>
        <p className="mt-3 text-gray-600">
          Connect directly with fresh local produce.
        </p>
        <div className="mt-6 flex justify-center gap-3">
          <Link
            to="/login"
            className="rounded-lg bg-green-600 px-5 py-3 font-semibold text-white"
          >
            Sign in
          </Link>
          <Link
            to="/register"
            className="rounded-lg border border-green-600 px-5 py-3 font-semibold text-green-700"
          >
            Register
          </Link>
        </div>
      </div>
    </main>
  );
}
