import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountService, Account } from '../../../../core/services/admin-system/account.service';
import { TheaterService, Theater } from '../../../../core/services/admin-system/theater.service';
import { RoleService, Role } from '../../../../core/services/admin-system/role.service';

@Component({
  selector: 'app-admin-accounts-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './accounts-form.component.html'
})
export class AdminAccountsFormComponent implements OnInit {
  private accountService = inject(AccountService);
  private theaterService = inject(TheaterService);
  private roleService = inject(RoleService);

  @Input() account: Account | null = null;
  @Output() close = new EventEmitter<boolean>();

  formData = {
    username: '',
    email: '',
    password: '',
    roleId: '',
    theaterIds: [] as string[]
  };

  roles: Role[] = [];
  theaters: Theater[] = [];
  loadingRoles = false;
  loadingTheaters = false;
  saving = false;
  errorMessage = '';

  // Helper: check if selected role is TheaterOwner (by name)
  get isTheaterOwnerSelected(): boolean {
    const selectedRole = this.roles.find(r => r.id === this.formData.roleId);
    if (!selectedRole) return false;
    const roleName = selectedRole.name.toUpperCase();
    return roleName === 'THEATEROWNER' || 
           roleName === 'THEATER_OWNER' || 
           roleName === 'THEATER OWNER' || 
           roleName === 'ADMIN-THEATER' || 
           roleName === 'ADMIN_THEATER' ||
           roleName === 'ADMIN-THEATRE';
  }

  onRoleChange(): void {
    if (!this.isTheaterOwnerSelected) {
      this.formData.theaterIds = [];
    }
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadTheaters();

    if (this.account) {
      this.formData.username = this.account.username;
      this.formData.email = this.account.email;
      this.formData.theaterIds = this.account.theaterIds || [];
      // roleId will be set after roles are loaded (see loadRoles())
    }
  }

  loadRoles(): void {
    this.loadingRoles = true;
    this.roleService.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.loadingRoles = false;

        // Pre-select role for edit mode by matching role name
        if (this.account && this.account.role) {
          const matched = roles.find(r =>
            r.name.toUpperCase() === this.account!.role.toUpperCase()
          );
          if (matched) {
            this.formData.roleId = matched.id;
          }
        } else if (roles.length > 0) {
          // Default to first non-admin role (USER) for new accounts
          const userRole = roles.find(r => r.name.toUpperCase() === 'USER');
          this.formData.roleId = userRole ? userRole.id : roles[0].id;
        }
      },
      error: (err) => {
        console.error('Error loading roles:', err);
        this.loadingRoles = false;
      }
    });
  }

  loadTheaters(): void {
    this.loadingTheaters = true;
    this.theaterService.getTheaters({ page: 1, pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        this.theaters = res.items;
        this.loadingTheaters = false;
      },
      error: (err) => {
        console.error('Error loading theaters:', err);
        this.loadingTheaters = false;
      }
    });
  }

  onTheaterToggle(theaterId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.formData.theaterIds.includes(theaterId)) {
        this.formData.theaterIds.push(theaterId);
      }
    } else {
      this.formData.theaterIds = this.formData.theaterIds.filter(id => id !== theaterId);
    }
  }

  isTheaterSelected(theaterId: string): boolean {
    return this.formData.theaterIds.includes(theaterId);
  }

  onSubmit(): void {
    if (!this.formData.username.trim()) {
      this.errorMessage = 'Vui lòng nhập tên tài khoản';
      return;
    }
    if (!this.formData.email.trim()) {
      this.errorMessage = 'Vui lòng nhập địa chỉ email';
      return;
    }
    if (!this.account && !this.formData.password) {
      this.errorMessage = 'Vui lòng nhập mật khẩu cho tài khoản mới';
      return;
    }
    if (!this.formData.roleId) {
      this.errorMessage = 'Vui lòng chọn quyền hạn';
      return;
    }

    if (this.isTheaterOwnerSelected && this.formData.theaterIds.length === 0) {
      this.errorMessage = 'Vui lòng chọn ít nhất một rạp chiếu để quản lý';
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const theaterIds = this.isTheaterOwnerSelected ? this.formData.theaterIds : [];

    const obs$ = this.account
      ? this.accountService.updateAccountRole(
        this.account.id,
        this.formData.roleId,
        this.formData.username,
        this.formData.email,
        theaterIds
      )
      : this.accountService.createAccount({
        username: this.formData.username,
        email: this.formData.email,
        roleId: this.formData.roleId,
        password: this.formData.password,
        theaterIds
      });

    obs$.subscribe({
      next: () => {
        this.saving = false;
        this.close.emit(true);
      },
      error: (err) => {
        this.errorMessage = err.error?.detail || err.message || 'Lỗi khi lưu thông tin tài khoản';
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit(false);
  }
}
