import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { MovieUserService, MovieDetailUser, ShowingUser, TheaterShowingGroup } from '../../../core/services/user/movie-user.service';
import { AuthService } from '../../../core/auth/auth.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';

interface DateOption {
  label: string;
  dateString: string;
  displayDate: string;
}

interface TheaterDisplayItem {
  theaterId: string;
  theaterName: string;
  location?: string;
  showingCount: number;
}

@Component({
  selector: 'app-user-movie-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './movie-detail.component.html'
})
export class UserMovieDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private movieUserService = inject(MovieUserService);
  authService = inject(AuthService);

  movieId = signal<string>('');
  movie = signal<MovieDetailUser | null>(null);
  showings = signal<ShowingUser[]>([]);
  theaters = signal<TheaterDisplayItem[]>([]);
  loadingMovie = signal<boolean>(true);
  loadingTheaters = signal<boolean>(false);
  loadingShowings = signal<boolean>(false);

  // Date & Theater Selection States
  dateOptions = signal<DateOption[]>([]);
  selectedDate = signal<string>('');
  selectedTheaterId = signal<string>('');

  constructor() {
    // Automatically reload showtimes if selected theater or selected date changes
    effect(() => {
      if (this.movieId() && this.selectedTheaterId() && this.selectedDate()) {
        this.loadShowings();
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.movieId.set(id);
        this.generateDateOptions();
        this.loadMovieDetail(id);
      }
    });
  }

  generateDateOptions(): void {
    const options: DateOption[] = [];
    const daysOfWeek = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    
    for (let i = 0; i < 7; i++) {
      const d = new Date();
      d.setDate(d.getDate() + i);

      // YYYY-MM-DD format
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      const dateStr = `${year}-${month}-${day}`;

      let label = '';
      if (i === 0) {
        label = 'Hôm nay';
      } else if (i === 1) {
        label = 'Ngày mai';
      } else {
        label = daysOfWeek[d.getDay()];
      }

      options.push({
        label,
        dateString: dateStr,
        displayDate: `${day}/${month}`
      });
    }

    this.dateOptions.set(options);
    if (options.length > 0) {
      this.selectedDate.set(options[0].dateString);
    }
  }

  loadMovieDetail(id: string): void {
    this.loadingMovie.set(true);
    this.movieUserService.getMovieDetail(id).subscribe({
      next: (res) => {
        this.movie.set(res);
        this.loadingMovie.set(false);

        if (res.theaters) {
          this.theaters.set(res.theaters.map(t => ({
            theaterId: t.theaterId,
            theaterName: t.theaterName,
            showingCount: t.showingCount
          })));
        }
        
        // Auto-select the first theater if any are available
        if (res.theaters && res.theaters.length > 0) {
          this.selectedTheaterId.set(res.theaters[0].theaterId);
        } else {
          this.selectedTheaterId.set('');
        }
      },
      error: (err) => {
        console.error('Error fetching movie detail:', err);
        this.loadingMovie.set(false);
      }
    });
  }

  loadShowings(): void {
    this.loadingShowings.set(true);
    this.movieUserService.getShowings(
      this.movieId(),
      this.selectedTheaterId(),
      this.selectedDate()
    ).subscribe({
      next: (res) => {
        // Sort showings chronologically
        this.showings.set(res.sort((a, b) => new Date(a.timeStart).getTime() - new Date(b.timeStart).getTime()));
        this.loadingShowings.set(false);
      },
      error: (err) => {
        console.error('Error fetching showings:', err);
        this.loadingShowings.set(false);
      }
    });
  }

  selectDate(dateStr: string): void {
    this.selectedDate.set(dateStr);
    this.loadTheatersForDate(dateStr);
  }

  loadTheatersForDate(dateStr: string): void {
    const movieId = this.movieId();
    if (!movieId) return;

    this.loadingTheaters.set(true);
    this.movieUserService.getTheatersForMovieAndDate(movieId, dateStr).subscribe({
      next: (res) => {
        const mappedTheaters = res.map(t => ({
          theaterId: t.id,
          theaterName: t.name,
          location: t.location,
          showingCount: t.showingCount
        }));
        
        this.theaters.set(mappedTheaters);
        this.loadingTheaters.set(false);

        if (mappedTheaters.length > 0) {
          const exists = mappedTheaters.some(t => t.theaterId === this.selectedTheaterId());
          if (!exists) {
            this.selectedTheaterId.set(mappedTheaters[0].theaterId);
          }
        } else {
          this.selectedTheaterId.set('');
          this.showings.set([]);
        }
      },
      error: (err) => {
        console.error('Error fetching theaters for date:', err);
        this.loadingTheaters.set(false);
        this.theaters.set([]);
        this.selectedTheaterId.set('');
        this.showings.set([]);
      }
    });
  }

  selectTheater(theaterId: string): void {
    this.selectedTheaterId.set(theaterId);
  }

  onBookTicket(showingId: string): void {
    // If not authenticated, alert the user and route to login
    if (!this.authService.isAuthenticated()) {
      alert('Vui lòng đăng nhập trước khi tiến hành đặt vé.');
      return;
    }
    
    // Placeholder alert showing ticket booking flow initiation
    alert(`Khởi chạy tiến trình đặt vé cho suất chiếu: ${showingId}. Giao diện chọn ghế đang được xây dựng.`);
  }

  formatTime(dateTimeStr: string): string {
    try {
      const date = new Date(dateTimeStr);
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      return `${hours}:${minutes}`;
    } catch {
      return '';
    }
  }
}
