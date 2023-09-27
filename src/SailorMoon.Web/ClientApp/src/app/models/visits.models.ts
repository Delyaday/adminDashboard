
export interface Visit {
    id?: number;
    clientId: number;
    serviceId: number;
    staffId: number;
    time: Date;
    description?: string;
}