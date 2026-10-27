export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
}

export interface Category {
  id: number;
  name: string;
  type: 'income' | 'expense';
  isDefault: boolean;
}

export interface Transaction {
  id: number;
  amount: number;
  type: 'income' | 'expense';
  categoryId: number;
  categoryName: string;
  date: string;
  note?: string;
}

export interface Budget {
  id: number;
  categoryId: number;
  categoryName: string;
  year: number;
  month: number;
  limitAmount: number;
  spentAmount: number;
  isExceeded: boolean;
}

export interface DashboardData {
  totalIncome: number;
  totalExpense: number;
  balance: number;
  monthlySummary: { month: string; income: number; expense: number }[];
  categoryBreakdown: { category: string; total: number }[];
}
