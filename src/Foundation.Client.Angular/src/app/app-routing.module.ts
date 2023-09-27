import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { MessagingComponent } from './messaging/messaging.component';

import { utils } from '../foundation/utils';

const routes: Routes = [
    { path: '', component: HomeComponent, canActivate: [utils.AuthGuard] },
    { path: 'messaging', component: MessagingComponent, canActivate: [utils.AuthGuard] },
    { path: 'login', component: LoginComponent },
    { path: '**', redirectTo: '' }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { useHash: true })],
    exports: [RouterModule]
})
export class AppRoutingModule { }
