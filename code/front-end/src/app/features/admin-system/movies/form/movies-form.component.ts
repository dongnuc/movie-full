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
    isActive: true,
    isHomePage: false
  };

  selectedBannerFile: File | null = null;
  selectedPosterFile: File | null = null;
  bannerPreviewUrl: string | null = null;
  posterPreviewUrl: string | null = null;
  clearBannerField = false;
  clearPosterField = false;

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
        this.bannerPreviewUrl = res.imageBanner || null;
        this.posterPreviewUrl = res.imagePoster || null;
        this.clearBannerField = false;
        this.clearPosterField = false;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải chi tiết phim';
        this.loading = false;
      }
    });
  }

  validateImageDimensions(file: File, minWidth: number, minHeight: number, isLandscape: boolean): Promise<boolean> {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        const img = new Image();
        img.onload = () => {
          const width = img.width;
          const height = img.height;
          const validOrientation = isLandscape ? (width > height) : (height > width);
          const validSize = width >= minWidth && height >= minHeight;
          resolve(validOrientation && validSize);
        };
        img.onerror = () => resolve(false);
        img.src = e.target.result;
      };
      reader.onerror = () => resolve(false);
      reader.readAsDataURL(file);
    });
  }

  async onBannerSelected(event: any): Promise<void> {
    const file = event.target.files?.[0];
    if (file) {
      this.errorMessage = '';
      const isValid = await this.validateImageDimensions(file, 1600, 500, true);
      if (!isValid) {
        this.errorMessage = 'Ảnh banner phải là ảnh ngang (Landscape) và có kích thước tối thiểu là 1600x500.';
        this.clearBanner(event, event.target);
        return;
      }
      this.selectedBannerFile = file;
      this.bannerPreviewUrl = URL.createObjectURL(file);
      this.clearBannerField = false;
    }
  }

  async onPosterSelected(event: any): Promise<void> {
    const file = event.target.files?.[0];
    if (file) {
      this.errorMessage = '';
      const isValid = await this.validateImageDimensions(file, 400, 800, false);
      if (!isValid) {
        this.errorMessage = 'Ảnh poster phải là ảnh dọc (Portrait) và có kích thước tối thiểu là 400x800.';
        this.clearPoster(event, event.target);
        return;
      }
      this.selectedPosterFile = file;
      this.posterPreviewUrl = URL.createObjectURL(file);
      this.clearPosterField = false;
    }
  }

  clearBanner(event: Event, inputEl?: HTMLInputElement): void {
    event.stopPropagation();
    this.selectedBannerFile = null;
    this.bannerPreviewUrl = null;
    this.clearBannerField = true;
    if (inputEl) {
      inputEl.value = '';
    }
  }

  clearPoster(event: Event, inputEl?: HTMLInputElement): void {
    event.stopPropagation();
    this.selectedPosterFile = null;
    this.posterPreviewUrl = null;
    this.clearPosterField = true;
    if (inputEl) {
      inputEl.value = '';
    }
  }

  onSubmit(): void {
    if (!this.movie.title?.trim() || !this.movie.categoryId || !this.movie.duration || !this.movie.basePrice) {
      this.errorMessage = 'Vui lòng điền đầy đủ các trường bắt buộc';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const moviePayload: Partial<Movie> = {
      ...this.movie,
      clearBanner: this.clearBannerField,
      clearPoster: this.clearPosterField
    };

    const obs$ = this.movieId
      ? this.movieService.updateMovie(this.movieId, moviePayload)
      : this.movieService.createMovie(moviePayload);

    obs$.subscribe({
      next: (res) => {
        if (this.selectedBannerFile || this.selectedPosterFile) {
          const targetId = this.movieId || res?.id;
          if (!targetId) {
            this.errorMessage = 'Lỗi hệ thống: Không xác định được ID phim để tải ảnh.';
            this.saving = false;
            return;
          }

          this.movieService.uploadMovieImages(targetId, this.selectedBannerFile, this.selectedPosterFile).subscribe({
            next: () => {
              this.saving = false;
              this.close.emit(true);
            },
            error: (err) => {
              const errorData = err.error;
              this.errorMessage = errorData?.detail || errorData?.message || err.message || 'Lỗi khi tải ảnh lên. Bạn có thể xóa ảnh bị lỗi này để lưu thông tin phim không kèm ảnh.';
              this.saving = false;
            }
          });
        } else {
          this.saving = false;
          this.close.emit(true);
        }
      },
      error: (err) => {
        const errorData = err.error;
        this.errorMessage = errorData?.detail || errorData?.message || err.message || 'Lỗi khi lưu phim';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
