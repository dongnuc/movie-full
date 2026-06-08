import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RoomService, Room, RoomLayoutResponse } from '../../../../core/services/admin-theater/room.service';
import { SeatService } from '../../../../core/services/admin-theater/seat.service';
import { SeatTypeService } from '../../../../core/services/admin-system/seattype.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-theater-rooms-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rooms-form.component.html'
})
export class TheaterRoomsFormComponent implements OnInit {
  private roomService = inject(RoomService);
  private seatService = inject(SeatService);
  private seatTypeService = inject(SeatTypeService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  @Input() roomId: string | null = null;
  @Output() close = new EventEmitter<boolean>();

  room: Partial<Room> = {
    name: '',
    totalRow: 8,
    totalCol: 10,
    isActive: true
  };

  loading = false;
  saving = false;
  errorMessage = '';

  isRouted = false;
  hasSeats = false;
  showLayout = false;
  layoutData: RoomLayoutResponse | null = null;
  checkingSeats = false;
  generatingSeats = false;
  loadingLayout = false;

  // Seat type update state
  activeSeatTypes: any[] = [];
  selectedSeatTypeId = '';
  selectedSeatIds = new Set<string>();
  updatingSeatType = false;
  seatUpdateSuccess = '';

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.roomId = idParam;
      this.isRouted = true;
    }

