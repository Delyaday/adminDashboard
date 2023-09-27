import { Component, OnInit } from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { BACKEND_SETTINGS } from '../../foundation/services/backend.service';

@Component({
    selector: 'messaging-page',
    templateUrl: './messaging.component.html',
    styleUrls: ['./messaging.component.scss']
})
export class MessagingComponent implements OnInit {

    constructor(private _http: HttpClient) {
    }

    async ngOnInit() {


    }

    async send() {
        var options = {
            messages: [{
                recipient: '89622660483',
                text: 'Привет! Это test!'
            }],
            //source: 'Lenium'
        };

        await this._http.post(`${BACKEND_SETTINGS.apiUrl}/messaging/sms/message/send`, options).toPromise();
    }

}
