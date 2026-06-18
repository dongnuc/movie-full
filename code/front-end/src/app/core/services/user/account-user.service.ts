import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface UserPointsResponse {
  points: number;
}

@Injectable({
  providedIn: 'root'
})
export class AccountUserService {
  private http = inject(HttpClient);
  private readonly pointsApiUrl = '/api/v1/accounts/points';

  readonly userPoints = signal<number | null>(null);

  getUserPoints(): Observable<UserPointsResponse> {
    return this.http.get<UserPointsResponse>(this.pointsApiUrl).pipe(
      tap(res => this.userPoints.set(res.points))
    );
  }

  clearPoints(): void {
    this.userPoints.set(null);
  }
}
