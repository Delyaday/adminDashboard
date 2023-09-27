import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core'

import { StaffModel } from '../models/staff.model';
import { UserDescriptionItem } from '../models/user-description.model';
import { User } from '../../foundation/models/user.model';

import { UserService } from '../../foundation/services/user.service';
import { UserDescriptionService } from './user-description.service';

@Injectable({ providedIn: 'root' })
export class StaffService {
    constructor(
        private _http: HttpClient,
        private _userService: UserService,
        private _userDescriptionService: UserDescriptionService
    ) { }

    public async create(staffModel: StaffModel, password: string) {
        var createdUser = await this._userService.create({ login: staffModel.login, password });

        if (createdUser && staffModel.description) {

            createdUser.firstName = staffModel.firstName;
            createdUser.lastName = staffModel.lastName;

            await this._userService.update(createdUser);

            staffModel.description.login = createdUser.login;

            await this._userDescriptionService.create(staffModel.description);

            return await this.getById(createdUser.id as number);
        }

        return;
    }

    public async getById(id: number): Promise<StaffModel | undefined> {
        var user = await this._userService.getById(id);
        if (!user)
            return;

        var description = await this._userDescriptionService.getByLogin(user.login);

        var result: StaffModel = user as StaffModel;

        if (description)
            result.description = description;

        return result;
    }

    public async getAll(): Promise<StaffModel[]> {
        var result: StaffModel[] = [];

        var users = await this._userService.getAll();

        if (users) {
            for (var i = 0; i < users.length; i++) {
                var user = users[i] as StaffModel;
                var description = await this._userDescriptionService.getByLogin(user.login);

                if (description) {
                    user.description = description;
                }

                result.push(user);
            }
        }

        return result;
    }

    public async update(staffModel: StaffModel): Promise<StaffModel | undefined> {
        await this._userService.update(staffModel);

        if (staffModel.description) {
            var description = await this._userDescriptionService.getByLogin(staffModel.login);

            if (description) {
                await this._userDescriptionService.update(staffModel.description);
            }
            else {
                staffModel.description.login = staffModel.login;
                await this._userDescriptionService.create(staffModel.description);
            }
        }


        return await this.getById(staffModel.id as number);
    }

    public async delete(staffModel: StaffModel): Promise<boolean> {
        if (staffModel.description)
            await this._userDescriptionService.deleteById(staffModel.description.id as number);

        await this._userService.delete(staffModel.id as number);

        return true;
    }
}
