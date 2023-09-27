import { User } from '../../foundation/models/user.model';
import { UserDescriptionItem } from './user-description.model';

export interface StaffModel extends User {
    description: UserDescriptionItem;
}