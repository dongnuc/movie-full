import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryService, Category } from '../../../../core/services/admin-system/category.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { AdminCategoriesFormComponent } from '../form/categories-form.component';
import { TableColumn } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-admin-categories-list',
  standalone: true,
  imports: [CommonModule, AdminTableComponent, AdminCategoriesFormComponent],
  templateUrl: './categories-list.component.html'
})
export class AdminCategoriesListComponent implements OnInit {
  private categoryService = inject(CategoryService);

  categories = signal<Category[]>([]);
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Modal form states
  showForm = signal(false);
  selectedCategoryId = signal<string | null>(null);

  columns: TableColumn[] = [
    { key: 'name', header: 'Tên Thể Loại', type: 'text' },
    { key: 'isActive', header: 'Trạng Thái', type: 'boolean' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.categoryService.getCategories(this.pageIndex(), this.pageSize()).subscribe({
      next: (res) => {
        this.categories.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching categories:', err);
        this.loading.set(false);
      }
    });
  }

  onPageChange(page: number): void {
    this.pageIndex.set(page);
    this.loadData();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageIndex.set(1); // Reset to first page
    this.loadData();
  }

  openCreate(): void {
    this.selectedCategoryId.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Category): void {
    this.selectedCategoryId.set(row.id);
    this.showForm.set(true);
  }

  onToggleActive(row: Category): void {
    const updatedStatus = !row.isActive;
    this.categoryService.updateCategory(row.id, { isActive: updatedStatus }).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error toggling active status:', err);
        alert('Có lỗi xảy ra khi đổi trạng thái thể loại.');
      }
    });
  }

  onDelete(row: Category): void {
    this.categoryService.deleteCategory(row.id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error deleting category:', err);
        alert('Không thể xóa thể loại. Hãy chắc chắn thể loại này không có phim nào đang thuộc về.');
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
