import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BACKEND_SETTINGS } from '../services/backend.service';

@Injectable()
export class APIInterceptor implements HttpInterceptor {
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        const apiReq = req.clone({ url: `${BACKEND_SETTINGS.apiUrl}/${req.url}`, withCredentials: true });
        return next.handle(apiReq);
    }
}