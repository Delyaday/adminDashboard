import { Component, OnInit } from '@angular/core';
import { faPlus, faUsers } from '@fortawesome/free-solid-svg-icons';
import { MatDialog } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { AddStaffComponent } from './modals/add-staff.component';
import { EditStaffComponent } from './modals/edit-staff.component';
import { StaffService } from '../../services/staff.service';
import * as _ from 'lodash';
import { StaffModel } from '../../models/staff.model';


@Component({
    selector: 'staff-page',
    templateUrl: './staff.component.html',
    styleUrls: ['./staff.component.scss'],
})
export class StaffComponent implements OnInit {
    faPlus = faPlus;
    faUsers = faUsers;

    public dataSource: MatTableDataSource<StaffModel> = new MatTableDataSource();

    displayedColumns: string[] = ['login', 'phone', 'position', 'accessLevel', 'description', 'actions'];


    days = ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"];
    months = ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"];

    todayDate: Date = new Date();

    fullDate = this.days[this.todayDate.getDay()] + ", " + this.todayDate.getDate() + " " + this.months[this.todayDate.getMonth()] +
        ", " + this.todayDate.getFullYear();


    constructor(public dialog: MatDialog, private _service: StaffService) {

    }

    async ngOnInit() {
        var users = await this._service.getAll();

        if (users)
            this.dataSource.data = users;
    }

    async deleteStaff(staff: StaffModel) {
        var result = await this._service.delete(staff);

        if (result)
            this.dataSource.data = this.dataSource.data.filter(f => f.id !== staff.id);
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value;
        this.dataSource.filter = filterValue.trim().toLowerCase();
    }

    openDialog(): void {
        var dialogRef = this.dialog.open(AddStaffComponent, {
            width: '25vw',
            //data: _.cloneDeep(staff),
            disableClose: true,
            autoFocus: true,

        });

        dialogRef.afterClosed().subscribe(async (result: { staff: StaffModel, password: string }) => {
            if (result && result.staff.login != '' && result.password != '') {
                var staff = await this._service.create(result.staff, result.password);

                if (staff) {
                    this.dataSource.data = [...this.dataSource.data, staff];
                }

            }
        });
    }

    openEditDialog(staff: StaffModel): void {
        var dialogRef = this.dialog.open(EditStaffComponent, {
            width: '25vw',
            data: _.cloneDeep(staff),
            disableClose: true,
            autoFocus: true
        });

        dialogRef.afterClosed().subscribe(async (result: { staff: StaffModel }) => {
            if (result && result.staff.login != '') {
                var staff = await this._service.update(result.staff);

                if (staff) {

                    var data = this.dataSource.data;

                    var item = _.find(data, f => f.id == staff?.id);

                    if (item) {
                        _.assign(item, staff);
                        this.dataSource.data = data;
                    }
                }

            }
        });
    }
}
