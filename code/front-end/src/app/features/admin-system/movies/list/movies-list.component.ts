import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MovieService, Movie } from '../../../../core/services/movie.service';
import { CategoryService, Category } from '../../../../core/services/category.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { AdminMoviesFormComponent } from '../form/movies-form.component';
import { TableColumn, TableFilter } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-admin-movies-list',
  standalone: true,
  imports: [CommonModule, AdminTableComponent, AdminMoviesFormComponent],
  templateUrl: './movies-list.component.html'
})
export class AdminMoviesListComponent implements OnInit {
  private movieService = inject(MovieService);
  private categoryService = inject(CategoryService);

  movies = signal<Movie[]>([]);
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Filters
  searchQuery = signal('');
  selectedCategoryId = signal('');
  selectedActive = signal<boolean | undefined>(undefined);

  filters = signal<TableFilter[]>([]);

  // Modal form states
  showForm = signal(false);
  selectedMovieId = signal<string | null>(null);

  columns: TableColumn[] = [
    { key: 'image', header: 'Poster', type: 'image' },
    { key: 'title', header: 'Tên phim', type: 'text' },
    { key: 'categoryName', header: 'Thể loại', type: 'text' },
    { key: 'basePrice', header: 'Giá cơ bản', type: 'currency' },
    { key: 'duration', header: 'Thời lượng (phút)', type: 'number' },
    { key: 'isActive', header: 'Trạng thái', type: 'boolean' }
  ];

  ngOnInit(): void {
    this.loadCategories();
    this.loadData();
  }

  loadCategories(): void {
    this.categoryService.getCategories(1, 100).subscribe({
      next: (res) => {
        const categoryOptions = res.items
          .filter(c => c.isActive)
          .map(c => ({ value: c.id, label: c.name }));

        this.filters.set([
          {
            key: 'categoryId',
            placeholder: 'Tất cả thể loại',
            options: categoryOptions
          },
          {
            key: 'isActive',
            placeholder: 'Tất cả trạng thái',
            options: [
              { value: 'true', label: 'Hoạt động' },
              { value: 'false', label: 'Tạm dừng' }
            ]
          }
        ]);
      }
    });
  }

  loadData(): void {
    this.loading.set(true);
    this.movieService.getMovies({
      search: this.searchQuery(),
      categoryId: this.selectedCategoryId() || undefined,
      isActive: this.selectedActive(),
      page: this.pageIndex(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        this.movies.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching movies:', err);
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
    if (filtersMap['categoryId'] !== undefined) {
      this.selectedCategoryId.set(filtersMap['categoryId']);
    }
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
    this.selectedMovieId.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Movie): void {
    this.selectedMovieId.set(row.id);
    this.showForm.set(true);
  }

  onToggleActive(row: Movie): void {
    this.movieService.updateMovie(row.id, { isActive: !row.isActive }).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error toggling movie status:', err);
        alert('Có lỗi xảy ra khi cập nhật trạng thái phim.');
      }
    });
  }

  onDelete(row: Movie): void {
    this.movieService.deleteMovie(row.id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error deleting movie:', err);
        alert('Không thể xóa phim này. Có thể phim đã có lịch chiếu được gán.');
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
