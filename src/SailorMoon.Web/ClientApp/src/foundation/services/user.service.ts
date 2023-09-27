
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
    constructor(private _http: HttpClient) { }

    getAll(): Promise<User[] | undefined> {
        return this._http.get<User[]>(`users`).toPromise();
    }

    getById(id: number): Promise<User | undefined> {
        return this._http.get<User>(`users/${id}`).toPromise();
    }

    getByLogin(login: string): Promise<User | undefined> {
        return this._http.get<User>(`users/${login}`).toPromise();
    }

    create(request: { login: string, password: string }): Promise<User | undefined> {
        return this._http.post<User>(`users/create`, request).toPromise();
    }

    update(user: User): Promise<User | undefined> {
        return this._http.post<User>(`users/update`, user).toPromise();
    }

    async delete(id: number): Promise<boolean> {
        await this._http.delete<boolean>(`users/${id}`).toPromise();

        return true;
    }
}