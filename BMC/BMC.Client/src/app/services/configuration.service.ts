import { Injectable, Inject } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConfigurationService {

    public static readonly defaultHomeUrl = '/dashboard';
    public static readonly defaultLoginUrl = '/login';

    constructor(@Inject('BASE_URL') private baseUrl: string) { }

    get apiUrl(): string {
        return this.baseUrl + 'api';
    }

    get loginUrl(): string {
        return environment.loginUrl;
    }

    get homeUrl(): string {
        return ConfigurationService.defaultHomeUrl;
    }

    get isProduction(): boolean {
        return environment.production;
    }

    import(settings: string) {
        // Placeholder for user-specific configuration import from token
    }

    clearLocalChanges() {
        // Placeholder for clearing user-specific local config changes
    }
}
