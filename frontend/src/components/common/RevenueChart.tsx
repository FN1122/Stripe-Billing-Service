import React from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler);

interface RevenueChartProps {
  data: { date: string; amount: number }[];
  title?: string;
  currency?: string;
}

export const RevenueChart: React.FC<RevenueChartProps> = ({
  data,
  title = 'Revenue Chart',
  currency = 'USD',
}) => {
  const chartData = {
    labels: data.map((d) => d.date),
    datasets: [
      {
        label: `Revenue (${currency})`,
        data: data.map((d) => d.amount),
        borderColor: '#667eea',
        backgroundColor: 'rgba(102, 126, 234, 0.1)',
        fill: true,
        tension: 0.4,
        pointBackgroundColor: '#667eea',
        pointBorderColor: '#fff',
        pointBorderWidth: 2,
        pointRadius: 5,
        pointHoverRadius: 7,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: { display: true },
      title: { display: !!title, text: title },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { formatter: (value: number) => `$${(value / 1000).toFixed(0)}K` },
      },
    },
  };

  return <Line data={chartData} options={options} />;
};
