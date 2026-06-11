import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, Router, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-admin-system',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-system.component.html'
})
export class AdminSystemComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        alert('Đăng xuất thành công!');
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        console.warn('Đăng xuất (API thất bại nhưng local session đã được xoá):', err);
        alert('Đăng xuất thành công!');
        this.router.navigate(['/auth/login']);
      }
    });
  }
}
