import { HttpInterceptorFn } from '@angular/common/http';

export const baseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const baseUrl = 'http://localhost:5241';
  
  // Prepend baseUrl if request starts with /api or relative path and doesn't have absolute URL
  if (req.url.startsWith('/api') || (!req.url.startsWith('http://') && !req.url.startsWith('https://') && req.url.startsWith('/'))) {
    const apiReq = req.clone({
      url: `${baseUrl}${req.url}`
    });
    return next(apiReq);
  }
  
  return next(req);
};
