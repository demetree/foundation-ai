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
import { WelcomeComponent } from './components/welcome/welcome.component';
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
import { AchievementService } from './bmc-data-services/achievement.service';
import { AchievementCategoryService } from './bmc-data-services/achievement-category.service';
import { ActivityEventService } from './bmc-data-services/activity-event.service';
import { ActivityEventTypeService } from './bmc-data-services/activity-event-type.service';
import { ApiKeyService } from './bmc-data-services/api-key.service';
import { ApiRequestLogService } from './bmc-data-services/api-request-log.service';
import { BrickCategoryService } from './bmc-data-services/brick-category.service';
import { BrickColourService } from './bmc-data-services/brick-colour.service';
import { BrickConnectionService } from './bmc-data-services/brick-connection.service';
import { BrickElementService } from './bmc-data-services/brick-element.service';
import { BrickPartService } from './bmc-data-services/brick-part.service';
import { BrickPartChangeHistoryService } from './bmc-data-services/brick-part-change-history.service';
import { BrickPartColourService } from './bmc-data-services/brick-part-colour.service';
import { BrickPartConnectorService } from './bmc-data-services/brick-part-connector.service';
import { BrickPartRelationshipService } from './bmc-data-services/brick-part-relationship.service';
import { BuildChallengeService } from './bmc-data-services/build-challenge.service';
import { BuildChallengeChangeHistoryService } from './bmc-data-services/build-challenge-change-history.service';
import { BuildChallengeEntryService } from './bmc-data-services/build-challenge-entry.service';
import { BuildManualService } from './bmc-data-services/build-manual.service';
import { BuildManualChangeHistoryService } from './bmc-data-services/build-manual-change-history.service';
import { BuildManualPageService } from './bmc-data-services/build-manual-page.service';
import { BuildManualStepService } from './bmc-data-services/build-manual-step.service';
import { BuildStepAnnotationService } from './bmc-data-services/build-step-annotation.service';
import { BuildStepAnnotationTypeService } from './bmc-data-services/build-step-annotation-type.service';
import { BuildStepPartService } from './bmc-data-services/build-step-part.service';
import { ColourFinishService } from './bmc-data-services/colour-finish.service';
import { ConnectorTypeService } from './bmc-data-services/connector-type.service';
import { ConnectorTypeCompatibilityService } from './bmc-data-services/connector-type-compatibility.service';
import { ContentReportService } from './bmc-data-services/content-report.service';
import { ContentReportReasonService } from './bmc-data-services/content-report-reason.service';
import { ExportFormatService } from './bmc-data-services/export-format.service';
import { LegoMinifigService } from './bmc-data-services/lego-minifig.service';
import { LegoSetService } from './bmc-data-services/lego-set.service';
import { LegoSetMinifigService } from './bmc-data-services/lego-set-minifig.service';
import { LegoSetPartService } from './bmc-data-services/lego-set-part.service';
import { LegoSetSubsetService } from './bmc-data-services/lego-set-subset.service';
import { LegoThemeService } from './bmc-data-services/lego-theme.service';
import { MocCommentService } from './bmc-data-services/moc-comment.service';
import { MocFavouriteService } from './bmc-data-services/moc-favourite.service';
import { MocLikeService } from './bmc-data-services/moc-like.service';
import { ModelBuildStepService } from './bmc-data-services/model-build-step.service';
import { ModelDocumentService } from './bmc-data-services/model-document.service';
import { ModelDocumentChangeHistoryService } from './bmc-data-services/model-document-change-history.service';
import { ModelStepPartService } from './bmc-data-services/model-step-part.service';
import { ModelSubFileService } from './bmc-data-services/model-sub-file.service';
import { ModerationActionService } from './bmc-data-services/moderation-action.service';
import { PartSubFileReferenceService } from './bmc-data-services/part-sub-file-reference.service';
import { PartTypeService } from './bmc-data-services/part-type.service';
import { PendingRegistrationService } from './bmc-data-services/pending-registration.service';
import { PlacedBrickService } from './bmc-data-services/placed-brick.service';
import { PlacedBrickChangeHistoryService } from './bmc-data-services/placed-brick-change-history.service';
import { PlatformAnnouncementService } from './bmc-data-services/platform-announcement.service';
import { ProjectService } from './bmc-data-services/project.service';
import { ProjectCameraPresetService } from './bmc-data-services/project-camera-preset.service';
import { ProjectChangeHistoryService } from './bmc-data-services/project-change-history.service';
import { ProjectExportService } from './bmc-data-services/project-export.service';
import { ProjectReferenceImageService } from './bmc-data-services/project-reference-image.service';
import { ProjectRenderService } from './bmc-data-services/project-render.service';
import { ProjectTagService } from './bmc-data-services/project-tag.service';
import { ProjectTagAssignmentService } from './bmc-data-services/project-tag-assignment.service';
import { PublishedMocService } from './bmc-data-services/published-moc.service';
import { PublishedMocChangeHistoryService } from './bmc-data-services/published-moc-change-history.service';
import { PublishedMocImageService } from './bmc-data-services/published-moc-image.service';
import { RebrickableUserLinkService } from './bmc-data-services/rebrickable-user-link.service';
import { RenderPresetService } from './bmc-data-services/render-preset.service';
import { SharedInstructionService } from './bmc-data-services/shared-instruction.service';
import { SharedInstructionChangeHistoryService } from './bmc-data-services/shared-instruction-change-history.service';
import { SubmodelService } from './bmc-data-services/submodel.service';
import { SubmodelChangeHistoryService } from './bmc-data-services/submodel-change-history.service';
import { SubmodelPlacedBrickService } from './bmc-data-services/submodel-placed-brick.service';
import { UserAchievementService } from './bmc-data-services/user-achievement.service';
import { UserBadgeService } from './bmc-data-services/user-badge.service';
import { UserBadgeAssignmentService } from './bmc-data-services/user-badge-assignment.service';
import { UserCollectionService } from './bmc-data-services/user-collection.service';
import { UserCollectionChangeHistoryService } from './bmc-data-services/user-collection-change-history.service';
import { UserCollectionPartService } from './bmc-data-services/user-collection-part.service';
import { UserCollectionSetImportService } from './bmc-data-services/user-collection-set-import.service';
import { UserFollowService } from './bmc-data-services/user-follow.service';
import { UserLostPartService } from './bmc-data-services/user-lost-part.service';
import { UserPartListService } from './bmc-data-services/user-part-list.service';
import { UserPartListChangeHistoryService } from './bmc-data-services/user-part-list-change-history.service';
import { UserPartListItemService } from './bmc-data-services/user-part-list-item.service';
import { UserProfileService } from './bmc-data-services/user-profile.service';
import { UserProfileChangeHistoryService } from './bmc-data-services/user-profile-change-history.service';
import { UserProfileLinkService } from './bmc-data-services/user-profile-link.service';
import { UserProfileLinkTypeService } from './bmc-data-services/user-profile-link-type.service';
import { UserProfilePreferredThemeService } from './bmc-data-services/user-profile-preferred-theme.service';
import { UserProfileStatService } from './bmc-data-services/user-profile-stat.service';
import { UserSetListService } from './bmc-data-services/user-set-list.service';
import { UserSetListChangeHistoryService } from './bmc-data-services/user-set-list-change-history.service';
import { UserSetListItemService } from './bmc-data-services/user-set-list-item.service';
import { UserSetOwnershipService } from './bmc-data-services/user-set-ownership.service';
import { UserWishlistItemService } from './bmc-data-services/user-wishlist-item.service';
//
// End of imports for BMC Data Services
//

