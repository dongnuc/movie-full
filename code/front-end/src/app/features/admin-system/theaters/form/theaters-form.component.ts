import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TheaterService, Theater } from '../../../../core/services/theater.service';

@Component({
  selector: 'app-admin-theaters-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './theaters-form.component.html'
})
export class AdminTheatersFormComponent implements OnInit {
  private theaterService = inject(TheaterService);

  @Input() theaterId: string | null = null;
  @Output() close = new EventEmitter<boolean>();

  theater: Partial<Theater> = {
    name: '',
    location: '',
    isActive: true
  };

  locationOptions = ['Hà Nội', 'TP.HCM', 'Đà Nẵng', 'Hải Phòng', 'Nha Trang', 'Khác'];

  loading = false;
  saving = false;
  errorMessage = '';

  ngOnInit(): void {
    if (this.theaterId) {
      this.loadTheater();
    }
  }

  loadTheater(): void {
    this.loading = true;
    this.theaterService.getTheaterById(this.theaterId!).subscribe({
      next: (res) => {
        this.theater = { ...res };
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải chi tiết rạp chiếu';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.theater.name?.trim() || !this.theater.location) {
      this.errorMessage = 'Vui lòng điền đầy đủ thông tin bắt buộc';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const obs$ = this.theaterId
      ? this.theaterService.updateTheater(this.theaterId, this.theater)
      : this.theaterService.createTheater(this.theater);

    obs$.subscribe({
      next: () => {
        this.saving = false;
        this.close.emit(true);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Lỗi khi lưu rạp chiếu';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
