import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MovieUserService, BookingDetailResponse } from '../../../core/services/user/movie-user.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './payment-success.component.html'
})
export class PaymentSuccessComponent implements OnInit {
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
      this.router.navigate(['/']);
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
        this.error.set('Không thể tải thông tin chi tiết vé xem phim. Vui lòng truy cập lịch sử đặt vé để kiểm tra.');
        this.loading.set(false);
      }
    });
  }

  getSeatNames(details: any[]): string {
    if (!details || details.length === 0) return 'N/A';
    return details.map(d => `${d.seatRow}${d.seatNumber}`).join(', ');
  }
}
