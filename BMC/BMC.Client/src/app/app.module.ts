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


//
// Custom confirmation dialog
//
import { ConfirmationService } from './services/confirmation-service';
import { ConfirmationDialogComponent } from './services/confirmation-dialog/confirmation-dialog.component';

import { InputDialogService } from './services/input-dialog.service';
import { InputDialogComponent } from './services/input-dialog/input-dialog.component';

import { NavigationService } from './utility-services/navigation.service';


//
// Beginning of imports for BMC Data Services 
//
import { BMCDataServiceManagerService } from './bmc-data-services/bmc-data-service-manager.service';
import { BrickCategoryService } from './bmc-data-services/brick-category.service';
import { BrickColourService } from './bmc-data-services/brick-colour.service';
import { BrickConnectionService } from './bmc-data-services/brick-connection.service';
import { BrickPartService } from './bmc-data-services/brick-part.service';
import { BrickPartChangeHistoryService } from './bmc-data-services/brick-part-change-history.service';
import { BrickPartColourService } from './bmc-data-services/brick-part-colour.service';
import { BrickPartConnectorService } from './bmc-data-services/brick-part-connector.service';
import { ColourFinishService } from './bmc-data-services/colour-finish.service';
import { ConnectorTypeService } from './bmc-data-services/connector-type.service';
import { PartTypeService } from './bmc-data-services/part-type.service';
import { PlacedBrickService } from './bmc-data-services/placed-brick.service';
import { PlacedBrickChangeHistoryService } from './bmc-data-services/placed-brick-change-history.service';
import { ProjectService } from './bmc-data-services/project.service';
import { ProjectChangeHistoryService } from './bmc-data-services/project-change-history.service';
//
// End of imports for BMC Data Services
//

