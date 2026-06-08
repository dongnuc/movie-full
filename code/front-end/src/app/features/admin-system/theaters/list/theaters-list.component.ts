import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TheaterService, Theater } from '../../../../core/services/admin-system/theater.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { AdminTheatersFormComponent } from '../form/theaters-form.component';
import { TableColumn, TableFilter } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-admin-theaters-list',
  standalone: true,
  imports: [CommonModule, AdminTableComponent, AdminTheatersFormComponent],
  templateUrl: './theaters-list.component.html'
})
export class AdminTheatersListComponent implements OnInit {
  private theaterService = inject(TheaterService);

  theaters = signal<Theater[]>([]);
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Filters
  searchQuery = signal('');
  selectedActive = signal<boolean | undefined>(undefined);

  filters: TableFilter[] = [
    {
      key: 'isActive',
      placeholder: 'Tất cả trạng thái',
      options: [
        { value: 'true', label: 'Hoạt động' },
        { value: 'false', label: 'Tạm dừng' }
      ]
    }
  ];

  // Modal form states
  showForm = signal(false);
  selectedTheaterId = signal<string | null>(null);

  columns: TableColumn[] = [
    { key: 'name', header: 'Tên Rạp', type: 'text' },
    { key: 'location', header: 'Khu Vực', type: 'text' },
    { key: 'isActive', header: 'Trạng Thái', type: 'boolean' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.theaterService.getTheaters({
      search: this.searchQuery(),
      isActive: this.selectedActive(),
      page: this.pageIndex(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        this.theaters.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching theaters:', err);
        this.loading.set(false);
      }
    });
  }

  onSearch(query: string): void {
    this.searchQuery.set(query);
    this.pageIndex.set(1);
    this.loadData();
  }

  onFilterChange(filtersMap: { [key: string]: any }): void {
    if (filtersMap['isActive'] !== undefined) {
      const val = filtersMap['isActive'];
      this.selectedActive.set(val === 'true' ? true : val === 'false' ? false : undefined);
    }
    this.pageIndex.set(1);
    this.loadData();
  }

  onPageChange(page: number): void {
    this.pageIndex.set(page);
    this.loadData();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageIndex.set(1);
    this.loadData();
  }

  openCreate(): void {
    this.selectedTheaterId.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Theater): void {
    this.selectedTheaterId.set(row.id);
    this.showForm.set(true);
  }

  onToggleActive(row: Theater): void {
    this.theaterService.updateTheater(row.id, { isActive: !row.isActive }).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error toggling theater active status:', err);
        alert('Có lỗi xảy ra khi đổi trạng thái rạp.');
      }
    });
  }

  onDelete(row: Theater): void {
    this.theaterService.deleteTheater(row.id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error deleting theater:', err);
        alert('Không thể xóa rạp chiếu. Hãy chắc chắn rạp không chứa phòng chiếu nào.');
      }
    });
  }

  onFormClose(saved: boolean): void {
    this.showForm.set(false);
    if (saved) {
      this.loadData();
    }
  }
}
