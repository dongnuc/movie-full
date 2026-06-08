import { HttpErrorResponse } from '@angular/common/http';

export interface ErrorConfig {
  type?: string;          // Matches error.error.type
  status?: number;        // Matches error.status
  displayTitle: string;   // UI Title
  format?: (body: any) => {
    message: string;
    html: string;
  };
}

export function parseDottedErrorCode(code: string): { entity: string; errorCode: string; displayTitle: string } | null {
  if (!code || !code.includes('.') || code.includes('/') || code.includes('://')) {
    return null;
  }
  const parts = code.split('.');
  const entity = parts[0];
  const errorCode = parts[1];

  const entityTranslations: { [key: string]: string } = {
    Room: 'Phòng Chiếu',
    Theater: 'Rạp Chiếu',
    Category: 'Thể Loại Phim',
    SeatType: 'Loại Ghế',
    Seat: 'Ghế',
    Showing: 'Lịch Chiếu',
    Movie: 'Phim',
    User: 'Người Dùng',
    Role: 'Vai Trò',
    Auth: 'Xác Thực'
  };

  const errorCodeTranslations: { [key: string]: string } = {
    NameAlreadyExists: 'Tên đã tồn tại',
    NotFound: 'Không tìm thấy tài nguyên',
    Invalid: 'Dữ liệu không hợp lệ',
    Expired: 'Phiên làm việc hết hạn',
    Duplicate: 'Dữ liệu trùng lặp',
    Unauthorized: 'Chưa đăng nhập',
    Forbidden: 'Không có quyền truy cập',
    Conflict: 'Xung đột dữ liệu'
  };

  const translatedEntity = entityTranslations[entity] || entity;
  const translatedCode = errorCodeTranslations[errorCode] || errorCode;

  return {
    entity: translatedEntity,
    errorCode: translatedCode,
    displayTitle: `Lỗi ${translatedEntity} (${translatedCode})`
  };
}

export const ERROR_CONFIGS: ErrorConfig[] = [
  // 1. Validation Error (status 400)
  {
    type: 'Validation.Error',
    status: 400,
    displayTitle: 'Lỗi Xác Thực',
    format: (body) => {
      const detail = body.detail || 'Vui lòng kiểm tra các thông tin sau:';
      const errors = body.errors || [];
      const html = `
        <div style="text-align: left; margin-top: 10px;">
          <div style="font-weight: 600; margin-bottom: 8px; color: #ff4a4a;">${detail}</div>
          <ul style="list-style-type: disc; padding-left: 20px; margin: 0; line-height: 1.6;">
            ${errors.map((err: any) => {
              const parsed = parseDottedErrorCode(err.code);
              if (parsed) {
                return `<li><strong>Lỗi ${parsed.entity} (${parsed.errorCode}):</strong> ${err.message || err}</li>`;
              }
              return `<li><strong>${err.code || 'Lỗi'}:</strong> ${err.message || err}</li>`;
            }).join('')}
          </ul>
        </div>
      `;
      const message = errors.map((err: any) => {
        const parsed = parseDottedErrorCode(err.code);
        if (parsed) {
          return `${parsed.entity} - ${parsed.errorCode}: ${err.message || err}`;
        }
        return `${err.code ? err.code + ': ' : ''}${err.message || err}`;
      }).join('\n');
      return { message, html };
    }
  },

  // 2. Unauthorized Access (status 401)
  {
    status: 401,
    displayTitle: 'Yêu Cầu Đăng Nhập',
    format: (body) => ({
      message: body.detail || 'Phiên đăng nhập đã hết hạn hoặc không hợp lệ.',
      html: `<div>${body.detail || 'Vui lòng đăng nhập lại để tiếp tục.'}</div>`
    })
  },

  // 3. Forbidden Access (status 403)
  {
    status: 403,
    displayTitle: 'Từ Chối Truy Cập',
    format: (body) => ({
      message: body.detail || 'Bạn không có quyền truy cập tính năng này.',
      html: `<div>${body.detail || 'Tài khoản của bạn không được cấp quyền truy cập.'}</div>`
    })
  },

  // 4. Not Found (status 404)
  {
    status: 404,
    displayTitle: 'Không Tìm Thấy',
    format: (body) => ({
      message: body.detail || 'Không tìm thấy tài nguyên yêu cầu.',
      html: `<div>${body.detail || 'Không tìm thấy tài nguyên yêu cầu.'}</div>`
    })
  },

  // 5. Internal Server Error (status 500)
  {
    status: 500,
    displayTitle: 'Lỗi Hệ Thống',
    format: (body) => ({
      message: body.detail || 'Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.',
      html: `<div>${body.detail || 'Hệ thống đang gặp sự cố. Vui lòng quay lại sau.'}</div>`
    })
  }
];

