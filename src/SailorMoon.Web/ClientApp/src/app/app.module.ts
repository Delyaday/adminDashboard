import { NgModule, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ServiceWorkerModule } from '@angular/service-worker';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { FullCalendarModule } from '@fullcalendar/angular';

import { NgxMatTimepickerModule } from 'ngx-mat-timepicker';

import { AppRoutingModule } from './app-routing.module';

import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { ClientsComponent } from './pages/clients/clients.component';
import { ServicesComponent } from './pages/services/services.component';
import { StaffComponent } from './pages/staff/staff.component';
import { VisitsComponent } from './pages/visits/visits.component';
import { AnalyticsComponent } from './pages/analytics/analytics.component';


import { AddClientComponent } from './pages/clients/modals/add-client.component';
import { AddServiceComponent } from './pages/services/modals/add-service.component';
import { AddVisitComponent } from './pages/visits/modals/add-visit.component';
import { AddStaffComponent } from './pages/staff/modals/add-staff.component';
import { EditStaffComponent } from './pages/staff/modals/edit-staff.component';
import { CalendarComponent } from './pages/calendar/calendar.component';

import { ChartComponent } from './components/chart/chart.component';

import { FoundationModule } from '../foundation/foundation.module';

import { MaterialModule } from './material.module';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

import { ClientsService } from './services/clients.service';
import { ServicesService } from './services/services.service';
import { UserDescriptionService } from './services/user-description.service';
import { VisitsService } from './services/visits.service';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import * as moment from 'moment';

import { CanvasJSAngularChartsModule } from '@canvasjs/angular-charts';
import { NgxChartsModule } from '@swimlane/ngx-charts';

moment.locale('ru');

@NgModule({
    declarations: [
        AppComponent,
        LoginComponent,
        ClientsComponent,
        AddClientComponent,
        ServicesComponent,
        AddServiceComponent,
        VisitsComponent,
        AddVisitComponent,
        StaffComponent,
        AddStaffComponent,
        EditStaffComponent,
        CalendarComponent,
        AnalyticsComponent,
        ChartComponent
    ],
    imports: [
        CommonModule,
        BrowserModule,
        AppRoutingModule,
        FormsModule,
        HttpClientModule,
        FoundationModule,
        MaterialModule,
        ServiceWorkerModule.register('ngsw-worker.js', {
            enabled: !isDevMode(),
            // Register the ServiceWorker as soon as the application is stable
            // or after 30 seconds (whichever comes first).
            registrationStrategy: 'registerWhenStable:30000'
        }),
        BrowserAnimationsModule,
        FontAwesomeModule,
        NgxMatTimepickerModule,
        FullCalendarModule,
        CanvasJSAngularChartsModule,
        NgxChartsModule,
    ],
    providers: [
        { provide: MAT_DATE_LOCALE, useValue: 'ru-RU' },
        ClientsService,
        ServicesService,
        UserDescriptionService,
        VisitsService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
