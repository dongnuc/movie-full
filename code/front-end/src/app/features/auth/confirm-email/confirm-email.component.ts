import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './confirm-email.component.html'
})
export class ConfirmEmailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  status = signal<'loading' | 'success' | 'error'>('loading');
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const userId = this.route.snapshot.queryParams['userId'];
    const token = this.route.snapshot.queryParams['token'];

    if (!userId || !token) {
      this.status.set('error');
      this.errorMessage.set('Yêu cầu xác nhận không hợp lệ (thiếu userId hoặc token).');
      return;
    }

    this.authService.confirmEmail(userId, token).subscribe({
      next: () => {
        this.status.set('success');
      },
      error: (err) => {
        this.status.set('error');
        let errMsg = 'Liên kết xác thực đã hết hạn hoặc không hợp lệ.';
        if (err && err.error) {
          if (typeof err.error === 'string') {
            errMsg = err.error;
          } else if (err.error.detail) {
            errMsg = err.error.detail;
          }
        }
        this.errorMessage.set(errMsg);
      }
    });
  }
}
