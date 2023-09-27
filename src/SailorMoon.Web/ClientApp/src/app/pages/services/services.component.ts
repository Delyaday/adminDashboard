import { Component, OnInit } from '@angular/core';
import { faPlus, faUsers, faSprayCanSparkles } from '@fortawesome/free-solid-svg-icons';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { AddServiceComponent } from './modals/add-service.component';
import { ServiceItem } from '../../models/services.model';
import { ServicesService } from '../../services/services.service';
import * as _ from 'lodash';


@Component({
    selector: 'services-page',
    templateUrl: './services.component.html',
    styleUrls: ['./services.component.scss'],
})
export class ServicesComponent implements OnInit {
    faPlus = faPlus;
    faUsers = faUsers;
    faSprayCanSparkles = faSprayCanSparkles;

    public dataSource: MatTableDataSource<ServiceItem> = new MatTableDataSource();


    displayedColumns: string[] = ['title', 'category', 'duration', 'price', 'description', 'actions'];

    days = ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"];
    months = ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"];

    todayDate: Date = new Date();

    fullDate = this.days[this.todayDate.getDay()] + ", " + this.todayDate.getDate() + " " + this.months[this.todayDate.getMonth()] +
        ", " + this.todayDate.getFullYear();


    constructor(public dialog: MatDialog, private _service: ServicesService) {

    }

    async ngOnInit() {
        var services = await this._service.getAll();

        if (services)
            this.dataSource.data = services;
    }

    async deleteService(client: ServiceItem) {
        var result = await this._service.deleteById(client.id as number);

        if (result)
            this.dataSource.data = this.dataSource.data.filter(f => f.id !== client.id);
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();
    }

    openDialog(service: ServiceItem | undefined): void {
        var dialogRef = this.dialog.open(AddServiceComponent, {
            width: '25vw',
            data: _.cloneDeep(service),
            disableClose: true
        });

        dialogRef.afterClosed().subscribe(async (result: ServiceItem) => {
            if (result && result.title != '') {

                var service: ServiceItem | undefined = undefined;

                if (result.id) {
                    service = await this._service.update(result);

                    var data = this.dataSource.data;

                    var item = _.find(data, f => f.id == service?.id);

                    if (item) {
                        _.assign(item, service);
                        this.dataSource.data = data;
                    }
                }
                else {
                    service = await this._service.create(result);

                    if (service) {
                        this.dataSource.data = [...this.dataSource.data, service];
                    }
                }

            }
        });
    }
}
