import { FormEvent, useEffect, useMemo, useState } from 'react';
import client from '../api/client';
import { Budget, Category } from '../types';

export default function BudgetsPage() {
  const [budgets, setBudgets] = useState<Budget[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [form, setForm] = useState({ categoryId: 0, year: new Date().getFullYear(), month: new Date().getMonth() + 1, limitAmount: 0 });

  const expenseCategories = useMemo(() => categories.filter((c) => c.type === 'expense'), [categories]);

  const load = async () => {
    const [budgetRes, categoryRes] = await Promise.all([
      client.get<Budget[]>('/budgets'),
      client.get<Category[]>('/categories')
    ]);
    setBudgets(budgetRes.data);
    setCategories(categoryRes.data);
  };

  useEffect(() => {
    load();
  }, []);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    await client.post('/budgets', form);
    setForm({ ...form, limitAmount: 0, categoryId: 0 });
    load();
  };

  return (
    <div className="grid md:grid-cols-[320px_1fr] gap-4">
      <form onSubmit={submit} className="bg-white p-4 rounded shadow space-y-3 h-fit">
        <h2 className="font-semibold">Set Monthly Budget</h2>
        <select className="w-full border rounded p-2" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })} required>
          <option value={0}>Expense category</option>
          {expenseCategories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <input className="w-full border rounded p-2" type="number" value={form.year} onChange={(e) => setForm({ ...form, year: Number(e.target.value) })} required />
        <input className="w-full border rounded p-2" type="number" min={1} max={12} value={form.month} onChange={(e) => setForm({ ...form, month: Number(e.target.value) })} required />
        <input className="w-full border rounded p-2" type="number" step="0.01" placeholder="Budget amount" value={form.limitAmount} onChange={(e) => setForm({ ...form, limitAmount: Number(e.target.value) })} required />
        <button className="w-full bg-blue-600 text-white p-2 rounded">Save Budget</button>
      </form>

      <div className="bg-white p-4 rounded shadow">
        <h2 className="font-semibold mb-3">Budgets & Alerts</h2>
        <div className="space-y-2">
          {budgets.map((b) => (
            <div key={b.id} className={`border rounded p-3 ${b.isExceeded ? 'border-red-400 bg-red-50' : 'border-slate-200'}`}>
              <p className="font-medium">{b.categoryName} • {b.year}-{String(b.month).padStart(2, '0')}</p>
              <p>Limit: ${b.limitAmount.toFixed(2)} | Spent: ${b.spentAmount.toFixed(2)}</p>
              {b.isExceeded && <p className="text-red-600 text-sm mt-1">⚠ Budget exceeded.</p>}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
