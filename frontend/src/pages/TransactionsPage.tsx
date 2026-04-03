import { FormEvent, useEffect, useState } from 'react';
import client from '../api/client';
import { Category, Transaction } from '../types';

const initial = { amount: 0, type: 'expense', categoryId: 0, date: '', note: '' };

export default function TransactionsPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [form, setForm] = useState(initial);

  const load = async () => {
    const [txRes, catRes] = await Promise.all([
      client.get<Transaction[]>('/transactions'),
      client.get<Category[]>('/categories')
    ]);
    setTransactions(txRes.data);
    setCategories(catRes.data);
  };

  useEffect(() => {
    load();
  }, []);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    await client.post('/transactions', {
      ...form,
      amount: Number(form.amount),
      categoryId: Number(form.categoryId)
    });
    setForm(initial);
    load();
  };

  return (
    <div className="grid md:grid-cols-[360px_1fr] gap-4">
      <form onSubmit={submit} className="bg-white p-4 rounded shadow space-y-3 h-fit">
        <h2 className="font-semibold">Add Transaction</h2>
        <input className="w-full border p-2 rounded" type="number" step="0.01" placeholder="Amount" value={form.amount} onChange={(e) => setForm({ ...form, amount: Number(e.target.value) })} required />
        <select className="w-full border p-2 rounded" value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })}>
          <option value="income">Income</option>
          <option value="expense">Expense</option>
        </select>
        <select className="w-full border p-2 rounded" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })} required>
          <option value={0}>Choose category</option>
          {categories.filter((c) => c.type === form.type).map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
        <input className="w-full border p-2 rounded" type="date" value={form.date} onChange={(e) => setForm({ ...form, date: e.target.value })} required />
        <textarea className="w-full border p-2 rounded" placeholder="Note" value={form.note} onChange={(e) => setForm({ ...form, note: e.target.value })} />
        <button className="w-full bg-blue-600 text-white p-2 rounded">Save</button>
      </form>

      <div className="bg-white rounded shadow p-4 overflow-x-auto">
        <h2 className="font-semibold mb-3">Transactions</h2>
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left border-b">
              <th className="py-2">Date</th><th>Type</th><th>Category</th><th>Amount</th><th>Note</th><th></th>
            </tr>
          </thead>
          <tbody>
            {transactions.map((tx) => (
              <tr key={tx.id} className="border-b">
                <td className="py-2">{tx.date.slice(0, 10)}</td>
                <td>{tx.type}</td>
                <td>{tx.categoryName}</td>
                <td className={tx.type === 'income' ? 'text-green-600' : 'text-red-600'}>${tx.amount.toFixed(2)}</td>
                <td>{tx.note}</td>
                <td>
                  <button className="text-red-500" onClick={async () => { await client.delete(`/transactions/${tx.id}`); load(); }}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
