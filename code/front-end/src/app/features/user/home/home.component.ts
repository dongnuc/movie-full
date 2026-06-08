import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { MovieUserService, MovieUser } from '../../../core/services/user/movie-user.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit, OnDestroy {
  authService = inject(AuthService);
  private movieUserService = inject(MovieUserService);

  playingMovies = signal<MovieUser[]>([]);
  upcomingMovies = signal<MovieUser[]>([]);
  loadingPlaying = signal<boolean>(true);
  loadingUpcoming = signal<boolean>(true);

  // Slider State
  playingIndex = signal<number>(0);
  upcomingIndex = signal<number>(0);
  playingPaused = signal<boolean>(false);
  upcomingPaused = signal<boolean>(false);
  private playingTimer: any;
  private upcomingTimer: any;

  ngOnInit(): void {
    this.loadPlayingMovies();
    this.loadUpcomingMovies();
  }

  ngOnDestroy(): void {
    this.clearTimers();
  }

  loadPlayingMovies(): void {
    this.loadingPlaying.set(true);
    this.movieUserService.getFeaturedToday().subscribe({
      next: (res) => {
        this.playingMovies.set(res);
        this.loadingPlaying.set(false);
        this.resetPlayingTimer();
      },
      error: (err) => {
        console.error('Error fetching playing movies:', err);
        this.loadingPlaying.set(false);
      }
    });
  }

  loadUpcomingMovies(): void {
    this.loadingUpcoming.set(true);
    this.movieUserService.getUpcoming().subscribe({
      next: (res) => {
        this.upcomingMovies.set(res);
        this.loadingUpcoming.set(false);
        this.resetUpcomingTimer();
      },
      error: (err) => {
        console.error('Error fetching upcoming movies:', err);
        this.loadingUpcoming.set(false);
      }
    });
  }

  // Slider Helpers
  getItemsPerPage(): number {
    if (typeof window !== 'undefined') {
      const width = window.innerWidth;
      if (width >= 1024) return 6; // lg
      if (width >= 768) return 4;  // md
      if (width >= 640) return 3;  // sm
      return 2;                    // mobile
    }
    return 6;
  }

  getMaxIndex(totalItems: number): number {
    const perPage = this.getItemsPerPage();
    return Math.max(0, totalItems - perPage);
  }

  slideLeft(type: 'playing' | 'upcoming'): void {
    if (type === 'playing') {
      this.playingIndex.update(idx => Math.max(0, idx - 1));
      this.resetPlayingTimer();
    } else {
      this.upcomingIndex.update(idx => Math.max(0, idx - 1));
      this.resetUpcomingTimer();
    }
  }

  slideRight(type: 'playing' | 'upcoming'): void {
    if (type === 'playing') {
      const max = this.getMaxIndex(this.playingMovies().length);
      this.playingIndex.update(idx => Math.min(max, idx + 1));
      this.resetPlayingTimer();
    } else {
      const max = this.getMaxIndex(this.upcomingMovies().length);
      this.upcomingIndex.update(idx => Math.min(max, idx + 1));
      this.resetUpcomingTimer();
    }
  }

  resetPlayingTimer(): void {
    if (this.playingTimer) clearInterval(this.playingTimer);
    this.playingTimer = setInterval(() => {
      if (this.playingPaused()) return; // Pause auto-sliding on hover
      const movies = this.playingMovies();
      if (movies.length > 0) {
        const maxIdx = this.getMaxIndex(movies.length);
        if (maxIdx > 0) {
          this.playingIndex.update(idx => (idx >= maxIdx ? 0 : idx + 1));
        } else {
          this.playingIndex.set(0);
        }
      }
    }, 3000);
  }

  resetUpcomingTimer(): void {
    if (this.upcomingTimer) clearInterval(this.upcomingTimer);
    this.upcomingTimer = setInterval(() => {
      if (this.upcomingPaused()) return; // Pause auto-sliding on hover
      const movies = this.upcomingMovies();
      if (movies.length > 0) {
        const maxIdx = this.getMaxIndex(movies.length);
        if (maxIdx > 0) {
          this.upcomingIndex.update(idx => (idx >= maxIdx ? 0 : idx + 1));
        } else {
          this.upcomingIndex.set(0);
        }
      }
    }, 3000);
  }

  private clearTimers(): void {
    if (this.playingTimer) clearInterval(this.playingTimer);
    if (this.upcomingTimer) clearInterval(this.upcomingTimer);
  }
}
