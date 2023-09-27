import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { AuthenticationService } from '../../foundation/';

@Component({
    selector: 'login-page',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

    isInitialized = false;

    login: string;
    password: string;

    returnUrl: string;

    isLoading = false;

    constructor(
        private _route: ActivatedRoute,
        private _router: Router,
        private _authenticationService: AuthenticationService
    ) {
        if (_authenticationService.user) {
            _router.navigate(['/'])
        }
    }

    ngOnInit(): void {
        setTimeout(() => {
            this.isInitialized = true;
        }, 500);

        this.returnUrl = this._route.snapshot.queryParams['returnUrl'] || '/';
    }

    async doLogin() {
        if (!this.login || !this.password || this.login == '' || this.password == '')
            return;

        this.isLoading = true;

        try {

            await this._authenticationService.login(this.login, this.password).toPromise();

            this._router.navigate([this.returnUrl]);

        } catch (e: any) {
            alert(e.toString());
        } finally {
            this.isLoading = false;
        }
    }
}
