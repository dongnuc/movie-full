import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MovieUserService, UserRoomLayout, UserSeatLayout } from '../../../core/services/user/movie-user.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-user-booking',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './booking.component.html'
})
export class UserBookingComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private movieUserService = inject(MovieUserService);

  movieId = signal<string>('');
  showingId = signal<string>('');
  roomId = signal<string>('');
  basePrice = signal<number>(0);
  movieTitle = signal<string>('');
  roomLayout = signal<UserRoomLayout | null>(null);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  // Local selection state
  selectedSeats = signal<UserSeatLayout[]>([]);

  // Computed properties
  selectedSeatsCount = computed(() => {
    return this.selectedSeats().reduce((acc, seat) => acc + (seat.seatIds.seatIds.length || 1), 0);
  });

  totalPrice = computed(() => {
    const base = this.basePrice();
    return this.selectedSeats().reduce((acc, seat) => {
      const seatCount = seat.seatIds.seatIds.length || 1;
      return acc + (base * seatCount) + seat.surcharge;
    }, 0);
  });

  ngOnInit(): void {
    const state = history.state;
    const movieId = state?.movieId;
    const showingId = state?.showingId;
    const roomId = state?.roomId;
    const price = state?.price;
    const title = state?.movieTitle;

    if (!showingId || !roomId) {
      this.error.set('Thông tin đặt vé không hợp lệ. Vui lòng quay lại trang chi tiết phim.');
      this.loading.set(false);
      return;
    }

    this.movieId.set(movieId || '');
    this.showingId.set(showingId);
    this.roomId.set(roomId);
    this.basePrice.set(Number(price) || 0);
    this.movieTitle.set(title || 'Phim');

    this.loadRoomLayout();
  }

  loadRoomLayout(): void {
    this.loading.set(true);
    this.error.set(null);
    this.movieUserService.getShowingSeatsLayout(this.showingId(), this.roomId()).subscribe({
      next: (layout) => {
        // Initialize local isSelected flag on each seat
        if (layout && layout.seatRows) {
          layout.seatRows.forEach(row => {
            if (row.seats) {
              row.seats.forEach(seat => {
                seat.isSelected = false;
              });
            }
          });
        }
        this.roomLayout.set(layout);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching showing seats layout:', err);
        this.error.set('Không thể tải sơ đồ phòng chiếu. Vui lòng thử lại sau.');
        this.loading.set(false);
      }
    });
  }

  toggleSeat(seat: UserSeatLayout): void {
    // Only allow toggling if the seat is Available
    if (seat.status !== 'Available') {
      return;
    }

    seat.isSelected = !seat.isSelected;

    const currentSelected = this.selectedSeats();
    if (seat.isSelected) {
      this.selectedSeats.set([...currentSelected, seat]);
    } else {
      this.selectedSeats.set(currentSelected.filter(s => s !== seat));
    }
  }

  clearSelection(): void {
    const layout = this.roomLayout();
    if (layout && layout.seatRows) {
      layout.seatRows.forEach(row => {
        if (row.seats) {
          row.seats.forEach(seat => {
            seat.isSelected = false;
          });
        }
      });
    }
    this.selectedSeats.set([]);
  }

  proceedToPayment(): void {
    if (this.selectedSeats().length === 0) {
      alert('Vui lòng chọn ít nhất 1 ghế trước khi thanh toán.');
      return;
    }

    // Temporary alert before checkout logic implementation
    const seatNames = this.selectedSeats().map(s => s.seatNumber).join(', ');
    alert(`Xác nhận đặt ghế: ${seatNames}\nTổng tiền: ${this.totalPrice().toLocaleString('vi-VN')} VND\n(Đây là chức năng mô phỏng, chưa thực hiện giao dịch thực tế)`);
  }
}
