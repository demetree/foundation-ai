import { Injectable, Inject } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConfigurationService {

    constructor(@Inject('BASE_URL') private baseUrl: string) { }

    get apiUrl(): string {
        return this.baseUrl + 'api';
    }

    get loginUrl(): string {
        return environment.loginUrl;
    }

    get isProduction(): boolean {
        return environment.production;
    }
}