//
// Beginning of imports for BMC Data Components
//
import { AchievementListingComponent } from './bmc-data-components/achievement/achievement-listing/achievement-listing.component';
import { AchievementAddEditComponent } from './bmc-data-components/achievement/achievement-add-edit/achievement-add-edit.component';
import { AchievementDetailComponent } from './bmc-data-components/achievement/achievement-detail/achievement-detail.component';
import { AchievementTableComponent } from './bmc-data-components/achievement/achievement-table/achievement-table.component';
import { AchievementCategoryListingComponent } from './bmc-data-components/achievement-category/achievement-category-listing/achievement-category-listing.component';
import { AchievementCategoryAddEditComponent } from './bmc-data-components/achievement-category/achievement-category-add-edit/achievement-category-add-edit.component';
import { AchievementCategoryDetailComponent } from './bmc-data-components/achievement-category/achievement-category-detail/achievement-category-detail.component';
import { AchievementCategoryTableComponent } from './bmc-data-components/achievement-category/achievement-category-table/achievement-category-table.component';
import { ActivityEventListingComponent } from './bmc-data-components/activity-event/activity-event-listing/activity-event-listing.component';
import { ActivityEventAddEditComponent } from './bmc-data-components/activity-event/activity-event-add-edit/activity-event-add-edit.component';
import { ActivityEventDetailComponent } from './bmc-data-components/activity-event/activity-event-detail/activity-event-detail.component';
import { ActivityEventTableComponent } from './bmc-data-components/activity-event/activity-event-table/activity-event-table.component';
import { ActivityEventTypeListingComponent } from './bmc-data-components/activity-event-type/activity-event-type-listing/activity-event-type-listing.component';
import { ActivityEventTypeAddEditComponent } from './bmc-data-components/activity-event-type/activity-event-type-add-edit/activity-event-type-add-edit.component';
import { ActivityEventTypeDetailComponent } from './bmc-data-components/activity-event-type/activity-event-type-detail/activity-event-type-detail.component';
import { ActivityEventTypeTableComponent } from './bmc-data-components/activity-event-type/activity-event-type-table/activity-event-type-table.component';
import { ApiKeyListingComponent } from './bmc-data-components/api-key/api-key-listing/api-key-listing.component';
import { ApiKeyAddEditComponent } from './bmc-data-components/api-key/api-key-add-edit/api-key-add-edit.component';
import { ApiKeyDetailComponent } from './bmc-data-components/api-key/api-key-detail/api-key-detail.component';
import { ApiKeyTableComponent } from './bmc-data-components/api-key/api-key-table/api-key-table.component';
import { ApiRequestLogListingComponent } from './bmc-data-components/api-request-log/api-request-log-listing/api-request-log-listing.component';
import { ApiRequestLogAddEditComponent } from './bmc-data-components/api-request-log/api-request-log-add-edit/api-request-log-add-edit.component';
import { ApiRequestLogDetailComponent } from './bmc-data-components/api-request-log/api-request-log-detail/api-request-log-detail.component';
import { ApiRequestLogTableComponent } from './bmc-data-components/api-request-log/api-request-log-table/api-request-log-table.component';
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
import { BuildChallengeListingComponent } from './bmc-data-components/build-challenge/build-challenge-listing/build-challenge-listing.component';
import { BuildChallengeAddEditComponent } from './bmc-data-components/build-challenge/build-challenge-add-edit/build-challenge-add-edit.component';
import { BuildChallengeDetailComponent } from './bmc-data-components/build-challenge/build-challenge-detail/build-challenge-detail.component';
import { BuildChallengeTableComponent } from './bmc-data-components/build-challenge/build-challenge-table/build-challenge-table.component';
import { BuildChallengeChangeHistoryListingComponent } from './bmc-data-components/build-challenge-change-history/build-challenge-change-history-listing/build-challenge-change-history-listing.component';
import { BuildChallengeChangeHistoryAddEditComponent } from './bmc-data-components/build-challenge-change-history/build-challenge-change-history-add-edit/build-challenge-change-history-add-edit.component';
import { BuildChallengeChangeHistoryDetailComponent } from './bmc-data-components/build-challenge-change-history/build-challenge-change-history-detail/build-challenge-change-history-detail.component';
import { BuildChallengeChangeHistoryTableComponent } from './bmc-data-components/build-challenge-change-history/build-challenge-change-history-table/build-challenge-change-history-table.component';
import { BuildChallengeEntryListingComponent } from './bmc-data-components/build-challenge-entry/build-challenge-entry-listing/build-challenge-entry-listing.component';
import { BuildChallengeEntryAddEditComponent } from './bmc-data-components/build-challenge-entry/build-challenge-entry-add-edit/build-challenge-entry-add-edit.component';
import { BuildChallengeEntryDetailComponent } from './bmc-data-components/build-challenge-entry/build-challenge-entry-detail/build-challenge-entry-detail.component';
import { BuildChallengeEntryTableComponent } from './bmc-data-components/build-challenge-entry/build-challenge-entry-table/build-challenge-entry-table.component';
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
import { ConnectorTypeCompatibilityListingComponent } from './bmc-data-components/connector-type-compatibility/connector-type-compatibility-listing/connector-type-compatibility-listing.component';
import { ConnectorTypeCompatibilityAddEditComponent } from './bmc-data-components/connector-type-compatibility/connector-type-compatibility-add-edit/connector-type-compatibility-add-edit.component';
import { ConnectorTypeCompatibilityDetailComponent } from './bmc-data-components/connector-type-compatibility/connector-type-compatibility-detail/connector-type-compatibility-detail.component';
import { ConnectorTypeCompatibilityTableComponent } from './bmc-data-components/connector-type-compatibility/connector-type-compatibility-table/connector-type-compatibility-table.component';
import { ContentReportListingComponent } from './bmc-data-components/content-report/content-report-listing/content-report-listing.component';
import { ContentReportAddEditComponent } from './bmc-data-components/content-report/content-report-add-edit/content-report-add-edit.component';
import { ContentReportDetailComponent } from './bmc-data-components/content-report/content-report-detail/content-report-detail.component';
import { ContentReportTableComponent } from './bmc-data-components/content-report/content-report-table/content-report-table.component';
import { ContentReportReasonListingComponent } from './bmc-data-components/content-report-reason/content-report-reason-listing/content-report-reason-listing.component';
import { ContentReportReasonAddEditComponent } from './bmc-data-components/content-report-reason/content-report-reason-add-edit/content-report-reason-add-edit.component';
import { ContentReportReasonDetailComponent } from './bmc-data-components/content-report-reason/content-report-reason-detail/content-report-reason-detail.component';
import { ContentReportReasonTableComponent } from './bmc-data-components/content-report-reason/content-report-reason-table/content-report-reason-table.component';
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
import { MocCommentListingComponent } from './bmc-data-components/moc-comment/moc-comment-listing/moc-comment-listing.component';
import { MocCommentAddEditComponent } from './bmc-data-components/moc-comment/moc-comment-add-edit/moc-comment-add-edit.component';
import { MocCommentDetailComponent } from './bmc-data-components/moc-comment/moc-comment-detail/moc-comment-detail.component';
import { MocCommentTableComponent } from './bmc-data-components/moc-comment/moc-comment-table/moc-comment-table.component';
import { MocFavouriteListingComponent } from './bmc-data-components/moc-favourite/moc-favourite-listing/moc-favourite-listing.component';
import { MocFavouriteAddEditComponent } from './bmc-data-components/moc-favourite/moc-favourite-add-edit/moc-favourite-add-edit.component';
import { MocFavouriteDetailComponent } from './bmc-data-components/moc-favourite/moc-favourite-detail/moc-favourite-detail.component';
import { MocFavouriteTableComponent } from './bmc-data-components/moc-favourite/moc-favourite-table/moc-favourite-table.component';
import { MocLikeListingComponent } from './bmc-data-components/moc-like/moc-like-listing/moc-like-listing.component';
import { MocLikeAddEditComponent } from './bmc-data-components/moc-like/moc-like-add-edit/moc-like-add-edit.component';
import { MocLikeDetailComponent } from './bmc-data-components/moc-like/moc-like-detail/moc-like-detail.component';
import { MocLikeTableComponent } from './bmc-data-components/moc-like/moc-like-table/moc-like-table.component';
import { ModelBuildStepListingComponent } from './bmc-data-components/model-build-step/model-build-step-listing/model-build-step-listing.component';
import { ModelBuildStepAddEditComponent } from './bmc-data-components/model-build-step/model-build-step-add-edit/model-build-step-add-edit.component';
import { ModelBuildStepDetailComponent } from './bmc-data-components/model-build-step/model-build-step-detail/model-build-step-detail.component';
import { ModelBuildStepTableComponent } from './bmc-data-components/model-build-step/model-build-step-table/model-build-step-table.component';
import { ModelDocumentListingComponent } from './bmc-data-components/model-document/model-document-listing/model-document-listing.component';
import { ModelDocumentAddEditComponent } from './bmc-data-components/model-document/model-document-add-edit/model-document-add-edit.component';
import { ModelDocumentDetailComponent } from './bmc-data-components/model-document/model-document-detail/model-document-detail.component';
import { ModelDocumentTableComponent } from './bmc-data-components/model-document/model-document-table/model-document-table.component';
import { ModelDocumentChangeHistoryListingComponent } from './bmc-data-components/model-document-change-history/model-document-change-history-listing/model-document-change-history-listing.component';
import { ModelDocumentChangeHistoryAddEditComponent } from './bmc-data-components/model-document-change-history/model-document-change-history-add-edit/model-document-change-history-add-edit.component';
import { ModelDocumentChangeHistoryDetailComponent } from './bmc-data-components/model-document-change-history/model-document-change-history-detail/model-document-change-history-detail.component';
import { ModelDocumentChangeHistoryTableComponent } from './bmc-data-components/model-document-change-history/model-document-change-history-table/model-document-change-history-table.component';
import { ModelStepPartListingComponent } from './bmc-data-components/model-step-part/model-step-part-listing/model-step-part-listing.component';
import { ModelStepPartAddEditComponent } from './bmc-data-components/model-step-part/model-step-part-add-edit/model-step-part-add-edit.component';
import { ModelStepPartDetailComponent } from './bmc-data-components/model-step-part/model-step-part-detail/model-step-part-detail.component';
import { ModelStepPartTableComponent } from './bmc-data-components/model-step-part/model-step-part-table/model-step-part-table.component';
import { ModelSubFileListingComponent } from './bmc-data-components/model-sub-file/model-sub-file-listing/model-sub-file-listing.component';
import { ModelSubFileAddEditComponent } from './bmc-data-components/model-sub-file/model-sub-file-add-edit/model-sub-file-add-edit.component';
import { ModelSubFileDetailComponent } from './bmc-data-components/model-sub-file/model-sub-file-detail/model-sub-file-detail.component';
import { ModelSubFileTableComponent } from './bmc-data-components/model-sub-file/model-sub-file-table/model-sub-file-table.component';
import { ModerationActionListingComponent } from './bmc-data-components/moderation-action/moderation-action-listing/moderation-action-listing.component';
import { ModerationActionAddEditComponent } from './bmc-data-components/moderation-action/moderation-action-add-edit/moderation-action-add-edit.component';
import { ModerationActionDetailComponent } from './bmc-data-components/moderation-action/moderation-action-detail/moderation-action-detail.component';
import { ModerationActionTableComponent } from './bmc-data-components/moderation-action/moderation-action-table/moderation-action-table.component';
import { PartSubFileReferenceListingComponent } from './bmc-data-components/part-sub-file-reference/part-sub-file-reference-listing/part-sub-file-reference-listing.component';
import { PartSubFileReferenceAddEditComponent } from './bmc-data-components/part-sub-file-reference/part-sub-file-reference-add-edit/part-sub-file-reference-add-edit.component';
import { PartSubFileReferenceDetailComponent } from './bmc-data-components/part-sub-file-reference/part-sub-file-reference-detail/part-sub-file-reference-detail.component';
import { PartSubFileReferenceTableComponent } from './bmc-data-components/part-sub-file-reference/part-sub-file-reference-table/part-sub-file-reference-table.component';
import { PartTypeListingComponent } from './bmc-data-components/part-type/part-type-listing/part-type-listing.component';
import { PartTypeAddEditComponent } from './bmc-data-components/part-type/part-type-add-edit/part-type-add-edit.component';
import { PartTypeDetailComponent } from './bmc-data-components/part-type/part-type-detail/part-type-detail.component';
import { PartTypeTableComponent } from './bmc-data-components/part-type/part-type-table/part-type-table.component';
import { PendingRegistrationListingComponent } from './bmc-data-components/pending-registration/pending-registration-listing/pending-registration-listing.component';
import { PendingRegistrationAddEditComponent } from './bmc-data-components/pending-registration/pending-registration-add-edit/pending-registration-add-edit.component';
import { PendingRegistrationDetailComponent } from './bmc-data-components/pending-registration/pending-registration-detail/pending-registration-detail.component';
import { PendingRegistrationTableComponent } from './bmc-data-components/pending-registration/pending-registration-table/pending-registration-table.component';
import { PlacedBrickListingComponent } from './bmc-data-components/placed-brick/placed-brick-listing/placed-brick-listing.component';
import { PlacedBrickAddEditComponent } from './bmc-data-components/placed-brick/placed-brick-add-edit/placed-brick-add-edit.component';
import { PlacedBrickDetailComponent } from './bmc-data-components/placed-brick/placed-brick-detail/placed-brick-detail.component';
import { PlacedBrickTableComponent } from './bmc-data-components/placed-brick/placed-brick-table/placed-brick-table.component';
import { PlacedBrickChangeHistoryListingComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-listing/placed-brick-change-history-listing.component';
import { PlacedBrickChangeHistoryAddEditComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-add-edit/placed-brick-change-history-add-edit.component';
import { PlacedBrickChangeHistoryDetailComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-detail/placed-brick-change-history-detail.component';
import { PlacedBrickChangeHistoryTableComponent } from './bmc-data-components/placed-brick-change-history/placed-brick-change-history-table/placed-brick-change-history-table.component';
import { PlatformAnnouncementListingComponent } from './bmc-data-components/platform-announcement/platform-announcement-listing/platform-announcement-listing.component';
import { PlatformAnnouncementAddEditComponent } from './bmc-data-components/platform-announcement/platform-announcement-add-edit/platform-announcement-add-edit.component';
import { PlatformAnnouncementDetailComponent } from './bmc-data-components/platform-announcement/platform-announcement-detail/platform-announcement-detail.component';
import { PlatformAnnouncementTableComponent } from './bmc-data-components/platform-announcement/platform-announcement-table/platform-announcement-table.component';
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
import { PublishedMocListingComponent } from './bmc-data-components/published-moc/published-moc-listing/published-moc-listing.component';
import { PublishedMocAddEditComponent } from './bmc-data-components/published-moc/published-moc-add-edit/published-moc-add-edit.component';
import { PublishedMocDetailComponent } from './bmc-data-components/published-moc/published-moc-detail/published-moc-detail.component';
import { PublishedMocTableComponent } from './bmc-data-components/published-moc/published-moc-table/published-moc-table.component';
import { PublishedMocChangeHistoryListingComponent } from './bmc-data-components/published-moc-change-history/published-moc-change-history-listing/published-moc-change-history-listing.component';
import { PublishedMocChangeHistoryAddEditComponent } from './bmc-data-components/published-moc-change-history/published-moc-change-history-add-edit/published-moc-change-history-add-edit.component';
import { PublishedMocChangeHistoryDetailComponent } from './bmc-data-components/published-moc-change-history/published-moc-change-history-detail/published-moc-change-history-detail.component';
import { PublishedMocChangeHistoryTableComponent } from './bmc-data-components/published-moc-change-history/published-moc-change-history-table/published-moc-change-history-table.component';
import { PublishedMocImageListingComponent } from './bmc-data-components/published-moc-image/published-moc-image-listing/published-moc-image-listing.component';
import { PublishedMocImageAddEditComponent } from './bmc-data-components/published-moc-image/published-moc-image-add-edit/published-moc-image-add-edit.component';
import { PublishedMocImageDetailComponent } from './bmc-data-components/published-moc-image/published-moc-image-detail/published-moc-image-detail.component';
import { PublishedMocImageTableComponent } from './bmc-data-components/published-moc-image/published-moc-image-table/published-moc-image-table.component';
import { RebrickableUserLinkListingComponent } from './bmc-data-components/rebrickable-user-link/rebrickable-user-link-listing/rebrickable-user-link-listing.component';
import { RebrickableUserLinkAddEditComponent } from './bmc-data-components/rebrickable-user-link/rebrickable-user-link-add-edit/rebrickable-user-link-add-edit.component';
import { RebrickableUserLinkDetailComponent } from './bmc-data-components/rebrickable-user-link/rebrickable-user-link-detail/rebrickable-user-link-detail.component';
import { RebrickableUserLinkTableComponent } from './bmc-data-components/rebrickable-user-link/rebrickable-user-link-table/rebrickable-user-link-table.component';
import { RenderPresetListingComponent } from './bmc-data-components/render-preset/render-preset-listing/render-preset-listing.component';
import { RenderPresetAddEditComponent } from './bmc-data-components/render-preset/render-preset-add-edit/render-preset-add-edit.component';
import { RenderPresetDetailComponent } from './bmc-data-components/render-preset/render-preset-detail/render-preset-detail.component';
import { RenderPresetTableComponent } from './bmc-data-components/render-preset/render-preset-table/render-preset-table.component';
import { SharedInstructionListingComponent } from './bmc-data-components/shared-instruction/shared-instruction-listing/shared-instruction-listing.component';
import { SharedInstructionAddEditComponent } from './bmc-data-components/shared-instruction/shared-instruction-add-edit/shared-instruction-add-edit.component';
import { SharedInstructionDetailComponent } from './bmc-data-components/shared-instruction/shared-instruction-detail/shared-instruction-detail.component';
import { SharedInstructionTableComponent } from './bmc-data-components/shared-instruction/shared-instruction-table/shared-instruction-table.component';
import { SharedInstructionChangeHistoryListingComponent } from './bmc-data-components/shared-instruction-change-history/shared-instruction-change-history-listing/shared-instruction-change-history-listing.component';
import { SharedInstructionChangeHistoryAddEditComponent } from './bmc-data-components/shared-instruction-change-history/shared-instruction-change-history-add-edit/shared-instruction-change-history-add-edit.component';
import { SharedInstructionChangeHistoryDetailComponent } from './bmc-data-components/shared-instruction-change-history/shared-instruction-change-history-detail/shared-instruction-change-history-detail.component';
import { SharedInstructionChangeHistoryTableComponent } from './bmc-data-components/shared-instruction-change-history/shared-instruction-change-history-table/shared-instruction-change-history-table.component';
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
import { UserAchievementListingComponent } from './bmc-data-components/user-achievement/user-achievement-listing/user-achievement-listing.component';
import { UserAchievementAddEditComponent } from './bmc-data-components/user-achievement/user-achievement-add-edit/user-achievement-add-edit.component';
import { UserAchievementDetailComponent } from './bmc-data-components/user-achievement/user-achievement-detail/user-achievement-detail.component';
import { UserAchievementTableComponent } from './bmc-data-components/user-achievement/user-achievement-table/user-achievement-table.component';
import { UserBadgeListingComponent } from './bmc-data-components/user-badge/user-badge-listing/user-badge-listing.component';
import { UserBadgeAddEditComponent } from './bmc-data-components/user-badge/user-badge-add-edit/user-badge-add-edit.component';
import { UserBadgeDetailComponent } from './bmc-data-components/user-badge/user-badge-detail/user-badge-detail.component';
import { UserBadgeTableComponent } from './bmc-data-components/user-badge/user-badge-table/user-badge-table.component';
import { UserBadgeAssignmentListingComponent } from './bmc-data-components/user-badge-assignment/user-badge-assignment-listing/user-badge-assignment-listing.component';
import { UserBadgeAssignmentAddEditComponent } from './bmc-data-components/user-badge-assignment/user-badge-assignment-add-edit/user-badge-assignment-add-edit.component';
import { UserBadgeAssignmentDetailComponent } from './bmc-data-components/user-badge-assignment/user-badge-assignment-detail/user-badge-assignment-detail.component';
import { UserBadgeAssignmentTableComponent } from './bmc-data-components/user-badge-assignment/user-badge-assignment-table/user-badge-assignment-table.component';
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
import { UserFollowListingComponent } from './bmc-data-components/user-follow/user-follow-listing/user-follow-listing.component';
import { UserFollowAddEditComponent } from './bmc-data-components/user-follow/user-follow-add-edit/user-follow-add-edit.component';
import { UserFollowDetailComponent } from './bmc-data-components/user-follow/user-follow-detail/user-follow-detail.component';
import { UserFollowTableComponent } from './bmc-data-components/user-follow/user-follow-table/user-follow-table.component';
import { UserLostPartListingComponent } from './bmc-data-components/user-lost-part/user-lost-part-listing/user-lost-part-listing.component';
import { UserLostPartAddEditComponent } from './bmc-data-components/user-lost-part/user-lost-part-add-edit/user-lost-part-add-edit.component';
import { UserLostPartDetailComponent } from './bmc-data-components/user-lost-part/user-lost-part-detail/user-lost-part-detail.component';
import { UserLostPartTableComponent } from './bmc-data-components/user-lost-part/user-lost-part-table/user-lost-part-table.component';
import { UserPartListListingComponent } from './bmc-data-components/user-part-list/user-part-list-listing/user-part-list-listing.component';
import { UserPartListAddEditComponent } from './bmc-data-components/user-part-list/user-part-list-add-edit/user-part-list-add-edit.component';
import { UserPartListDetailComponent } from './bmc-data-components/user-part-list/user-part-list-detail/user-part-list-detail.component';
import { UserPartListTableComponent } from './bmc-data-components/user-part-list/user-part-list-table/user-part-list-table.component';
import { UserPartListChangeHistoryListingComponent } from './bmc-data-components/user-part-list-change-history/user-part-list-change-history-listing/user-part-list-change-history-listing.component';
import { UserPartListChangeHistoryAddEditComponent } from './bmc-data-components/user-part-list-change-history/user-part-list-change-history-add-edit/user-part-list-change-history-add-edit.component';
import { UserPartListChangeHistoryDetailComponent } from './bmc-data-components/user-part-list-change-history/user-part-list-change-history-detail/user-part-list-change-history-detail.component';
import { UserPartListChangeHistoryTableComponent } from './bmc-data-components/user-part-list-change-history/user-part-list-change-history-table/user-part-list-change-history-table.component';
import { UserPartListItemListingComponent } from './bmc-data-components/user-part-list-item/user-part-list-item-listing/user-part-list-item-listing.component';
import { UserPartListItemAddEditComponent } from './bmc-data-components/user-part-list-item/user-part-list-item-add-edit/user-part-list-item-add-edit.component';
import { UserPartListItemDetailComponent } from './bmc-data-components/user-part-list-item/user-part-list-item-detail/user-part-list-item-detail.component';
import { UserPartListItemTableComponent } from './bmc-data-components/user-part-list-item/user-part-list-item-table/user-part-list-item-table.component';
import { UserProfileListingComponent } from './bmc-data-components/user-profile/user-profile-listing/user-profile-listing.component';
import { UserProfileAddEditComponent } from './bmc-data-components/user-profile/user-profile-add-edit/user-profile-add-edit.component';
import { UserProfileDetailComponent } from './bmc-data-components/user-profile/user-profile-detail/user-profile-detail.component';
import { UserProfileTableComponent } from './bmc-data-components/user-profile/user-profile-table/user-profile-table.component';
import { UserProfileChangeHistoryListingComponent } from './bmc-data-components/user-profile-change-history/user-profile-change-history-listing/user-profile-change-history-listing.component';
import { UserProfileChangeHistoryAddEditComponent } from './bmc-data-components/user-profile-change-history/user-profile-change-history-add-edit/user-profile-change-history-add-edit.component';
import { UserProfileChangeHistoryDetailComponent } from './bmc-data-components/user-profile-change-history/user-profile-change-history-detail/user-profile-change-history-detail.component';
import { UserProfileChangeHistoryTableComponent } from './bmc-data-components/user-profile-change-history/user-profile-change-history-table/user-profile-change-history-table.component';
import { UserProfileLinkListingComponent } from './bmc-data-components/user-profile-link/user-profile-link-listing/user-profile-link-listing.component';
import { UserProfileLinkAddEditComponent } from './bmc-data-components/user-profile-link/user-profile-link-add-edit/user-profile-link-add-edit.component';
import { UserProfileLinkDetailComponent } from './bmc-data-components/user-profile-link/user-profile-link-detail/user-profile-link-detail.component';
import { UserProfileLinkTableComponent } from './bmc-data-components/user-profile-link/user-profile-link-table/user-profile-link-table.component';
import { UserProfileLinkTypeListingComponent } from './bmc-data-components/user-profile-link-type/user-profile-link-type-listing/user-profile-link-type-listing.component';
import { UserProfileLinkTypeAddEditComponent } from './bmc-data-components/user-profile-link-type/user-profile-link-type-add-edit/user-profile-link-type-add-edit.component';
import { UserProfileLinkTypeDetailComponent } from './bmc-data-components/user-profile-link-type/user-profile-link-type-detail/user-profile-link-type-detail.component';
import { UserProfileLinkTypeTableComponent } from './bmc-data-components/user-profile-link-type/user-profile-link-type-table/user-profile-link-type-table.component';
import { UserProfilePreferredThemeListingComponent } from './bmc-data-components/user-profile-preferred-theme/user-profile-preferred-theme-listing/user-profile-preferred-theme-listing.component';
import { UserProfilePreferredThemeAddEditComponent } from './bmc-data-components/user-profile-preferred-theme/user-profile-preferred-theme-add-edit/user-profile-preferred-theme-add-edit.component';
import { UserProfilePreferredThemeDetailComponent } from './bmc-data-components/user-profile-preferred-theme/user-profile-preferred-theme-detail/user-profile-preferred-theme-detail.component';
import { UserProfilePreferredThemeTableComponent } from './bmc-data-components/user-profile-preferred-theme/user-profile-preferred-theme-table/user-profile-preferred-theme-table.component';
import { UserProfileStatListingComponent } from './bmc-data-components/user-profile-stat/user-profile-stat-listing/user-profile-stat-listing.component';
import { UserProfileStatAddEditComponent } from './bmc-data-components/user-profile-stat/user-profile-stat-add-edit/user-profile-stat-add-edit.component';
import { UserProfileStatDetailComponent } from './bmc-data-components/user-profile-stat/user-profile-stat-detail/user-profile-stat-detail.component';
import { UserProfileStatTableComponent } from './bmc-data-components/user-profile-stat/user-profile-stat-table/user-profile-stat-table.component';
import { UserSetListListingComponent } from './bmc-data-components/user-set-list/user-set-list-listing/user-set-list-listing.component';
import { UserSetListAddEditComponent } from './bmc-data-components/user-set-list/user-set-list-add-edit/user-set-list-add-edit.component';
import { UserSetListDetailComponent } from './bmc-data-components/user-set-list/user-set-list-detail/user-set-list-detail.component';
import { UserSetListTableComponent } from './bmc-data-components/user-set-list/user-set-list-table/user-set-list-table.component';
import { UserSetListChangeHistoryListingComponent } from './bmc-data-components/user-set-list-change-history/user-set-list-change-history-listing/user-set-list-change-history-listing.component';
import { UserSetListChangeHistoryAddEditComponent } from './bmc-data-components/user-set-list-change-history/user-set-list-change-history-add-edit/user-set-list-change-history-add-edit.component';
import { UserSetListChangeHistoryDetailComponent } from './bmc-data-components/user-set-list-change-history/user-set-list-change-history-detail/user-set-list-change-history-detail.component';
import { UserSetListChangeHistoryTableComponent } from './bmc-data-components/user-set-list-change-history/user-set-list-change-history-table/user-set-list-change-history-table.component';
import { UserSetListItemListingComponent } from './bmc-data-components/user-set-list-item/user-set-list-item-listing/user-set-list-item-listing.component';
import { UserSetListItemAddEditComponent } from './bmc-data-components/user-set-list-item/user-set-list-item-add-edit/user-set-list-item-add-edit.component';
import { UserSetListItemDetailComponent } from './bmc-data-components/user-set-list-item/user-set-list-item-detail/user-set-list-item-detail.component';
import { UserSetListItemTableComponent } from './bmc-data-components/user-set-list-item/user-set-list-item-table/user-set-list-item-table.component';
import { UserSetOwnershipListingComponent } from './bmc-data-components/user-set-ownership/user-set-ownership-listing/user-set-ownership-listing.component';
import { UserSetOwnershipAddEditComponent } from './bmc-data-components/user-set-ownership/user-set-ownership-add-edit/user-set-ownership-add-edit.component';
import { UserSetOwnershipDetailComponent } from './bmc-data-components/user-set-ownership/user-set-ownership-detail/user-set-ownership-detail.component';
import { UserSetOwnershipTableComponent } from './bmc-data-components/user-set-ownership/user-set-ownership-table/user-set-ownership-table.component';
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
import { ProfileComponent } from './components/profile/profile.component';
import { ProfileSettingsComponent } from './components/profile-settings/profile-settings.component';
import { PublicProfileComponent } from './components/public-profile/public-profile.component';
import { MatrixRainComponent } from './components/matrix-rain/matrix-rain.component';
import { AiAssistantComponent } from './components/ai-assistant/ai-assistant.component';
import { LegoUniverseComponent } from './components/lego-universe/lego-universe.component';
import { SetExplorerComponent } from './components/set-explorer/set-explorer.component';
import { SetDetailComponent } from './components/set-detail/set-detail.component';
import { MinifigGalleryComponent } from './components/minifig-gallery/minifig-gallery.component';
import { MinifigDetailComponent } from './components/minifig-detail/minifig-detail.component';
import { ThemeExplorerComponent } from './components/theme-explorer/theme-explorer.component';
import { ThemeDetailComponent } from './components/theme-detail/theme-detail.component';
import { PartsUniverseComponent } from './components/parts-universe/parts-universe.component';
import { SetComparisonComponent } from './components/set-comparison/set-comparison.component';
import { PartRendererComponent } from './components/part-renderer/part-renderer.component';
import { ManualGeneratorComponent } from './components/manual-generator/manual-generator.component';
import { CollectionService } from './services/collection.service';
import { PublicLandingComponent } from './components/public-landing/public-landing.component';


@NgModule({
    declarations: [
        AppComponent,
        HeaderComponent,
        SidebarComponent,
        LoginComponent,
        DashboardComponent,
        WelcomeComponent,
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
        ProfileComponent,
        ProfileSettingsComponent,
        PublicProfileComponent,
        MatrixRainComponent,
        AiAssistantComponent,
        LegoUniverseComponent,
        SetExplorerComponent,
        SetDetailComponent,
        MinifigGalleryComponent,
        MinifigDetailComponent,
        ThemeExplorerComponent,
        ThemeDetailComponent,
        PartsUniverseComponent,
        SetComparisonComponent,
        PartRendererComponent,
        ManualGeneratorComponent,
        PublicLandingComponent,


        //
        // Beginning of declarations for BMC Data Components
//
AchievementListingComponent,
AchievementAddEditComponent,
AchievementDetailComponent,
AchievementTableComponent,
AchievementCategoryListingComponent,
AchievementCategoryAddEditComponent,
AchievementCategoryDetailComponent,
AchievementCategoryTableComponent,
ActivityEventListingComponent,
ActivityEventAddEditComponent,
ActivityEventDetailComponent,
ActivityEventTableComponent,
ActivityEventTypeListingComponent,
ActivityEventTypeAddEditComponent,
ActivityEventTypeDetailComponent,
ActivityEventTypeTableComponent,
ApiKeyListingComponent,
ApiKeyAddEditComponent,
ApiKeyDetailComponent,
ApiKeyTableComponent,
ApiRequestLogListingComponent,
ApiRequestLogAddEditComponent,
ApiRequestLogDetailComponent,
ApiRequestLogTableComponent,
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
BuildChallengeListingComponent,
BuildChallengeAddEditComponent,
BuildChallengeDetailComponent,
BuildChallengeTableComponent,
BuildChallengeChangeHistoryListingComponent,
BuildChallengeChangeHistoryAddEditComponent,
BuildChallengeChangeHistoryDetailComponent,
BuildChallengeChangeHistoryTableComponent,
BuildChallengeEntryListingComponent,
BuildChallengeEntryAddEditComponent,
BuildChallengeEntryDetailComponent,
BuildChallengeEntryTableComponent,
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
ConnectorTypeCompatibilityListingComponent,
ConnectorTypeCompatibilityAddEditComponent,
ConnectorTypeCompatibilityDetailComponent,
ConnectorTypeCompatibilityTableComponent,
ContentReportListingComponent,
ContentReportAddEditComponent,
ContentReportDetailComponent,
ContentReportTableComponent,
ContentReportReasonListingComponent,
ContentReportReasonAddEditComponent,
ContentReportReasonDetailComponent,
ContentReportReasonTableComponent,
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
MocCommentListingComponent,
MocCommentAddEditComponent,
MocCommentDetailComponent,
MocCommentTableComponent,
MocFavouriteListingComponent,
MocFavouriteAddEditComponent,
MocFavouriteDetailComponent,
MocFavouriteTableComponent,
MocLikeListingComponent,
MocLikeAddEditComponent,
MocLikeDetailComponent,
MocLikeTableComponent,
ModelBuildStepListingComponent,
ModelBuildStepAddEditComponent,
ModelBuildStepDetailComponent,
ModelBuildStepTableComponent,
ModelDocumentListingComponent,
ModelDocumentAddEditComponent,
ModelDocumentDetailComponent,
ModelDocumentTableComponent,
ModelDocumentChangeHistoryListingComponent,
ModelDocumentChangeHistoryAddEditComponent,
ModelDocumentChangeHistoryDetailComponent,
ModelDocumentChangeHistoryTableComponent,
ModelStepPartListingComponent,
ModelStepPartAddEditComponent,
ModelStepPartDetailComponent,
ModelStepPartTableComponent,
ModelSubFileListingComponent,
ModelSubFileAddEditComponent,
ModelSubFileDetailComponent,
ModelSubFileTableComponent,
ModerationActionListingComponent,
ModerationActionAddEditComponent,
ModerationActionDetailComponent,
ModerationActionTableComponent,
PartSubFileReferenceListingComponent,
PartSubFileReferenceAddEditComponent,
PartSubFileReferenceDetailComponent,
PartSubFileReferenceTableComponent,
PartTypeListingComponent,
PartTypeAddEditComponent,
PartTypeDetailComponent,
PartTypeTableComponent,
PendingRegistrationListingComponent,
PendingRegistrationAddEditComponent,
PendingRegistrationDetailComponent,
PendingRegistrationTableComponent,
PlacedBrickListingComponent,
PlacedBrickAddEditComponent,
PlacedBrickDetailComponent,
PlacedBrickTableComponent,
PlacedBrickChangeHistoryListingComponent,
PlacedBrickChangeHistoryAddEditComponent,
PlacedBrickChangeHistoryDetailComponent,
PlacedBrickChangeHistoryTableComponent,
PlatformAnnouncementListingComponent,
PlatformAnnouncementAddEditComponent,
PlatformAnnouncementDetailComponent,
PlatformAnnouncementTableComponent,
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
PublishedMocListingComponent,
PublishedMocAddEditComponent,
PublishedMocDetailComponent,
PublishedMocTableComponent,
PublishedMocChangeHistoryListingComponent,
PublishedMocChangeHistoryAddEditComponent,
PublishedMocChangeHistoryDetailComponent,
PublishedMocChangeHistoryTableComponent,
PublishedMocImageListingComponent,
PublishedMocImageAddEditComponent,
PublishedMocImageDetailComponent,
PublishedMocImageTableComponent,
RebrickableUserLinkListingComponent,
RebrickableUserLinkAddEditComponent,
RebrickableUserLinkDetailComponent,
RebrickableUserLinkTableComponent,
RenderPresetListingComponent,
RenderPresetAddEditComponent,
RenderPresetDetailComponent,
RenderPresetTableComponent,
SharedInstructionListingComponent,
SharedInstructionAddEditComponent,
SharedInstructionDetailComponent,
SharedInstructionTableComponent,
SharedInstructionChangeHistoryListingComponent,
SharedInstructionChangeHistoryAddEditComponent,
SharedInstructionChangeHistoryDetailComponent,
SharedInstructionChangeHistoryTableComponent,
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
UserAchievementListingComponent,
UserAchievementAddEditComponent,
UserAchievementDetailComponent,
UserAchievementTableComponent,
UserBadgeListingComponent,
UserBadgeAddEditComponent,
UserBadgeDetailComponent,
UserBadgeTableComponent,
UserBadgeAssignmentListingComponent,
UserBadgeAssignmentAddEditComponent,
UserBadgeAssignmentDetailComponent,
UserBadgeAssignmentTableComponent,
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
UserFollowListingComponent,
UserFollowAddEditComponent,
UserFollowDetailComponent,
UserFollowTableComponent,
UserLostPartListingComponent,
UserLostPartAddEditComponent,
UserLostPartDetailComponent,
UserLostPartTableComponent,
UserPartListListingComponent,
UserPartListAddEditComponent,
UserPartListDetailComponent,
UserPartListTableComponent,
UserPartListChangeHistoryListingComponent,
UserPartListChangeHistoryAddEditComponent,
UserPartListChangeHistoryDetailComponent,
UserPartListChangeHistoryTableComponent,
UserPartListItemListingComponent,
UserPartListItemAddEditComponent,
UserPartListItemDetailComponent,
UserPartListItemTableComponent,
UserProfileListingComponent,
UserProfileAddEditComponent,
UserProfileDetailComponent,
UserProfileTableComponent,
UserProfileChangeHistoryListingComponent,
UserProfileChangeHistoryAddEditComponent,
UserProfileChangeHistoryDetailComponent,
UserProfileChangeHistoryTableComponent,
UserProfileLinkListingComponent,
UserProfileLinkAddEditComponent,
UserProfileLinkDetailComponent,
UserProfileLinkTableComponent,
UserProfileLinkTypeListingComponent,
UserProfileLinkTypeAddEditComponent,
UserProfileLinkTypeDetailComponent,
UserProfileLinkTypeTableComponent,
UserProfilePreferredThemeListingComponent,
UserProfilePreferredThemeAddEditComponent,
UserProfilePreferredThemeDetailComponent,
UserProfilePreferredThemeTableComponent,
UserProfileStatListingComponent,
UserProfileStatAddEditComponent,
UserProfileStatDetailComponent,
UserProfileStatTableComponent,
UserSetListListingComponent,
UserSetListAddEditComponent,
UserSetListDetailComponent,
UserSetListTableComponent,
UserSetListChangeHistoryListingComponent,
UserSetListChangeHistoryAddEditComponent,
UserSetListChangeHistoryDetailComponent,
UserSetListChangeHistoryTableComponent,
UserSetListItemListingComponent,
UserSetListItemAddEditComponent,
UserSetListItemDetailComponent,
UserSetListItemTableComponent,
UserSetOwnershipListingComponent,
UserSetOwnershipAddEditComponent,
UserSetOwnershipDetailComponent,
UserSetOwnershipTableComponent,
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
AchievementService,
AchievementCategoryService,
ActivityEventService,
ActivityEventTypeService,
ApiKeyService,
ApiRequestLogService,
BrickCategoryService,
BrickColourService,
BrickConnectionService,
BrickElementService,
BrickPartService,
BrickPartChangeHistoryService,
BrickPartColourService,
BrickPartConnectorService,
BrickPartRelationshipService,
BuildChallengeService,
BuildChallengeChangeHistoryService,
BuildChallengeEntryService,
BuildManualService,
BuildManualChangeHistoryService,
BuildManualPageService,
BuildManualStepService,
BuildStepAnnotationService,
BuildStepAnnotationTypeService,
BuildStepPartService,
ColourFinishService,
ConnectorTypeService,
ConnectorTypeCompatibilityService,
ContentReportService,
ContentReportReasonService,
ExportFormatService,
LegoMinifigService,
LegoSetService,
LegoSetMinifigService,
LegoSetPartService,
LegoSetSubsetService,
LegoThemeService,
MocCommentService,
MocFavouriteService,
MocLikeService,
ModelBuildStepService,
ModelDocumentService,
ModelDocumentChangeHistoryService,
ModelStepPartService,
ModelSubFileService,
ModerationActionService,
PartSubFileReferenceService,
PartTypeService,
PendingRegistrationService,
PlacedBrickService,
PlacedBrickChangeHistoryService,
PlatformAnnouncementService,
ProjectService,
ProjectCameraPresetService,
ProjectChangeHistoryService,
ProjectExportService,
ProjectReferenceImageService,
ProjectRenderService,
ProjectTagService,
ProjectTagAssignmentService,
PublishedMocService,
PublishedMocChangeHistoryService,
PublishedMocImageService,
RebrickableUserLinkService,
RenderPresetService,
SharedInstructionService,
SharedInstructionChangeHistoryService,
SubmodelService,
SubmodelChangeHistoryService,
SubmodelPlacedBrickService,
UserAchievementService,
UserBadgeService,
UserBadgeAssignmentService,
UserCollectionService,
UserCollectionChangeHistoryService,
UserCollectionPartService,
UserCollectionSetImportService,
UserFollowService,
UserLostPartService,
UserPartListService,
UserPartListChangeHistoryService,
UserPartListItemService,
UserProfileService,
UserProfileChangeHistoryService,
UserProfileLinkService,
UserProfileLinkTypeService,
UserProfilePreferredThemeService,
UserProfileStatService,
UserSetListService,
UserSetListChangeHistoryService,
UserSetListItemService,
UserSetOwnershipService,
UserWishlistItemService,
//
        // End of provider declarations for BMC Data Services
        //


    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
