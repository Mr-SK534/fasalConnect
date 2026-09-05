import { useState } from 'react';
import heroImg from './assets/hero.png';
import reactLogo from './assets/react.svg';
import viteLogo from './assets/vite.svg';
import './App.css';

import { AuthProvider } from "./context/AuthContext";
import AppRoutes from "./routes/AppRoutes"; // adjust path if needed

function App() {
  const [count, setCount] = useState(0);

  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}

export default App;
