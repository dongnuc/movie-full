import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService, Category } from '../../../../core/services/category.service';

@Component({
  selector: 'app-admin-categories-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories-form.component.html'
})
export class AdminCategoriesFormComponent implements OnInit {
  private categoryService = inject(CategoryService);

  @Input() categoryId: string | null = null;
  @Output() close = new EventEmitter<boolean>(); // emits true if saved successfully

  category: Partial<Category> = {
    name: '',
    isActive: true
  };

  loading = false;
  saving = false;
  errorMessage = '';

  ngOnInit(): void {
    if (this.categoryId) {
      this.loadCategory();
    }
  }

  loadCategory(): void {
    this.loading = true;
    this.categoryService.getCategoryById(this.categoryId!).subscribe({
      next: (res) => {
        this.category = { ...res };
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err.message || 'Không thể tải thông tin thể loại';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.category.name?.trim()) {
      this.errorMessage = 'Vui lòng nhập tên thể loại';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const obs$ = this.categoryId 
      ? this.categoryService.updateCategory(this.categoryId, this.category)
      : this.categoryService.createCategory(this.category);

    obs$.subscribe({
      next: () => {
        this.saving = false;
        this.close.emit(true);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Lỗi khi lưu thể loại';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
