import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private authService = inject(AuthService);

  username = '';
  email = '';
  password = '';
  confirmPassword = '';
  
  isLoading = signal(false);
  isRegistered = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  onSubmit(): void {
    if (this.password !== this.confirmPassword) {
      this.errorMessage.set('Mật khẩu xác nhận không khớp.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authService.register(this.username, this.email, this.password, this.confirmPassword).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        this.isRegistered.set(true);
        this.successMessage.set('Đăng ký thành công! Vui lòng đăng nhập.');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.message || 'Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.');
      }
    });
  }
}
