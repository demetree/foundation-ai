import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { PartsCatalogComponent } from './components/parts-catalog/parts-catalog.component';
import { ColourLibraryComponent } from './components/colour-library/colour-library.component';
import { SystemHealthComponent } from './components/system-health/system-health.component';
import { CatalogPartDetailComponent } from './components/catalog-part-detail/catalog-part-detail.component';

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
import { BrickElementListingComponent } from './bmc-data-components/brick-element/brick-element-listing/brick-element-listing.component';
import { BrickElementDetailComponent } from './bmc-data-components/brick-element/brick-element-detail/brick-element-detail.component';
import { BrickPartListingComponent } from './bmc-data-components/brick-part/brick-part-listing/brick-part-listing.component';
import { BrickPartDetailComponent } from './bmc-data-components/brick-part/brick-part-detail/brick-part-detail.component';
import { BrickPartChangeHistoryListingComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-listing/brick-part-change-history-listing.component';
import { BrickPartChangeHistoryDetailComponent } from './bmc-data-components/brick-part-change-history/brick-part-change-history-detail/brick-part-change-history-detail.component';
import { BrickPartColourListingComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-listing/brick-part-colour-listing.component';
import { BrickPartColourDetailComponent } from './bmc-data-components/brick-part-colour/brick-part-colour-detail/brick-part-colour-detail.component';
import { BrickPartConnectorListingComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-listing/brick-part-connector-listing.component';
import { BrickPartConnectorDetailComponent } from './bmc-data-components/brick-part-connector/brick-part-connector-detail/brick-part-connector-detail.component';
import { BrickPartRelationshipListingComponent } from './bmc-data-components/brick-part-relationship/brick-part-relationship-listing/brick-part-relationship-listing.component';
import { BrickPartRelationshipDetailComponent } from './bmc-data-components/brick-part-relationship/brick-part-relationship-detail/brick-part-relationship-detail.component';
import { BuildManualListingComponent } from './bmc-data-components/build-manual/build-manual-listing/build-manual-listing.component';
import { BuildManualDetailComponent } from './bmc-data-components/build-manual/build-manual-detail/build-manual-detail.component';
import { BuildManualChangeHistoryListingComponent } from './bmc-data-components/build-manual-change-history/build-manual-change-history-listing/build-manual-change-history-listing.component';
import { BuildManualChangeHistoryDetailComponent } from './bmc-data-components/build-manual-change-history/build-manual-change-history-detail/build-manual-change-history-detail.component';
import { BuildManualPageListingComponent } from './bmc-data-components/build-manual-page/build-manual-page-listing/build-manual-page-listing.component';
import { BuildManualPageDetailComponent } from './bmc-data-components/build-manual-page/build-manual-page-detail/build-manual-page-detail.component';
import { BuildManualStepListingComponent } from './bmc-data-components/build-manual-step/build-manual-step-listing/build-manual-step-listing.component';
import { BuildManualStepDetailComponent } from './bmc-data-components/build-manual-step/build-manual-step-detail/build-manual-step-detail.component';
import { BuildStepAnnotationListingComponent } from './bmc-data-components/build-step-annotation/build-step-annotation-listing/build-step-annotation-listing.component';
import { BuildStepAnnotationDetailComponent } from './bmc-data-components/build-step-annotation/build-step-annotation-detail/build-step-annotation-detail.component';
import { BuildStepAnnotationTypeListingComponent } from './bmc-data-components/build-step-annotation-type/build-step-annotation-type-listing/build-step-annotation-type-listing.component';
import { BuildStepAnnotationTypeDetailComponent } from './bmc-data-components/build-step-annotation-type/build-step-annotation-type-detail/build-step-annotation-type-detail.component';
import { BuildStepPartListingComponent } from './bmc-data-components/build-step-part/build-step-part-listing/build-step-part-listing.component';
import { BuildStepPartDetailComponent } from './bmc-data-components/build-step-part/build-step-part-detail/build-step-part-detail.component';
import { ColourFinishListingComponent } from './bmc-data-components/colour-finish/colour-finish-listing/colour-finish-listing.component';
import { ColourFinishDetailComponent } from './bmc-data-components/colour-finish/colour-finish-detail/colour-finish-detail.component';
import { ConnectorTypeListingComponent } from './bmc-data-components/connector-type/connector-type-listing/connector-type-listing.component';
import { ConnectorTypeDetailComponent } from './bmc-data-components/connector-type/connector-type-detail/connector-type-detail.component';
import { ExportFormatListingComponent } from './bmc-data-components/export-format/export-format-listing/export-format-listing.component';
import { ExportFormatDetailComponent } from './bmc-data-components/export-format/export-format-detail/export-format-detail.component';
import { LegoMinifigListingComponent } from './bmc-data-components/lego-minifig/lego-minifig-listing/lego-minifig-listing.component';
import { LegoMinifigDetailComponent } from './bmc-data-components/lego-minifig/lego-minifig-detail/lego-minifig-detail.component';
import { LegoSetListingComponent } from './bmc-data-components/lego-set/lego-set-listing/lego-set-listing.component';
import { LegoSetDetailComponent } from './bmc-data-components/lego-set/lego-set-detail/lego-set-detail.component';
import { LegoSetMinifigListingComponent } from './bmc-data-components/lego-set-minifig/lego-set-minifig-listing/lego-set-minifig-listing.component';
import { LegoSetMinifigDetailComponent } from './bmc-data-components/lego-set-minifig/lego-set-minifig-detail/lego-set-minifig-detail.component';
import { LegoSetPartListingComponent } from './bmc-data-components/lego-set-part/lego-set-part-listing/lego-set-part-listing.component';
import { LegoSetPartDetailComponent } from './bmc-data-components/lego-set-part/lego-set-part-detail/lego-set-part-detail.component';
import { LegoSetSubsetListingComponent } from './bmc-data-components/lego-set-subset/lego-set-subset-listing/lego-set-subset-listing.component';
import { LegoSetSubsetDetailComponent } from './bmc-data-components/lego-set-subset/lego-set-subset-detail/lego-set-subset-detail.component';
import { LegoThemeListingComponent } from './bmc-data-components/lego-theme/lego-theme-listing/lego-theme-listing.component';
import { LegoThemeDetailComponent } from './bmc-data-components/lego-theme/lego-theme-detail/lego-theme-detail.component';
import { PartTypeListingComponent } from './bmc-data-components/part-type/part-type-listing/part-type-listing.component';
import { PartTypeDetailComponent } from './bmc-data-components/part-type/part-type-detail/part-type-detail.component';
import { PlacedBrickListingComponent } from './bmc-data-components/placed-brick/placed-brick-listing/placed-brick-listing.component';
import { PlacedBrickDetailComponent } from './bmc-data-components/placed-brick/placed-brick-detail/placed-brick-detail.component';
import { PlacedBrickChangeHistoryListingComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-listing/placed-brick-change-history-listing.component';
import { PlacedBrickChangeHistoryDetailComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-detail/placed-brick-change-history-detail.component';
import { ProjectListingComponent } from './bmc-data-components/project/project-listing/project-listing.component';
import { ProjectDetailComponent } from './bmc-data-components/project/project-detail/project-detail.component';
import { ProjectCameraPresetListingComponent } from './bmc-data-components/project-camera-preset/project-camera-preset-listing/project-camera-preset-listing.component';
import { ProjectCameraPresetDetailComponent } from './bmc-data-components/project-camera-preset/project-camera-preset-detail/project-camera-preset-detail.component';
import { ProjectChangeHistoryListingComponent } from './bmc-data-components/project-change-history/project-change-history-listing/project-change-history-listing.component';
import { ProjectChangeHistoryDetailComponent } from './bmc-data-components/project-change-history/project-change-history-detail/project-change-history-detail.component';
import { ProjectExportListingComponent } from './bmc-data-components/project-export/project-export-listing/project-export-listing.component';
import { ProjectExportDetailComponent } from './bmc-data-components/project-export/project-export-detail/project-export-detail.component';
import { ProjectReferenceImageListingComponent } from './bmc-data-components/project-reference-image/project-reference-image-listing/project-reference-image-listing.component';
import { ProjectReferenceImageDetailComponent } from './bmc-data-components/project-reference-image/project-reference-image-detail/project-reference-image-detail.component';
import { ProjectRenderListingComponent } from './bmc-data-components/project-render/project-render-listing/project-render-listing.component';
import { ProjectRenderDetailComponent } from './bmc-data-components/project-render/project-render-detail/project-render-detail.component';
import { ProjectTagListingComponent } from './bmc-data-components/project-tag/project-tag-listing/project-tag-listing.component';
import { ProjectTagDetailComponent } from './bmc-data-components/project-tag/project-tag-detail/project-tag-detail.component';
import { ProjectTagAssignmentListingComponent } from './bmc-data-components/project-tag-assignment/project-tag-assignment-listing/project-tag-assignment-listing.component';
import { ProjectTagAssignmentDetailComponent } from './bmc-data-components/project-tag-assignment/project-tag-assignment-detail/project-tag-assignment-detail.component';
import { RenderPresetListingComponent } from './bmc-data-components/render-preset/render-preset-listing/render-preset-listing.component';
import { RenderPresetDetailComponent } from './bmc-data-components/render-preset/render-preset-detail/render-preset-detail.component';
import { SubmodelListingComponent } from './bmc-data-components/submodel/submodel-listing/submodel-listing.component';
import { SubmodelDetailComponent } from './bmc-data-components/submodel/submodel-detail/submodel-detail.component';
import { SubmodelChangeHistoryListingComponent } from './bmc-data-components/submodel-change-history/submodel-change-history-listing/submodel-change-history-listing.component';
import { SubmodelChangeHistoryDetailComponent } from './bmc-data-components/submodel-change-history/submodel-change-history-detail/submodel-change-history-detail.component';
import { SubmodelPlacedBrickListingComponent } from './bmc-data-components/submodel-placed-brick/submodel-placed-brick-listing/submodel-placed-brick-listing.component';
import { SubmodelPlacedBrickDetailComponent } from './bmc-data-components/submodel-placed-brick/submodel-placed-brick-detail/submodel-placed-brick-detail.component';
import { UserCollectionListingComponent } from './bmc-data-components/user-collection/user-collection-listing/user-collection-listing.component';
import { UserCollectionDetailComponent } from './bmc-data-components/user-collection/user-collection-detail/user-collection-detail.component';
import { UserCollectionChangeHistoryListingComponent } from './bmc-data-components/user-collection-change-history/user-collection-change-history-listing/user-collection-change-history-listing.component';
import { UserCollectionChangeHistoryDetailComponent } from './bmc-data-components/user-collection-change-history/user-collection-change-history-detail/user-collection-change-history-detail.component';
import { UserCollectionPartListingComponent } from './bmc-data-components/user-collection-part/user-collection-part-listing/user-collection-part-listing.component';
import { UserCollectionPartDetailComponent } from './bmc-data-components/user-collection-part/user-collection-part-detail/user-collection-part-detail.component';
import { UserCollectionSetImportListingComponent } from './bmc-data-components/user-collection-set-import/user-collection-set-import-listing/user-collection-set-import-listing.component';
import { UserCollectionSetImportDetailComponent } from './bmc-data-components/user-collection-set-import/user-collection-set-import-detail/user-collection-set-import-detail.component';
import { UserWishlistItemListingComponent } from './bmc-data-components/user-wishlist-item/user-wishlist-item-listing/user-wishlist-item-listing.component';
import { UserWishlistItemDetailComponent } from './bmc-data-components/user-wishlist-item/user-wishlist-item-detail/user-wishlist-item-detail.component';
//
// End of imports for BMC Data Components
//
import { MyCollectionComponent } from './components/my-collection/my-collection.component';



const routes: Routes = [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'login', component: LoginComponent, data: { title: 'Login' } },
    { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard], data: { title: 'Dashboard' } },

    // Premium custom UI routes
    { path: 'parts', component: PartsCatalogComponent, canActivate: [AuthGuard], title: 'Parts Catalog' },
    { path: 'colours', component: ColourLibraryComponent, canActivate: [AuthGuard], title: 'Colour Library' },
    { path: 'system-health', component: SystemHealthComponent, canActivate: [AuthGuard], title: 'System Health' },
    { path: 'my-collection', component: MyCollectionComponent, canActivate: [AuthGuard], title: 'My Collection' },
    { path: 'parts/:partId', component: CatalogPartDetailComponent, canActivate: [AuthGuard], title: 'Part Detail' },


    //
    // Beginning of routes for BMC Data Components
//
  {path: 'brickcategories', component: BrickCategoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Categories' },
  {path: 'brickcategories/new', component: BrickCategoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Category' },
  {path: 'brickcategories/:brickCategoryId', component: BrickCategoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Category' },
  {path: 'brickcategory/:brickCategoryId', component: BrickCategoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Category' },
  {path: 'brickcategory',  redirectTo: 'brickcategories'},
  {path: 'brickcolours', component: BrickColourListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Colours' },
  {path: 'brickcolours/new', component: BrickColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Colour' },
  {path: 'brickcolours/:brickColourId', component: BrickColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Colour' },
  {path: 'brickcolour/:brickColourId', component: BrickColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Colour' },
  {path: 'brickcolour',  redirectTo: 'brickcolours'},
  {path: 'brickconnections', component: BrickConnectionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Connections' },
  {path: 'brickconnections/new', component: BrickConnectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Connection' },
  {path: 'brickconnections/:brickConnectionId', component: BrickConnectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Connection' },
  {path: 'brickconnection/:brickConnectionId', component: BrickConnectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Connection' },
  {path: 'brickconnection',  redirectTo: 'brickconnections'},
  {path: 'brickelements', component: BrickElementListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Elements' },
  {path: 'brickelements/new', component: BrickElementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Element' },
  {path: 'brickelements/:brickElementId', component: BrickElementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Element' },
  {path: 'brickelement/:brickElementId', component: BrickElementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Element' },
  {path: 'brickelement',  redirectTo: 'brickelements'},
  {path: 'brickparts', component: BrickPartListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Parts' },
  {path: 'brickparts/new', component: BrickPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part' },
  {path: 'brickparts/:brickPartId', component: BrickPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part' },
  {path: 'brickpart/:brickPartId', component: BrickPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part' },
  {path: 'brickpart',  redirectTo: 'brickparts'},
  {path: 'brickpartchangehistories', component: BrickPartChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Change Histories' },
  {path: 'brickpartchangehistories/new', component: BrickPartChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Change History' },
  {path: 'brickpartchangehistories/:brickPartChangeHistoryId', component: BrickPartChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Change History' },
  {path: 'brickpartchangehistory/:brickPartChangeHistoryId', component: BrickPartChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Change History' },
  {path: 'brickpartchangehistory',  redirectTo: 'brickpartchangehistories'},
  {path: 'brickpartcolours', component: BrickPartColourListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Colours' },
  {path: 'brickpartcolours/new', component: BrickPartColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Colour' },
  {path: 'brickpartcolours/:brickPartColourId', component: BrickPartColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Colour' },
  {path: 'brickpartcolour/:brickPartColourId', component: BrickPartColourDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Colour' },
  {path: 'brickpartcolour',  redirectTo: 'brickpartcolours'},
  {path: 'brickpartconnectors', component: BrickPartConnectorListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Connectors' },
  {path: 'brickpartconnectors/new', component: BrickPartConnectorDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Connector' },
  {path: 'brickpartconnectors/:brickPartConnectorId', component: BrickPartConnectorDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Connector' },
  {path: 'brickpartconnector/:brickPartConnectorId', component: BrickPartConnectorDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Connector' },
  {path: 'brickpartconnector',  redirectTo: 'brickpartconnectors'},
  {path: 'brickpartrelationships', component: BrickPartRelationshipListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Brick Part Relationships' },
  {path: 'brickpartrelationships/new', component: BrickPartRelationshipDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Brick Part Relationship' },
  {path: 'brickpartrelationships/:brickPartRelationshipId', component: BrickPartRelationshipDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Relationship' },
  {path: 'brickpartrelationship/:brickPartRelationshipId', component: BrickPartRelationshipDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Brick Part Relationship' },
  {path: 'brickpartrelationship',  redirectTo: 'brickpartrelationships'},
  {path: 'buildmanuals', component: BuildManualListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Manuals' },
  {path: 'buildmanuals/new', component: BuildManualDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Manual' },
  {path: 'buildmanuals/:buildManualId', component: BuildManualDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual' },
  {path: 'buildmanual/:buildManualId', component: BuildManualDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual' },
  {path: 'buildmanual',  redirectTo: 'buildmanuals'},
  {path: 'buildmanualchangehistories', component: BuildManualChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Manual Change Histories' },
  {path: 'buildmanualchangehistories/new', component: BuildManualChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Manual Change History' },
  {path: 'buildmanualchangehistories/:buildManualChangeHistoryId', component: BuildManualChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual Change History' },
  {path: 'buildmanualchangehistory/:buildManualChangeHistoryId', component: BuildManualChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual Change History' },
  {path: 'buildmanualchangehistory',  redirectTo: 'buildmanualchangehistories'},
  {path: 'buildmanualpages', component: BuildManualPageListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Manual Pages' },
  {path: 'buildmanualpages/new', component: BuildManualPageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Manual Page' },
  {path: 'buildmanualpages/:buildManualPageId', component: BuildManualPageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual Page' },
  {path: 'buildmanualpage/:buildManualPageId', component: BuildManualPageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual Page' },
  {path: 'buildmanualpage',  redirectTo: 'buildmanualpages'},
  {path: 'buildmanualsteps', component: BuildManualStepListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Manual Steps' },
  {path: 'buildmanualsteps/new', component: BuildManualStepDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Manual Step' },
  {path: 'buildmanualsteps/:buildManualStepId', component: BuildManualStepDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual Step' },
  {path: 'buildmanualstep/:buildManualStepId', component: BuildManualStepDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Manual Step' },
  {path: 'buildmanualstep',  redirectTo: 'buildmanualsteps'},
  {path: 'buildstepannotations', component: BuildStepAnnotationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Step Annotations' },
  {path: 'buildstepannotations/new', component: BuildStepAnnotationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Step Annotation' },
  {path: 'buildstepannotations/:buildStepAnnotationId', component: BuildStepAnnotationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Step Annotation' },
  {path: 'buildstepannotation/:buildStepAnnotationId', component: BuildStepAnnotationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Step Annotation' },
  {path: 'buildstepannotation',  redirectTo: 'buildstepannotations'},
  {path: 'buildstepannotationtypes', component: BuildStepAnnotationTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Step Annotation Types' },
  {path: 'buildstepannotationtypes/new', component: BuildStepAnnotationTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Step Annotation Type' },
  {path: 'buildstepannotationtypes/:buildStepAnnotationTypeId', component: BuildStepAnnotationTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Step Annotation Type' },
  {path: 'buildstepannotationtype/:buildStepAnnotationTypeId', component: BuildStepAnnotationTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Step Annotation Type' },
  {path: 'buildstepannotationtype',  redirectTo: 'buildstepannotationtypes'},
  {path: 'buildstepparts', component: BuildStepPartListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Build Step Parts' },
  {path: 'buildstepparts/new', component: BuildStepPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Build Step Part' },
  {path: 'buildstepparts/:buildStepPartId', component: BuildStepPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Step Part' },
  {path: 'buildsteppart/:buildStepPartId', component: BuildStepPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Build Step Part' },
  {path: 'buildsteppart',  redirectTo: 'buildstepparts'},
  {path: 'colourfinishes', component: ColourFinishListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Colour Finishes' },
  {path: 'colourfinishes/new', component: ColourFinishDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Colour Finish' },
  {path: 'colourfinishes/:colourFinishId', component: ColourFinishDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Colour Finish' },
  {path: 'colourfinish/:colourFinishId', component: ColourFinishDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Colour Finish' },
  {path: 'colourfinish',  redirectTo: 'colourfinishes'},
  {path: 'connectortypes', component: ConnectorTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Connector Types' },
  {path: 'connectortypes/new', component: ConnectorTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Connector Type' },
  {path: 'connectortypes/:connectorTypeId', component: ConnectorTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Connector Type' },
  {path: 'connectortype/:connectorTypeId', component: ConnectorTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Connector Type' },
  {path: 'connectortype',  redirectTo: 'connectortypes'},
  {path: 'exportformats', component: ExportFormatListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Export Formats' },
  {path: 'exportformats/new', component: ExportFormatDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Export Format' },
  {path: 'exportformats/:exportFormatId', component: ExportFormatDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Export Format' },
  {path: 'exportformat/:exportFormatId', component: ExportFormatDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Export Format' },
  {path: 'exportformat',  redirectTo: 'exportformats'},
  {path: 'legominifigs', component: LegoMinifigListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Lego Minifigs' },
  {path: 'legominifigs/new', component: LegoMinifigDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Lego Minifig' },
  {path: 'legominifigs/:legoMinifigId', component: LegoMinifigDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Minifig' },
  {path: 'legominifig/:legoMinifigId', component: LegoMinifigDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Minifig' },
  {path: 'legominifig',  redirectTo: 'legominifigs'},
  {path: 'legosets', component: LegoSetListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Lego Sets' },
  {path: 'legosets/new', component: LegoSetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Lego Set' },
  {path: 'legosets/:legoSetId', component: LegoSetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set' },
  {path: 'legoset/:legoSetId', component: LegoSetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set' },
  {path: 'legoset',  redirectTo: 'legosets'},
  {path: 'legosetminifigs', component: LegoSetMinifigListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Lego Set Minifigs' },
  {path: 'legosetminifigs/new', component: LegoSetMinifigDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Lego Set Minifig' },
  {path: 'legosetminifigs/:legoSetMinifigId', component: LegoSetMinifigDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set Minifig' },
  {path: 'legosetminifig/:legoSetMinifigId', component: LegoSetMinifigDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set Minifig' },
  {path: 'legosetminifig',  redirectTo: 'legosetminifigs'},
  {path: 'legosetparts', component: LegoSetPartListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Lego Set Parts' },
  {path: 'legosetparts/new', component: LegoSetPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Lego Set Part' },
  {path: 'legosetparts/:legoSetPartId', component: LegoSetPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set Part' },
  {path: 'legosetpart/:legoSetPartId', component: LegoSetPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set Part' },
  {path: 'legosetpart',  redirectTo: 'legosetparts'},
  {path: 'legosetsubsets', component: LegoSetSubsetListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Lego Set Subsets' },
  {path: 'legosetsubsets/new', component: LegoSetSubsetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Lego Set Subset' },
  {path: 'legosetsubsets/:legoSetSubsetId', component: LegoSetSubsetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set Subset' },
  {path: 'legosetsubset/:legoSetSubsetId', component: LegoSetSubsetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Set Subset' },
  {path: 'legosetsubset',  redirectTo: 'legosetsubsets'},
  {path: 'legothemes', component: LegoThemeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Lego Themes' },
  {path: 'legothemes/new', component: LegoThemeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Lego Theme' },
  {path: 'legothemes/:legoThemeId', component: LegoThemeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Theme' },
  {path: 'legotheme/:legoThemeId', component: LegoThemeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Lego Theme' },
  {path: 'legotheme',  redirectTo: 'legothemes'},
  {path: 'parttypes', component: PartTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Part Types' },
  {path: 'parttypes/new', component: PartTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Part Type' },
  {path: 'parttypes/:partTypeId', component: PartTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Part Type' },
  {path: 'parttype/:partTypeId', component: PartTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Part Type' },
  {path: 'parttype',  redirectTo: 'parttypes'},
  {path: 'placedbricks', component: PlacedBrickListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Placed Bricks' },
  {path: 'placedbricks/new', component: PlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Placed Brick' },
  {path: 'placedbricks/:placedBrickId', component: PlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick' },
  {path: 'placedbrick/:placedBrickId', component: PlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick' },
  {path: 'placedbrick',  redirectTo: 'placedbricks'},
  {path: 'placedbrickchangehistories', component: PlacedBrickChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Placed Brick Change Histories' },
  {path: 'placedbrickchangehistories/new', component: PlacedBrickChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Placed Brick Change History' },
  {path: 'placedbrickchangehistories/:placedBrickChangeHistoryId', component: PlacedBrickChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick Change History' },
  {path: 'placedbrickchangehistory/:placedBrickChangeHistoryId', component: PlacedBrickChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Placed Brick Change History' },
  {path: 'placedbrickchangehistory',  redirectTo: 'placedbrickchangehistories'},
  {path: 'projects', component: ProjectListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Projects' },
  {path: 'projects/new', component: ProjectDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project' },
  {path: 'projects/:projectId', component: ProjectDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project' },
  {path: 'project/:projectId', component: ProjectDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project' },
  {path: 'project',  redirectTo: 'projects'},
  {path: 'projectcamerapresets', component: ProjectCameraPresetListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Camera Presets' },
  {path: 'projectcamerapresets/new', component: ProjectCameraPresetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Camera Preset' },
  {path: 'projectcamerapresets/:projectCameraPresetId', component: ProjectCameraPresetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Camera Preset' },
  {path: 'projectcamerapreset/:projectCameraPresetId', component: ProjectCameraPresetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Camera Preset' },
  {path: 'projectcamerapreset',  redirectTo: 'projectcamerapresets'},
  {path: 'projectchangehistories', component: ProjectChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Change Histories' },
  {path: 'projectchangehistories/new', component: ProjectChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Change History' },
  {path: 'projectchangehistories/:projectChangeHistoryId', component: ProjectChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Change History' },
  {path: 'projectchangehistory/:projectChangeHistoryId', component: ProjectChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Change History' },
  {path: 'projectchangehistory',  redirectTo: 'projectchangehistories'},
  {path: 'projectexports', component: ProjectExportListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Exports' },
  {path: 'projectexports/new', component: ProjectExportDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Export' },
  {path: 'projectexports/:projectExportId', component: ProjectExportDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Export' },
  {path: 'projectexport/:projectExportId', component: ProjectExportDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Export' },
  {path: 'projectexport',  redirectTo: 'projectexports'},
  {path: 'projectreferenceimages', component: ProjectReferenceImageListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Reference Images' },
  {path: 'projectreferenceimages/new', component: ProjectReferenceImageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Reference Image' },
  {path: 'projectreferenceimages/:projectReferenceImageId', component: ProjectReferenceImageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Reference Image' },
  {path: 'projectreferenceimage/:projectReferenceImageId', component: ProjectReferenceImageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Reference Image' },
  {path: 'projectreferenceimage',  redirectTo: 'projectreferenceimages'},
  {path: 'projectrenders', component: ProjectRenderListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Renders' },
  {path: 'projectrenders/new', component: ProjectRenderDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Render' },
  {path: 'projectrenders/:projectRenderId', component: ProjectRenderDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Render' },
  {path: 'projectrender/:projectRenderId', component: ProjectRenderDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Render' },
  {path: 'projectrender',  redirectTo: 'projectrenders'},
  {path: 'projecttags', component: ProjectTagListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Tags' },
  {path: 'projecttags/new', component: ProjectTagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Tag' },
  {path: 'projecttags/:projectTagId', component: ProjectTagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Tag' },
  {path: 'projecttag/:projectTagId', component: ProjectTagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Tag' },
  {path: 'projecttag',  redirectTo: 'projecttags'},
  {path: 'projecttagassignments', component: ProjectTagAssignmentListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Project Tag Assignments' },
  {path: 'projecttagassignments/new', component: ProjectTagAssignmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Project Tag Assignment' },
  {path: 'projecttagassignments/:projectTagAssignmentId', component: ProjectTagAssignmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Tag Assignment' },
  {path: 'projecttagassignment/:projectTagAssignmentId', component: ProjectTagAssignmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Project Tag Assignment' },
  {path: 'projecttagassignment',  redirectTo: 'projecttagassignments'},
  {path: 'renderpresets', component: RenderPresetListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Render Presets' },
  {path: 'renderpresets/new', component: RenderPresetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Render Preset' },
  {path: 'renderpresets/:renderPresetId', component: RenderPresetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Render Preset' },
  {path: 'renderpreset/:renderPresetId', component: RenderPresetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Render Preset' },
  {path: 'renderpreset',  redirectTo: 'renderpresets'},
  {path: 'submodels', component: SubmodelListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Submodels' },
  {path: 'submodels/new', component: SubmodelDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Submodel' },
  {path: 'submodels/:submodelId', component: SubmodelDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Submodel' },
  {path: 'submodel/:submodelId', component: SubmodelDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Submodel' },
  {path: 'submodel',  redirectTo: 'submodels'},
  {path: 'submodelchangehistories', component: SubmodelChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Submodel Change Histories' },
  {path: 'submodelchangehistories/new', component: SubmodelChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Submodel Change History' },
  {path: 'submodelchangehistories/:submodelChangeHistoryId', component: SubmodelChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Submodel Change History' },
  {path: 'submodelchangehistory/:submodelChangeHistoryId', component: SubmodelChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Submodel Change History' },
  {path: 'submodelchangehistory',  redirectTo: 'submodelchangehistories'},
  {path: 'submodelplacedbricks', component: SubmodelPlacedBrickListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Submodel Placed Bricks' },
  {path: 'submodelplacedbricks/new', component: SubmodelPlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Submodel Placed Brick' },
  {path: 'submodelplacedbricks/:submodelPlacedBrickId', component: SubmodelPlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Submodel Placed Brick' },
  {path: 'submodelplacedbrick/:submodelPlacedBrickId', component: SubmodelPlacedBrickDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Submodel Placed Brick' },
  {path: 'submodelplacedbrick',  redirectTo: 'submodelplacedbricks'},
  {path: 'usercollections', component: UserCollectionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Collections' },
  {path: 'usercollections/new', component: UserCollectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Collection' },
  {path: 'usercollections/:userCollectionId', component: UserCollectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection' },
  {path: 'usercollection/:userCollectionId', component: UserCollectionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection' },
  {path: 'usercollection',  redirectTo: 'usercollections'},
  {path: 'usercollectionchangehistories', component: UserCollectionChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Collection Change Histories' },
  {path: 'usercollectionchangehistories/new', component: UserCollectionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Collection Change History' },
  {path: 'usercollectionchangehistories/:userCollectionChangeHistoryId', component: UserCollectionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection Change History' },
  {path: 'usercollectionchangehistory/:userCollectionChangeHistoryId', component: UserCollectionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection Change History' },
  {path: 'usercollectionchangehistory',  redirectTo: 'usercollectionchangehistories'},
  {path: 'usercollectionparts', component: UserCollectionPartListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Collection Parts' },
  {path: 'usercollectionparts/new', component: UserCollectionPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Collection Part' },
  {path: 'usercollectionparts/:userCollectionPartId', component: UserCollectionPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection Part' },
  {path: 'usercollectionpart/:userCollectionPartId', component: UserCollectionPartDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection Part' },
  {path: 'usercollectionpart',  redirectTo: 'usercollectionparts'},
  {path: 'usercollectionsetimports', component: UserCollectionSetImportListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Collection Set Imports' },
  {path: 'usercollectionsetimports/new', component: UserCollectionSetImportDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Collection Set Import' },
  {path: 'usercollectionsetimports/:userCollectionSetImportId', component: UserCollectionSetImportDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection Set Import' },
  {path: 'usercollectionsetimport/:userCollectionSetImportId', component: UserCollectionSetImportDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Collection Set Import' },
  {path: 'usercollectionsetimport',  redirectTo: 'usercollectionsetimports'},
  {path: 'userwishlistitems', component: UserWishlistItemListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Wishlist Items' },
  {path: 'userwishlistitems/new', component: UserWishlistItemDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Wishlist Item' },
  {path: 'userwishlistitems/:userWishlistItemId', component: UserWishlistItemDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Wishlist Item' },
  {path: 'userwishlistitem/:userWishlistItemId', component: UserWishlistItemDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Wishlist Item' },
  {path: 'userwishlistitem',  redirectTo: 'userwishlistitems'},
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
