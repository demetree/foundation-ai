import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ToastaModule } from 'ngx-toasta';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NotFoundComponent } from './components/not-found/not-found.component';

import { AuthService } from './services/auth.service';
import { AlertService } from './services/alert.service';
import { ConfigurationService } from './services/configuration.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { OidcHelperService } from './services/oidc-helper.service';
import { AppTitleService } from './services/app-title.service';
import { DBkeys } from './services/db-keys';
import { JwtHelper } from './services/jwt-helper';


@NgModule({
    declarations: [
        AppComponent,
        HeaderComponent,
        SidebarComponent,
        LoginComponent,
        DashboardComponent,
        NotFoundComponent,
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        NgbModule,
        ToastaModule.forRoot(),
        AppRoutingModule,
    ],
    providers: [
        AuthService,
        AlertService,
        ConfigurationService,
        LocalStoreManager,
        OidcHelperService,
        AppTitleService,
        DBkeys,
        JwtHelper,
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