    if (this.roomId) {
      this.loadRoom();
      this.checkSeats();
      if (this.isRouted) {
        this.loadActiveSeatTypes();
      }
    }
  }

  loadRoom(): void {
    this.loading = true;
    this.roomService.getRoomById(this.roomId!).subscribe({
      next: (res) => {
        this.room = { ...res };
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải thông tin phòng chiếu';
        this.loading = false;
      }
    });
  }

  checkSeats(): void {
    if (!this.roomId) return;
    this.checkingSeats = true;
    this.roomService.checkSeatsExist(this.roomId).subscribe({
      next: (exists) => {
        this.hasSeats = exists;
        this.checkingSeats = false;
      },
      error: (err) => {
        console.error('Lỗi khi kiểm tra ghế tồn tại:', err);
        this.checkingSeats = false;
      }
    });
  }

  loadLayout(): void {
    if (!this.roomId) return;
    this.loadingLayout = true;
    this.roomService.getRoomLayout(this.roomId).subscribe({
      next: (res) => {
        this.layoutData = res;
        this.showLayout = true;
        this.loadingLayout = false;
      },
      error: (err) => {
        console.error('Lỗi khi tải sơ đồ ghế:', err);
        this.errorMessage = 'Không thể tải sơ đồ chỗ ngồi của phòng chiếu';
        this.loadingLayout = false;
      }
    });
  }

  generateSeats(): void {
    if (!this.roomId) return;
    this.generatingSeats = true;
    this.errorMessage = '';
    this.seatService.bulkCreateSeats(this.roomId).subscribe({
      next: () => {
        this.generatingSeats = false;
        Swal.fire({
          icon: 'success',
          title: 'Khởi tạo sơ đồ ghế hàng loạt thành công!',
          customClass: {
            popup: 'netflix-swal-popup',
            confirmButton: 'netflix-swal-button'
          }
        });
        this.checkSeats();
      },
      error: (err) => {
        console.error('Lỗi khi tạo ghế hàng loạt:', err);
        this.errorMessage = err.message || 'Lỗi khi khởi tạo sơ đồ ghế.';
        this.generatingSeats = false;
      }
    });
  }

  loadActiveSeatTypes(): void {
    this.seatTypeService.getActiveSeatTypes().subscribe({
      next: (types) => {
        this.activeSeatTypes = types;
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách loại ghế:', err);
      }
    });
  }

  isCoupleSeatType(seatTypeId: string): boolean {
    const st = this.activeSeatTypes.find(t => t.id === seatTypeId);
    if (!st) return false;
    const name = (st.name || '').toLowerCase();
    return name.includes('couple') || name.includes('đôi');
  }

  getSeatTypeColorClasses(st: any, isSelected: boolean): string {
    const name = (st.name || '').toLowerCase();
    const base = 'px-3 py-2 rounded text-xs font-bold border-2 transition-all duration-150 flex items-center gap-2 cursor-pointer ';
    if (isSelected) {
      if (name.includes('couple') || name.includes('đôi')) {
        return base + 'bg-red-600 border-red-400 text-white ring-2 ring-red-400/50';
      }
      if (name.includes('vip')) {
        return base + 'bg-blue-600 border-blue-400 text-white ring-2 ring-blue-400/50';
      }
      return base + 'bg-[#4a4a4a] border-[#888] text-white ring-2 ring-white/30';
    }
    if (name.includes('couple') || name.includes('đôi')) {
      return base + 'bg-red-900/30 border-red-500/30 text-red-300 hover:bg-red-900/50';
    }
    if (name.includes('vip')) {
      return base + 'bg-blue-900/30 border-blue-500/30 text-blue-300 hover:bg-blue-900/50';
    }
    return base + 'bg-[#2a2a2a] border-[#3a3a3a] text-netflix-gray-light hover:bg-[#333]';
  }

  getSeatTypeDotClass(st: any): string {
    const name = (st.name || '').toLowerCase();
    if (name.includes('couple') || name.includes('đôi')) return 'w-3 h-3 rounded-full bg-red-500';
    if (name.includes('vip')) return 'w-3 h-3 rounded-full bg-blue-500';
    return 'w-3 h-3 rounded-full bg-[#888]';
  }

  getSeatIdsList(seat: any): string[] {
    return seat.seatIds?.seatIds || [];
  }

  isCoupleSeat(seat: any): boolean {
    const typeName = (seat.seatTypeName || '').toLowerCase();
    return (typeName.includes('couple') || typeName.includes('đôi')) && (this.getSeatIdsList(seat).length > 1);
  }

  toggleSeatSelection(seat: any): void {
    if (!seat.isActive || !this.showLayout) return;
    const ids = this.getSeatIdsList(seat);
    if (ids.length === 0) return;

    const allSelected = ids.every(id => this.selectedSeatIds.has(id));
    if (allSelected) {
      ids.forEach(id => this.selectedSeatIds.delete(id));
    } else {
      ids.forEach(id => this.selectedSeatIds.add(id));
    }
  }

  isSeatSelected(seatId: string): boolean {
    return this.selectedSeatIds.has(seatId);
  }

  clearSelection(): void {
    this.selectedSeatIds.clear();
    this.selectedSeatTypeId = '';
    this.seatUpdateSuccess = '';
  }

  submitSeatTypeUpdate(): void {
    this.errorMessage = '';
    this.seatUpdateSuccess = '';

    if (!this.selectedSeatTypeId) {
      this.errorMessage = 'Vui lòng chọn loại ghế muốn cập nhật.';
      return;
    }
    if (this.selectedSeatIds.size === 0) {
      this.errorMessage = 'Vui lòng chọn ít nhất một ghế trên sơ đồ.';
      return;
    }

    const seatIds = [...this.selectedSeatIds];
    const isCouple = this.isCoupleSeatType(this.selectedSeatTypeId);

    if (isCouple && seatIds.length % 2 !== 0) {
      this.errorMessage = 'Ghế Couple yêu cầu chọn số lượng ghế chẵn (2, 4, 6...).';
      return;
    }

    this.updatingSeatType = true;

    const obs$ = isCouple
      ? this.seatService.updateSeatsToCouple(seatIds)
      : this.seatService.bulkUpdateSeatType(seatIds, this.selectedSeatTypeId);

    obs$.subscribe({
      next: () => {
        this.updatingSeatType = false;
        this.seatUpdateSuccess = `Cập nhật ${seatIds.length} ghế thành công!`;
        this.selectedSeatIds.clear();
        this.loadLayout();
      },
      error: (err) => {
        console.error('Lỗi khi cập nhật loại ghế:', err);
        this.errorMessage = err.message || 'Lỗi khi cập nhật loại ghế.';
        this.updatingSeatType = false;
      }
    });
  }

  getSeatClasses(seat: any): string {
    const ids = this.getSeatIdsList(seat);
    const selected = ids.length > 0 && ids.every(id => this.selectedSeatIds.has(id));
    const typeName = (seat.seatTypeName || '').toLowerCase();
    const isCouple = this.isCoupleSeat(seat);
    const singleBase = 'w-10 h-10 rounded flex flex-col items-center justify-center text-white font-bold transition-all duration-150 ';
    const coupleBase = 'w-[86px] h-10 rounded flex flex-col items-center justify-center text-white font-bold transition-all duration-150 ';

    if (!seat.isActive) {
      const w = isCouple ? coupleBase : singleBase;
      return w + 'border border-dashed border-red-500/40 bg-transparent text-red-500/40 cursor-not-allowed';
    }
    if (selected) {
      const w = isCouple ? coupleBase : singleBase;
      return w + 'bg-yellow-500 border-2 border-yellow-300 ring-2 ring-yellow-400/50 cursor-pointer scale-105';
    }
    if (isCouple) {
      return coupleBase + 'bg-red-600 border border-red-400 hover:bg-red-500 cursor-pointer';
    }
    if (typeName.includes('vip')) {
      return singleBase + 'bg-blue-600 border border-blue-400 hover:bg-blue-500 cursor-pointer';
    }
    return singleBase + 'bg-[#3a3a3a] border border-[#555] hover:bg-[#4a4a4a] cursor-pointer';
  }

  getSeatTypeLabel(seat: any): string {
    const typeName = (seat.seatTypeName || '').toLowerCase();
    if (typeName.includes('couple') || typeName.includes('đôi')) return 'Đôi';
    if (typeName.includes('vip')) return 'VIP';
    return 'Thường';
  }

  onSubmit(): void {
    if (!this.room.name?.trim() || !this.room.totalRow || !this.room.totalCol) {
      this.errorMessage = 'Vui lòng nhập đầy đủ các trường bắt buộc';
      return;
    }

    if (this.room.totalRow > 15 || this.room.totalCol > 20) {
      this.errorMessage = 'Kích thước hàng tối đa là 15, cột tối đa là 20';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const obs$ = this.roomId
      ? this.roomService.updateRoom(this.roomId, this.room)
      : this.roomService.createRoom(this.room);

    obs$.subscribe({
      next: () => {
        this.saving = false;
        if (this.roomId) {
          Swal.fire({
            icon: 'success',
            title: 'Cập nhật thông tin phòng chiếu thành công!',
            customClass: {
              popup: 'netflix-swal-popup',
              confirmButton: 'netflix-swal-button'
            }
          });
          this.loadRoom();
          this.checkSeats();
          if (this.showLayout) {
            this.loadLayout();
          }
        } else {
          Swal.fire({
            icon: 'success',
            title: 'Tạo phòng chiếu mới thành công!',
            customClass: {
              popup: 'netflix-swal-popup',
              confirmButton: 'netflix-swal-button'
            }
          });
          if (this.isRouted) {
            this.router.navigate(['/admin-theater/rooms']);
          } else {
            this.close.emit(true);
          }
        }
      },
      error: (err) => {
        this.errorMessage = err.message || 'Lỗi khi lưu phòng chiếu';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    if (this.isRouted) {
      this.router.navigate(['/admin-theater/rooms']);
    } else {
      this.close.emit(false);
    }
  }
}
