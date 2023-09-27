export interface UserDescriptionItem {
    id?: number;
    login: string;
    phone?: string;
    position: string;
    accessLevel?: AccessLevels;
    description?: string;
}

export enum AccessLevels {
    worker,
    admin,
}