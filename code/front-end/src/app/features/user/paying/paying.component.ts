import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MovieUserService } from '../../../core/services/user/movie-user.service';
import { AccountUserService } from '../../../core/services/user/account-user.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user-paying',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './paying.component.html'
})
export class UserPayingComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private movieUserService = inject(MovieUserService);
  private accountUserService = inject(AccountUserService);

  showingId = '';
  movieTitle = '';
  theaterName = '';
  theaterAddress = '';
  roomName = '';
  selectedSeatsCount = 0;
  totalPrice = 0;
  seatIds: string[] = [];
  selectedSeatNames = '';

  remainingTime = signal<number | null>(null);
  formattedRemainingTime = computed(() => {
    const time = this.remainingTime();
    if (time === null || time <= 0) return '';
    const minutes = Math.floor(time / 60);
    const seconds = time % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  });
  private countdownInterval: any = null;
  submitting = signal<boolean>(false);

  ngOnInit(): void {
    const state = history.state;
    if (!state || !state.showingId || !state.seatIds || state.seatIds.length === 0) {
      // Prevent direct URL entry, redirect back to home
      this.router.navigate(['/']);
      return;
    }

    this.showingId = state.showingId;
    this.movieTitle = state.movieTitle;
    this.theaterName = state.theaterName;
    this.theaterAddress = state.theaterAddress;
    this.roomName = state.roomName;
    this.selectedSeatsCount = state.selectedSeatsCount;
    this.totalPrice = state.totalPrice;
    this.seatIds = state.seatIds;
    this.selectedSeatNames = state.selectedSeats.map((s: any) => s.seatNumber).join(', ');

    // Start timer with remaining seconds passed from booking component
    this.startCountdown(state.remainingSeconds || 120);
  }

  startCountdown(seconds: number): void {
    this.clearCountdown();
    this.remainingTime.set(seconds);

    this.countdownInterval = setInterval(() => {
      const current = this.remainingTime();
      if (current === null || current <= 1) {
        this.clearCountdown();
        this.handleSessionExpired();
        return;
      }
      this.remainingTime.set(current - 1);
    }, 1000);
  }

  clearCountdown(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
      this.countdownInterval = null;
    }
    this.remainingTime.set(null);
  }

  private handleSessionExpired(): void {
    Swal.fire({
      title: 'Hết Thời Gian Thanh Toán',
      text: 'Thời gian giữ ghế của bạn đã hết hạn. Vui lòng tiến hành chọn lại ghế.',
      icon: 'error',
      background: '#181818',
      color: '#ffffff',
      confirmButtonText: 'Đồng Ý',
      buttonsStyling: false,
      customClass: {
        popup: 'netflix-swal-popup',
        confirmButton: 'netflix-swal-button'
      }
    }).then(() => {
      this.router.navigate(['/booking', this.showingId]);
    });
  }

  confirmPayment(): void {
    this.submitting.set(true);
    this.movieUserService.createBooking(this.showingId).subscribe({
      next: (bookingId) => {
        this.clearCountdown();
        this.submitting.set(false);
        // Refresh the user points in the shared service so the navbar updates immediately
        this.accountUserService.getUserPoints().subscribe({
          error: (err) => console.warn('Failed to refresh points after booking:', err)
        });
        this.router.navigate(['/payment-success', bookingId]);
      },
      error: (err) => {
        this.submitting.set(false);
        console.error('Error confirming booking:', err);

        // If it is 401 Unauthorized, the auth interceptor and error interceptor
        // have already handled session cleanup, redirect to login, and shown the expiration popup.
        if (err.status === 401) {
          return;
        }

        Swal.fire({
          title: 'Thanh Toán Thất Bại',
          text: err.message || 'Không thể xử lý giao dịch. Ghế giữ của bạn có thể đã hết hạn hoặc xảy ra lỗi hệ thống.',
          icon: 'error',
          background: '#181818',
          color: '#ffffff',
          confirmButtonText: 'Quay Lại Chọn Ghế',
          buttonsStyling: false,
          customClass: {
            popup: 'netflix-swal-popup',
            confirmButton: 'netflix-swal-button'
          }
        }).then(() => {
          this.router.navigate(['/booking', this.showingId]);
        });
      }
    });
  }

  ngOnDestroy(): void {
    this.clearCountdown();
  }
}
