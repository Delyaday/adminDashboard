import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserDescriptionItem } from '../models/user-description.model';

@Injectable({ providedIn: 'root' })
export class UserDescriptionService {

    constructor(private _http: HttpClient) { }

    public async getAll(): Promise<UserDescriptionItem[]> {
        var result = await this._http.get<UserDescriptionItem[]>("UserDescription").toPromise();

        if (result)
            return result;

        return [];
    }

    public async getById(id: number): Promise<UserDescriptionItem | undefined> {
        var result = await this._http.get<UserDescriptionItem>(`UserDescription/${id}`).toPromise();

        return result;
    }

    public async getByLogin(login: string): Promise<UserDescriptionItem | undefined> {
        var result = await this._http.get<UserDescriptionItem>(`UserDescription/login/${login}`).toPromise();

        return result;
    }

    public async create(UserDescriptionItem: UserDescriptionItem): Promise<UserDescriptionItem | undefined> {
        var result = await this._http.put<UserDescriptionItem>(`UserDescription`, UserDescriptionItem).toPromise();

        return result
    }

    public async update(UserDescriptionItem: UserDescriptionItem): Promise<UserDescriptionItem | undefined> {
        var result = await this._http.post<UserDescriptionItem>(`UserDescription`, UserDescriptionItem).toPromise();

        return result
    }

    public async deleteById(id: number): Promise<boolean> {
        await this._http.delete(`UserDescription/${id}`).toPromise();

        return true;
    }

    public async deleteByLogin(login: string): Promise<boolean> {
        await this._http.delete(`UserDescription/login/${login}`).toPromise();

        return true;
    }
}