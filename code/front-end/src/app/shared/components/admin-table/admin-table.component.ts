import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableColumn, TableFilter } from '../../models/table-config';

@Component({
  selector: 'app-admin-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-table.component.html'
})
export class AdminTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() filters: TableFilter[] = [];
  @Input() data: any[] = [];
  @Input() totalCount = 0;
  @Input() pageIndex = 1;
  @Input() pageSize = 10;
  @Input() loading = false;
  @Input() searchPlaceholder = 'Tìm kiếm...';
  @Input() showSearch = true;
  
  // Custom button config
  @Input() showEditAction = true;
  @Input() showToggleActiveAction = true;
  @Input() showDeleteAction = false;

  @Output() searchChange = new EventEmitter<string>();
  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();
  @Output() filterChange = new EventEmitter<{ [key: string]: any }>();
  
  @Output() edit = new EventEmitter<any>();
  @Output() toggleActive = new EventEmitter<any>();
  @Output() delete = new EventEmitter<any>();

  searchQuery = '';
  filterValues: { [key: string]: any } = {};

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }

  get pages(): number[] {
    const list: number[] = [];
    const maxPagesToShow = 5;
    let start = Math.max(1, this.pageIndex - Math.floor(maxPagesToShow / 2));
    let end = Math.min(this.totalPages, start + maxPagesToShow - 1);
    
    if (end - start + 1 < maxPagesToShow) {
      start = Math.max(1, end - maxPagesToShow + 1);
    }
    
    for (let i = start; i <= end; i++) {
      list.push(i);
    }
    return list;
  }

  onSearch(): void {
    this.searchChange.emit(this.searchQuery);
  }

  onFilterChange(key: string, value: any): void {
    this.filterValues[key] = value;
    this.filterChange.emit(this.filterValues);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.pageIndex) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeChange(event: any): void {
    const size = parseInt(event.target.value, 10);
    this.pageSizeChange.emit(size);
  }

  onEdit(row: any): void {
    this.edit.emit(row);
  }

  onToggleActive(row: any): void {
    this.toggleActive.emit(row);
  }

  onDelete(row: any): void {
    if (confirm('Bạn có chắc chắn muốn xóa bản ghi này?')) {
      this.delete.emit(row);
    }
  }

  getValue(row: any, key: string): any {
    return row[key];
  }
}
