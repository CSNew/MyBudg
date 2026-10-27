import { FormEvent, useEffect, useState } from 'react';
import client from '../api/client';
import { Category } from '../types';

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [name, setName] = useState('');
  const [type, setType] = useState<'income' | 'expense'>('expense');

  const load = async () => {
    const { data } = await client.get<Category[]>('/categories');
    setCategories(data);
  };

  useEffect(() => {
    load();
  }, []);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    await client.post('/categories', { name, type });
    setName('');
    load();
  };

  return (
    <div className="grid md:grid-cols-[300px_1fr] gap-4">
      <form onSubmit={submit} className="bg-white p-4 rounded shadow space-y-3 h-fit">
        <h2 className="font-semibold">Add Category</h2>
        <input className="w-full border rounded p-2" value={name} onChange={(e) => setName(e.target.value)} placeholder="Category name" required />
        <select className="w-full border rounded p-2" value={type} onChange={(e) => setType(e.target.value as 'income' | 'expense')}>
          <option value="expense">Expense</option>
          <option value="income">Income</option>
        </select>
        <button className="w-full bg-blue-600 text-white p-2 rounded">Create</button>
      </form>

      <div className="bg-white rounded shadow p-4">
        <h2 className="font-semibold mb-3">All Categories</h2>
        <ul className="space-y-2">
          {categories.map((c) => (
            <li key={c.id} className="flex justify-between border-b pb-2">
              <span>{c.name} <span className="text-xs text-slate-500">({c.type})</span></span>
              {!c.isDefault && <button className="text-red-500" onClick={async () => { await client.delete(`/categories/${c.id}`); load(); }}>Delete</button>}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
