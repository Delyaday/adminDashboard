
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
    private _refreshTokenTimer: any;

    private _userSubject: BehaviorSubject<User | null>;
    private _userObservable: Observable<User | null>;

    constructor(private _http: HttpClient, private _router: Router) {
        this._userSubject = new BehaviorSubject<User | null>(null);
        this._userObservable = this._userSubject.asObservable();
    }

    public get user(): User | null { return this._userSubject.value; }

    login(login: string, password: string) {
        return this._http.post<any>(`users/authenticate`, { login, password }, { withCredentials: true })
            .pipe(map(user => {
                this._userSubject.next(user);
                this.startRefreshTokenTimer();
                return user;
            }));
    }

    logout() {
        this._http.post<any>(`users/token/revoke`, {}, { withCredentials: true }).subscribe();
        this.stopRefreshTokenTimer();
        this._userSubject.next(null);
        this._router.navigate(['/login']);
    }

    refreshToken() {
        return this._http.post<any>(`users/token/refresh`, {}, { withCredentials: true })
            .pipe(map((user) => {
                this._userSubject.next(user);
                this.startRefreshTokenTimer();
                return user;
            }));
    }

    private startRefreshTokenTimer() {
        const jwtToken = JSON.parse(atob(this.user?.jwtToken?.split('.')[1] as any));

        const expires = new Date(jwtToken.exp * 1000);
        const timeout = expires.getTime() - Date.now() - (60 * 1000);
        this._refreshTokenTimer = setTimeout(() => this.refreshToken().subscribe(), timeout);
    }

    private stopRefreshTokenTimer() {
        clearTimeout(this._refreshTokenTimer);
    }
}