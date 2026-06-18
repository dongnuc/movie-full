import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MovieUserService, BookingHistoryItem } from '../../../core/services/user/movie-user.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-booking-history',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './booking-history.component.html'
})
export class BookingHistoryComponent implements OnInit {
  private movieUserService = inject(MovieUserService);

  bookings = signal<BookingHistoryItem[]>([]);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchBookingHistory();
  }

  fetchBookingHistory(): void {
    this.loading.set(true);
    this.movieUserService.getBookings().subscribe({
      next: (data) => {
        this.bookings.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching bookings history:', err);
        this.error.set('Không thể tải lịch sử đặt vé. Vui lòng thử lại sau.');
        this.loading.set(false);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed':
        return 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20';
      case 'cancelled':
        return 'bg-rose-500/10 text-rose-400 border border-rose-500/20';
      default:
        return 'bg-amber-500/10 text-amber-400 border border-amber-500/20';
    }
  }

  getStatusLabel(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed':
        return 'Đã Xác Nhận';
      case 'cancelled':
        return 'Đã Hủy';
      default:
        return 'Chờ Thanh Toán';
    }
  }
}
