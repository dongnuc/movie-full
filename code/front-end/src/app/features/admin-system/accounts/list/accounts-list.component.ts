import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountService, Account } from '../../../../core/services/admin-system/account.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { AdminAccountsFormComponent } from '../form/accounts-form.component';
import { TableColumn, TableFilter } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-admin-accounts-list',
  standalone: true,
  imports: [CommonModule, AdminTableComponent, AdminAccountsFormComponent],
  templateUrl: './accounts-list.component.html'
})
export class AdminAccountsListComponent implements OnInit {
  private accountService = inject(AccountService);

  accounts = signal<Account[]>([]);
  totalCount = signal(0);
  pageIndex = signal(1);
  pageSize = signal(10);
  loading = signal(false);

  // Form modal states
  showForm = signal(false);
  selectedAccount = signal<Account | null>(null);

  // Filters
  searchQuery = signal('');
  selectedRole = signal('');

  filters: TableFilter[] = [
    {
      key: 'role',
      placeholder: 'Tất cả quyền hạn',
      options: [
        { value: 'SystemAdmin', label: 'Quản trị hệ thống' },
        { value: 'TheaterOwner', label: 'Chủ rạp' },
        { value: 'User', label: 'Khách hàng' }
      ]
    }
  ];

  columns: TableColumn[] = [
    { key: 'username', header: 'Tên Tài Khoản', type: 'text' },
    { key: 'email', header: 'Địa Chỉ Email', type: 'text' },
    {
      key: 'rolesDisplay',
      header: 'Quyền Hạn',
      type: 'text'
    },
    { key: 'isActive', header: 'Trạng Thái', type: 'boolean' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.accountService.getAccounts({
      search: this.searchQuery(),
      role: this.selectedRole() || undefined,
      page: this.pageIndex(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        // Normalize roles: support both `roles[]` (new) and `role` (legacy)
        const normalized = res.items.map(acc => ({
          ...acc,
          roles: acc.roles?.length ? acc.roles : (acc.role ? [acc.role] : []),
          rolesDisplay: (acc.roles?.length ? acc.roles : (acc.role ? [acc.role] : [])).join(', ')
        }));
        this.accounts.set(normalized as any);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching accounts:', err);
        this.loading.set(false);
      }
    });
  }

  onSearch(query: string): void {
    this.searchQuery.set(query);
    this.pageIndex.set(1);
    this.loadData();
  }

  onFilterChange(filtersMap: { [key: string]: any }): void {
    if (filtersMap['role'] !== undefined) {
      this.selectedRole.set(filtersMap['role']);
    }
    this.pageIndex.set(1);
    this.loadData();
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

  onToggleActive(row: Account): void {
    this.accountService.toggleAccountStatus(row.id).subscribe({
      next: () => {
        this.loadData();
      }
    });
  }

  openCreate(): void {
    this.selectedAccount.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Account): void {
    this.selectedAccount.set(row);
    this.showForm.set(true);
  }

  onFormClose(saved: boolean): void {
    this.showForm.set(false);
    if (saved) {
      this.loadData();
    }
  }
}
