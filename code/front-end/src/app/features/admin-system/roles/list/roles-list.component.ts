import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoleService, Role } from '../../../../core/services/admin-system/role.service';
import { AdminTableComponent } from '../../../../shared/components/admin-table/admin-table.component';
import { AdminRolesFormComponent } from '../form/roles-form.component';
import { TableColumn } from '../../../../shared/models/table-config';

@Component({
  selector: 'app-admin-roles-list',
  standalone: true,
  imports: [CommonModule, AdminTableComponent, AdminRolesFormComponent],
  templateUrl: './roles-list.component.html'
})
export class AdminRolesListComponent implements OnInit {
  private roleService = inject(RoleService);

  roles = signal<Role[]>([]);
  loading = signal(false);

  // Modal form states
  showForm = signal(false);
  selectedRole = signal<Role | null>(null);

  columns: TableColumn[] = [
    { key: 'name', header: 'Tên Vai Trò', type: 'text' }
  ];

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.roleService.getRoles().subscribe({
      next: (roles) => {
        this.roles.set(roles);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching roles:', err);
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.selectedRole.set(null);
    this.showForm.set(true);
  }

  onEdit(row: Role): void {
    this.selectedRole.set(row);
    this.showForm.set(true);
  }

  onDelete(row: Role): void {
    // Table component already confirmed via browser dialog before emitting
    this.roleService.deleteRole(row.id).subscribe({
      next: () => this.loadData(),
      error: (err) => {
        console.error('Error deleting role:', err);
        alert(err.error?.detail || 'Không thể xóa vai trò này. Có thể đang có tài khoản được gán vai trò này.');
      }
    });
  }

  onFormClose(saved: boolean): void {
    this.showForm.set(false);
    if (saved) {
      this.loadData();
    }
  }
}
