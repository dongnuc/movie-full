import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import Swal from 'sweetalert2';
import { getParsedError } from '../constants/error-mappings';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Parse using the matching logic inside constants
      const parsedError = getParsedError(error);

      console.error('HTTP Error occurred:', error);

      // Display alert using SweetAlert2 styled with Netflix dark theme
      Swal.fire({
        title: parsedError.title,
        html: parsedError.htmlContent,
        icon: 'error',
        background: '#181818',
        color: '#ffffff',
        confirmButtonText: 'Xác Nhận',
        buttonsStyling: false, // Apply custom styles completely
        customClass: {
          popup: 'netflix-swal-popup',
          confirmButton: 'netflix-swal-button'
        }
      });

      // Return custom formatted error object
      return throwError(() => ({
        message: parsedError.errorMessage,
        status: error.status,
        originalError: error
      }));
    })
  );
};
