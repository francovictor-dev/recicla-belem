import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { defer, finalize } from 'rxjs';
import { HttpLoadingService } from './http-loading.service';

export const loadingInterceptor: HttpInterceptorFn = (request, next) => {
  const loading = inject(HttpLoadingService);
  return defer(() => {
    loading.start();
    return next(request);
  }).pipe(finalize(() => loading.finish()));
};
