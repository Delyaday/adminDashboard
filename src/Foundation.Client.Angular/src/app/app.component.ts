import { Component, HostListener } from '@angular/core';

import { AuthenticationService } from '../foundation/services/authentication.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent {

    isSidebarOpened = false;

    constructor(private _authenticationService: AuthenticationService) {
    }

    public get user() { return this._authenticationService.user; }

    logout() {
        this._authenticationService.logout();
    }

    @HostListener('window:mouseup', ['$event'])
    mouseUp(event: any) {
        event.stopPropagation();
        this.isSidebarOpened = false;
    }

}
