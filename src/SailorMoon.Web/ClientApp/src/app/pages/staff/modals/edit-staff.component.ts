import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { StaffModel } from '../../../models/staff.model';
import { UserDescriptionItem } from '../../../models/user-description.model';
import { validatePhone } from '../../../utils';


@Component({
    selector: 'edit-staff-modal',
    templateUrl: './edit-staff.component.html',
    styleUrls: ['./edit-staff.component.scss'],

})
export class EditStaffComponent {

    faUserPlus = faUserPlus;

    constructor(
        public dialogRef: MatDialogRef<EditStaffComponent>,
        @Inject(MAT_DIALOG_DATA) public data: StaffModel) {
        if (!data.description) {
            data.description = {
                login: data.login,
                position: ""
            };
;
        }
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
