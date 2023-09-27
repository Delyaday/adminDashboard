import { Component, Input, OnInit } from '@angular/core';
import { faPlus, faIdCard } from '@fortawesome/free-solid-svg-icons';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { AddClientComponent } from './modals/add-client.component';
import { ClientItem } from '../../models/clients.model';
import { ClientsService } from '../../services/clients.service';
import * as _ from 'lodash';

@Component({
    selector: 'clients-page',
    templateUrl: './clients.component.html',
    styleUrls: ['./clients.component.scss'],
})
export class ClientsComponent implements OnInit {
    faPlus = faPlus;
    faIdCard = faIdCard;

    public dataSource: MatTableDataSource<ClientItem> = new MatTableDataSource();
        
    displayedColumns: string[] = ['firstName', 'lastName', 'phone', 'birthday', 'description', 'actions'];


    days = ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"];
    months = ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"];

    todayDate: Date = new Date();

    fullDate = this.days[this.todayDate.getDay()] + ", " + this.todayDate.getDate() + " " + this.months[this.todayDate.getMonth()] +
        ", " + this.todayDate.getFullYear();


    constructor(public dialog: MatDialog, private _service: ClientsService) {
      
    }

    async ngOnInit() {
        var clients = await this._service.getAll();

        if (clients)
            this.dataSource.data = clients;
    }

    async deleteClient(client: ClientItem) {
        var result = await this._service.deleteById(client.id as number);

        if (result)
            this.dataSource.data = this.dataSource.data.filter(f => f.id !== client.id);
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();
    }

    openDialog(client: ClientItem | undefined): void {
        var dialogRef = this.dialog.open(AddClientComponent, {
            width: '25vw',
            data: _.cloneDeep(client),
            disableClose: true,
            autoFocus: true,
            
        });



        dialogRef.afterClosed().subscribe(async (result: ClientItem) => {
            if (result && result.firstName != '' && result.phone != '') {

                var client: ClientItem | undefined = undefined;

                if (result.id) {
                    client = await this._service.update(result);

                    var data = this.dataSource.data;

                    var item = _.find(data, f => f.id == client?.id);

                    if (item) {
                        _.assign(item, client);
                        this.dataSource.data = data;
                    }
                }
                else {
                    client = await this._service.create(result);

                    if (client) {
                        this.dataSource.data = [...this.dataSource.data, client];
                    }
                }

            }

        });
    }
}
