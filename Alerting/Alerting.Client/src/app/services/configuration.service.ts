import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

import { AppTranslationService } from './app-translation.service';
import { LocalStoreManager } from './local-store-manager.service';
import { DBkeys } from './db-keys';
import { Utilities } from './utilities';
import { environment } from '../../environments/environment';

interface UserConfiguration {
  language: string | null;
  homeUrl: string | null;
}

@Injectable()
export class ConfigurationService {
  constructor(
    private localStorage: LocalStoreManager,
    private translationService: AppTranslationService) {

    this._globalLanguage = this.localStorage.getDataObject<string>(DBkeys.GLOBAL_LANGUAGE);
    this.loadLocalChanges();
  }

  set language(value: string | null) {
    this._language = value;
    this.saveToLocalStore(value, DBkeys.LANGUAGE);
    this.translationService.changeLanguage(value);
  }
  get language(): string {
    return this._language ?? ConfigurationService.defaultLanguage;
  }

  set globalLanguage(value: string | null) {
    this._globalLanguage = value;
    this.saveToLocalStore(value, DBkeys.GLOBAL_LANGUAGE);
  }
  get globalLanguage() {
    return this._globalLanguage;
  }
  set homeUrl(value: string | null) {
    this._homeUrl = value;
    this.saveToLocalStore(value, DBkeys.HOME_URL);
  }
  get homeUrl(): string {
    return this._homeUrl ?? ConfigurationService.defaultHomeUrl;
  }


  // ***Specify default configurations here***
  public static readonly defaultLanguage = 'en';
  public static readonly defaultHomeUrl = '/';
  public static readonly defaultShowDashboardStatistics = true;
  public static readonly defaultShowDashboardNotifications = true;
  public static readonly defaultShowDashboardTodo = false;
  public static readonly defaultShowDashboardBanner = true;

  public baseUrl = environment.baseUrl ?? Utilities.baseUrl();
  public loginUrl = environment.loginUrl;
  public googleClientId = environment.googleClientId;
  public facebookClientId = environment.facebookClientId;
  public microsoftClientId = environment.microsoftClientId;
  public fallbackBaseUrl = 'https://www.k2research.ca';
  // ***End of defaults***

  private _language: string | null = null;
  private _globalLanguage: string | null = null;
  private _homeUrl: string | null = null;
  private onConfigurationImported: Subject<void> = new Subject<void>();

  configurationImported$ = this.onConfigurationImported.asObservable();

  private loadLocalChanges() {
    if (this.localStorage.exists(DBkeys.LANGUAGE)) {
      this._language = this.localStorage.getDataObject<string>(DBkeys.LANGUAGE);
      this.translationService.changeLanguage(this._language);
    } else {
      this.resetLanguage();
    }

    if (this.localStorage.exists(DBkeys.HOME_URL)) {
      this._homeUrl = this.localStorage.getDataObject<string>(DBkeys.HOME_URL);
    }
  }

  private saveToLocalStore(data: unknown, key: string) {
    setTimeout(() => this.localStorage.savePermanentData(data, key));
  }

  public import(jsonValue: string | null) {
    this.clearLocalChanges();

    if (jsonValue) {
      const importValue: UserConfiguration = Utilities.JsonTryParse(jsonValue);

      if (importValue.language != null) {
        this.language = importValue.language;
      }

      if (importValue.homeUrl != null) {
        this.homeUrl = importValue.homeUrl;
      }
    }

    this.onConfigurationImported.next();
  }

  public export(changesOnly = true): string {
    const exportValue: UserConfiguration = {
      language: changesOnly ? this._language : this.language,
      homeUrl: changesOnly ? this._homeUrl : this.homeUrl,
    };

    return JSON.stringify(exportValue);
  }

  public clearLocalChanges() {
    this._language = null;
    this._homeUrl = null;

    this.localStorage.deleteData(DBkeys.LANGUAGE);
    this.localStorage.deleteData(DBkeys.HOME_URL);

    this.resetLanguage();
    this.clearUserConfigKeys();
  }

  private resetLanguage() {

    if (this._globalLanguage) {
      this._language = this.translationService.changeLanguage(this._globalLanguage);
    } else {
      const language = this.translationService.useBrowserLanguage();

      if (language) {
        this._language = language;
      } else {
        this._language = this.translationService.useDefaultLanguage();
      }
    }
  }

  private addKeyToUserConfigKeys(configKey: string) {
    const configKeys = this.localStorage.getDataObject<string[]>(DBkeys.USER_CONFIG_KEYS) ?? [];

    if (!configKeys.includes(configKey)) {
      configKeys.push(configKey);
      this.localStorage.savePermanentData(configKeys, DBkeys.USER_CONFIG_KEYS);
    }
  }

  private clearUserConfigKeys() {
    const configKeys = this.localStorage.getDataObject<string[]>(DBkeys.USER_CONFIG_KEYS);

    if (configKeys != null && configKeys.length > 0) {
      for (const key of configKeys) {
        this.localStorage.deleteData(key);
      }

      this.localStorage.deleteData(DBkeys.USER_CONFIG_KEYS);
    }
  }

  public saveConfiguration(data: unknown, configKey: string) {
    this.addKeyToUserConfigKeys(configKey);
    this.localStorage.savePermanentData(data, configKey);
  }

  public getConfiguration<T>(configKey: string, isDateType = false) {
    return this.localStorage.getDataObject<T>(configKey, isDateType);
  }
}
