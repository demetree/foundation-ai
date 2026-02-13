import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NotFoundComponent } from './components/not-found/not-found.component';

import { AuthGuard } from './services/auth-guard';

const routes: Routes = [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'login', component: LoginComponent, data: { title: 'Login' } },
    { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard], data: { title: 'Dashboard' } },
    { path: '**', component: NotFoundComponent, data: { title: 'Page Not Found' } }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }
