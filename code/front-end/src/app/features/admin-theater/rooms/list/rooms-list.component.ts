import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RoomService, Room } from '../../../../core/services/admin-theater/room.service';
import { SeatService } from '../../../../core/services/admin-theater/seat.service';
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
  private seatService = inject(SeatService);
  private router = inject(Router);

  rooms = signal<Room[]>([]);
  
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
    { key: 'matrix', header: 'Ma Trận Phòng', type: 'text' },
    { key: 'isActive', header: 'Trạng Thái', type: 'boolean' }
  ];

  constructor() {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.roomService.getRooms({
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
    this.selectedRoomId.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Room): void {
    this.router.navigate(['/admin-theater/rooms/edit', row.id]);
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
