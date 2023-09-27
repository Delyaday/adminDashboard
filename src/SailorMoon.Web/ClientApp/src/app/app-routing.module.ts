import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ClientsComponent } from './pages/clients/clients.component';
import { ServicesComponent } from './pages/services/services.component';
import { VisitsComponent } from './pages/visits/visits.component';
import { StaffComponent } from './pages/staff/staff.component';
import { CalendarComponent } from './pages/calendar/calendar.component';
import { AnalyticsComponent } from './pages/analytics/analytics.component';

import { utils } from '../foundation/utils';

const routes: Routes = [
    { path: '', component: CalendarComponent, canActivate: [utils.AuthGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'calendar', component: CalendarComponent, canActivate: [utils.AuthGuard] },
    { path: 'clients', component: ClientsComponent, canActivate: [utils.AuthGuard] },
    { path: 'services', component: ServicesComponent, canActivate: [utils.AuthGuard] },
    { path: 'visits', component: VisitsComponent, canActivate: [utils.AuthGuard] },
    { path: 'staff', component: StaffComponent, canActivate: [utils.AuthGuard] },
    { path: 'analytics', component: AnalyticsComponent, canActivate: [utils.AuthGuard] },
    { path: '**', redirectTo: '' }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { useHash: true })],
    exports: [RouterModule],

})
export class AppRoutingModule { }
