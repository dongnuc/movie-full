export interface TableColumn {
  key: string;
  header: string;
  type?: 'text' | 'boolean' | 'number' | 'currency' | 'date' | 'badge' | 'image' | 'custom';
  badgeClassMap?: { [key: string]: string };
  dateFormat?: string;
  sortable?: boolean;
}

export interface TableFilter {
  key: string;
  placeholder: string;
  options: { value: any; label: string }[];
  defaultValue?: any;
}

export interface TableAction {
  label: string;
  icon?: string;
  class?: string;
  callback: (row: any) => void;
}

export interface PagedResult<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPrevious: boolean;
}
