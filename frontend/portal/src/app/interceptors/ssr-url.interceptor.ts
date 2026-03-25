import { HttpInterceptorFn } from '@angular/common/http';
import { isPlatformServer } from '@angular/common';
import { PLATFORM_ID, inject } from '@angular/core';

export const ssrUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const platformId = inject(PLATFORM_ID);

  if (isPlatformServer(platformId) && req.url.startsWith('/')) {
    const baseUrl = process.env['API_BASE_URL'] || 'http://localhost:8080';
    const serverReq = req.clone({ url: `${baseUrl}${req.url}` });
    return next(serverReq);
  }

  return next(req);
};
