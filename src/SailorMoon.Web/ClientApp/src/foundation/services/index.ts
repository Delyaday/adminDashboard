import { BACKEND_SETTINGS } from './backend.service';
import { UserService } from './user.service';
import { AuthenticationService } from './authentication.service'

export const services = {
    BACKEND_SETTINGS,
    UserService,
    AuthenticationService
};

export * from './backend.service';
export * from './user.service';
export * from './authentication.service'