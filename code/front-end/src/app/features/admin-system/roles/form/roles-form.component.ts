import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoleService, Role } from '../../../../core/services/role.service';

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

  get isEditMode(): boolean {
    return this.role !== null;
  }

  ngOnInit(): void {
    if (this.role) {
      this.roleName = this.role.name;
    }
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
      this.roleService.updateRole(this.role!.id, trimmed).subscribe({
        next: onSuccess,
        error: onError
      });
    } else {
      this.roleService.createRole(trimmed).subscribe({
        next: onSuccess,
        error: onError
      });
    }
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
