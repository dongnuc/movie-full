import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShowingService, Showing } from '../../../../core/services/showing.service';
import { MovieService, Movie } from '../../../../core/services/movie.service';
import { RoomService, Room } from '../../../../core/services/room.service';

@Component({
  selector: 'app-theater-showtimes-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './showtimes-form.component.html'
})
export class TheaterShowtimesFormComponent implements OnInit {
  private showingService = inject(ShowingService);
  private movieService = inject(MovieService);
  private roomService = inject(RoomService);

  @Input() showingId: string | null = null;
  @Input() theaterId!: string;
  @Output() close = new EventEmitter<boolean>();

  showing: Partial<Showing> = {
    timeStart: '',
    price: 75000,
    maxTicketsPerUser: 8,
    movieId: '',
    roomId: '',
    isActive: true
  };

  movies: Movie[] = [];
  rooms: Room[] = [];
  
  loading = false;
  saving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDropdowns();
  }

  loadDropdowns(): void {
    this.loading = true;
    
    // Fetch active movies
    this.movieService.getMovies({ page: 1, pageSize: 100, isActive: true }).subscribe({
      next: (movieRes) => {
        this.movies = movieRes.items;
        
        // Fetch active rooms for this theater
        this.roomService.getRooms({ theaterId: this.theaterId, page: 1, pageSize: 100, isActive: true }).subscribe({
          next: (roomRes) => {
            this.rooms = roomRes.items;
            
            if (this.showingId) {
              this.loadShowing();
            } else {
              // Pre-set time to today + 2 hours
              const defaultDate = new Date();
              defaultDate.setHours(defaultDate.getHours() + 2);
              defaultDate.setMinutes(0);
              defaultDate.setSeconds(0);
              
              // Format to YYYY-MM-DDTHH:mm
              const offset = defaultDate.getTimezoneOffset();
              const formattedDate = new Date(defaultDate.getTime() - (offset * 60 * 1000))
                .toISOString()
                .slice(0, 16);
                
              this.showing.timeStart = formattedDate;
              this.loading = false;
            }
          },
          error: () => {
            this.errorMessage = 'Không thể tải danh sách phòng chiếu';
            this.loading = false;
          }
        });
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách phim';
        this.loading = false;
      }
    });
  }

  loadShowing(): void {
    this.showingService.getShowingById(this.showingId!).subscribe({
      next: (res) => {
        this.showing = { ...res };
        // Format ISO date string to YYYY-MM-DDTHH:mm for datetime-local input
        if (res.timeStart) {
          const dateVal = new Date(res.timeStart);
          const offset = dateVal.getTimezoneOffset();
          this.showing.timeStart = new Date(dateVal.getTime() - (offset * 60 * 1000))
            .toISOString()
            .slice(0, 16);
        }
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải chi tiết lịch chiếu';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.showing.timeStart || !this.showing.price || !this.showing.movieId || !this.showing.roomId) {
      this.errorMessage = 'Vui lòng nhập đầy đủ thông tin bắt buộc';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    // Convert local time back to UTC/ISO standard
    const payload = {
      ...this.showing,
      timeStart: new Date(this.showing.timeStart).toISOString()
    };

    const obs$ = this.showingId
      ? this.showingService.updateShowing(this.showingId, payload)
      : this.showingService.createShowing(payload);

    obs$.subscribe({
      next: () => {
        this.saving = false;
        this.close.emit(true);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Lỗi khi lưu lịch chiếu';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
