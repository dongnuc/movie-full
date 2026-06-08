import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MovieService, Movie } from '../../../../core/services/admin-system/movie.service';
import { CategoryService, Category } from '../../../../core/services/admin-system/category.service';

@Component({
  selector: 'app-admin-movies-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './movies-form.component.html'
})
export class AdminMoviesFormComponent implements OnInit {
  private movieService = inject(MovieService);
  private categoryService = inject(CategoryService);

  @Input() movieId: string | null = null;
  @Output() close = new EventEmitter<boolean>();

  movie: Partial<Movie> = {
    title: '',
    description: '',
    image: '',
    basePrice: 0,
    duration: 0,
    categoryId: '',
    isActive: true
  };

  categories: Category[] = [];
  loading = false;
  saving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getCategories(1, 100).subscribe({
      next: (res) => {
        // Display only active categories
        this.categories = res.items.filter((c: Category) => c.isActive);
        if (this.movieId) {
          this.loadMovie();
        } else {
          this.loading = false;
        }
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải danh sách thể loại';
        this.loading = false;
      }
    });
  }

  loadMovie(): void {
    this.movieService.getMovieById(this.movieId!).subscribe({
      next: (res) => {
        this.movie = { ...res };
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải chi tiết phim';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.movie.title?.trim() || !this.movie.categoryId || !this.movie.duration || !this.movie.basePrice) {
      this.errorMessage = 'Vui lòng điền đầy đủ các trường bắt buộc';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const obs$ = this.movieId
      ? this.movieService.updateMovie(this.movieId, this.movie)
      : this.movieService.createMovie(this.movie);

    obs$.subscribe({
      next: () => {
        this.saving = false;
        this.close.emit(true);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Lỗi khi lưu phim';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
