import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { faUserPlus, faWandMagicSparkles } from '@fortawesome/free-solid-svg-icons';
import { ServiceItem } from '../../../models/services.model';


@Component({
    selector: 'add-service-modal',
    templateUrl: './add-service.component.html',
    styleUrls: ['./add-service.component.scss'],

})
export class AddServiceComponent {

    faUserPlus = faUserPlus;
    faWandMagicSparkles = faWandMagicSparkles;

    constructor(public dialogRef: MatDialogRef<AddServiceComponent>, @Inject(MAT_DIALOG_DATA) public data: ServiceItem) {
        if (!data)
            this.data = {
                title: ''
            };
    }

    onNoClick(): void {
        this.dialogRef.close();
    }


}
