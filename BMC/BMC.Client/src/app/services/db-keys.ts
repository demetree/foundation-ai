import { Injectable } from '@angular/core';

@Injectable()
export class DBkeys {
    public static readonly CURRENT_USER = 'bmc_current_user';
    public static readonly USER_ROLES = 'bmc_user_roles';
    public static readonly ACCESS_TOKEN = 'bmc_access_token';
    public static readonly REFRESH_TOKEN = 'bmc_refresh_token';
    public static readonly TOKEN_EXPIRES_IN = 'bmc_expires_in';
    public static readonly REMEMBER_ME = 'bmc_remember_me';
    public static readonly HOME_URL = 'bmc_home_url';
}
