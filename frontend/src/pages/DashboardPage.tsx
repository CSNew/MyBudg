import { useEffect, useState } from 'react';
import client from '../api/client';
import { DashboardData } from '../types';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, LineChart, Line, CartesianGrid, XAxis, YAxis } from 'recharts';

const colors = ['#3B82F6', '#22C55E', '#F97316', '#EF4444', '#8B5CF6', '#06B6D4'];

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);

  useEffect(() => {
    client.get<DashboardData>('/reports/dashboard').then((res) => setData(res.data));
  }, []);

  if (!data) return <p>Loading dashboard...</p>;

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card label="Total Income" value={data.totalIncome} color="text-green-600" />
        <Card label="Total Expense" value={data.totalExpense} color="text-red-600" />
        <Card label="Balance" value={data.balance} color="text-blue-600" />
      </div>

      <div className="grid md:grid-cols-2 gap-4">
        <div className="bg-white p-4 rounded shadow h-80">
          <h3 className="font-semibold mb-4">Monthly Summary</h3>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data.monthlySummary}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="income" stroke="#22C55E" />
              <Line type="monotone" dataKey="expense" stroke="#EF4444" />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white p-4 rounded shadow h-80">
          <h3 className="font-semibold mb-4">Expense Categories</h3>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={data.categoryBreakdown} dataKey="total" nameKey="category" outerRadius={110} label>
                {data.categoryBreakdown.map((_, i) => (
                  <Cell key={i} fill={colors[i % colors.length]} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}

function Card({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div className="bg-white rounded shadow p-4">
      <p className="text-sm text-slate-500">{label}</p>
      <p className={`text-2xl font-bold ${color}`}>${value.toFixed(2)}</p>
    </div>
  );
}
