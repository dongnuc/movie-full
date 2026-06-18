import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MovieUserService, BookingDetailResponse } from '../../../core/services/user/movie-user.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './booking-detail.component.html'
})
export class BookingDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private movieUserService = inject(MovieUserService);

  bookingId = '';
  booking = signal<BookingDetailResponse | null>(null);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/profile/history']);
      return;
    }
    this.bookingId = id;
    this.fetchBookingDetail();
  }

  fetchBookingDetail(): void {
    this.loading.set(true);
    this.movieUserService.getBookingDetail(this.bookingId).subscribe({
      next: (data) => {
        this.booking.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching booking detail:', err);
        this.error.set('Không thể tải thông tin chi tiết vé xem phim. Vui lòng quay lại lịch sử đặt vé.');
        this.loading.set(false);
      }
    });
  }

  getSeatNames(details: any[]): string {
    if (!details || details.length === 0) return 'N/A';
    return details.map(d => `${d.seatRow}${d.seatNumber}`).join(', ');
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
