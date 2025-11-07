import { Injectable, inject } from '@angular/core';
import {
    HttpInterceptor as AngularHttpInterceptor,
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpResponse,
    HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { NotificationService } from '../services/notification.service';

@Injectable({ providedIn: 'root' })
export class CustomHttpInterceptor implements AngularHttpInterceptor {
    private notification = inject(NotificationService);

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            tap(event => {
                if (event instanceof HttpResponse && !event.ok) {
                    this.notification.show(`Request returned status ${event.status}`, 'error');
                }
            }),
            catchError((err: unknown) => {
                if (err instanceof HttpErrorResponse) {
                    const message =
                        (typeof err.error === 'object' && err.error?.message) ||
                        err.message ||
                        `HTTP ${err.status}`;
                    this.notification.show(message, 'error');
                }
                return throwError(() => err);
            })
        );
    }
}
