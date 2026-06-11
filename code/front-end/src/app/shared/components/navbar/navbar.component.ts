import { Component, HostListener, Input, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent implements OnInit {
  authService = inject(AuthService);
  private router = inject(Router);

  @Input() transparentOnScroll = false;
  isScrolled = signal(false);

  ngOnInit(): void {
    // If not transparent on scroll, it should always be solid black
    if (!this.transparentOnScroll) {
      this.isScrolled.set(true);
    }
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    if (this.transparentOnScroll) {
      this.isScrolled.set(window.scrollY > 50);
    }
  }

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
