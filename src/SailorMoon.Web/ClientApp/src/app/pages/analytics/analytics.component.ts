import { Component } from '@angular/core';
import { faUser, faBookOpen, faChartColumn, faCircleDollarToSlot, faChartLine } from '@fortawesome/free-solid-svg-icons';
import { VisitsService } from '../../services/visits.service';
import { ClientsService } from '../../services/clients.service';
import { ServicesService } from '../../services/services.service';
import { StaffService } from '../../services/staff.service';
import { Visit } from '../../models/visits.models';
import { ClientItem } from '../../models/clients.model';
import { StaffModel } from '../../models/staff.model';
import { ServiceItem } from '../../models/services.model';
import * as _ from 'lodash';
import * as moment from 'moment';

interface VisitEntry {
    id?: number;
    time: Date;
    description?: string;
    staff?: StaffModel;
    client?: ClientItem;
    service?: ServiceItem;
}

@Component({
    selector: 'analytics-page',
    templateUrl: './analytics.component.html',
    styleUrls: ['./analytics.component.scss']
})
export class AnalyticsComponent {
    faUser = faUser;
    faChartLine = faChartLine;
    faBookOpen = faBookOpen;
    faChartColumn = faChartColumn;
    faCircleDollarToSlot = faCircleDollarToSlot;

    public visits: VisitEntry[] = [];
    public clients: ClientItem[] = [];
    public masters: StaffModel[] = [];


    public chartPieOptions = {
        type: 'pie',
        data: {
            labels: ['Парикмахерская', 'Маникюр', 'Педикюр', 'Макияж'],
            datasets: [{
                label: '# of Votes',
                data: [12, 19, 3, 5],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    };

    public chartBarOptions = {
        type: 'bar',
        data: {
            labels: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август'],
            datasets: [{
                label: 'Доходы',
                data: [65678, 59432, 80128, 81403, 56197, 55516, 40873, 32569],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(255, 159, 64, 0.2)',
                    'rgba(255, 205, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(201, 203, 207, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(255, 159, 64)',
                    'rgb(255, 205, 86)',
                    'rgb(75, 192, 192)',
                    'rgb(54, 162, 235)',
                    'rgb(153, 102, 255)',
                    'rgb(201, 203, 207)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    };


    constructor(private _visitsService: VisitsService,
        private _clientsService: ClientsService,
        private _servicesService: ServicesService,
        private _staffService: StaffService) {
    }

    get currentDate() { return moment().format('dddd, D MMMM, YYYY'); }

    async ngOnInit() {
        this.visits = await this.loadVisits();
    }


    async loadVisits(): Promise<VisitEntry[]> {
        var clients = await this._clientsService.getAll();
        var services = await this._servicesService.getAll();
        var masters = await this._staffService.getAll();
        var visits = await this._visitsService.getAll();

        this.clients = clients;
        this.masters = masters;

        var getClientById = (id: number) => _.find(clients, f => f.id == id);

        var getMasterById = (id: number) => _.find(masters, f => f.id == id);

        var getServiceById = (id: number) => _.find(services, f => f.id == id);

        return visits.map(visit => {
            var entry: VisitEntry = {
                id: visit.id,
                time: visit.time,
                description: visit.description,
                staff: getMasterById(visit.staffId),
                client: getClientById(visit.clientId),
                service: getServiceById(visit.serviceId)
            }

            return entry;
        });
    }
}
