import { NgModule } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';
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
import { LDrawThumbnailService } from './services/ldraw-thumbnail.service';
import { IndexedDBCacheService } from './services/indexeddb-cache.service';


//
// Custom confirmation dialog
//
import { ConfirmationService } from './services/confirmation-service';
import { ConfirmationDialogComponent } from './services/confirmation-dialog/confirmation-dialog.component';

import { InputDialogService } from './services/input-dialog.service';
import { InputDialogComponent } from './services/input-dialog/input-dialog.component';

import { ImportSetModalComponent } from './components/import-set-modal/import-set-modal.component';

import { NavigationService } from './utility-services/navigation.service';

//
// Shared directives, components, and pipes used by auto-generated Foundation components
//
import { SpinnerDirective } from './directives/spinner.directive';
import { SpinnerComponent } from './directives/spinner.component';
import { BooleanIconComponent } from './components/controls/boolean-icon.component';
import { BigNumberFormatPipe } from './pipes/big-number-format.pipe';


//
// Beginning of imports for BMC Data Services
//
import { BMCDataServiceManagerService } from './bmc-data-services/bmc-data-service-manager.service';
import { BrickCategoryService } from './bmc-data-services/brick-category.service';
import { BrickColourService } from './bmc-data-services/brick-colour.service';
import { BrickConnectionService } from './bmc-data-services/brick-connection.service';
import { BrickElementService } from './bmc-data-services/brick-element.service';
import { BrickPartService } from './bmc-data-services/brick-part.service';
import { BrickPartChangeHistoryService } from './bmc-data-services/brick-part-change-history.service';
import { BrickPartColourService } from './bmc-data-services/brick-part-colour.service';
import { BrickPartConnectorService } from './bmc-data-services/brick-part-connector.service';
import { BrickPartRelationshipService } from './bmc-data-services/brick-part-relationship.service';
import { BuildManualService } from './bmc-data-services/build-manual.service';
import { BuildManualChangeHistoryService } from './bmc-data-services/build-manual-change-history.service';
import { BuildManualPageService } from './bmc-data-services/build-manual-page.service';
import { BuildManualStepService } from './bmc-data-services/build-manual-step.service';
import { BuildStepAnnotationService } from './bmc-data-services/build-step-annotation.service';
import { BuildStepAnnotationTypeService } from './bmc-data-services/build-step-annotation-type.service';
import { BuildStepPartService } from './bmc-data-services/build-step-part.service';
import { ColourFinishService } from './bmc-data-services/colour-finish.service';
import { ConnectorTypeService } from './bmc-data-services/connector-type.service';
import { ExportFormatService } from './bmc-data-services/export-format.service';
import { LegoMinifigService } from './bmc-data-services/lego-minifig.service';
import { LegoSetService } from './bmc-data-services/lego-set.service';
import { LegoSetMinifigService } from './bmc-data-services/lego-set-minifig.service';
import { LegoSetPartService } from './bmc-data-services/lego-set-part.service';
import { LegoSetSubsetService } from './bmc-data-services/lego-set-subset.service';
import { LegoThemeService } from './bmc-data-services/lego-theme.service';
import { PartTypeService } from './bmc-data-services/part-type.service';
import { PlacedBrickService } from './bmc-data-services/placed-brick.service';
import { PlacedBrickChangeHistoryService } from './bmc-data-services/placed-brick-change-history.service';
import { ProjectService } from './bmc-data-services/project.service';
import { ProjectCameraPresetService } from './bmc-data-services/project-camera-preset.service';
import { ProjectChangeHistoryService } from './bmc-data-services/project-change-history.service';
import { ProjectExportService } from './bmc-data-services/project-export.service';
import { ProjectReferenceImageService } from './bmc-data-services/project-reference-image.service';
import { ProjectRenderService } from './bmc-data-services/project-render.service';
import { ProjectTagService } from './bmc-data-services/project-tag.service';
import { ProjectTagAssignmentService } from './bmc-data-services/project-tag-assignment.service';
import { RenderPresetService } from './bmc-data-services/render-preset.service';
import { SubmodelService } from './bmc-data-services/submodel.service';
import { SubmodelChangeHistoryService } from './bmc-data-services/submodel-change-history.service';
import { SubmodelPlacedBrickService } from './bmc-data-services/submodel-placed-brick.service';
import { UserCollectionService } from './bmc-data-services/user-collection.service';
import { UserCollectionChangeHistoryService } from './bmc-data-services/user-collection-change-history.service';
import { UserCollectionPartService } from './bmc-data-services/user-collection-part.service';
import { UserCollectionSetImportService } from './bmc-data-services/user-collection-set-import.service';
import { UserWishlistItemService } from './bmc-data-services/user-wishlist-item.service';
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
import { BrickElementListingComponent } from './bmc-data-components/brick-element/brick-element-listing/brick-element-listing.component';
import { BrickElementAddEditComponent } from './bmc-data-components/brick-element/brick-element-add-edit/brick-element-add-edit.component';
import { BrickElementDetailComponent } from './bmc-data-components/brick-element/brick-element-detail/brick-element-detail.component';
import { BrickElementTableComponent } from './bmc-data-components/brick-element/brick-element-table/brick-element-table.component';
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
import { BrickPartRelationshipListingComponent } from './bmc-data-components/brick-part-relationship/brick-part-relationship-listing/brick-part-relationship-listing.component';
import { BrickPartRelationshipAddEditComponent } from './bmc-data-components/brick-part-relationship/brick-part-relationship-add-edit/brick-part-relationship-add-edit.component';
import { BrickPartRelationshipDetailComponent } from './bmc-data-components/brick-part-relationship/brick-part-relationship-detail/brick-part-relationship-detail.component';
import { BrickPartRelationshipTableComponent } from './bmc-data-components/brick-part-relationship/brick-part-relationship-table/brick-part-relationship-table.component';
import { BuildManualListingComponent } from './bmc-data-components/build-manual/build-manual-listing/build-manual-listing.component';
import { BuildManualAddEditComponent } from './bmc-data-components/build-manual/build-manual-add-edit/build-manual-add-edit.component';
import { BuildManualDetailComponent } from './bmc-data-components/build-manual/build-manual-detail/build-manual-detail.component';
import { BuildManualTableComponent } from './bmc-data-components/build-manual/build-manual-table/build-manual-table.component';
import { BuildManualChangeHistoryListingComponent } from './bmc-data-components/build-manual-change-history/build-manual-change-history-listing/build-manual-change-history-listing.component';
import { BuildManualChangeHistoryAddEditComponent } from './bmc-data-components/build-manual-change-history/build-manual-change-history-add-edit/build-manual-change-history-add-edit.component';
import { BuildManualChangeHistoryDetailComponent } from './bmc-data-components/build-manual-change-history/build-manual-change-history-detail/build-manual-change-history-detail.component';
import { BuildManualChangeHistoryTableComponent } from './bmc-data-components/build-manual-change-history/build-manual-change-history-table/build-manual-change-history-table.component';
import { BuildManualPageListingComponent } from './bmc-data-components/build-manual-page/build-manual-page-listing/build-manual-page-listing.component';
import { BuildManualPageAddEditComponent } from './bmc-data-components/build-manual-page/build-manual-page-add-edit/build-manual-page-add-edit.component';
import { BuildManualPageDetailComponent } from './bmc-data-components/build-manual-page/build-manual-page-detail/build-manual-page-detail.component';
import { BuildManualPageTableComponent } from './bmc-data-components/build-manual-page/build-manual-page-table/build-manual-page-table.component';
import { BuildManualStepListingComponent } from './bmc-data-components/build-manual-step/build-manual-step-listing/build-manual-step-listing.component';
import { BuildManualStepAddEditComponent } from './bmc-data-components/build-manual-step/build-manual-step-add-edit/build-manual-step-add-edit.component';
import { BuildManualStepDetailComponent } from './bmc-data-components/build-manual-step/build-manual-step-detail/build-manual-step-detail.component';
import { BuildManualStepTableComponent } from './bmc-data-components/build-manual-step/build-manual-step-table/build-manual-step-table.component';
import { BuildStepAnnotationListingComponent } from './bmc-data-components/build-step-annotation/build-step-annotation-listing/build-step-annotation-listing.component';
import { BuildStepAnnotationAddEditComponent } from './bmc-data-components/build-step-annotation/build-step-annotation-add-edit/build-step-annotation-add-edit.component';
import { BuildStepAnnotationDetailComponent } from './bmc-data-components/build-step-annotation/build-step-annotation-detail/build-step-annotation-detail.component';
import { BuildStepAnnotationTableComponent } from './bmc-data-components/build-step-annotation/build-step-annotation-table/build-step-annotation-table.component';
import { BuildStepAnnotationTypeListingComponent } from './bmc-data-components/build-step-annotation-type/build-step-annotation-type-listing/build-step-annotation-type-listing.component';
import { BuildStepAnnotationTypeAddEditComponent } from './bmc-data-components/build-step-annotation-type/build-step-annotation-type-add-edit/build-step-annotation-type-add-edit.component';
import { BuildStepAnnotationTypeDetailComponent } from './bmc-data-components/build-step-annotation-type/build-step-annotation-type-detail/build-step-annotation-type-detail.component';
import { BuildStepAnnotationTypeTableComponent } from './bmc-data-components/build-step-annotation-type/build-step-annotation-type-table/build-step-annotation-type-table.component';
import { BuildStepPartListingComponent } from './bmc-data-components/build-step-part/build-step-part-listing/build-step-part-listing.component';
import { BuildStepPartAddEditComponent } from './bmc-data-components/build-step-part/build-step-part-add-edit/build-step-part-add-edit.component';
import { BuildStepPartDetailComponent } from './bmc-data-components/build-step-part/build-step-part-detail/build-step-part-detail.component';
import { BuildStepPartTableComponent } from './bmc-data-components/build-step-part/build-step-part-table/build-step-part-table.component';
import { ColourFinishListingComponent } from './bmc-data-components/colour-finish/colour-finish-listing/colour-finish-listing.component';
import { ColourFinishAddEditComponent } from './bmc-data-components/colour-finish/colour-finish-add-edit/colour-finish-add-edit.component';
import { ColourFinishDetailComponent } from './bmc-data-components/colour-finish/colour-finish-detail/colour-finish-detail.component';
import { ColourFinishTableComponent } from './bmc-data-components/colour-finish/colour-finish-table/colour-finish-table.component';
import { ConnectorTypeListingComponent } from './bmc-data-components/connector-type/connector-type-listing/connector-type-listing.component';
import { ConnectorTypeAddEditComponent } from './bmc-data-components/connector-type/connector-type-add-edit/connector-type-add-edit.component';
import { ConnectorTypeDetailComponent } from './bmc-data-components/connector-type/connector-type-detail/connector-type-detail.component';
import { ConnectorTypeTableComponent } from './bmc-data-components/connector-type/connector-type-table/connector-type-table.component';
import { ExportFormatListingComponent } from './bmc-data-components/export-format/export-format-listing/export-format-listing.component';
import { ExportFormatAddEditComponent } from './bmc-data-components/export-format/export-format-add-edit/export-format-add-edit.component';
import { ExportFormatDetailComponent } from './bmc-data-components/export-format/export-format-detail/export-format-detail.component';
import { ExportFormatTableComponent } from './bmc-data-components/export-format/export-format-table/export-format-table.component';
import { LegoMinifigListingComponent } from './bmc-data-components/lego-minifig/lego-minifig-listing/lego-minifig-listing.component';
import { LegoMinifigAddEditComponent } from './bmc-data-components/lego-minifig/lego-minifig-add-edit/lego-minifig-add-edit.component';
import { LegoMinifigDetailComponent } from './bmc-data-components/lego-minifig/lego-minifig-detail/lego-minifig-detail.component';
import { LegoMinifigTableComponent } from './bmc-data-components/lego-minifig/lego-minifig-table/lego-minifig-table.component';
import { LegoSetListingComponent } from './bmc-data-components/lego-set/lego-set-listing/lego-set-listing.component';
import { LegoSetAddEditComponent } from './bmc-data-components/lego-set/lego-set-add-edit/lego-set-add-edit.component';
import { LegoSetDetailComponent } from './bmc-data-components/lego-set/lego-set-detail/lego-set-detail.component';
import { LegoSetTableComponent } from './bmc-data-components/lego-set/lego-set-table/lego-set-table.component';
import { LegoSetMinifigListingComponent } from './bmc-data-components/lego-set-minifig/lego-set-minifig-listing/lego-set-minifig-listing.component';
import { LegoSetMinifigAddEditComponent } from './bmc-data-components/lego-set-minifig/lego-set-minifig-add-edit/lego-set-minifig-add-edit.component';
import { LegoSetMinifigDetailComponent } from './bmc-data-components/lego-set-minifig/lego-set-minifig-detail/lego-set-minifig-detail.component';
import { LegoSetMinifigTableComponent } from './bmc-data-components/lego-set-minifig/lego-set-minifig-table/lego-set-minifig-table.component';
import { LegoSetPartListingComponent } from './bmc-data-components/lego-set-part/lego-set-part-listing/lego-set-part-listing.component';
import { LegoSetPartAddEditComponent } from './bmc-data-components/lego-set-part/lego-set-part-add-edit/lego-set-part-add-edit.component';
import { LegoSetPartDetailComponent } from './bmc-data-components/lego-set-part/lego-set-part-detail/lego-set-part-detail.component';
import { LegoSetPartTableComponent } from './bmc-data-components/lego-set-part/lego-set-part-table/lego-set-part-table.component';
import { LegoSetSubsetListingComponent } from './bmc-data-components/lego-set-subset/lego-set-subset-listing/lego-set-subset-listing.component';
import { LegoSetSubsetAddEditComponent } from './bmc-data-components/lego-set-subset/lego-set-subset-add-edit/lego-set-subset-add-edit.component';
import { LegoSetSubsetDetailComponent } from './bmc-data-components/lego-set-subset/lego-set-subset-detail/lego-set-subset-detail.component';
import { LegoSetSubsetTableComponent } from './bmc-data-components/lego-set-subset/lego-set-subset-table/lego-set-subset-table.component';
import { LegoThemeListingComponent } from './bmc-data-components/lego-theme/lego-theme-listing/lego-theme-listing.component';
import { LegoThemeAddEditComponent } from './bmc-data-components/lego-theme/lego-theme-add-edit/lego-theme-add-edit.component';
import { LegoThemeDetailComponent } from './bmc-data-components/lego-theme/lego-theme-detail/lego-theme-detail.component';
import { LegoThemeTableComponent } from './bmc-data-components/lego-theme/lego-theme-table/lego-theme-table.component';
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
import { ProjectCameraPresetListingComponent } from './bmc-data-components/project-camera-preset/project-camera-preset-listing/project-camera-preset-listing.component';
import { ProjectCameraPresetAddEditComponent } from './bmc-data-components/project-camera-preset/project-camera-preset-add-edit/project-camera-preset-add-edit.component';
import { ProjectCameraPresetDetailComponent } from './bmc-data-components/project-camera-preset/project-camera-preset-detail/project-camera-preset-detail.component';
import { ProjectCameraPresetTableComponent } from './bmc-data-components/project-camera-preset/project-camera-preset-table/project-camera-preset-table.component';
import { ProjectChangeHistoryListingComponent } from './bmc-data-components/project-change-history/project-change-history-listing/project-change-history-listing.component';
import { ProjectChangeHistoryAddEditComponent } from './bmc-data-components/project-change-history/project-change-history-add-edit/project-change-history-add-edit.component';
import { ProjectChangeHistoryDetailComponent } from './bmc-data-components/project-change-history/project-change-history-detail/project-change-history-detail.component';
import { ProjectChangeHistoryTableComponent } from './bmc-data-components/project-change-history/project-change-history-table/project-change-history-table.component';
import { ProjectExportListingComponent } from './bmc-data-components/project-export/project-export-listing/project-export-listing.component';
import { ProjectExportAddEditComponent } from './bmc-data-components/project-export/project-export-add-edit/project-export-add-edit.component';
import { ProjectExportDetailComponent } from './bmc-data-components/project-export/project-export-detail/project-export-detail.component';
import { ProjectExportTableComponent } from './bmc-data-components/project-export/project-export-table/project-export-table.component';
import { ProjectReferenceImageListingComponent } from './bmc-data-components/project-reference-image/project-reference-image-listing/project-reference-image-listing.component';
import { ProjectReferenceImageAddEditComponent } from './bmc-data-components/project-reference-image/project-reference-image-add-edit/project-reference-image-add-edit.component';
import { ProjectReferenceImageDetailComponent } from './bmc-data-components/project-reference-image/project-reference-image-detail/project-reference-image-detail.component';
import { ProjectReferenceImageTableComponent } from './bmc-data-components/project-reference-image/project-reference-image-table/project-reference-image-table.component';
import { ProjectRenderListingComponent } from './bmc-data-components/project-render/project-render-listing/project-render-listing.component';
import { ProjectRenderAddEditComponent } from './bmc-data-components/project-render/project-render-add-edit/project-render-add-edit.component';
import { ProjectRenderDetailComponent } from './bmc-data-components/project-render/project-render-detail/project-render-detail.component';
import { ProjectRenderTableComponent } from './bmc-data-components/project-render/project-render-table/project-render-table.component';
import { ProjectTagListingComponent } from './bmc-data-components/project-tag/project-tag-listing/project-tag-listing.component';
import { ProjectTagAddEditComponent } from './bmc-data-components/project-tag/project-tag-add-edit/project-tag-add-edit.component';
import { ProjectTagDetailComponent } from './bmc-data-components/project-tag/project-tag-detail/project-tag-detail.component';
import { ProjectTagTableComponent } from './bmc-data-components/project-tag/project-tag-table/project-tag-table.component';
import { ProjectTagAssignmentListingComponent } from './bmc-data-components/project-tag-assignment/project-tag-assignment-listing/project-tag-assignment-listing.component';
import { ProjectTagAssignmentAddEditComponent } from './bmc-data-components/project-tag-assignment/project-tag-assignment-add-edit/project-tag-assignment-add-edit.component';
import { ProjectTagAssignmentDetailComponent } from './bmc-data-components/project-tag-assignment/project-tag-assignment-detail/project-tag-assignment-detail.component';
import { ProjectTagAssignmentTableComponent } from './bmc-data-components/project-tag-assignment/project-tag-assignment-table/project-tag-assignment-table.component';
import { RenderPresetListingComponent } from './bmc-data-components/render-preset/render-preset-listing/render-preset-listing.component';
import { RenderPresetAddEditComponent } from './bmc-data-components/render-preset/render-preset-add-edit/render-preset-add-edit.component';
import { RenderPresetDetailComponent } from './bmc-data-components/render-preset/render-preset-detail/render-preset-detail.component';
import { RenderPresetTableComponent } from './bmc-data-components/render-preset/render-preset-table/render-preset-table.component';
import { SubmodelListingComponent } from './bmc-data-components/submodel/submodel-listing/submodel-listing.component';
import { SubmodelAddEditComponent } from './bmc-data-components/submodel/submodel-add-edit/submodel-add-edit.component';
import { SubmodelDetailComponent } from './bmc-data-components/submodel/submodel-detail/submodel-detail.component';
import { SubmodelTableComponent } from './bmc-data-components/submodel/submodel-table/submodel-table.component';
import { SubmodelChangeHistoryListingComponent } from './bmc-data-components/submodel-change-history/submodel-change-history-listing/submodel-change-history-listing.component';
import { SubmodelChangeHistoryAddEditComponent } from './bmc-data-components/submodel-change-history/submodel-change-history-add-edit/submodel-change-history-add-edit.component';
import { SubmodelChangeHistoryDetailComponent } from './bmc-data-components/submodel-change-history/submodel-change-history-detail/submodel-change-history-detail.component';
import { SubmodelChangeHistoryTableComponent } from './bmc-data-components/submodel-change-history/submodel-change-history-table/submodel-change-history-table.component';
import { SubmodelPlacedBrickListingComponent } from './bmc-data-components/submodel-placed-brick/submodel-placed-brick-listing/submodel-placed-brick-listing.component';
import { SubmodelPlacedBrickAddEditComponent } from './bmc-data-components/submodel-placed-brick/submodel-placed-brick-add-edit/submodel-placed-brick-add-edit.component';
import { SubmodelPlacedBrickDetailComponent } from './bmc-data-components/submodel-placed-brick/submodel-placed-brick-detail/submodel-placed-brick-detail.component';
import { SubmodelPlacedBrickTableComponent } from './bmc-data-components/submodel-placed-brick/submodel-placed-brick-table/submodel-placed-brick-table.component';
import { UserCollectionListingComponent } from './bmc-data-components/user-collection/user-collection-listing/user-collection-listing.component';
import { UserCollectionAddEditComponent } from './bmc-data-components/user-collection/user-collection-add-edit/user-collection-add-edit.component';
import { UserCollectionDetailComponent } from './bmc-data-components/user-collection/user-collection-detail/user-collection-detail.component';
import { UserCollectionTableComponent } from './bmc-data-components/user-collection/user-collection-table/user-collection-table.component';
import { UserCollectionChangeHistoryListingComponent } from './bmc-data-components/user-collection-change-history/user-collection-change-history-listing/user-collection-change-history-listing.component';
import { UserCollectionChangeHistoryAddEditComponent } from './bmc-data-components/user-collection-change-history/user-collection-change-history-add-edit/user-collection-change-history-add-edit.component';
import { UserCollectionChangeHistoryDetailComponent } from './bmc-data-components/user-collection-change-history/user-collection-change-history-detail/user-collection-change-history-detail.component';
import { UserCollectionChangeHistoryTableComponent } from './bmc-data-components/user-collection-change-history/user-collection-change-history-table/user-collection-change-history-table.component';
import { UserCollectionPartListingComponent } from './bmc-data-components/user-collection-part/user-collection-part-listing/user-collection-part-listing.component';
import { UserCollectionPartAddEditComponent } from './bmc-data-components/user-collection-part/user-collection-part-add-edit/user-collection-part-add-edit.component';
import { UserCollectionPartDetailComponent } from './bmc-data-components/user-collection-part/user-collection-part-detail/user-collection-part-detail.component';
import { UserCollectionPartTableComponent } from './bmc-data-components/user-collection-part/user-collection-part-table/user-collection-part-table.component';
import { UserCollectionSetImportListingComponent } from './bmc-data-components/user-collection-set-import/user-collection-set-import-listing/user-collection-set-import-listing.component';
import { UserCollectionSetImportAddEditComponent } from './bmc-data-components/user-collection-set-import/user-collection-set-import-add-edit/user-collection-set-import-add-edit.component';
import { UserCollectionSetImportDetailComponent } from './bmc-data-components/user-collection-set-import/user-collection-set-import-detail/user-collection-set-import-detail.component';
import { UserCollectionSetImportTableComponent } from './bmc-data-components/user-collection-set-import/user-collection-set-import-table/user-collection-set-import-table.component';
import { UserWishlistItemListingComponent } from './bmc-data-components/user-wishlist-item/user-wishlist-item-listing/user-wishlist-item-listing.component';
import { UserWishlistItemAddEditComponent } from './bmc-data-components/user-wishlist-item/user-wishlist-item-add-edit/user-wishlist-item-add-edit.component';
import { UserWishlistItemDetailComponent } from './bmc-data-components/user-wishlist-item/user-wishlist-item-detail/user-wishlist-item-detail.component';
import { UserWishlistItemTableComponent } from './bmc-data-components/user-wishlist-item/user-wishlist-item-table/user-wishlist-item-table.component';
//
// End of imports for BMC Data Components
//

