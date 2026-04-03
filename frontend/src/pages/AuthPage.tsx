import { FormEvent, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export default function AuthPage({ mode }: { mode: 'login' | 'register' }) {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login, register } = useAuth();
  const navigate = useNavigate();

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      if (mode === 'login') {
        await login(email, password);
      } else {
        await register(fullName, email, password);
      }
      navigate('/');
    } catch {
      setError('Authentication failed.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-100">
      <form onSubmit={onSubmit} className="bg-white p-8 rounded shadow w-full max-w-md space-y-4">
        <h2 className="text-2xl font-semibold">{mode === 'login' ? 'Login' : 'Create account'}</h2>
        {mode === 'register' && (
          <input className="w-full border p-2 rounded" value={fullName} onChange={(e) => setFullName(e.target.value)} placeholder="Full name" required />
        )}
        <input className="w-full border p-2 rounded" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" type="email" required />
        <input className="w-full border p-2 rounded" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" type="password" required />
        {error && <p className="text-red-500 text-sm">{error}</p>}
        <button className="w-full bg-slate-900 text-white p-2 rounded">{mode === 'login' ? 'Login' : 'Register'}</button>
      </form>
    </div>
  );
}
