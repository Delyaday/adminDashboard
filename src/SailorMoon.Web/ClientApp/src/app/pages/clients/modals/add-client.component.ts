import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { ClientItem } from '../../../models/clients.model';
import { validatePhone } from '../../../utils';


@Component({
    selector: 'add-client-modal',
    templateUrl: './add-client.component.html',
    styleUrls: ['./add-client.component.scss'],

})
export class AddClientComponent {

    faUserPlus = faUserPlus;

    constructor(public dialogRef: MatDialogRef<AddClientComponent>, @Inject(MAT_DIALOG_DATA) public data: ClientItem) {
        if (!data)
            this.data = {
                firstName: '', phone: ''
            };
    }

    isValid() {
        return !!this.data.firstName && !!this.data.phone;
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    getDateFormatString(): string {

        return 'DD/MM/YYYY';

    }

}
