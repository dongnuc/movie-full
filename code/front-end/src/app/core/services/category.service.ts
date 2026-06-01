import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../shared/models/table-config';

export interface Category {
  id: string;
  name: string;
  isActive: boolean;
  dateCreate?: string;
  dateUpdate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/categories';

  getCategories(page: number = 1, pageSize: number = 10, id?: string): Observable<PagedResult<Category>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (id) {
      params = params.set('id', id);
    }

    return this.http.get<PagedResult<Category>>(this.apiBase, { params });
  }

  getCategoryById(id: string): Observable<Category> {
    return this.http.get<Category>(`${this.apiBase}/${id}`);
  }

  createCategory(category: Partial<Category>): Observable<any> {
    return this.http.post(this.apiBase, category);
  }

  updateCategory(id: string, category: Partial<Category>): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, category);
  }

  deleteCategory(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }
}