// Premium custom UI components
import { PartsCatalogComponent } from './components/parts-catalog/parts-catalog.component';
import { CatalogPartDetailComponent } from './components/catalog-part-detail/catalog-part-detail.component';
import { ColourLibraryComponent } from './components/colour-library/colour-library.component';
import { SystemHealthComponent } from './components/system-health/system-health.component';
import { MyCollectionComponent } from './components/my-collection/my-collection.component';
import { CollectionService } from './services/collection.service';


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
        ImportSetModalComponent,

        SpinnerDirective,
        SpinnerComponent,
        BooleanIconComponent,
        BigNumberFormatPipe,

        // Premium custom UI components
        PartsCatalogComponent,
        CatalogPartDetailComponent,
        ColourLibraryComponent,
        SystemHealthComponent,
        MyCollectionComponent,


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
        BrickElementListingComponent,
        BrickElementAddEditComponent,
        BrickElementDetailComponent,
        BrickElementTableComponent,
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
        BrickPartRelationshipListingComponent,
        BrickPartRelationshipAddEditComponent,
        BrickPartRelationshipDetailComponent,
        BrickPartRelationshipTableComponent,
        BuildManualListingComponent,
        BuildManualAddEditComponent,
        BuildManualDetailComponent,
        BuildManualTableComponent,
        BuildManualChangeHistoryListingComponent,
        BuildManualChangeHistoryAddEditComponent,
        BuildManualChangeHistoryDetailComponent,
        BuildManualChangeHistoryTableComponent,
        BuildManualPageListingComponent,
        BuildManualPageAddEditComponent,
        BuildManualPageDetailComponent,
        BuildManualPageTableComponent,
        BuildManualStepListingComponent,
        BuildManualStepAddEditComponent,
        BuildManualStepDetailComponent,
        BuildManualStepTableComponent,
        BuildStepAnnotationListingComponent,
        BuildStepAnnotationAddEditComponent,
        BuildStepAnnotationDetailComponent,
        BuildStepAnnotationTableComponent,
        BuildStepAnnotationTypeListingComponent,
        BuildStepAnnotationTypeAddEditComponent,
        BuildStepAnnotationTypeDetailComponent,
        BuildStepAnnotationTypeTableComponent,
        BuildStepPartListingComponent,
        BuildStepPartAddEditComponent,
        BuildStepPartDetailComponent,
        BuildStepPartTableComponent,
        ColourFinishListingComponent,
        ColourFinishAddEditComponent,
        ColourFinishDetailComponent,
        ColourFinishTableComponent,
        ConnectorTypeListingComponent,
        ConnectorTypeAddEditComponent,
        ConnectorTypeDetailComponent,
        ConnectorTypeTableComponent,
        ExportFormatListingComponent,
        ExportFormatAddEditComponent,
        ExportFormatDetailComponent,
        ExportFormatTableComponent,
        LegoMinifigListingComponent,
        LegoMinifigAddEditComponent,
        LegoMinifigDetailComponent,
        LegoMinifigTableComponent,
        LegoSetListingComponent,
        LegoSetAddEditComponent,
        LegoSetDetailComponent,
        LegoSetTableComponent,
        LegoSetMinifigListingComponent,
        LegoSetMinifigAddEditComponent,
        LegoSetMinifigDetailComponent,
        LegoSetMinifigTableComponent,
        LegoSetPartListingComponent,
        LegoSetPartAddEditComponent,
        LegoSetPartDetailComponent,
        LegoSetPartTableComponent,
        LegoSetSubsetListingComponent,
        LegoSetSubsetAddEditComponent,
        LegoSetSubsetDetailComponent,
        LegoSetSubsetTableComponent,
        LegoThemeListingComponent,
        LegoThemeAddEditComponent,
        LegoThemeDetailComponent,
        LegoThemeTableComponent,
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
        ProjectCameraPresetListingComponent,
        ProjectCameraPresetAddEditComponent,
        ProjectCameraPresetDetailComponent,
        ProjectCameraPresetTableComponent,
        ProjectChangeHistoryListingComponent,
        ProjectChangeHistoryAddEditComponent,
        ProjectChangeHistoryDetailComponent,
        ProjectChangeHistoryTableComponent,
        ProjectExportListingComponent,
        ProjectExportAddEditComponent,
        ProjectExportDetailComponent,
        ProjectExportTableComponent,
        ProjectReferenceImageListingComponent,
        ProjectReferenceImageAddEditComponent,
        ProjectReferenceImageDetailComponent,
        ProjectReferenceImageTableComponent,
        ProjectRenderListingComponent,
        ProjectRenderAddEditComponent,
        ProjectRenderDetailComponent,
        ProjectRenderTableComponent,
        ProjectTagListingComponent,
        ProjectTagAddEditComponent,
        ProjectTagDetailComponent,
        ProjectTagTableComponent,
        ProjectTagAssignmentListingComponent,
        ProjectTagAssignmentAddEditComponent,
        ProjectTagAssignmentDetailComponent,
        ProjectTagAssignmentTableComponent,
        RenderPresetListingComponent,
        RenderPresetAddEditComponent,
        RenderPresetDetailComponent,
        RenderPresetTableComponent,
        SubmodelListingComponent,
        SubmodelAddEditComponent,
        SubmodelDetailComponent,
        SubmodelTableComponent,
        SubmodelChangeHistoryListingComponent,
        SubmodelChangeHistoryAddEditComponent,
        SubmodelChangeHistoryDetailComponent,
        SubmodelChangeHistoryTableComponent,
        SubmodelPlacedBrickListingComponent,
        SubmodelPlacedBrickAddEditComponent,
        SubmodelPlacedBrickDetailComponent,
        SubmodelPlacedBrickTableComponent,
        UserCollectionListingComponent,
        UserCollectionAddEditComponent,
        UserCollectionDetailComponent,
        UserCollectionTableComponent,
        UserCollectionChangeHistoryListingComponent,
        UserCollectionChangeHistoryAddEditComponent,
        UserCollectionChangeHistoryDetailComponent,
        UserCollectionChangeHistoryTableComponent,
        UserCollectionPartListingComponent,
        UserCollectionPartAddEditComponent,
        UserCollectionPartDetailComponent,
        UserCollectionPartTableComponent,
        UserCollectionSetImportListingComponent,
        UserCollectionSetImportAddEditComponent,
        UserCollectionSetImportDetailComponent,
        UserCollectionSetImportTableComponent,
        UserWishlistItemListingComponent,
        UserWishlistItemAddEditComponent,
        UserWishlistItemDetailComponent,
        UserWishlistItemTableComponent,
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
        ScrollingModule,
        ToastaModule.forRoot(),
        AppRoutingModule,
    ],
    exports: [SpinnerDirective],
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
        LDrawThumbnailService,
        IndexedDBCacheService,
        CollectionService,

        //
        // Beginning of provider declarations for BMC Data Services
        //
        BMCDataServiceManagerService,
        BrickCategoryService,
        BrickColourService,
        BrickConnectionService,
        BrickElementService,
        BrickPartService,
        BrickPartChangeHistoryService,
        BrickPartColourService,
        BrickPartConnectorService,
        BrickPartRelationshipService,
        BuildManualService,
        BuildManualChangeHistoryService,
        BuildManualPageService,
        BuildManualStepService,
        BuildStepAnnotationService,
        BuildStepAnnotationTypeService,
        BuildStepPartService,
        ColourFinishService,
        ConnectorTypeService,
        ExportFormatService,
        LegoMinifigService,
        LegoSetService,
        LegoSetMinifigService,
        LegoSetPartService,
        LegoSetSubsetService,
        LegoThemeService,
        PartTypeService,
        PlacedBrickService,
        PlacedBrickChangeHistoryService,
        ProjectService,
        ProjectCameraPresetService,
        ProjectChangeHistoryService,
        ProjectExportService,
        ProjectReferenceImageService,
        ProjectRenderService,
        ProjectTagService,
        ProjectTagAssignmentService,
        RenderPresetService,
        SubmodelService,
        SubmodelChangeHistoryService,
        SubmodelPlacedBrickService,
        UserCollectionService,
        UserCollectionChangeHistoryService,
        UserCollectionPartService,
        UserCollectionSetImportService,
        UserWishlistItemService,
        //
        // End of provider declarations for BMC Data Services
        //


    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
