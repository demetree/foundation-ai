import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Components
import { HubLoginComponent } from './components/hub-login/hub-login.component';
import { HubShellComponent } from './components/hub-shell/hub-shell.component';
import { HubDashboardComponent } from './components/hub-dashboard/hub-dashboard.component';
import { HubScheduleComponent } from './components/hub-schedule/hub-schedule.component';
import { HubHoursComponent } from './components/hub-hours/hub-hours.component';
import { HubProfileComponent } from './components/hub-profile/hub-profile.component';

@NgModule({
    declarations: [
        AppComponent,
        HubLoginComponent,
        HubShellComponent,
        HubDashboardComponent,
        HubScheduleComponent,
        HubHoursComponent,
        HubProfileComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        AppRoutingModule
    ],
    providers: [],
    bootstrap: [AppComponent]
})
export class AppModule { }
