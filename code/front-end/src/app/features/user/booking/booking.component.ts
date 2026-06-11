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
        const initialSelected: UserSeatLayout[] = [];
        if (layout && layout.seatRows) {
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
    if (payload.showingId !== this.showingId()) return;

    const currentUserId = this.authService.currentUser()?.id;
    const isMine = payload.lockedBy === currentUserId;

    const layout = this.roomLayout();
    if (!layout || !layout.seatRows) return;

    let updatedSelected = [...this.selectedSeats()];
    let layoutChanged = false;

    layout.seatRows.forEach(row => {
      if (row.seats) {
        row.seats.forEach(seat => {
          const hasMatchingSeat = seat.seatIds.seatIds.some(id => payload.seatIds.includes(id));
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
    if (payload.showingId !== this.showingId()) return;

    const layout = this.roomLayout();
    if (!layout || !layout.seatRows) return;

    let updatedSelected = [...this.selectedSeats()];
    let layoutChanged = false;

    layout.seatRows.forEach(row => {
      if (row.seats) {
        row.seats.forEach(seat => {
          const hasMatchingSeat = seat.seatIds.seatIds.some(id => payload.seatIds.includes(id));
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
    }
  }

  private handleSessionTimeoutWarning(payload: { showingId: string; userId: string; remainingSeconds: number; message: string }): void {
    if (payload.showingId !== this.showingId()) return;
    const currentUserId = this.authService.currentUser()?.id;
    if (payload.userId !== currentUserId) return;

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
        next: (success) => {
          if (!success) {
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

    // Temporary alert before checkout logic implementation
    const seatNames = this.selectedSeats().map(s => s.seatNumber).join(', ');
    alert(`Xác nhận đặt ghế: ${seatNames}\nTổng tiền: ${this.totalPrice().toLocaleString('vi-VN')} VND\n(Đây là chức năng mô phỏng, chưa thực hiện giao dịch thực tế)`);
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR connection stopped.'))
        .catch(err => console.error('Error stopping SignalR connection:', err));
    }
  }
}
