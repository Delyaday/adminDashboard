import { Component, HostListener } from '@angular/core';
import { faAddressBook, faAddressCard, faMagnifyingGlass, faSprayCanSparkles, faRightFromBracket, faCalendar, faIdCard, faChartLine, faGear, faUsers, faChartSimple } from '@fortawesome/free-solid-svg-icons';
import { faBell } from '@fortawesome/free-regular-svg-icons';
import { AuthenticationService } from '../foundation/services/authentication.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent {

    isSidebarOpened = false;
    faCalendar = faCalendar;
    faIdCard = faIdCard;
    faChartLine = faChartLine;
    faGear = faGear;
    faChartSimple = faChartSimple;
    faUsers = faUsers;
    faRightFromBracket = faRightFromBracket;
    faMagnifyingGlass = faMagnifyingGlass;
    faBell = faBell;
    faSprayCanSparkles = faSprayCanSparkles;
    faAddressBook = faAddressBook;
    faAddressCard = faAddressCard;

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
