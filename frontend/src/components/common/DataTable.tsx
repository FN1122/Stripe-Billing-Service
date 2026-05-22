import React, { useState } from 'react';
import { ChevronUp, ChevronDown } from 'lucide-react';
import './DataTable.scss';

export interface DataTableColumn<T> {
  key: keyof T;
  label: string;
  width?: string;
  sortable?: boolean;
  render?: (value: any, row: T) => React.ReactNode;
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[];
  data: T[];
  isLoading?: boolean;
  onSort?: (key: string, order: 'asc' | 'desc') => void;
  rowKey: keyof T;
  onRowClick?: (row: T) => void;
}

export const DataTable = <T,>({
  columns,
  data,
  isLoading = false,
  onSort,
  rowKey,
  onRowClick,
}: DataTableProps<T>) => {
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  const handleSort = (key: string) => {
    const newOrder = sortKey === key && sortOrder === 'asc' ? 'desc' : 'asc';
    setSortKey(key);
    setSortOrder(newOrder);
    onSort?.(key, newOrder);
  };

  return (
    <div className="data-table-wrapper">
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={String(col.key)} style={{ width: col.width }} className={col.sortable ? 'sortable' : ''}>
                {col.sortable ? (
                  <button onClick={() => handleSort(String(col.key))} className="sort-btn">
                    {col.label}
                    {sortKey === String(col.key) && (
                      <span className="sort-icon">
                        {sortOrder === 'asc' ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
                      </span>
                    )}
                  </button>
                ) : (
                  col.label
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <tr className="loading-row">
              <td colSpan={columns.length}>Loading...</td>
            </tr>
          ) : data.length === 0 ? (
            <tr className="empty-row">
              <td colSpan={columns.length}>No data available</td>
            </tr>
          ) : (
            data.map((row) => (
              <tr
                key={String(row[rowKey])}
                className={onRowClick ? 'clickable' : ''}
                onClick={() => onRowClick?.(row)}
              >
                {columns.map((col) => (
                  <td key={String(col.key)}>
                    {col.render ? col.render(row[col.key], row) : String(row[col.key])}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};