export function getParsedError(error: HttpErrorResponse): { title: string; errorMessage: string; htmlContent: string } {
  // 1. Check client-side errors
  if (error.error instanceof ErrorEvent) {
    const msg = `Lỗi Client: ${error.error.message}`;
    return {
      title: 'Lỗi Ứng Dụng',
      errorMessage: msg,
      htmlContent: `<div>${msg}</div>`
    };
  }

  const errBody = error.error || {};

  // 2. Dynamic check for dotted error type (e.g. Room.NameAlreadyExists)
  if (errBody.type) {
    const parsed = parseDottedErrorCode(errBody.type);
    if (parsed) {
      const fallbackMsg = errBody.detail || errBody.message || 'Đã xảy ra lỗi.';
      const htmlContent = `
        <div style="text-align: left; margin-top: 10px;">
          <div style="font-weight: 600; margin-bottom: 8px; color: #ff4a4a; font-size: 1.1em;">
            ${parsed.errorCode} (${errBody.type.split('.')[1]})
          </div>
          <div style="line-height: 1.6; color: #cccccc;">
            ${fallbackMsg}
          </div>
        </div>
      `;
      return {
        title: `Lỗi ${parsed.entity}`,
        errorMessage: `${parsed.errorCode}: ${fallbackMsg}`,
        htmlContent
      };
    }
  }

  // 3. Query configurations to find the matching rule based on type and status
  let matchedConfig = ERROR_CONFIGS.find(cfg => cfg.type === errBody.type && cfg.status === error.status);

  // Fallback to match by type only
  if (!matchedConfig && errBody.type) {
    matchedConfig = ERROR_CONFIGS.find(cfg => cfg.type === errBody.type);
  }

  // Fallback to match by status only
  if (!matchedConfig) {
    matchedConfig = ERROR_CONFIGS.find(cfg => cfg.status === error.status && !cfg.type);
  }

  // 4. Parse matching config or execute default fallback
  if (matchedConfig) {
    const displayTitle = matchedConfig.displayTitle;
    const formatted = matchedConfig.format
      ? matchedConfig.format(errBody)
      : {
          message: errBody.detail || errBody.message || 'Đã xảy ra lỗi.',
          html: `<div>${errBody.detail || errBody.message || 'Đã xảy ra lỗi.'}</div>`
        };

    return {
      title: displayTitle,
      errorMessage: formatted.message,
      htmlContent: formatted.html
    };
  }

  // Fallback if no matching config is found in the list
  const fallbackTitle = errBody.title || 'Lỗi Yêu Cầu';
  const fallbackMsg = errBody.detail || errBody.message || error.message || 'Đã xảy ra lỗi không xác định.';
  let fallbackHtml = `<div>${fallbackMsg}</div>`;

  // If there's an array of errors, format it even in fallback
  if (Array.isArray(errBody.errors)) {
    fallbackHtml = `
      <div style="text-align: left; margin-top: 10px;">
        <ul style="list-style-type: disc; padding-left: 20px; margin: 0; line-height: 1.6;">
          ${errBody.errors.map((err: any) => {
            const parsed = parseDottedErrorCode(err.code);
            if (parsed) {
              return `<li><strong>Lỗi ${parsed.entity} (${parsed.errorCode}):</strong> ${err.message || err}</li>`;
            }
            return `<li><strong>${err.code || 'Lỗi'}:</strong> ${err.message || err}</li>`;
          }).join('')}
        </ul>
      </div>
    `;
  }

  return {
    title: fallbackTitle,
    errorMessage: fallbackMsg,
    htmlContent: fallbackHtml
  };
}
