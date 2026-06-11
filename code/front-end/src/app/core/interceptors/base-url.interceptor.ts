import { HttpInterceptorFn } from '@angular/common/http';
import { API_BASE_URL } from '../constants/api.constants';

export const baseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  // Prepend baseUrl if request starts with /api or relative path and doesn't have absolute URL
  if (req.url.startsWith('/api') || (!req.url.startsWith('http://') && !req.url.startsWith('https://') && req.url.startsWith('/'))) {
    const apiReq = req.clone({
      url: `${API_BASE_URL}${req.url}`,
      withCredentials: true
    });

    return next(apiReq);
  }
  
  return next(req);
};
