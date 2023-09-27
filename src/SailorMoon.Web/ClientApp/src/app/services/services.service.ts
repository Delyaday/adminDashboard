import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ServiceItem } from '../models/services.model';

@Injectable({ providedIn: 'root' })
export class ServicesService {

    constructor(private _http: HttpClient) { }

    public async getAll(): Promise<ServiceItem[]> {
        var result = await this._http.get<ServiceItem[]>("Services").toPromise();

        if (result)
            return result;

        return [];
    }

    public async getById(id: number): Promise<ServiceItem | undefined> {
        var result = await this._http.get<ServiceItem>(`Services/${id}`).toPromise();

        return result;
    }

    public async create(ServiceItem: ServiceItem): Promise<ServiceItem | undefined> {
        var result = await this._http.put<ServiceItem>(`Services`, ServiceItem).toPromise();

        return result
    }

    public async update(ServiceItem: ServiceItem): Promise<ServiceItem | undefined> {
        var result = await this._http.post<ServiceItem>(`Services`, ServiceItem).toPromise();

        return result
    }

    public async deleteById(id: number): Promise<boolean> {
        await this._http.delete(`Services/${id}`).toPromise();

        return true;
    }

}