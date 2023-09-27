import { Component, OnInit } from '@angular/core';
import { faPlus, faAddressBook } from '@fortawesome/free-solid-svg-icons';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { AddVisitComponent } from './modals/add-visit.component';
import { Visit } from '../../models/visits.models';
import { VisitsService } from '../../services/visits.service';
import * as _ from 'lodash';
import { ClientItem } from '../../models/clients.model';
import { StaffModel } from '../../models/staff.model';
import { ServiceItem } from '../../models/services.model';


import { ClientsService } from '../../services/clients.service';
import { ServicesService } from '../../services/services.service';
import { StaffService } from '../../services/staff.service';
import { AccessLevels } from '../../models/user-description.model';

@Component({
    selector: 'visits-page',
    templateUrl: './visits.component.html',
    styleUrls: ['./visits.component.scss'],
})
export class VisitsComponent implements OnInit {
    faPlus = faPlus;
    faAddressBook = faAddressBook;

    private _clients: ClientItem[] = [];
    private _masters: StaffModel[] = [];
    private _services: ServiceItem[] = [];

    public dataSource: MatTableDataSource<Visit> = new MatTableDataSource();

    displayedColumns: string[] = ['time', 'client', 'service', 'staff', 'description', 'actions'];


    days = ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"];
    months = ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"];

    todayDate: Date = new Date();

    fullDate = this.days[this.todayDate.getDay()] + ", " + this.todayDate.getDate() + " " + this.months[this.todayDate.getMonth()] +
        ", " + this.todayDate.getFullYear();


    constructor(public dialog: MatDialog,
        private _service: VisitsService,
        private _clientsService: ClientsService,
        private _servicesService: ServicesService,
        private _staffService: StaffService
    ) {
        this.dataSource.filterPredicate = (visit: Visit, filter: string) => {
            var service = this.getServiceById(visit.serviceId);
            var client = this.getClientById(visit.clientId);
            var master = this.getMasterById(visit.staffId);

            if (service?.title.toLowerCase().includes(filter))
                return true;

            if (service?.description?.toLowerCase().includes(filter))
                return true;

            if (client?.firstName.toLowerCase().includes(filter))
                return true;

            if (client?.lastName?.toLowerCase().includes(filter))
                return true;

            if (client?.phone?.toLowerCase().includes(filter))
                return true;

            if (client?.description?.toLowerCase().includes(filter))
                return true;

            if (master?.firstName?.toLowerCase().includes(filter))
                return true;

            if (master?.lastName?.toLowerCase().includes(filter))
                return true;

            if (master?.login?.toLowerCase().includes(filter))
                return true;

            return false;

        }
    }

    async ngOnInit() {
        this._clients = await this._clientsService.getAll();
        this._services = await this._servicesService.getAll();

        var staff = await this._staffService.getAll();
        this._masters = staff.filter(f => f.description?.accessLevel == AccessLevels.worker);

        var clients = await this._service.getAll();

        if (clients)
            this.dataSource.data = clients;
    }

    getClientById(id: number) {
        return _.find(this._clients, f => f.id == id);
    }

    getMasterById(id: number) {
        return _.find(this._masters, f => f.id == id);
    }

    getServiceById(id: number) {
        return _.find(this._services, f => f.id == id);
    }

    async deleteVisit(visit: Visit) {
        var result = await this._service.deleteById(visit.id as number);

        if (result)
            this.dataSource.data = this.dataSource.data.filter(f => f.id !== visit.id);
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();
    }

    openDialog(client: Visit | undefined = undefined): void {
        var dialogRef = this.dialog.open(AddVisitComponent, {
            width: '25vw',
            data: _.cloneDeep(client),
            disableClose: true,
            autoFocus: true
        });

        dialogRef.afterClosed().subscribe(async (result: Visit) => {
            if (result) {

                var visit: Visit | undefined = undefined;

                if (result.id) {
                    visit = await this._service.update(result);

                    var data = this.dataSource.data;

                    var item = _.find(data, f => f.id == visit?.id);

                    if (item) {
                        _.assign(item, visit);
                        this.dataSource.data = data;
                    }
                }
                else {
                    visit = await this._service.create(result);

                    if (visit) {
                        this.dataSource.data = [...this.dataSource.data, visit];
                    }
                }

            }

        });
    }
}
