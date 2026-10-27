import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import DashboardPage from './pages/DashboardPage';
import TransactionsPage from './pages/TransactionsPage';
import CategoriesPage from './pages/CategoriesPage';
import BudgetsPage from './pages/BudgetsPage';
import ReportsPage from './pages/ReportsPage';
import AuthPage from './pages/AuthPage';

function ProtectedRoutes() {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;

  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/transactions" element={<TransactionsPage />} />
        <Route path="/categories" element={<CategoriesPage />} />
        <Route path="/budgets" element={<BudgetsPage />} />
        <Route path="/reports" element={<ReportsPage />} />
      </Route>
    </Routes>
  );
}

export default function App() {
  const { token } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={<AuthPage mode="login" />} />
      <Route path="/register" element={<AuthPage mode="register" />} />
      <Route path="/*" element={token ? <ProtectedRoutes /> : <Navigate to="/login" replace />} />
    </Routes>
  );
}
