import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Visit } from '../models/visits.models';

@Injectable({ providedIn: 'root' })
export class VisitsService {

    constructor(private _http: HttpClient) { }

    public async getAll(): Promise<Visit[]> {
        var result = await this._http.get<Visit[]>("Visits").toPromise();

        if (result)
            return result;

        return [];
    }

    public async getById(id: number): Promise<Visit | undefined> {
        var result = await this._http.get<Visit>(`Visits/${id}`).toPromise();

        return result;
    }

    public async getByClient(clientId: number): Promise<Visit[]> {
        var result = await this._http.get<Visit[]>(`Visits/byClient/${clientId}`).toPromise();

        if (result)
            return result;

        return [];
    }

    public async getByService(serviceId: number): Promise<Visit[]> {
        var result = await this._http.get<Visit[]>(`Visits/byService/${serviceId}`).toPromise();

        if (result)
            return result;

        return [];
    }

    public async getByStaff(staffId: number): Promise<Visit[]> {
        var result = await this._http.get<Visit[]>(`Visits/byStaff/${staffId}`).toPromise();

        if (result)
            return result;

        return [];
    }

    public async create(clientItem: Visit): Promise<Visit | undefined> {
        var result = await this._http.put<Visit>(`Visits`, clientItem).toPromise();

        return result
    }

    public async update(clientItem: Visit): Promise<Visit | undefined> {
        var result = await this._http.post<Visit>(`Visits`, clientItem).toPromise();

        return result
    }

    public async deleteById(id: number): Promise<boolean> {
        await this._http.delete(`Visits/${id}`).toPromise();

        return true;
    }
}