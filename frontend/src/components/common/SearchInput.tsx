import React, { useState } from 'react';
import { Search, X } from 'lucide-react';
import { useDebounce } from '../../hooks/useDebounce';
import './SearchInput.scss';

interface SearchInputProps {
  placeholder?: string;
  onSearch: (value: string) => void;
  delay?: number;
}

export const SearchInput: React.FC<SearchInputProps> = ({
  placeholder = 'Search...',
  onSearch,
  delay = 300,
}) => {
  const [value, setValue] = useState('');
  const debouncedValue = useDebounce(value, delay);

  React.useEffect(() => {
    onSearch(debouncedValue);
  }, [debouncedValue, onSearch]);

  const handleClear = () => {
    setValue('');
  };

  return (
    <div className="search-input">
      <Search size={18} className="search-icon" />
      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={(e) => setValue(e.target.value)}
        className="search-field"
      />
      {value && <X size={18} className="clear-icon" onClick={handleClear} />}
    </div>
  );
};