//
// Beginning of imports for BMC Data Components
//
import { BrickCategoryListingComponent } from './bmc-data-components/brick-category/brick-category-listing/brick-category-listing.component';
import { BrickCategoryAddEditComponent } from './bmc-data-components/brick-category/brick-category-add-edit/brick-category-add-edit.component';
import { BrickCategoryDetailComponent } from './bmc-data-components/brick-category/brick-category-detail/brick-category-detail.component';
import { BrickCategoryTableComponent } from './bmc-data-components/brick-category/brick-category-table/brick-category-table.component';
import { BrickColourListingComponent } from './bmc-data-components/brick-colour/brick-colour-listing/brick-colour-listing.component';
import { BrickColourAddEditComponent } from './bmc-data-components/brick-colour/brick-colour-add-edit/brick-colour-add-edit.component';
import { BrickColourDetailComponent } from './bmc-data-components/brick-colour/brick-colour-detail/brick-colour-detail.component';
import { BrickColourTableComponent } from './bmc-data-components/brick-colour/brick-colour-table/brick-colour-table.component';
import { BrickConnectionListingComponent } from './bmc-data-components/brick-connection/brick-connection-listing/brick-connection-listing.component';
import { BrickConnectionAddEditComponent } from './bmc-data-components/brick-connection/brick-connection-add-edit/brick-connection-add-edit.component';
import { BrickConnectionDetailComponent } from './bmc-data-components/brick-connection/brick-connection-detail/brick-connection-detail.component';
import { BrickConnectionTableComponent } from './bmc-data-components/brick-connection/brick-connection-table/brick-connection-table.component';
import { BrickPartListingComponent } from './bmc-data-components/brick-part/brick-part-listing/brick-part-listing.component';
import { BrickPartAddEditComponent } from './bmc-data-components/brick-part/brick-part-add-edit/brick-part-add-edit.component';
import { BrickPartDetailComponent } from './bmc-data-components/brick-part/brick-part-detail/brick-part-detail.component';
import { BrickPartTableComponent } from './bmc-data-components/brick-part/brick-part-table/brick-part-table.component';
import { BrickPartChangeHistoryListingComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-listing/brick-part-change-history-listing.component';
import { BrickPartChangeHistoryAddEditComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-add-edit/brick-part-change-history-add-edit.component';
import { BrickPartChangeHistoryDetailComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-detail/brick-part-change-history-detail.component';
import { BrickPartChangeHistoryTableComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-table/brick-part-change-history-table.component';
import { BrickPartColourListingComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-listing/brick-part-colour-listing.component';
import { BrickPartColourAddEditComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-add-edit/brick-part-colour-add-edit.component';
import { BrickPartColourDetailComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-detail/brick-part-colour-detail.component';
import { BrickPartColourTableComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-table/brick-part-colour-table.component';
import { BrickPartConnectorListingComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-listing/brick-part-connector-listing.component';
import { BrickPartConnectorAddEditComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-add-edit/brick-part-connector-add-edit.component';
import { BrickPartConnectorDetailComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-detail/brick-part-connector-detail.component';
import { BrickPartConnectorTableComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-table/brick-part-connector-table.component';
import { ColourFinishListingComponent } from './bmc-data-components/colour-finish/colour-finish-listing/colour-finish-listing.component';
import { ColourFinishAddEditComponent } from './bmc-data-components/colour-finish/colour-finish-add-edit/colour-finish-add-edit.component';
import { ColourFinishDetailComponent } from './bmc-data-components/colour-finish/colour-finish-detail/colour-finish-detail.component';
import { ColourFinishTableComponent } from './bmc-data-components/colour-finish/colour-finish-table/colour-finish-table.component';
import { ConnectorTypeListingComponent } from './bmc-data-components/connector-type/connector-type-listing/connector-type-listing.component';
import { ConnectorTypeAddEditComponent } from './bmc-data-components/connector-type/connector-type-add-edit/connector-type-add-edit.component';
import { ConnectorTypeDetailComponent } from './bmc-data-components/connector-type/connector-type-detail/connector-type-detail.component';
import { ConnectorTypeTableComponent } from './bmc-data-components/connector-type/connector-type-table/connector-type-table.component';
import { PartTypeListingComponent } from './bmc-data-components/part-type/part-type-listing/part-type-listing.component';
import { PartTypeAddEditComponent } from './bmc-data-components/part-type/part-type-add-edit/part-type-add-edit.component';
import { PartTypeDetailComponent } from './bmc-data-components/part-type/part-type-detail/part-type-detail.component';
import { PartTypeTableComponent } from './bmc-data-components/part-type/part-type-table/part-type-table.component';
import { PlacedBrickListingComponent } from './bmc-data-components/placed-brick/placed-brick-listing/placed-brick-listing.component';
import { PlacedBrickAddEditComponent } from './bmc-data-components/placed-brick/placed-brick-add-edit/placed-brick-add-edit.component';
import { PlacedBrickDetailComponent } from './bmc-data-components/placed-brick/placed-brick-detail/placed-brick-detail.component';
import { PlacedBrickTableComponent } from './bmc-data-components/placed-brick/placed-brick-table/placed-brick-table.component';
import { PlacedBrickChangeHistoryListingComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-listing/placed-brick-change-history-listing.component';
import { PlacedBrickChangeHistoryAddEditComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-add-edit/placed-brick-change-history-add-edit.component';
import { PlacedBrickChangeHistoryDetailComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-detail/placed-brick-change-history-detail.component';
import { PlacedBrickChangeHistoryTableComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-table/placed-brick-change-history-table.component';
import { ProjectListingComponent } from './bmc-data-components/project/project-listing/project-listing.component';
import { ProjectAddEditComponent } from './bmc-data-components/project/project-add-edit/project-add-edit.component';
import { ProjectDetailComponent } from './bmc-data-components/project/project-detail/project-detail.component';
import { ProjectTableComponent } from './bmc-data-components/project/project-table/project-table.component';
import { ProjectChangeHistoryListingComponent } from './bmc-data-components/project-change-history/project-change-history-listing/project-change-history-listing.component';
import { ProjectChangeHistoryAddEditComponent } from './bmc-data-components/project-change-history/project-change-history-add-edit/project-change-history-add-edit.component';
import { ProjectChangeHistoryDetailComponent } from './bmc-data-components/project-change-history/project-change-history-detail/project-change-history-detail.component';
import { ProjectChangeHistoryTableComponent } from './bmc-data-components/project-change-history/project-change-history-table/project-change-history-table.component';
//
// End of imports for BMC Data Components
//


@NgModule({
    declarations: [
        AppComponent,
        HeaderComponent,
        SidebarComponent,
        LoginComponent,
        DashboardComponent,
        NotFoundComponent,


        ConfirmationDialogComponent,
        InputDialogComponent,


        //
        // Beginning of declarations for BMC Data Components
        //
        BrickCategoryListingComponent,
        BrickCategoryAddEditComponent,
        BrickCategoryDetailComponent,
        BrickCategoryTableComponent,
        BrickColourListingComponent,
        BrickColourAddEditComponent,
        BrickColourDetailComponent,
        BrickColourTableComponent,
        BrickConnectionListingComponent,
        BrickConnectionAddEditComponent,
        BrickConnectionDetailComponent,
        BrickConnectionTableComponent,
        BrickPartListingComponent,
        BrickPartAddEditComponent,
        BrickPartDetailComponent,
        BrickPartTableComponent,
        BrickPartChangeHistoryListingComponent,
        BrickPartChangeHistoryAddEditComponent,
        BrickPartChangeHistoryDetailComponent,
        BrickPartChangeHistoryTableComponent,
        BrickPartColourListingComponent,
        BrickPartColourAddEditComponent,
        BrickPartColourDetailComponent,
        BrickPartColourTableComponent,
        BrickPartConnectorListingComponent,
        BrickPartConnectorAddEditComponent,
        BrickPartConnectorDetailComponent,
        BrickPartConnectorTableComponent,
        ColourFinishListingComponent,
        ColourFinishAddEditComponent,
        ColourFinishDetailComponent,
        ColourFinishTableComponent,
        ConnectorTypeListingComponent,
        ConnectorTypeAddEditComponent,
        ConnectorTypeDetailComponent,
        ConnectorTypeTableComponent,
        PartTypeListingComponent,
        PartTypeAddEditComponent,
        PartTypeDetailComponent,
        PartTypeTableComponent,
        PlacedBrickListingComponent,
        PlacedBrickAddEditComponent,
        PlacedBrickDetailComponent,
        PlacedBrickTableComponent,
        PlacedBrickChangeHistoryListingComponent,
        PlacedBrickChangeHistoryAddEditComponent,
        PlacedBrickChangeHistoryDetailComponent,
        PlacedBrickChangeHistoryTableComponent,
        ProjectListingComponent,
        ProjectAddEditComponent,
        ProjectDetailComponent,
        ProjectTableComponent,
        ProjectChangeHistoryListingComponent,
        ProjectChangeHistoryAddEditComponent,
        ProjectChangeHistoryDetailComponent,
        ProjectChangeHistoryTableComponent,
        //
        // End of declarations for BMC Data Components
        //


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

        ConfirmationService,
        InputDialogService,
        NavigationService,

        //
        // Beginning of provider declarations for BMC Data Services 
        //
        BMCDataServiceManagerService,
        BrickCategoryService,
        BrickColourService,
        BrickConnectionService,
        BrickPartService,
        BrickPartChangeHistoryService,
        BrickPartColourService,
        BrickPartConnectorService,
        ColourFinishService,
        ConnectorTypeService,
        PartTypeService,
        PlacedBrickService,
        PlacedBrickChangeHistoryService,
        ProjectService,
        ProjectChangeHistoryService,
        //
        // End of provider declarations for BMC Data Services
        //


    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
