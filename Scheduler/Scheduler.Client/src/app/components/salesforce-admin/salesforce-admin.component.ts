import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';


interface SalesforceConfig {
  configured: boolean;
  id?: number;
  syncEnabled: boolean;
  syncDirectionFlags: string;
  pullIntervalMinutes: number;
  lastPullDate?: string;
  loginUrl: string;
  sfClientId: string;
  sfUsername: string;
  apiVersion: string;
  instanceUrl?: string;
  hasClientSecret: boolean;
  hasPassword: boolean;
  hasSecurityToken: boolean;
  versionNumber?: number;
}

interface QueueStatus {
  pending: number;
  inProgress: number;
  completed: number;
  failed: number;
  abandoned: number;
  total: number;
  recentFailed: any[];
}


@Component({
  selector: 'app-salesforce-admin',
  templateUrl: './salesforce-admin.component.html',
  styleUrl: './salesforce-admin.component.scss'
})
export class SalesforceAdminComponent implements OnInit {

  //
  // Configuration form fields
  //
  public syncEnabled = false;
  public syncDirectionFlags = 'None';
  public pullIntervalMinutes = 5;
  public loginUrl = 'https://login.salesforce.com/services/oauth2/token';
  public sfClientId = '';
  public sfClientSecret = '';
  public sfUsername = '';
  public sfPassword = '';
  public sfSecurityToken = '';
  public apiVersion = 'v56.0';

  //
  // State tracking
  //
  public isConfigured = false;
  public hasClientSecret = false;
  public hasPassword = false;
  public hasSecurityToken = false;
  public lastPullDate: string | null = null;
  public instanceUrl: string | null = null;

  //
  // Loading states
  //
  public isLoadingConfig = true;
  public isSavingConfig = false;
  public isTestingConnection = false;
  public isPulling = false;
  public isPullingAccounts = false;
  public isPullingContacts = false;
  public isPullingEvents = false;
  public isRetrying = false;

  //
  // Queue status
  //
  public queueStatus: QueueStatus | null = null;
  public isLoadingQueue = true;

