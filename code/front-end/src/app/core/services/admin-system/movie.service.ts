import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../../shared/models/table-config';

export interface Movie {
  id: string;
  title: string;
  description: string;
  image: string;
  basePrice: number;
  duration: number;
  categoryId: string;
  categoryName?: string;
  isActive: boolean;
  dateCreate?: string;
  dateUpdate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/movies';

  getMovies(params: {
    search?: string;
    categoryId?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<Movie>> {
    let httpParams = new HttpParams();

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    
    const page = params.page || 1;
    const pageSize = params.pageSize || 10;
    httpParams = httpParams.set('page', page.toString());
    httpParams = httpParams.set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Movie>>(this.apiBase, { params: httpParams });
  }

  getMovieById(id: string): Observable<Movie> {
    return this.http.get<Movie>(`${this.apiBase}/${id}`);
  }

  createMovie(movie: Partial<Movie>): Observable<any> {
    return this.http.post(this.apiBase, movie);
  }

  updateMovie(id: string, movie: Partial<Movie>): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, movie);
  }

  deleteMovie(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }
}
