import { createContext, useContext, useState, ReactNode } from 'react';
import client from '../api/client';
import { AuthResponse } from '../types';

interface AuthContextType {
  token: string | null;
  fullName: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (fullName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [fullName, setFullName] = useState(localStorage.getItem('fullName'));

  const handleAuth = (data: AuthResponse) => {
    localStorage.setItem('token', data.token);
    localStorage.setItem('fullName', data.fullName);
    setToken(data.token);
    setFullName(data.fullName);
  };

  const login = async (email: string, password: string) => {
    const { data } = await client.post<AuthResponse>('/auth/login', { email, password });
    handleAuth(data);
  };

  const register = async (fullName: string, email: string, password: string) => {
    const { data } = await client.post<AuthResponse>('/auth/register', { fullName, email, password });
    handleAuth(data);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('fullName');
    setToken(null);
    setFullName(null);
  };

  return <AuthContext.Provider value={{ token, fullName, login, register, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('AuthContext missing');
  return context;
}
