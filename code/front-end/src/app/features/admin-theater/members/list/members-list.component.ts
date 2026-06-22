import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TheaterMemberService, TheaterMember } from '../../../../core/services/admin-theater/theater-member.service';
import { RoleService, Role } from '../../../../core/services/admin-system/role.service';

@Component({
  selector: 'app-theater-members-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './members-list.component.html'
})
export class TheaterMembersListComponent implements OnInit {
  private memberService = inject(TheaterMemberService);
  private roleService = inject(RoleService);

  // Expose Math for use in template
  readonly Math = Math;

  // ── State ─────────────────────────────────────────────────────────────────
  members = signal<TheaterMember[]>([]);
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);
  saving = signal(false);

  searchQuery = '';
  errorMessage = '';
  successMessage = '';

  // Modal state
  showAddModal = signal(false);
  showEditModal = signal(false);
  selectedMember = signal<TheaterMember | null>(null);

  // Roles
  availableRoles: Role[] = [];
  readonly THEATER_STAFF_ROLES = ['BOX-OFFICE-STAFF', 'MANAGEMENT-THEATER', 'ADMIN-THEATER'];

  // Form data
  addForm = { username: '', roleId: '' };
  editForm = { roleId: '' };

  // ── Computed ───────────────────────────────────────────────────────────────
  activeCount = computed(() => this.members().filter(m => m.isActive).length);

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize());
  }

  // ── Lifecycle ──────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.loadMembers();
    this.loadRoles();
  }

  // ── Data Loading ───────────────────────────────────────────────────────────
  loadMembers(): void {
    this.loading.set(true);
    this.memberService.getMembers({
      search: this.searchQuery.trim() || undefined,
      page: this.pageIndex(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        this.members.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading members:', err);
        this.loading.set(false);
      }
    });
  }

  loadRoles(): void {
    this.roleService.getTheaterRoles().subscribe({
      next: (roles) => {
        console.log(roles);
        this.availableRoles = roles;
        // Set default on add form if empty
        if (!this.addForm.roleId && this.availableRoles.length > 0) {
          this.addForm.roleId = this.availableRoles[0].id;
        }
      },
      error: (err) => console.error('Error loading roles:', err)
    });
  }

  // ── Search & Pagination ────────────────────────────────────────────────────
  onSearch(): void {
    this.pageIndex.set(1);
    this.loadMembers();
  }

  onPageChange(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.pageIndex.set(page);
    this.loadMembers();
  }

  /** Returns array of page numbers to display (max 5 pages window) */
  getPageRange(): number[] {
    const total = this.totalPages;
    const current = this.pageIndex();
    const delta = 2;
    const start = Math.max(1, current - delta);
    const end = Math.min(total, current + delta);
    const range: number[] = [];
    for (let i = start; i <= end; i++) range.push(i);
    return range;
  }

  // ── Add Modal ──────────────────────────────────────────────────────────────
  openAddModal(): void {
    this.addForm = {
      username: '',
      roleId: this.availableRoles[0]?.id || ''
    };
    this.errorMessage = '';
    this.showAddModal.set(true);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.errorMessage = '';
  }

  onAddMember(): void {
    if (!this.addForm.username.trim()) {
      this.errorMessage = 'Vui lòng nhập tên đăng nhập của nhân viên';
      return;
    }
    if (!this.addForm.roleId) {
      this.errorMessage = 'Vui lòng chọn vai trò cho nhân viên';
      return;
    }

    this.saving.set(true);
    this.errorMessage = '';

    this.memberService.addMember(this.addForm.username.trim(), this.addForm.roleId).subscribe({
      next: () => {
        const username = this.addForm.username.trim();
        this.saving.set(false);
        this.showAddModal.set(false);
        this.showSuccess(`Đã thêm nhân viên "${username}" thành công!`);
        this.loadMembers();
      },
      error: (err) => {
        this.errorMessage =
          err.error?.detail ||
          err.error?.errors?.[0]?.message ||
          err.message ||
          'Lỗi khi thêm nhân viên. Hãy kiểm tra username có tồn tại không.';
        this.saving.set(false);
      }
    });
  }

  // ── Edit Modal ─────────────────────────────────────────────────────────────
  openEditModal(member: TheaterMember): void {
    this.selectedMember.set(member);
    const currentRole = this.availableRoles.find(r =>
      member.roles.some(mr => mr.toUpperCase() === r.name.toUpperCase())
    );
    this.editForm = { roleId: currentRole?.id || this.availableRoles[0]?.id || '' };
    this.errorMessage = '';
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    this.selectedMember.set(null);
    this.errorMessage = '';
  }

  onUpdateMember(): void {
    const member = this.selectedMember();
    if (!member) return;
    if (!this.editForm.roleId) {
      this.errorMessage = 'Vui lòng chọn vai trò mới';
      return;
    }

    this.saving.set(true);
    this.errorMessage = '';

    this.memberService.updateMember(member.id, this.editForm.roleId).subscribe({
      next: () => {
        this.saving.set(false);
        this.showEditModal.set(false);
        this.showSuccess(`Đã cập nhật vai trò cho "${member.username}" thành công!`);
        this.loadMembers();
      },
      error: (err) => {
        this.errorMessage = err.error?.detail || err.message || 'Lỗi khi cập nhật vai trò';
        this.saving.set(false);
      }
    });
  }

  // ── Remove ─────────────────────────────────────────────────────────────────
  onRemoveMember(member: TheaterMember): void {
    if (!confirm(
      `Bạn có chắc chắn muốn gỡ "${member.username}" khỏi rạp?\n\n` +
      `Tài khoản của họ sẽ trở về vai trò USER thông thường.`
    )) return;

    this.memberService.removeMember(member.id).subscribe({
      next: () => {
        this.showSuccess(`Đã gỡ "${member.username}" khỏi rạp thành công!`);
        this.loadMembers();
      },
      error: (err) => {
        this.errorMessage = err.error?.detail || err.message || 'Lỗi khi gỡ nhân viên';
      }
    });
  }

  // ── UI Helpers ─────────────────────────────────────────────────────────────
  private showSuccess(message: string): void {
    this.successMessage = message;
    setTimeout(() => (this.successMessage = ''), 4000);
  }

  /** Returns role badge Tailwind classes based on role name */
  getBadgeClass(role: string): string {
    const r = role.toUpperCase();
    const base = 'px-2.5 py-0.5 rounded-md text-xs font-medium border ';
    if (r.includes('MANAGEMENT'))
      return base + 'bg-purple-900/30 text-purple-300 border-purple-800/60';
    if (r.includes('BOX-OFFICE'))
      return base + 'bg-teal-900/30 text-teal-300 border-teal-800/60';
    if (r.includes('ADMIN'))
      return base + 'bg-blue-900/30 text-blue-300 border-blue-800/60';
    return base + 'bg-gray-800/50 text-gray-300 border-gray-700/60';
  }

  /** Deterministic avatar color based on username */
  getAvatarColor(username: string): string {
    const colors = [
      '#2563eb', '#7c3aed', '#db2777', '#059669',
      '#d97706', '#dc2626', '#0891b2', '#65a30d'
    ];
    let hash = 0;
    for (let i = 0; i < username.length; i++) {
      hash = username.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }
}
