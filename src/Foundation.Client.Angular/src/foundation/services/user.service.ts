
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { User } from '../models/user.model';

import { BACKEND_SETTINGS } from './backend.service';

@Injectable({ providedIn: 'root' })
export class UserService {
    constructor(private _http: HttpClient) { }

    getAll() {
        return this._http.get<User[]>(`${BACKEND_SETTINGS.apiUrl}/users`);
    }
}