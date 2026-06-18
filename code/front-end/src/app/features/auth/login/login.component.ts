import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  emailOrUsername = '';
  password = '';
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  // States for Resend Confirm Email
  showResendButton = signal(false);
  showEmailInput = signal(false);
  resendEmailValue = '';
  resendSuccessMessage = signal<string | null>(null);
  isResending = signal(false);

  onSubmit(): void {
    if (!this.emailOrUsername || !this.password) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.showResendButton.set(false);
    this.showEmailInput.set(false);
    this.resendSuccessMessage.set(null);

    this.authService.login(this.emailOrUsername, this.password).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.isLoading.set(false);
        
        let message = 'Tên đăng nhập hoặc mật khẩu không chính xác.';
        if (err && err.error) {
          if (typeof err.error === 'string') {
            message = err.error;
          } else if (err.error.errors && Array.isArray(err.error.errors) && err.error.errors.length > 0) {
            message = err.error.errors.map((e: any) => e.message || e.description || e.detail || e).join(', ');
          } else if (err.error.detail) {
            message = err.error.detail;
          } else if (err.error.title) {
            message = err.error.title;
          }

          // Check if error is due to Email Not Confirmed
          if (err.error.type === 'Auth.EmailNotConfirmed') {
            this.showResendButton.set(true);
            
            // If the login input contains '@', it is likely an email.
            // Otherwise, we immediately show the email input to let them provide it.
            if (this.emailOrUsername.includes('@')) {
              this.resendEmailValue = this.emailOrUsername;
              this.showEmailInput.set(false);
            } else {
              this.resendEmailValue = '';
              this.showEmailInput.set(true);
            }
          }
        }
        this.errorMessage.set(message);
      }
    });
  }

  onResendConfirmEmail(): void {
    const emailToUse = this.showEmailInput() 
      ? this.resendEmailValue 
      : (this.emailOrUsername.includes('@') ? this.emailOrUsername : '');

    if (!emailToUse || !emailToUse.trim() || !emailToUse.includes('@')) {
      this.errorMessage.set('Vui lòng nhập địa chỉ email hợp lệ để nhận link xác nhận.');
      this.showEmailInput.set(true);
      return;
    }

    this.isResending.set(true);
    this.errorMessage.set(null);
    this.resendSuccessMessage.set(null);

    this.authService.resendConfirmEmail(emailToUse.trim()).subscribe({
      next: (res) => {
        this.isResending.set(false);
        this.showResendButton.set(false);
        this.showEmailInput.set(false);
        this.resendSuccessMessage.set(res.message || 'Email xác thực đã được gửi lại thành công! Vui lòng kiểm tra hộp thư.');
        this.errorMessage.set(null);
      },
      error: (err) => {
        this.isResending.set(false);
        
        let errMsg = 'Không thể gửi lại email xác thực. Vui lòng thử lại sau.';
        let isInvalidFormat = false;

        if (err && err.error) {
          if (typeof err.error === 'string') {
            errMsg = err.error;
          } else if (err.error.detail) {
            errMsg = err.error.detail;
          }
          
          if (err.error.type === 'Auth.InvalidEmailFormat' || 
              err.error.type === 'Auth.InvalidEmail' || 
              err.error.type === 'Auth.UserNotFound') {
            isInvalidFormat = true;
          }
        }

        this.errorMessage.set(errMsg);

        if (isInvalidFormat) {
          this.showEmailInput.set(true);
        }
      }
    });
  }
}
