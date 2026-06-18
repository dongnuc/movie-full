import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MovieUserService, UserRoomLayout, UserSeatLayout } from '../../../core/services/user/movie-user.service';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { AuthService } from '../../../core/auth/auth.service';
import { SIGNALR_HUB_URL } from '../../../core/constants/api.constants';
import * as signalR from '@microsoft/signalr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user-booking',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './booking.component.html'
})
export class UserBookingComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private movieUserService = inject(MovieUserService);
  private authService = inject(AuthService);

  private hubConnection: signalR.HubConnection | null = null;

  movieId = signal<string>('');
  showingId = signal<string>('');
  roomId = signal<string>('');
  basePrice = signal<number>(0);
  movieTitle = signal<string>('');
  theaterName = signal<string>('Rạp phim');
  theaterAddress = signal<string>('Địa chỉ rạp');
  roomLayout = signal<UserRoomLayout | null>(null);
  loading = signal<boolean>(true);
  error = signal<string | null>(null);

  // Countdown timer signals
  remainingTime = signal<number | null>(null);
  formattedRemainingTime = computed(() => {
    const time = this.remainingTime();
    if (time === null || time <= 0) return '';
    const minutes = Math.floor(time / 60);
    const seconds = time % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  });
  private countdownInterval: any = null;

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
    const showingId = this.route.snapshot.paramMap.get('showingId');
    if (!showingId) {
      this.error.set('Thông tin đặt vé không hợp lệ. Vui lòng quay lại trang chi tiết phim.');
      this.loading.set(false);
      return;
    }

    this.showingId.set(showingId);

    // Load any initial values from history.state if available to populate UI immediately before layout completes
    const state = history.state;
    if (state) {
      this.movieId.set(state.movieId || '');
      this.roomId.set(state.roomId || '');
      this.basePrice.set(Number(state.price) || 0);
      this.movieTitle.set(state.movieTitle || 'Phim');
      this.theaterName.set(state.theaterName || 'Rạp phim');
      this.theaterAddress.set(state.theaterAddress || 'Địa chỉ rạp');
    }

    this.loadRoomLayout();
    this.fetchAndStartSessionTtl();
  }

  loadRoomLayout(): void {
    this.loading.set(true);
    this.error.set(null);
    this.movieUserService.getShowingSeatsLayout(this.showingId(), this.roomId() || '').subscribe({
      next: (layout) => {
        const initialSelected: UserSeatLayout[] = [];
        if (layout) {
          this.movieId.set(layout.movieId || '');
          this.roomId.set(layout.roomId || '');
          this.basePrice.set(layout.basePrice || 0);
          this.movieTitle.set(layout.movieTitle || 'Phim');

          if (layout.seatRows) {
            layout.seatRows.forEach(row => {
              if (row.seats) {
                row.seats.forEach(seat => {
                  seat.isSelected = false;
                  if (seat.status === 'Holding' && seat.isMine) {
                    initialSelected.push(seat);
                  }
                });
              }
            });
          }
        }
        this.roomLayout.set(layout);
        this.selectedSeats.set(initialSelected);
        this.loading.set(false);
        this.initSignalR();
      },
      error: (err) => {
        console.error('Error fetching showing seats layout:', err);
        this.error.set('Không thể tải sơ đồ phòng chiếu. Vui lòng thử lại sau.');
        this.loading.set(false);
      }
    });
  }

  fetchAndStartSessionTtl(): void {
    this.movieUserService.getSessionTtl(this.showingId()).subscribe({
      next: (res) => {
        if (res && res.isActive && res.remainingSeconds > 0) {
          this.startCountdown(res.remainingSeconds);
        } else {
          this.clearCountdown();
        }
      },
      error: (err) => {
        // If unauthorized or error, simply clear the timer and let it initiate on seat selection
        this.clearCountdown();
      }
    });
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
      title: 'Hết Thời Gian Giữ Ghế',
      text: 'Thời gian giữ ghế đã hết hạn. Vui lòng chọn lại ghế.',
      icon: 'error',
      background: '#181818',
      color: '#ffffff',
      confirmButtonText: 'Đồng Ý',
      buttonsStyling: false,
      customClass: {
        popup: 'netflix-swal-popup',
        confirmButton: 'netflix-swal-button'
      }
    });
    this.loadRoomLayout();
  }

  initSignalR(): void {
    const showingId = this.showingId();
    if (!showingId) return;

    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${SIGNALR_HUB_URL}?showingId=${showingId}`, {
        accessTokenFactory: () => this.authService.accessToken() || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('SeatsLocked', (payload: { showingId: string; seatIds: string[]; lockedBy: string }) => {
      this.handleSeatsLocked(payload);
    });

    this.hubConnection.on('SeatsUnlocked', (payload: { showingId: string; seatIds: string[] }) => {
      this.handleSeatsUnlocked(payload);
    });

    this.hubConnection.on('SeatsBooked', (payload: { showingId: string; seatIds: string[] }) => {
      this.handleSeatsBooked(payload);
    });

    this.hubConnection.on('SessionTimeoutWarning', (payload: { showingId: string; userId: string; remainingSeconds: number; message: string }) => {
      this.handleSessionTimeoutWarning(payload);
    });

    this.hubConnection.start()
      .then(() => {
        console.log('SignalR connection established.');
      })
      .catch(err => {
        console.error('Error starting SignalR connection:', err);
      });
  }

  private handleSeatsLocked(payload: { showingId: string; seatIds: string[]; lockedBy: string }): void {
    if (payload.showingId?.toLowerCase() !== this.showingId()?.toLowerCase()) return;

    const currentUserId = this.authService.currentUser()?.id;
    const isMine = payload.lockedBy?.toLowerCase() === currentUserId?.toLowerCase();

    const layout = this.roomLayout();
    if (!layout || !layout.seatRows) return;

    let updatedSelected = [...this.selectedSeats()];
    let layoutChanged = false;

    layout.seatRows.forEach(row => {
      if (row.seats) {
        row.seats.forEach(seat => {
          const hasMatchingSeat = seat.seatIds.seatIds.some(id => 
            payload.seatIds.some(pid => pid.toLowerCase() === id.toLowerCase())
          );
          if (hasMatchingSeat) {
            seat.status = 'Holding';
            seat.isMine = isMine;
            layoutChanged = true;

            if (isMine) {
              const alreadySelected = updatedSelected.some(s => 
                s.seatIds.seatIds.some(id => seat.seatIds.seatIds.includes(id))
              );
              if (!alreadySelected) {
                updatedSelected.push(seat);
              }
            } else {
              updatedSelected = updatedSelected.filter(s => 
                !s.seatIds.seatIds.some(id => seat.seatIds.seatIds.includes(id))
              );
            }
          }
        });
      }
    });

    if (layoutChanged) {
      this.roomLayout.set({ ...layout });
      this.selectedSeats.set(updatedSelected);
    }
  }

  private handleSeatsUnlocked(payload: { showingId: string; seatIds: string[] }): void {
    if (payload.showingId?.toLowerCase() !== this.showingId()?.toLowerCase()) return;

    const layout = this.roomLayout();
    if (!layout || !layout.seatRows) return;

    let updatedSelected = [...this.selectedSeats()];
    let layoutChanged = false;

    layout.seatRows.forEach(row => {
      if (row.seats) {
        row.seats.forEach(seat => {
          const hasMatchingSeat = seat.seatIds.seatIds.some(id => 
            payload.seatIds.some(pid => pid.toLowerCase() === id.toLowerCase())
          );
          if (hasMatchingSeat) {
            seat.status = 'Available';
            seat.isMine = false;
            layoutChanged = true;

            updatedSelected = updatedSelected.filter(s => 
              !s.seatIds.seatIds.some(id => seat.seatIds.seatIds.includes(id))
            );
          }
        });
      }
    });

    if (layoutChanged) {
      this.roomLayout.set({ ...layout });
      this.selectedSeats.set(updatedSelected);
      if (updatedSelected.length === 0) {
        this.clearCountdown();
      }
    }
  }

  private handleSeatsBooked(payload: { showingId: string; seatIds: string[] }): void {
    if (payload.showingId?.toLowerCase() !== this.showingId()?.toLowerCase()) return;

    const layout = this.roomLayout();
    if (!layout || !layout.seatRows) return;

    let updatedSelected = [...this.selectedSeats()];
    let layoutChanged = false;

    layout.seatRows.forEach(row => {
      if (row.seats) {
        row.seats.forEach(seat => {
          const hasMatchingSeat = seat.seatIds.seatIds.some(id => 
            payload.seatIds.some(pid => pid.toLowerCase() === id.toLowerCase())
          );
          if (hasMatchingSeat) {
            seat.status = 'Booked';
            seat.isMine = false;
            layoutChanged = true;

            updatedSelected = updatedSelected.filter(s => 
              !s.seatIds.seatIds.some(id => seat.seatIds.seatIds.includes(id))
            );
          }
        });
      }
    });

    if (layoutChanged) {
      this.roomLayout.set({ ...layout });
      this.selectedSeats.set(updatedSelected);
      if (updatedSelected.length === 0) {
        this.clearCountdown();
      }
    }
  }

  private handleSessionTimeoutWarning(payload: { showingId: string; userId: string; remainingSeconds: number; message: string }): void {
    if (payload.showingId?.toLowerCase() !== this.showingId()?.toLowerCase()) return;
    const currentUserId = this.authService.currentUser()?.id;
    if (payload.userId?.toLowerCase() !== currentUserId?.toLowerCase()) return;

    // Sync remaining time with backend TTL warning event
    this.startCountdown(payload.remainingSeconds);

    Swal.fire({
      title: 'Cảnh Báo Hết Hạn Giữ Ghế',
      text: payload.message,
      icon: 'warning',
      background: '#181818',
      color: '#ffffff',
      confirmButtonText: 'Đồng Ý',
      buttonsStyling: false,
      customClass: {
        popup: 'netflix-swal-popup',
        confirmButton: 'netflix-swal-button'
      }
    });
  }

  toggleSeat(seat: UserSeatLayout): void {
    const currentUserId = this.authService.currentUser()?.id;
    if (!currentUserId) {
      alert('Vui lòng đăng nhập để thực hiện đặt ghế.');
      this.router.navigate(['/auth/login']);
      return;
    }

    if (seat.status === 'Available') {
      this.movieUserService.holdSeats(this.showingId(), seat.seatIds.seatIds).subscribe({
        next: (res) => {
          if (res) {
            if (res.remainingSeconds && res.remainingSeconds > 0) {
              this.startCountdown(res.remainingSeconds);
            }
          } else {
            alert('Không thể giữ ghế. Ghế có thể đã được người khác chọn.');
          }
        },
        error: (err) => {
          console.error('Error holding seats:', err);
          alert('Không thể giữ ghế. Vui lòng thử lại sau.');
        }
      });
    } else if (seat.status === 'Holding' && seat.isMine) {
      this.movieUserService.unholdSeats(this.showingId(), seat.seatIds.seatIds).subscribe({
        next: (success) => {
          if (!success) {
            alert('Không thể hủy giữ ghế. Vui lòng thử lại.');
          }
        },
        error: (err) => {
          console.error('Error unholding seats:', err);
          alert('Không thể hủy giữ ghế. Vui lòng thử lại sau.');
        }
      });
    }
  }

  clearSelection(): void {
    const myHeldSeats = this.selectedSeats();
    if (myHeldSeats.length === 0) return;

    const allSeatIds = myHeldSeats.flatMap(seat => seat.seatIds.seatIds);

    this.movieUserService.unholdSeats(this.showingId(), allSeatIds).subscribe({
      next: (success) => {
        if (!success) {
          alert('Không thể hủy giữ ghế. Vui lòng thử lại.');
        }
      },
      error: (err) => {
        console.error('Error clearing selection:', err);
        alert('Không thể hủy giữ ghế. Vui lòng thử lại sau.');
      }
    });
  }

  proceedToPayment(): void {
    if (this.selectedSeats().length === 0) {
      alert('Vui lòng chọn ít nhất 1 ghế trước khi thanh toán.');
      return;
    }

    this.movieUserService.markSessionAsPaying(this.showingId()).subscribe({
      next: (res) => {
        if (res && res.remainingSeconds !== null) {
          const seatIds = this.selectedSeats().flatMap(s => s.seatIds.seatIds);
          const remainingSeconds = res.remainingSeconds;

          // Stop our local countdown before leaving
          this.clearCountdown();

          this.router.navigate(['/paying'], {
            state: {
              showingId: this.showingId(),
              movieId: this.movieId(),
              movieTitle: this.movieTitle(),
              theaterName: this.theaterName(),
              theaterAddress: this.theaterAddress(),
              roomName: this.roomLayout()?.name || 'Phòng chiếu',
              selectedSeats: this.selectedSeats(),
              totalPrice: this.totalPrice(),
              selectedSeatsCount: this.selectedSeatsCount(),
              seatIds: seatIds,
              remainingSeconds: remainingSeconds
            }
          });
        } else {
          alert('Không thể bắt đầu thanh toán. Vui lòng thử lại.');
        }
      },
      error: (err) => {
        console.error('Error transitioning session to paying:', err);
        alert('Không thể bắt đầu thanh toán. Vui lòng thử lại sau.');
      }
    });
  }

  ngOnDestroy(): void {
    this.clearCountdown();
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR connection stopped.'))
        .catch(err => console.error('Error stopping SignalR connection:', err));
    }
  }
}
