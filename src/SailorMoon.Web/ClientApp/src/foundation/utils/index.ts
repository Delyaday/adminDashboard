import { appInitializer } from './app.initializer';
import { AuthGuard } from './auth.guard';
import { ErrorInterceptor } from './error.interceptor';
import { JwtInterceptor } from './jwt.interceptor';
import { APIInterceptor } from './api.interceptor';

export const utils = {
    appInitializer,
    AuthGuard,
    ErrorInterceptor,
    JwtInterceptor,
    APIInterceptor
};