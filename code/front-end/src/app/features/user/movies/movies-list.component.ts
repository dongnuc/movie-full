import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MovieUserService, MovieUser } from '../../../core/services/user/movie-user.service';
import { Category } from '../../../core/services/admin-system/category.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-user-movies-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NavbarComponent],
  templateUrl: './movies-list.component.html'
})
export class UserMoviesListComponent implements OnInit {
  private movieUserService = inject(MovieUserService);

  movies = signal<MovieUser[]>([]);
  categories = signal<Category[]>([]);
  
  // Search & Filter State
  searchQuery = signal<string>('');
  selectedCategoryId = signal<string>('');
  
  // Pagination State
  pageIndex = signal<number>(1);
  pageSize = signal<number>(12);
  totalCount = signal<number>(0);
  loading = signal<boolean>(false);

  constructor() {
    // Automatically re-fetch movies whenever search, category, or page changes
    effect(() => {
      this.loadMovies();
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.movieUserService.getCategories().subscribe({
      next: (res) => {
        // Only show active categories
        this.categories.set(res);
      },
      error: (err) => console.error('Error fetching categories:', err)
    });
  }

  loadMovies(): void {
    this.loading.set(true);
    this.movieUserService.getMovies({
      search: this.searchQuery(),
      categoryId: this.selectedCategoryId() || undefined,
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

  selectCategory(categoryId: string): void {
    this.selectedCategoryId.set(categoryId);
    this.pageIndex.set(1); // Reset page on filter change
  }

  onSearch(query: string): void {
    this.searchQuery.set(query);
    this.pageIndex.set(1); // Reset page on search change
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedCategoryId.set('');
    this.pageIndex.set(1);
  }

  onPageChange(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageIndex.set(page);
  }

  totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize());
  }

  getPagesArray(): number[] {
    const total = this.totalPages();
    return Array.from({ length: total }, (_, i) => i + 1);
  }
}
