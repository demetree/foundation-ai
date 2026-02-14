import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NotFoundComponent } from './components/not-found/not-found.component';

import { AuthGuard } from './services/auth-guard';


import { UnsavedChangesGuard } from './guards/unsaved-changes.guard';

//
// Beginning of imports for BMC Data Components
//
import { BrickCategoryListingComponent } from './bmc-data-components/brick-category/brick-category-listing/brick-category-listing.component';
import { BrickCategoryDetailComponent } from './bmc-data-components/brick-category/brick-category-detail/brick-category-detail.component';
import { BrickColourListingComponent } from './bmc-data-components/brick-colour/brick-colour-listing/brick-colour-listing.component';
import { BrickColourDetailComponent } from './bmc-data-components/brick-colour/brick-colour-detail/brick-colour-detail.component';
import { BrickConnectionListingComponent } from './bmc-data-components/brick-connection/brick-connection-listing/brick-connection-listing.component';
import { BrickConnectionDetailComponent } from './bmc-data-components/brick-connection/brick-connection-detail/brick-connection-detail.component';
import { BrickPartListingComponent } from './bmc-data-components/brick-part/brick-part-listing/brick-part-listing.component';
import { BrickPartDetailComponent } from './bmc-data-components/brick-part/brick-part-detail/brick-part-detail.component';
import { BrickPartChangeHistoryListingComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-listing/brick-part-change-history-listing.component';
import { BrickPartChangeHistoryDetailComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-detail/brick-part-change-history-detail.component';
import { BrickPartColourListingComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-listing/brick-part-colour-listing.component';
import { BrickPartColourDetailComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-detail/brick-part-colour-detail.component';
import { BrickPartConnectorListingComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-listing/brick-part-connector-listing.component';
import { BrickPartConnectorDetailComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-detail/brick-part-connector-detail.component';
import { ColourFinishListingComponent } from './bmc-data-components/colour-finish/colour-finish-listing/colour-finish-listing.component';
import { ColourFinishDetailComponent } from './bmc-data-components/colour-finish/colour-finish-detail/colour-finish-detail.component';
import { ConnectorTypeListingComponent } from './bmc-data-components/connector-type/connector-type-listing/connector-type-listing.component';
import { ConnectorTypeDetailComponent } from './bmc-data-components/connector-type/connector-type-detail/connector-type-detail.component';
import { PartTypeListingComponent } from './bmc-data-components/part-type/part-type-listing/part-type-listing.component';
import { PartTypeDetailComponent } from './bmc-data-components/part-type/part-type-detail/part-type-detail.component';
import { PlacedBrickListingComponent } from './bmc-data-components/placed-brick/placed-brick-listing/placed-brick-listing.component';
import { PlacedBrickDetailComponent } from './bmc-data-components/placed-brick/placed-brick-detail/placed-brick-detail.component';
import { PlacedBrickChangeHistoryListingComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-listing/placed-brick-change-history-listing.component';
import { PlacedBrickChangeHistoryDetailComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-detail/placed-brick-change-history-detail.component';
import { ProjectListingComponent } from './bmc-data-components/project/project-listing/project-listing.component';
import { ProjectDetailComponent } from './bmc-data-components/project/project-detail/project-detail.component';
import { ProjectChangeHistoryListingComponent } from './bmc-data-components/project-change-history/project-change-history-listing/project-change-history-listing.component';
import { ProjectChangeHistoryDetailComponent } from './bmc-data-components/project-change-history/project-change-history-detail/project-change-history-detail.component';
//
// End of imports for BMC Data Components
//



const routes: Routes = [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'login', component: LoginComponent, data: { title: 'Login' } },
    { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard], data: { title: 'Dashboard' } },


    //
    // Beginning of routes for BMC Data Components
    //
    { path: 'brickcategories', component: BrickCategoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Categories' },
    { path: 'brickcategories/new', component: BrickCategoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Category' },
    { path: 'brickcategories/:brickCategoryId', component: BrickCategoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Category' },
    { path: 'brickcategory/:brickCategoryId', component: BrickCategoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Category' },
    { path: 'brickcategory', redirectTo: 'brickcategories' },
    { path: 'brickcolours', component: BrickColourListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Colours' },
    { path: 'brickcolours/new', component: BrickColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Colour' },
    { path: 'brickcolours/:brickColourId', component: BrickColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Colour' },
    { path: 'brickcolour/:brickColourId', component: BrickColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Colour' },
    { path: 'brickcolour', redirectTo: 'brickcolours' },
    { path: 'brickconnections', component: BrickConnectionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Connections' },
    { path: 'brickconnections/new', component: BrickConnectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Connection' },
    { path: 'brickconnections/:brickConnectionId', component: BrickConnectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Connection' },
    { path: 'brickconnection/:brickConnectionId', component: BrickConnectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Connection' },
    { path: 'brickconnection', redirectTo: 'brickconnections' },
    { path: 'brickparts', component: BrickPartListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Parts' },
    { path: 'brickparts/new', component: BrickPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part' },
    { path: 'brickparts/:brickPartId', component: BrickPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part' },
    { path: 'brickpart/:brickPartId', component: BrickPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part' },
    { path: 'brickpart', redirectTo: 'brickparts' },
    { path: 'brickpartchangehistories', component: BrickPartChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Change Histories' },
    { path: 'brickpartchangehistories/new', component: BrickPartChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Change History' },
    { path: 'brickpartchangehistories/:brickPartChangeHistoryId', component: BrickPartChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Change History' },
    { path: 'brickpartchangehistory/:brickPartChangeHistoryId', component: BrickPartChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Change History' },
    { path: 'brickpartchangehistory', redirectTo: 'brickpartchangehistories' },
    { path: 'brickpartcolours', component: BrickPartColourListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Colours' },
    { path: 'brickpartcolours/new', component: BrickPartColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Colour' },
    { path: 'brickpartcolours/:brickPartColourId', component: BrickPartColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Colour' },
    { path: 'brickpartcolour/:brickPartColourId', component: BrickPartColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Colour' },
    { path: 'brickpartcolour', redirectTo: 'brickpartcolours' },
    { path: 'brickpartconnectors', component: BrickPartConnectorListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Connectors' },
    { path: 'brickpartconnectors/new', component: BrickPartConnectorDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Connector' },
    { path: 'brickpartconnectors/:brickPartConnectorId', component: BrickPartConnectorDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Connector' },
    { path: 'brickpartconnector/:brickPartConnectorId', component: BrickPartConnectorDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Connector' },
    { path: 'brickpartconnector', redirectTo: 'brickpartconnectors' },
    { path: 'colourfinishs', component: ColourFinishListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Colour Finishs' },
    { path: 'colourfinishs/new', component: ColourFinishDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Colour Finish' },
    { path: 'colourfinishs/:colourFinishId', component: ColourFinishDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Colour Finish' },
    { path: 'colourfinish/:colourFinishId', component: ColourFinishDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Colour Finish' },
    { path: 'colourfinish', redirectTo: 'colourfinishs' },
    { path: 'connectortypes', component: ConnectorTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Connector Types' },
    { path: 'connectortypes/new', component: ConnectorTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Connector Type' },
    { path: 'connectortypes/:connectorTypeId', component: ConnectorTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Connector Type' },
    { path: 'connectortype/:connectorTypeId', component: ConnectorTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Connector Type' },
    { path: 'connectortype', redirectTo: 'connectortypes' },
    { path: 'parttypes', component: PartTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Part Types' },
    { path: 'parttypes/new', component: PartTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Part Type' },
    { path: 'parttypes/:partTypeId', component: PartTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Part Type' },
    { path: 'parttype/:partTypeId', component: PartTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Part Type' },
    { path: 'parttype', redirectTo: 'parttypes' },
    { path: 'placedbricks', component: PlacedBrickListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Placed Bricks' },
    { path: 'placedbricks/new', component: PlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Placed Brick' },
    { path: 'placedbricks/:placedBrickId', component: PlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick' },
    { path: 'placedbrick/:placedBrickId', component: PlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick' },
    { path: 'placedbrick', redirectTo: 'placedbricks' },
    { path: 'placedbrickchangehistories', component: PlacedBrickChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Placed Brick Change Histories' },
    { path: 'placedbrickchangehistories/new', component: PlacedBrickChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Placed Brick Change History' },
    { path: 'placedbrickchangehistories/:placedBrickChangeHistoryId', component: PlacedBrickChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick Change History' },
    { path: 'placedbrickchangehistory/:placedBrickChangeHistoryId', component: PlacedBrickChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick Change History' },
    { path: 'placedbrickchangehistory', redirectTo: 'placedbrickchangehistories' },
    { path: 'projects', component: ProjectListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Projects' },
    { path: 'projects/new', component: ProjectDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project' },
    { path: 'projects/:projectId', component: ProjectDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project' },
    { path: 'project/:projectId', component: ProjectDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project' },
    { path: 'project', redirectTo: 'projects' },
    { path: 'projectchangehistories', component: ProjectChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Change Histories' },
    { path: 'projectchangehistories/new', component: ProjectChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Change History' },
    { path: 'projectchangehistories/:projectChangeHistoryId', component: ProjectChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Change History' },
    { path: 'projectchangehistory/:projectChangeHistoryId', component: ProjectChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Change History' },
    { path: 'projectchangehistory', redirectTo: 'projectchangehistories' },
    //
    // End of routes for BMC Data Components
    //





    { path: '**', component: NotFoundComponent, data: { title: 'Page Not Found' } }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }
