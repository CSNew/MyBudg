import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { fullName, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-slate-100">
      <header className="bg-slate-900 text-white px-6 py-4 flex justify-between items-center">
        <h1 className="font-bold text-xl">MyBudg</h1>
        <nav className="space-x-4 text-sm">
          <Link to="/">Dashboard</Link>
          <Link to="/transactions">Transactions</Link>
          <Link to="/categories">Categories</Link>
          <Link to="/budgets">Budgets</Link>
          <Link to="/reports">Reports</Link>
        </nav>
        <div className="flex items-center gap-3">
          <span>{fullName}</span>
          <button
            className="bg-red-500 px-3 py-1 rounded"
            onClick={() => {
              logout();
              navigate('/login');
            }}
          >
            Logout
          </button>
        </div>
      </header>
      <main className="max-w-6xl mx-auto py-6 px-4">
        <Outlet />
      </main>
    </div>
  );
}
