import { Component, signal, OnInit } from '@angular/core';
import { faPlus, faCalendar } from '@fortawesome/free-solid-svg-icons';
import { CalendarOptions, DateSelectArg, EventClickArg, EventApi, EventInput } from '@fullcalendar/core';
import ruLocale from '@fullcalendar/core/locales/ru';
import interactionPlugin from '@fullcalendar/interaction';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import listPlugin from '@fullcalendar/list';

import * as _ from 'lodash';
import { ClientItem } from '../../models/clients.model';
import { StaffModel } from '../../models/staff.model';
import { ServiceItem } from '../../models/services.model';
import { Visit } from '../../models/visits.models';

import { VisitsService } from '../../services/visits.service';
import { ClientsService } from '../../services/clients.service';
import { ServicesService } from '../../services/services.service';
import { StaffService } from '../../services/staff.service';
import { AccessLevels } from '../../models/user-description.model';

import * as moment from 'moment';


@Component({
    selector: 'calendar-page',
    templateUrl: './calendar.component.html',
    styleUrls: ['./calendar.component.scss'],
})
export class CalendarComponent implements OnInit {
    faPlus = faPlus;
    faCalendar = faCalendar;

    private _clients: ClientItem[] = [];
    private _masters: StaffModel[] = [];
    private _services: ServiceItem[] = [];
    private _visits: Visit[] = [];

    events: EventInput[] = [];

    constructor(
        private _visitsService: VisitsService,
        private _clientsService: ClientsService,
        private _servicesService: ServicesService,
        private _staffService: StaffService
    ) {

    }

    get currentDate() { return moment().format('dddd, D MMMM, YYYY'); }

    async ngOnInit() {
        this._clients = await this._clientsService.getAll();
        this._services = await this._servicesService.getAll();

        var staff = await this._staffService.getAll();
        this._masters = staff.filter(f => f.description?.accessLevel == AccessLevels.worker);

        this._visits = await this._visitsService.getAll();

        await this.updateCalendar();
    }

    async updateCalendar() {
        var visits = this._visits;

        var events: EventInput[] = [];

        for (var i = 0; i < visits.length; i++) {
            var visit = visits[i];

            var service = this.getServiceById(visit.serviceId);
            var client = this.getClientById(visit.clientId);
            var master = this.getMasterById(visit.staffId);

            var serviceDuration = moment.duration(service?.duration, 'minutes');

            events.push({
                id: visit.id + '',
                title: '' + client?.firstName + client?.phone,
                start: visit.time,
                end: moment(visit.time).clone().add(serviceDuration).toDate(),
                extendedProps: {
                    duration: serviceDuration,
                    service,
                    client,
                    master
                }
            });
        }

        this.events = events;
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

    handleWeekendsToggle() {
        this.calendarOptions.mutate((options) => {
            options.weekends = !options.weekends;
        });
    }

    humanizeSeconds(seconds: number) {
        var levels = [
            [Math.floor(seconds / 31536000), 'г'],
            [Math.floor((seconds % 31536000) / 86400), 'д'],
            [Math.floor(((seconds % 31536000) % 86400) / 3600), 'ч'],
            [Math.floor((((seconds % 31536000) % 86400) % 3600) / 60), 'мин'],
            [(((seconds % 31536000) % 86400) % 3600) % 60, 'сек'],
        ];
        var returntext = '';

        for (var i = 0, max = levels.length; i < max; i++) {
            if (levels[i][0] === 0) continue;
            returntext += ' ' + (levels[i][0] as number).toFixed(0) + ' ' + levels[i][1];
        };
        return returntext.trim();
    }

    calendarOptions = signal<CalendarOptions>({
        locale: ruLocale,
        plugins: [
            interactionPlugin,
            dayGridPlugin,
            timeGridPlugin,
            listPlugin,
        ],
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        initialView: 'dayGridMonth',
        initialEvents: [], // alternatively, use the `events` setting to fetch from a feed
        weekends: true,
        editable: true,
        selectable: true,
        selectMirror: true,
        dayMaxEvents: true,
        slotMinTime: "09:00:00",
        buttonText: {
            list: "Список"
        }
        //select: this.handleDateSelect.bind(this),
        //eventClick: this.handleEventClick.bind(this),
        //eventsSet: this.handleEvents.bind(this)
        /* you can update a remote database when these fire:
        eventAdd:
        eventChange:
        eventRemove:
        */
    });
}