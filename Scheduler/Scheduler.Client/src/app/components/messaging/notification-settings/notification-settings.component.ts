import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MessagingApiService, NotificationProfile, NotificationProfileUpdate } from '../../../services/messaging-api.service';

@Component({
  selector: 'app-notification-settings',
  templateUrl: './notification-settings.component.html',
  styleUrls: ['./notification-settings.component.scss']
})
export class NotificationSettingsComponent implements OnInit {

  @Output() closed = new EventEmitter<void>();
  @Output() profileUpdated = new EventEmitter<NotificationProfile>();

  //
  // Form state
  //
  profile: NotificationProfile | null = null;
  isLoading = true;
  isSaving = false;
  saveMessage: string | null = null;
  saveMessageType: 'success' | 'error' = 'success';

  // Editable fields (bound to form)
  email = '';
  phone = '';
  emailEnabled = false;
  smsEnabled = false;
  emailPreference = 'mentions';
  smsPreference = 'mentions';
  quietHoursEnabled = false;
  quietHoursStart = '22:00';
  quietHoursEnd = '07:00';
  timezone = 'America/St_Johns';

  // Common timezone options
  timezones = [
    { value: 'America/St_Johns', label: 'Newfoundland (NST)' },
    { value: 'America/Halifax', label: 'Atlantic (AST)' },
    { value: 'America/Toronto', label: 'Eastern (EST)' },
    { value: 'America/Chicago', label: 'Central (CST)' },
    { value: 'America/Denver', label: 'Mountain (MST)' },
    { value: 'America/Los_Angeles', label: 'Pacific (PST)' },
    { value: 'America/Vancouver', label: 'Pacific - Vancouver' },
    { value: 'Europe/London', label: 'London (GMT)' },
    { value: 'Europe/Paris', label: 'Central Europe (CET)' },
    { value: 'Australia/Sydney', label: 'Sydney (AEST)' },
    { value: 'UTC', label: 'UTC' }
  ];

  // Preference options
  preferenceOptions = [
    { value: 'all', label: 'All messages' },
    { value: 'mentions', label: 'Mentions only' },
    { value: 'none', label: 'None' }
  ];

  constructor(private messagingApi: MessagingApiService) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.messagingApi.getNotificationProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.populateForm(profile);
        this.isLoading = false;
      },
      error: () => {
        // If no profile exists yet, use defaults
        this.isLoading = false;
      }
    });
  }

  private populateForm(profile: NotificationProfile): void {
    this.email = profile.email || '';
    this.phone = profile.phone || '';
    this.emailEnabled = profile.emailEnabled;
    this.smsEnabled = profile.smsEnabled;
    this.emailPreference = profile.emailPreference || 'mentions';
    this.smsPreference = profile.smsPreference || 'mentions';
    this.quietHoursEnabled = profile.quietHoursEnabled;
    this.quietHoursStart = profile.quietHoursStart || '22:00';
    this.quietHoursEnd = profile.quietHoursEnd || '07:00';
    this.timezone = profile.timezone || 'America/St_Johns';
  }

  saveProfile(): void {
    this.isSaving = true;
    this.saveMessage = null;

    const update: NotificationProfileUpdate = {
      email: this.email || null,
      phone: this.phone || null,
      emailEnabled: this.emailEnabled,
      smsEnabled: this.smsEnabled,
      emailPreference: this.emailPreference,
      smsPreference: this.smsPreference,
      quietHoursEnabled: this.quietHoursEnabled,
      quietHoursStart: this.quietHoursStart,
      quietHoursEnd: this.quietHoursEnd,
      timezone: this.timezone
    };

    this.messagingApi.updateNotificationProfile(update).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.isSaving = false;
        this.saveMessage = 'Settings saved';
        this.saveMessageType = 'success';
        this.profileUpdated.emit(updated);

        setTimeout(() => this.saveMessage = null, 3000);
      },
      error: () => {
        this.isSaving = false;
        this.saveMessage = 'Failed to save settings';
        this.saveMessageType = 'error';

        setTimeout(() => this.saveMessage = null, 5000);
      }
    });
  }

  close(): void {
    this.closed.emit();
  }

  get hasChanges(): boolean {
    if (!this.profile) return true; // new profile
    return this.email !== (this.profile.email || '')
        || this.phone !== (this.profile.phone || '')
        || this.emailEnabled !== this.profile.emailEnabled
        || this.smsEnabled !== this.profile.smsEnabled
        || this.emailPreference !== (this.profile.emailPreference || 'mentions')
        || this.smsPreference !== (this.profile.smsPreference || 'mentions')
        || this.quietHoursEnabled !== this.profile.quietHoursEnabled
        || this.quietHoursStart !== (this.profile.quietHoursStart || '22:00')
        || this.quietHoursEnd !== (this.profile.quietHoursEnd || '07:00')
        || this.timezone !== (this.profile.timezone || 'America/St_Johns');
  }

  //
  // Simple validation helpers
  //
  get isEmailValid(): boolean {
    return !this.email || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email);
  }

  get isPhoneValid(): boolean {
    return !this.phone || /^[\d\s\-+().]{7,20}$/.test(this.phone);
  }
}
