import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../../shared/models/table-config';
import { Category } from '../admin-system/category.service';

export interface MovieUser {
  id: string;
  title: string;
  description?: string;
  image: string;
  duration: number;
  basePrice: number;
  categoryId: string;
  categoryName: string;
}

export interface TheaterShowingGroup {
  theaterId: string;
  theaterName: string;
  showingCount: number;
}

export interface MovieDetailUser {
  id: string;
  title: string;
  description?: string;
  image: string;
  basePrice: number;
  duration: number;
  categoryId: string;
  categoryName: string;
  theaters: TheaterShowingGroup[];
}

export interface ShowingUser {
  id: string;
  timeStart: string;
  price: number;
  roomId: string;
}

export interface UserSeatGroup {
  seatIds: string[];
}

export interface UserSeatLayout {
  colIndex: number;
  seatNumber: number;
  groupId: string | null;
  seatTypeId: string;
  seatTypeName: string;
  surcharge: number;
  seatIds: UserSeatGroup;
  isActive: boolean;
  status: string; // "Available", "Holding", "Booked"
  isMine: boolean;
  isSelected?: boolean;
}

export interface UserSeatRowLayout {
  rowIndex: number;
  rowName: string;
  seats: UserSeatLayout[];
}

export interface UserRoomLayout {
  id: string;
  name: string;
  seatRows: UserSeatRowLayout[];
  movieId: string;
  movieTitle: string;
  basePrice: number;
  roomId: string;
  showingId: string;
}

@Injectable({
  providedIn: 'root'
})
export class MovieUserService {
  private http = inject(HttpClient);
  private readonly movieApiBase = '/api/v1/movies';
  private readonly categoryApiBase = '/api/v1/categories';

  getMovies(params: {
    search?: string;
    categoryId?: string;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<MovieUser>> {
    let httpParams = new HttpParams();

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    
    const page = params.page || 1;
    const pageSize = params.pageSize || 12; // 12 fits grid layouts (2, 3, 4 columns)
    httpParams = httpParams.set('page', page.toString());
    httpParams = httpParams.set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<MovieUser>>(this.movieApiBase, { params: httpParams });
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(this.categoryApiBase);
  }

  getMovieDetail(id: string): Observable<MovieDetailUser> {
    return this.http.get<MovieDetailUser>(`${this.movieApiBase}/${id}`);
  }

  getFeaturedToday(): Observable<MovieUser[]> {
    return this.http.get<MovieUser[]>(`${this.movieApiBase}/today-featured`);
  }

  getUpcoming(): Observable<MovieUser[]> {
    return this.http.get<MovieUser[]>(`${this.movieApiBase}/upcoming`);
  }

  getTheatersForMovieAndDate(movieId: string, date: string): Observable<any[]> {
    const params = new HttpParams()
      .set('movieId', movieId)
      .set('date', date);
    return this.http.get<any[]>('/api/v1/theaters', { params });
  }

  getShowings(movieId: string, theaterId: string, date: string): Observable<ShowingUser[]> {
    const params = new HttpParams()
      .set('movieId', movieId)
      .set('theaterId', theaterId)
      .set('date', date);

    return this.http.get<ShowingUser[]>('/api/v1/showings', { params });
  }

  getShowingSeatsLayout(showingId: string, roomId?: string): Observable<UserRoomLayout> {
    let params = new HttpParams().set('showingId', showingId);
    if (roomId) {
      params = params.set('roomId', roomId);
    }

    return this.http.get<UserRoomLayout>('/api/v1/showing-seats/layout', { params });
  }

  holdSeats(showingId: string, seatIds: string[]): Observable<{ sessionId: string | null; remainingSeconds: number | null }> {
    return this.http.post<{ sessionId: string | null; remainingSeconds: number | null }>('/api/v1/showing-seats/hold', { showingId, seatIds });
  }

  unholdSeats(showingId: string, seatIds: string[]): Observable<boolean> {
    return this.http.post<boolean>('/api/v1/showing-seats/unhold', { showingId, seatIds });
  }

  getSessionTtl(showingId: string): Observable<{ showingId: string; remainingSeconds: number; isActive: boolean }> {
    const params = new HttpParams().set('showingId', showingId);
    return this.http.get<{ showingId: string; remainingSeconds: number; isActive: boolean }>('/api/v1/showing-seats/session-ttl', { params });
  }

  markSessionAsPaying(showingId: string): Observable<{ remainingSeconds: number | null }> {
    return this.http.post<{ remainingSeconds: number | null }>('/api/v1/showing-seats/paying', { showingId });
  }

  createBooking(showingId: string): Observable<string> {
    return this.http.post<string>('/api/v1/bookings', { showingId });
  }

  getBookings(): Observable<BookingHistoryItem[]> {
    return this.http.get<BookingHistoryItem[]>('/api/v1/bookings');
  }

  getBookingDetail(id: string): Observable<BookingDetailResponse> {
    return this.http.get<BookingDetailResponse>(`/api/v1/bookings/${id}`);
  }
}

export interface BookingHistoryItem {
  bookingId: string;
  movieTitle: string;
  movieImage: string | null;
  showingStart: string;
  roomName: string;
  theaterName: string;
  totalAmount: number;
  status: string;
  seats: string[];
  dateCreate: string;
}

export interface BookingDetailItem {
  bookingDetailId: string;
  seatRow: string;
  seatNumber: number;
  seatType: string;
  price: number;
}

export interface BookingDetailResponse {
  bookingId: string;
  movieTitle: string;
  movieImage: string | null;
  showingStart: string;
  roomName: string;
  theaterName: string;
  theaterLocation: string;
  totalAmount: number;
  status: string;
  details: BookingDetailItem[];
  dateCreate: string;
}
