import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoomService, Room } from '../../../../core/services/room.service';

@Component({
  selector: 'app-theater-rooms-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rooms-form.component.html'
})
export class TheaterRoomsFormComponent implements OnInit {
  private roomService = inject(RoomService);

  @Input() roomId: string | null = null;
  @Input() theaterId!: string;
  @Output() close = new EventEmitter<boolean>();

  room: Partial<Room> = {
    name: '',
    totalRow: 8,
    totalCol: 10,
    isActive: true,
    theaterId: ''
  };

  loading = false;
  saving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.room.theaterId = this.theaterId;
    if (this.roomId) {
      this.loadRoom();
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
        this.close.emit(true);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Lỗi khi lưu phòng chiếu';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
