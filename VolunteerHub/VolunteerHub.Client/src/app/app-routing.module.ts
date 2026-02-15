import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HubLoginComponent } from './components/hub-login/hub-login.component';
import { HubShellComponent } from './components/hub-shell/hub-shell.component';
import { HubDashboardComponent } from './components/hub-dashboard/hub-dashboard.component';
import { HubScheduleComponent } from './components/hub-schedule/hub-schedule.component';
import { HubHoursComponent } from './components/hub-hours/hub-hours.component';
import { HubProfileComponent } from './components/hub-profile/hub-profile.component';
import { HubAuthGuard } from './guards/hub-auth.guard';

const routes: Routes = [
    { path: 'login', component: HubLoginComponent },
    {
        path: '',
        component: HubShellComponent,
        canActivate: [HubAuthGuard],
        children: [
            { path: 'dashboard', component: HubDashboardComponent },
            { path: 'schedule', component: HubScheduleComponent },
            { path: 'hours', component: HubHoursComponent },
            { path: 'profile', component: HubProfileComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    },
    { path: '**', redirectTo: '' }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }
