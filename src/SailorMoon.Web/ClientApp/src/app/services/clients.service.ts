import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ClientItem } from '../models/clients.model';

@Injectable({ providedIn: 'root' })
export class ClientsService {

    constructor(private _http: HttpClient) { }

    public async getAll(): Promise<ClientItem[]> {
        var result = await this._http.get<ClientItem[]>("Clients").toPromise();

        if (result)
            return result;

        return [];
    }

    public async getById(id: number): Promise<ClientItem | undefined> {
        var result = await this._http.get<ClientItem>(`Clients/${id}`).toPromise();

        return result;
    }

    public async getByPhone(phone: string): Promise<ClientItem | undefined> {
        var result = await this._http.get<ClientItem>(`Clients/phone/${phone}`).toPromise();

        return result;
    }

    public async create(clientItem: ClientItem): Promise<ClientItem | undefined> {
        var result = await this._http.put<ClientItem>(`Clients`, clientItem).toPromise();

        return result
    }

    public async update(clientItem: ClientItem): Promise<ClientItem | undefined> {
        var result = await this._http.post<ClientItem>(`Clients`, clientItem).toPromise();

        return result
    }

    public async deleteById(id: number): Promise<boolean> {
        await this._http.delete(`Clients/${id}`).toPromise();

        return true;
    }

    public async deleteByPhone(phone: string): Promise<boolean> {
        await this._http.delete(`Clients/phone/${phone}`).toPromise();

        return true;
    }
}