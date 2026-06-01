import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomService, Room } from '../../../../core/services/room.service';
import { TheaterService, Theater } from '../../../../core/services/theater.service';
import { SeatService } from '../../../../core/services/seat.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { TheaterRoomsFormComponent } from '../form/rooms-form.component';
import { TableColumn } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-theater-rooms-list',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminTableComponent, TheaterRoomsFormComponent],
  templateUrl: './rooms-list.component.html'
})
export class TheaterRoomsListComponent implements OnInit {
  private roomService = inject(RoomService);
  private theaterService = inject(TheaterService);
  private seatService = inject(SeatService);

  rooms = signal<Room[]>([]);
  theaters = signal<Theater[]>([]);
  selectedTheaterId = signal<string>('');
  
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Modal form states
  showForm = signal(false);
  selectedRoomId = signal<string | null>(null);

  columns: TableColumn[] = [
    { key: 'name', header: 'Tên Phòng Chiếu', type: 'text' },
    { key: 'totalSeat', header: 'Số Lượng Ghế', type: 'number' },
    { key: 'totalRow', header: 'Hàng Ghế', type: 'number' },
    { key: 'totalCol', header: 'Cột Ghế', type: 'number' },
    { key: 'isActive', header: 'Trạng Thái', type: 'boolean' }
  ];

  constructor() {
    // Automatically load data when selectedTheaterId changes
    effect(() => {
      if (this.selectedTheaterId()) {
        this.loadData();
      }
    });
  }

  ngOnInit(): void {
    this.loadTheaters();
  }

  loadTheaters(): void {
    this.theaterService.getTheaters({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.theaters.set(res.items);
        const storedTheaterId = localStorage.getItem('owner_selected_theater_id');
        
        if (storedTheaterId && res.items.some(t => t.id === storedTheaterId)) {
          this.selectedTheaterId.set(storedTheaterId);
        } else if (res.items.length > 0) {
          this.selectedTheaterId.set(res.items[0].id);
        }
      }
    });
  }

  loadData(): void {
    if (!this.selectedTheaterId()) return;

    this.loading.set(true);
    this.roomService.getRooms({
      theaterId: this.selectedTheaterId(),
      page: this.pageIndex(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        this.rooms.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching rooms:', err);
        this.loading.set(false);
      }
    });
  }

  onTheaterChange(theaterId: string): void {
    this.selectedTheaterId.set(theaterId);
    localStorage.setItem('owner_selected_theater_id', theaterId);
    this.pageIndex.set(1);
  }

  onPageChange(page: number): void {
    this.pageIndex.set(page);
    this.loadData();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageIndex.set(1);
    this.loadData();
  }

  openCreate(): void {
    if (!this.selectedTheaterId()) {
      alert('Vui lòng chọn rạp chiếu trước.');
      return;
    }
    this.selectedRoomId.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Room): void {
    this.selectedRoomId.set(row.id);
    this.showForm.set(true);
  }

  onToggleActive(row: Room): void {
    this.roomService.updateRoom(row.id, { isActive: !row.isActive }).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error toggling room active status:', err);
        alert('Có lỗi xảy ra khi đổi trạng thái phòng.');
      }
    });
  }

  onDelete(row: Room): void {
    this.roomService.deleteRoom(row.id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        console.error('Error deleting room:', err);
        alert('Không thể xóa phòng chiếu này. Hãy chắc chắn phòng không có lịch chiếu nào.');
      }
    });
  }

  onFormClose(saved: boolean): void {
    this.showForm.set(false);
    if (saved) {
      this.loadData();
    }
  }

  generateSeats(room: Room): void {
    if (confirm(`Bạn có chắc chắn muốn khởi tạo sơ đồ gồm ${room.totalRow * room.totalCol} ghế cho phòng này?`)) {
      this.loading.set(true);
      this.seatService.bulkCreateSeats(room.id).subscribe({
        next: () => {
          this.loadData();
          alert('Khởi tạo sơ đồ ghế hàng loạt thành công!');
        },
        error: (err) => {
          console.error('Error bulk creating seats:', err);
          alert(err.message || 'Lỗi khi khởi tạo sơ đồ ghế.');
          this.loading.set(false);
        }
      });
    }
  }
}
