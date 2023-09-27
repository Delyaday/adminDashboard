import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { Visit } from '../../../models/visits.models';
import { ClientItem } from '../../../models/clients.model';
import { ServiceItem } from '../../../models/services.model';
import { StaffModel } from '../../../models/staff.model';

import { ClientsService } from '../../../services/clients.service';
import { ServicesService } from '../../../services/services.service';
import { StaffService } from '../../../services/staff.service';
import { AccessLevels } from '../../../models/user-description.model';

import * as moment from 'moment';

@Component({
    selector: 'add-visit-modal',
    templateUrl: './add-visit.component.html',
    styleUrls: ['./add-visit.component.scss'],

})
export class AddVisitComponent implements OnInit {

    faUserPlus = faUserPlus;

    private _clients: ClientItem[] = [];
    private _masters: StaffModel[] = [];
    private _services: ServiceItem[] = [];

    time: string;

    constructor(
        public dialogRef: MatDialogRef<AddVisitComponent>,
        @Inject(MAT_DIALOG_DATA) public data: Visit,
        private _clientsService: ClientsService,
        private _servicesService: ServicesService,
        private _staffService: StaffService
    ) {
        if (!data)
            this.data = {} as any;

        if (this.data.time) {
            this.time = moment(this.data.time).format("HH:mm");
        }
    }

    async ngOnInit() {
        this._clients = await this._clientsService.getAll();
        this._services = await this._servicesService.getAll();

        var staff = await this._staffService.getAll();
        this._masters = staff.filter(f => f.description?.accessLevel == AccessLevels.worker);

    }

    get clients() { return this._clients; }

    get masters() { return this._masters; }

    get services() { return this._services; }

    isValid() {
        return !!this.data.clientId && !!this.data.serviceId && !!this.data.staffId && !!this.data.time;
    }

    dateChange(value: any) {

        if (this.time)
            this.changeTime(this.time);

        //debugger;
    }

    changeTime(value: string) {
        this.time = value;

        var duration = moment.duration(this.time);

        this.data.time = moment(this.data.time).startOf('d').add(duration).toDate();

        //debugger;
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    getDateFormatString(): string {

        return 'DD/MM/YYYY';

    }

}
