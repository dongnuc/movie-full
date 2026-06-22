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
    /** IDs of selected roles (multi-select) */
    roleIds: [] as string[],
    theaterIds: [] as string[]
  };

  roles: Role[] = [];
  theaters: Theater[] = [];
  loadingRoles = false;
  loadingTheaters = false;
  saving = false;
  errorMessage = '';

  // ── Role helpers ──────────────────────────────────────────────────────────

  isRoleSelected(roleId: string): boolean {
    return this.formData.roleIds.includes(roleId);
  }

  onRoleToggle(roleId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.formData.roleIds.includes(roleId)) {
        this.formData.roleIds = [...this.formData.roleIds, roleId];
      }
    } else {
      this.formData.roleIds = this.formData.roleIds.filter(id => id !== roleId);
    }
    // Clear theaters if Theater-Owner role is no longer selected
    if (!this.isTheaterOwnerSelected) {
      this.formData.theaterIds = [];
    }
  }

  /** True if any currently selected role is a Theater-Admin variant */
  get isTheaterOwnerSelected(): boolean {
    const theaterAdminNames = ['THEATEROWNER', 'THEATER_OWNER', 'THEATER OWNER',
                               'ADMIN-THEATER', 'ADMIN_THEATER', 'ADMIN-THEATRE'];
    return this.formData.roleIds.some(id => {
      const role = this.roles.find(r => r.id === id);
      return role ? theaterAdminNames.includes(role.name.toUpperCase()) : false;
    });
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.loadRoles();
    this.loadTheaters();

    if (this.account) {
      this.formData.username  = this.account.username;
      this.formData.email     = this.account.email;
      this.formData.theaterIds = this.account.theaterIds || [];
      // roleIds will be pre-selected after roles finish loading (see loadRoles)
    }
  }

  loadRoles(): void {
    this.loadingRoles = true;
    this.roleService.getSystemRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.loadingRoles = false;

        if (this.account) {
          // Edit mode: pre-select all matching roles from the account
          const accountRoles: string[] = this.account.roles?.length
            ? this.account.roles
            : (this.account.role ? [this.account.role] : []);

          this.formData.roleIds = roles
            .filter(r => accountRoles.some(ar => ar.toUpperCase() === r.name.toUpperCase()))
            .map(r => r.id);
        } else if (roles.length > 0) {
          // Create mode: default to USER role
          const userRole = roles.find(r => r.name.toUpperCase() === 'USER');
          this.formData.roleIds = userRole ? [userRole.id] : [roles[0].id];
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

  // ── Theater helpers ───────────────────────────────────────────────────────

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

  // ── Submit ────────────────────────────────────────────────────────────────

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
    if (this.formData.roleIds.length === 0) {
      this.errorMessage = 'Vui lòng chọn ít nhất một quyền hạn';
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
          this.formData.roleIds,
          this.formData.username,
          this.formData.email,
          theaterIds
        )
      : this.accountService.createAccount({
          username: this.formData.username,
          email: this.formData.email,
          roleId: this.formData.roleIds[0], // create still uses single roleId
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