  //
  // Sync direction options
  //
  public syncDirectionOptions = [
    { value: 'None', label: 'Disabled' },
    { value: 'ImportOnly', label: 'Import Only (SF → Scheduler)' },
    { value: 'PushOnly', label: 'Push Only (Scheduler → SF)' },
    { value: 'RealTime', label: 'Bidirectional (Real-Time)' }
  ];


  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private authService: AuthService,
    private alertService: AlertService
  ) { }


  ngOnInit(): void {
    this.loadConfig();
    this.loadQueueStatus();
  }


  private getHeaders(): HttpHeaders {
    return this.authService.GetAuthenticationHeaders();
  }


  //
  // ─── Config Management ─────────────────────
  //

  public loadConfig(): void {
    this.isLoadingConfig = true;
    const headers = this.getHeaders();

    this.http.get<SalesforceConfig>(`${this.baseUrl}api/SalesforceSync/config`, { headers }).subscribe({
      next: (config) => {
        this.isConfigured = config.configured;
        this.syncEnabled = config.syncEnabled;
        this.syncDirectionFlags = config.syncDirectionFlags;
        this.pullIntervalMinutes = config.pullIntervalMinutes;
        this.loginUrl = config.loginUrl || 'https://login.salesforce.com/services/oauth2/token';
        this.sfClientId = config.sfClientId || '';
        this.sfUsername = config.sfUsername || '';
        this.apiVersion = config.apiVersion || 'v56.0';
        this.instanceUrl = config.instanceUrl || null;
        this.lastPullDate = config.lastPullDate || null;
        this.hasClientSecret = config.hasClientSecret;
        this.hasPassword = config.hasPassword;
        this.hasSecurityToken = config.hasSecurityToken;

        // Clear password fields — they're never returned from the server
        this.sfClientSecret = '';
        this.sfPassword = '';
        this.sfSecurityToken = '';

        this.isLoadingConfig = false;
      },
      error: (err) => {
        this.alertService.showStickyMessage('Load Failed', 'Unable to load Salesforce configuration', MessageSeverity.error, err);
        this.isLoadingConfig = false;
      }
    });
  }


  public saveConfig(): void {
    this.isSavingConfig = true;
    const headers = this.getHeaders();

    const payload = {
      syncEnabled: this.syncEnabled,
      syncDirectionFlags: this.syncDirectionFlags,
      pullIntervalMinutes: this.pullIntervalMinutes,
      loginUrl: this.loginUrl,
      sfClientId: this.sfClientId,
      sfClientSecret: this.sfClientSecret,
      sfUsername: this.sfUsername,
      sfPassword: this.sfPassword,
      sfSecurityToken: this.sfSecurityToken,
      apiVersion: this.apiVersion
    };

    this.http.put<any>(`${this.baseUrl}api/SalesforceSync/config`, payload, { headers }).subscribe({
      next: (result) => {
        this.isSavingConfig = false;
        if (result.success) {
          this.isConfigured = true;
          this.hasClientSecret = this.sfClientSecret ? true : this.hasClientSecret;
          this.hasPassword = this.sfPassword ? true : this.hasPassword;
          this.hasSecurityToken = this.sfSecurityToken ? true : this.hasSecurityToken;

          // Clear secret fields after save
          this.sfClientSecret = '';
          this.sfPassword = '';
          this.sfSecurityToken = '';

          this.alertService.showMessage('Configuration saved successfully', '', MessageSeverity.success);
        } else {
          this.alertService.showMessage('Save Failed', result.error || 'Unknown error', MessageSeverity.error);
        }
      },
      error: (err) => {
        this.isSavingConfig = false;
        this.alertService.showStickyMessage('Save Failed', 'Unable to save Salesforce configuration', MessageSeverity.error, err);
      }
    });
  }


  //
  // ─── Connection Testing ─────────────────────
  //

  public testConnection(): void {
    this.isTestingConnection = true;
    const headers = this.getHeaders();

    this.http.post<any>(`${this.baseUrl}api/SalesforceSync/testConnection`, {}, { headers }).subscribe({
      next: (result) => {
        this.isTestingConnection = false;
        if (result.success) {
          this.instanceUrl = result.instanceUrl;
          this.alertService.showMessage('Connection Successful', result.message, MessageSeverity.success);
        } else {
          this.alertService.showMessage('Connection Failed', result.error, MessageSeverity.error);
        }
      },
      error: (err) => {
        this.isTestingConnection = false;
        this.alertService.showStickyMessage('Connection Test Failed', 'Unable to reach Salesforce', MessageSeverity.error, err);
      }
    });
  }


  //
  // ─── Sync Operations ─────────────────────
  //

  public pullAll(): void {
    this.isPulling = true;
    this.executePull('pullAll', () => { this.isPulling = false; });
  }

  public pullAccounts(): void {
    this.isPullingAccounts = true;
    this.executePull('pullAccounts', () => { this.isPullingAccounts = false; });
  }

  public pullContacts(): void {
    this.isPullingContacts = true;
    this.executePull('pullContacts', () => { this.isPullingContacts = false; });
  }

  public pullEvents(): void {
    this.isPullingEvents = true;
    this.executePull('pullEvents', () => { this.isPullingEvents = false; });
  }

  private executePull(endpoint: string, doneCallback: () => void): void {
    const headers = this.getHeaders();

    this.http.post<any>(`${this.baseUrl}api/SalesforceSync/${endpoint}`, {}, { headers }).subscribe({
      next: (result) => {
        doneCallback();
        if (result.success) {
          this.alertService.showMessage(
            'Sync Complete',
            `Created: ${result.created}, Updated: ${result.updated}, Errors: ${result.errors}`,
            MessageSeverity.success
          );
          this.loadQueueStatus();
        } else {
          this.alertService.showMessage('Sync Failed', result.error, MessageSeverity.error);
        }
      },
      error: (err) => {
        doneCallback();
        this.alertService.showStickyMessage('Sync Failed', 'Error during sync operation', MessageSeverity.error, err);
      }
    });
  }


  //
  // ─── Queue Management ─────────────────────
  //

  public loadQueueStatus(): void {
    this.isLoadingQueue = true;
    const headers = this.getHeaders();

    this.http.get<QueueStatus>(`${this.baseUrl}api/SalesforceSync/queueStatus`, { headers }).subscribe({
      next: (status) => {
        this.queueStatus = status;
        this.isLoadingQueue = false;
      },
      error: (err) => {
        this.isLoadingQueue = false;
        this.alertService.showStickyMessage('Queue Error', 'Unable to load sync queue status', MessageSeverity.error, err);
      }
    });
  }


  public retryFailed(): void {
    this.isRetrying = true;
    const headers = this.getHeaders();

    this.http.post<any>(`${this.baseUrl}api/SalesforceSync/retryFailed`, {}, { headers }).subscribe({
      next: (result) => {
        this.isRetrying = false;
        if (result.success) {
          this.alertService.showMessage('Retry Queued', `${result.resetCount} failed items reset to Pending`, MessageSeverity.success);
          this.loadQueueStatus();
        }
      },
      error: (err) => {
        this.isRetrying = false;
        this.alertService.showStickyMessage('Retry Failed', 'Unable to retry failed items', MessageSeverity.error, err);
      }
    });
  }
}
