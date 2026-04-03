import jsPDF from 'jspdf';

export default function ReportsPage() {
  const download = async (kind: 'monthly' | 'yearly') => {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;
    const endpoint = kind === 'monthly' ? `/reports/monthly?year=${year}&month=${month}` : `/reports/yearly?year=${year}`;
    const token = localStorage.getItem('token');
    const response = await fetch(`${import.meta.env.VITE_API_URL ?? 'https://localhost:5001/api'}${endpoint}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const csvText = await response.text();

    const pdf = new jsPDF();
    pdf.setFontSize(12);
    pdf.text(`${kind.toUpperCase()} Report`, 10, 10);
    const lines = csvText.split('\n').slice(0, 35);
    lines.forEach((line, idx) => pdf.text(line.substring(0, 95), 10, 20 + idx * 6));
    pdf.save(`${kind}-report.pdf`);
  };

  return (
    <div className="bg-white rounded shadow p-6 space-y-3">
      <h2 className="text-xl font-semibold">Reports</h2>
      <p>Generate monthly and yearly reports and export to PDF.</p>
      <div className="space-x-3">
        <button className="bg-blue-600 text-white px-4 py-2 rounded" onClick={() => download('monthly')}>Export Monthly PDF</button>
        <button className="bg-slate-800 text-white px-4 py-2 rounded" onClick={() => download('yearly')}>Export Yearly PDF</button>
      </div>
    </div>
  );
}
