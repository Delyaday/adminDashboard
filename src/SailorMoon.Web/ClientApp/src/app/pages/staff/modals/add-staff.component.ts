import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { StaffModel } from '../../../models/staff.model';
import { UserDescriptionItem } from '../../../models/user-description.model';
import { validatePhone } from '../../../utils';


@Component({
    selector: 'add-staff-modal',
    templateUrl: './add-staff.component.html',
    styleUrls: ['./add-staff.component.scss'],

})
export class AddStaffComponent {

    faUserPlus = faUserPlus;

    public password: string;
    public data: StaffModel = {
        description: {
            login: '',
            position: ''
        },
        login: '',
        firstName: '',
        lastName: ''
    }

    constructor(public dialogRef: MatDialogRef<AddStaffComponent>) {

    }

    isValid() {
        return !!this.data.login && !!this.data.firstName;
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    getDateFormatString(): string {

        return 'DD/MM/YYYY';

    }

}
