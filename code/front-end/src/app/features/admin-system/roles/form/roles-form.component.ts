import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoleService, Role } from '../../../../core/services/admin-system/role.service';

@Component({
  selector: 'app-admin-roles-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './roles-form.component.html'
})
export class AdminRolesFormComponent implements OnInit {
  private roleService = inject(RoleService);

  @Input() role: Role | null = null;
  @Output() close = new EventEmitter<boolean>();

  roleName = '';
  saving = false;
  errorMessage = '';

  // Permission definitions matching PermissionConstants on backend
  readonly CATEGORIES = [
    { key: 'MOVIE', name: 'Phim (Movie)' },
    { key: 'TICKET', name: 'Vé (Ticket)' },
    { key: 'STAFF', name: 'Nhân viên (Staff)' },
    { key: 'SHOWING', name: 'Lịch chiếu (Showing)' },
    { key: 'THEATER', name: 'Rạp phim (Theater)' },
    { key: 'ROOM', name: 'Phòng chiếu (Room)' },
    { key: 'CATEGORY', name: 'Thể loại (Category)' },
    { key: 'SEAT', name: 'Ghế (Seat)' },
    { key: 'SEATTYPE', name: 'Loại ghế (SeatType)' },
    { key: 'ACCOUNT', name: 'Tài khoản (Account)' },
    { key: 'ROLE', name: 'Vai trò (Role)' },
    { key: 'BOOKING', name: 'Đặt vé (Booking)' }
  ];

  // Actions matching ActionType enum flags on backend
  readonly ACTIONS = [
    { label: 'Xem (Read)', value: 1 },
    { label: 'Thêm (Create)', value: 2 },
    { label: 'Sửa (Update)', value: 4 },
    { label: 'Xóa (Delete)', value: 8 },
    { label: 'Nhập (Import)', value: 16 },
    { label: 'Xuất (Export)', value: 32 },
    { label: 'Duyệt (Approve)', value: 64 }
  ];

  // Store permissions state as category -> mask value
  permissionMatrix: { [category: string]: number } = {};

  get isEditMode(): boolean {
    return this.role !== null;
  }

  ngOnInit(): void {
    // Initialize matrix with 0 for all categories
    this.CATEGORIES.forEach(c => {
      this.permissionMatrix[c.key] = 0;
    });

    if (this.role) {
      this.roleName = this.role.name;
      if (this.role.permissions) {
        // Map existing permissions from database to uppercase keys
        Object.keys(this.role.permissions).forEach(k => {
          const keyUpper = k.toUpperCase();
          if (this.permissionMatrix.hasOwnProperty(keyUpper)) {
            this.permissionMatrix[keyUpper] = this.role!.permissions![k];
          }
        });
      }
    }
  }

  isActionChecked(category: string, actionValue: number): boolean {
    return (this.permissionMatrix[category] & actionValue) === actionValue;
  }

  onActionToggle(category: string, actionValue: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      this.permissionMatrix[category] |= actionValue;
    } else {
      this.permissionMatrix[category] &= ~actionValue;
    }
  }

  isCategoryAllChecked(category: string): boolean {
    // Mask for all actions is 127 (1|2|4|8|16|32|64)
    return this.permissionMatrix[category] === 127;
  }

  toggleCategoryAll(category: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.permissionMatrix[category] = checked ? 127 : 0;
  }

  onSubmit(): void {
    const trimmed = this.roleName.trim();
    if (!trimmed) {
      this.errorMessage = 'Vui lòng nhập tên vai trò';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const onSuccess = () => {
      this.saving = false;
      this.close.emit(true);
    };

    const onError = (err: any) => {
      this.errorMessage = err.error?.detail || err.message || 'Lỗi khi lưu vai trò';
      this.saving = false;
    };

    if (this.isEditMode) {
      this.roleService.updateRole(this.role!.id, trimmed, this.permissionMatrix).subscribe({
        next: onSuccess,
        error: onError
      });
    } else {
      this.roleService.createRole(trimmed, this.permissionMatrix).subscribe({
        next: onSuccess,
        error: onError
      });
    }
  }

  onCancel(): void {
    this.close.emit(false);
  }
}

