import { NgModule, APP_INITIALIZER } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { services } from './services';
import { utils } from './utils';

@NgModule({
    imports: [
        HttpClientModule,
    ],
    providers: [
        services.UserService,
        services.AuthenticationService,
        utils.AuthGuard,
        utils.ErrorInterceptor,
        utils.JwtInterceptor,
        { provide: APP_INITIALIZER, useFactory: utils.appInitializer, multi: true, deps: [services.AuthenticationService] },
        { provide: HTTP_INTERCEPTORS, useClass: utils.JwtInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: utils.ErrorInterceptor, multi: true },
    ]
})
export class FoundationModule { }