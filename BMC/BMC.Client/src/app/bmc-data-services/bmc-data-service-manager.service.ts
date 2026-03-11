/*

   GENERATED SERVICE FOR THE BMC TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BMC table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import {Injectable} from '@angular/core';
import {AchievementService} from  './achievement.service';
import {AchievementCategoryService} from  './achievement-category.service';
import {ActivityEventService} from  './activity-event.service';
import {ActivityEventTypeService} from  './activity-event-type.service';
import {ApiKeyService} from  './api-key.service';
import {ApiRequestLogService} from  './api-request-log.service';
import {BrickCategoryService} from  './brick-category.service';
import {BrickColourService} from  './brick-colour.service';
import {BrickConnectionService} from  './brick-connection.service';
import {BrickEconomyTransactionService} from  './brick-economy-transaction.service';
import {BrickEconomyUserLinkService} from  './brick-economy-user-link.service';
import {BrickElementService} from  './brick-element.service';
import {BrickLinkTransactionService} from  './brick-link-transaction.service';
import {BrickLinkUserLinkService} from  './brick-link-user-link.service';
import {BrickOwlTransactionService} from  './brick-owl-transaction.service';
import {BrickOwlUserLinkService} from  './brick-owl-user-link.service';
import {BrickPartService} from  './brick-part.service';
import {BrickPartChangeHistoryService} from  './brick-part-change-history.service';
import {BrickPartColourService} from  './brick-part-colour.service';
import {BrickPartConnectorService} from  './brick-part-connector.service';
import {BrickPartRelationshipService} from  './brick-part-relationship.service';
import {BrickSetSetReviewService} from  './brick-set-set-review.service';
import {BrickSetTransactionService} from  './brick-set-transaction.service';
import {BrickSetUserLinkService} from  './brick-set-user-link.service';
import {BuildChallengeService} from  './build-challenge.service';
import {BuildChallengeChangeHistoryService} from  './build-challenge-change-history.service';
import {BuildChallengeEntryService} from  './build-challenge-entry.service';
import {BuildManualService} from  './build-manual.service';
import {BuildManualChangeHistoryService} from  './build-manual-change-history.service';
import {BuildManualPageService} from  './build-manual-page.service';
import {BuildManualStepService} from  './build-manual-step.service';
import {BuildStepAnnotationService} from  './build-step-annotation.service';
import {BuildStepAnnotationTypeService} from  './build-step-annotation-type.service';
import {BuildStepPartService} from  './build-step-part.service';
import {ColourFinishService} from  './colour-finish.service';
import {ConnectorTypeService} from  './connector-type.service';
import {ConnectorTypeCompatibilityService} from  './connector-type-compatibility.service';
import {ContentReportService} from  './content-report.service';
import {ContentReportReasonService} from  './content-report-reason.service';
import {ExportFormatService} from  './export-format.service';
import {LegoMinifigService} from  './lego-minifig.service';
import {LegoSetService} from  './lego-set.service';
import {LegoSetMinifigService} from  './lego-set-minifig.service';
import {LegoSetPartService} from  './lego-set-part.service';
import {LegoSetSubsetService} from  './lego-set-subset.service';
import {LegoThemeService} from  './lego-theme.service';
import {MarketDataCacheService} from  './market-data-cache.service';
import {MocCommentService} from  './moc-comment.service';
import {MocFavouriteService} from  './moc-favourite.service';
import {MocLikeService} from  './moc-like.service';
import {ModelBuildStepService} from  './model-build-step.service';
import {ModelDocumentService} from  './model-document.service';
import {ModelDocumentChangeHistoryService} from  './model-document-change-history.service';
import {ModelStepPartService} from  './model-step-part.service';
import {ModelSubFileService} from  './model-sub-file.service';
import {ModerationActionService} from  './moderation-action.service';
import {PartSubFileReferenceService} from  './part-sub-file-reference.service';
import {PartTypeService} from  './part-type.service';
import {PendingRegistrationService} from  './pending-registration.service';
import {PlacedBrickService} from  './placed-brick.service';
import {PlacedBrickChangeHistoryService} from  './placed-brick-change-history.service';
import {PlatformAnnouncementService} from  './platform-announcement.service';
import {ProjectService} from  './project.service';
import {ProjectCameraPresetService} from  './project-camera-preset.service';
import {ProjectChangeHistoryService} from  './project-change-history.service';
import {ProjectExportService} from  './project-export.service';
import {ProjectReferenceImageService} from  './project-reference-image.service';
import {ProjectRenderService} from  './project-render.service';
import {ProjectTagService} from  './project-tag.service';
import {ProjectTagAssignmentService} from  './project-tag-assignment.service';
import {PublishedMocService} from  './published-moc.service';
import {PublishedMocChangeHistoryService} from  './published-moc-change-history.service';
import {PublishedMocImageService} from  './published-moc-image.service';
import {RebrickableSyncQueueService} from  './rebrickable-sync-queue.service';
import {RebrickableTransactionService} from  './rebrickable-transaction.service';
import {RebrickableUserLinkService} from  './rebrickable-user-link.service';
import {RenderPresetService} from  './render-preset.service';
import {SharedInstructionService} from  './shared-instruction.service';
import {SharedInstructionChangeHistoryService} from  './shared-instruction-change-history.service';
import {SubmodelService} from  './submodel.service';
import {SubmodelChangeHistoryService} from  './submodel-change-history.service';
import {SubmodelInstanceService} from  './submodel-instance.service';
import {SubmodelPlacedBrickService} from  './submodel-placed-brick.service';
import {UserAchievementService} from  './user-achievement.service';
import {UserBadgeService} from  './user-badge.service';
import {UserBadgeAssignmentService} from  './user-badge-assignment.service';
import {UserCollectionService} from  './user-collection.service';
import {UserCollectionChangeHistoryService} from  './user-collection-change-history.service';
import {UserCollectionPartService} from  './user-collection-part.service';
import {UserCollectionSetImportService} from  './user-collection-set-import.service';
import {UserFollowService} from  './user-follow.service';
import {UserLostPartService} from  './user-lost-part.service';
import {UserPartListService} from  './user-part-list.service';
import {UserPartListChangeHistoryService} from  './user-part-list-change-history.service';
import {UserPartListItemService} from  './user-part-list-item.service';
import {UserProfileService} from  './user-profile.service';
import {UserProfileChangeHistoryService} from  './user-profile-change-history.service';
import {UserProfileLinkService} from  './user-profile-link.service';
import {UserProfileLinkTypeService} from  './user-profile-link-type.service';
import {UserProfilePreferredThemeService} from  './user-profile-preferred-theme.service';
import {UserProfileStatService} from  './user-profile-stat.service';
import {UserSetListService} from  './user-set-list.service';
import {UserSetListChangeHistoryService} from  './user-set-list-change-history.service';
import {UserSetListItemService} from  './user-set-list-item.service';
import {UserSetOwnershipService} from  './user-set-ownership.service';
import {UserWishlistItemService} from  './user-wishlist-item.service';


@Injectable({
  providedIn: 'root'
})
export class BMCDataServiceManagerService  {

    constructor(public achievementService: AchievementService
              , public achievementCategoryService: AchievementCategoryService
              , public activityEventService: ActivityEventService
              , public activityEventTypeService: ActivityEventTypeService
              , public apiKeyService: ApiKeyService
              , public apiRequestLogService: ApiRequestLogService
              , public brickCategoryService: BrickCategoryService
              , public brickColourService: BrickColourService
              , public brickConnectionService: BrickConnectionService
              , public brickEconomyTransactionService: BrickEconomyTransactionService
              , public brickEconomyUserLinkService: BrickEconomyUserLinkService
              , public brickElementService: BrickElementService
              , public brickLinkTransactionService: BrickLinkTransactionService
              , public brickLinkUserLinkService: BrickLinkUserLinkService
              , public brickOwlTransactionService: BrickOwlTransactionService
              , public brickOwlUserLinkService: BrickOwlUserLinkService
              , public brickPartService: BrickPartService
              , public brickPartChangeHistoryService: BrickPartChangeHistoryService
              , public brickPartColourService: BrickPartColourService
              , public brickPartConnectorService: BrickPartConnectorService
              , public brickPartRelationshipService: BrickPartRelationshipService
              , public brickSetSetReviewService: BrickSetSetReviewService
              , public brickSetTransactionService: BrickSetTransactionService
              , public brickSetUserLinkService: BrickSetUserLinkService
              , public buildChallengeService: BuildChallengeService
              , public buildChallengeChangeHistoryService: BuildChallengeChangeHistoryService
              , public buildChallengeEntryService: BuildChallengeEntryService
              , public buildManualService: BuildManualService
              , public buildManualChangeHistoryService: BuildManualChangeHistoryService
              , public buildManualPageService: BuildManualPageService
              , public buildManualStepService: BuildManualStepService
              , public buildStepAnnotationService: BuildStepAnnotationService
              , public buildStepAnnotationTypeService: BuildStepAnnotationTypeService
              , public buildStepPartService: BuildStepPartService
              , public colourFinishService: ColourFinishService
              , public connectorTypeService: ConnectorTypeService
              , public connectorTypeCompatibilityService: ConnectorTypeCompatibilityService
              , public contentReportService: ContentReportService
              , public contentReportReasonService: ContentReportReasonService
              , public exportFormatService: ExportFormatService
              , public legoMinifigService: LegoMinifigService
              , public legoSetService: LegoSetService
              , public legoSetMinifigService: LegoSetMinifigService
              , public legoSetPartService: LegoSetPartService
              , public legoSetSubsetService: LegoSetSubsetService
              , public legoThemeService: LegoThemeService
              , public marketDataCacheService: MarketDataCacheService
              , public mocCommentService: MocCommentService
              , public mocFavouriteService: MocFavouriteService
              , public mocLikeService: MocLikeService
              , public modelBuildStepService: ModelBuildStepService
              , public modelDocumentService: ModelDocumentService
              , public modelDocumentChangeHistoryService: ModelDocumentChangeHistoryService
              , public modelStepPartService: ModelStepPartService
              , public modelSubFileService: ModelSubFileService
              , public moderationActionService: ModerationActionService
              , public partSubFileReferenceService: PartSubFileReferenceService
              , public partTypeService: PartTypeService
              , public pendingRegistrationService: PendingRegistrationService
              , public placedBrickService: PlacedBrickService
              , public placedBrickChangeHistoryService: PlacedBrickChangeHistoryService
              , public platformAnnouncementService: PlatformAnnouncementService
              , public projectService: ProjectService
              , public projectCameraPresetService: ProjectCameraPresetService
              , public projectChangeHistoryService: ProjectChangeHistoryService
              , public projectExportService: ProjectExportService
              , public projectReferenceImageService: ProjectReferenceImageService
              , public projectRenderService: ProjectRenderService
              , public projectTagService: ProjectTagService
              , public projectTagAssignmentService: ProjectTagAssignmentService
              , public publishedMocService: PublishedMocService
              , public publishedMocChangeHistoryService: PublishedMocChangeHistoryService
              , public publishedMocImageService: PublishedMocImageService
              , public rebrickableSyncQueueService: RebrickableSyncQueueService
              , public rebrickableTransactionService: RebrickableTransactionService
              , public rebrickableUserLinkService: RebrickableUserLinkService
              , public renderPresetService: RenderPresetService
              , public sharedInstructionService: SharedInstructionService
              , public sharedInstructionChangeHistoryService: SharedInstructionChangeHistoryService
              , public submodelService: SubmodelService
              , public submodelChangeHistoryService: SubmodelChangeHistoryService
              , public submodelInstanceService: SubmodelInstanceService
              , public submodelPlacedBrickService: SubmodelPlacedBrickService
              , public userAchievementService: UserAchievementService
              , public userBadgeService: UserBadgeService
              , public userBadgeAssignmentService: UserBadgeAssignmentService
              , public userCollectionService: UserCollectionService
              , public userCollectionChangeHistoryService: UserCollectionChangeHistoryService
              , public userCollectionPartService: UserCollectionPartService
              , public userCollectionSetImportService: UserCollectionSetImportService
              , public userFollowService: UserFollowService
              , public userLostPartService: UserLostPartService
              , public userPartListService: UserPartListService
              , public userPartListChangeHistoryService: UserPartListChangeHistoryService
              , public userPartListItemService: UserPartListItemService
              , public userProfileService: UserProfileService
              , public userProfileChangeHistoryService: UserProfileChangeHistoryService
              , public userProfileLinkService: UserProfileLinkService
              , public userProfileLinkTypeService: UserProfileLinkTypeService
              , public userProfilePreferredThemeService: UserProfilePreferredThemeService
              , public userProfileStatService: UserProfileStatService
              , public userSetListService: UserSetListService
              , public userSetListChangeHistoryService: UserSetListChangeHistoryService
              , public userSetListItemService: UserSetListItemService
              , public userSetOwnershipService: UserSetOwnershipService
              , public userWishlistItemService: UserWishlistItemService
) { }  


    public ClearAllCaches() {

        this.achievementService.ClearAllCaches();
        this.achievementCategoryService.ClearAllCaches();
        this.activityEventService.ClearAllCaches();
        this.activityEventTypeService.ClearAllCaches();
        this.apiKeyService.ClearAllCaches();
        this.apiRequestLogService.ClearAllCaches();
        this.brickCategoryService.ClearAllCaches();
        this.brickColourService.ClearAllCaches();
        this.brickConnectionService.ClearAllCaches();
        this.brickEconomyTransactionService.ClearAllCaches();
        this.brickEconomyUserLinkService.ClearAllCaches();
        this.brickElementService.ClearAllCaches();
        this.brickLinkTransactionService.ClearAllCaches();
        this.brickLinkUserLinkService.ClearAllCaches();
        this.brickOwlTransactionService.ClearAllCaches();
        this.brickOwlUserLinkService.ClearAllCaches();
        this.brickPartService.ClearAllCaches();
        this.brickPartChangeHistoryService.ClearAllCaches();
        this.brickPartColourService.ClearAllCaches();
        this.brickPartConnectorService.ClearAllCaches();
        this.brickPartRelationshipService.ClearAllCaches();
        this.brickSetSetReviewService.ClearAllCaches();
        this.brickSetTransactionService.ClearAllCaches();
        this.brickSetUserLinkService.ClearAllCaches();
        this.buildChallengeService.ClearAllCaches();
        this.buildChallengeChangeHistoryService.ClearAllCaches();
        this.buildChallengeEntryService.ClearAllCaches();
        this.buildManualService.ClearAllCaches();
        this.buildManualChangeHistoryService.ClearAllCaches();
        this.buildManualPageService.ClearAllCaches();
        this.buildManualStepService.ClearAllCaches();
        this.buildStepAnnotationService.ClearAllCaches();
        this.buildStepAnnotationTypeService.ClearAllCaches();
        this.buildStepPartService.ClearAllCaches();
        this.colourFinishService.ClearAllCaches();
        this.connectorTypeService.ClearAllCaches();
        this.connectorTypeCompatibilityService.ClearAllCaches();
        this.contentReportService.ClearAllCaches();
        this.contentReportReasonService.ClearAllCaches();
        this.exportFormatService.ClearAllCaches();
        this.legoMinifigService.ClearAllCaches();
        this.legoSetService.ClearAllCaches();
        this.legoSetMinifigService.ClearAllCaches();
        this.legoSetPartService.ClearAllCaches();
        this.legoSetSubsetService.ClearAllCaches();
        this.legoThemeService.ClearAllCaches();
        this.marketDataCacheService.ClearAllCaches();
        this.mocCommentService.ClearAllCaches();
        this.mocFavouriteService.ClearAllCaches();
        this.mocLikeService.ClearAllCaches();
        this.modelBuildStepService.ClearAllCaches();
        this.modelDocumentService.ClearAllCaches();
        this.modelDocumentChangeHistoryService.ClearAllCaches();
        this.modelStepPartService.ClearAllCaches();
        this.modelSubFileService.ClearAllCaches();
        this.moderationActionService.ClearAllCaches();
        this.partSubFileReferenceService.ClearAllCaches();
        this.partTypeService.ClearAllCaches();
        this.pendingRegistrationService.ClearAllCaches();
        this.placedBrickService.ClearAllCaches();
        this.placedBrickChangeHistoryService.ClearAllCaches();
        this.platformAnnouncementService.ClearAllCaches();
        this.projectService.ClearAllCaches();
        this.projectCameraPresetService.ClearAllCaches();
        this.projectChangeHistoryService.ClearAllCaches();
        this.projectExportService.ClearAllCaches();
        this.projectReferenceImageService.ClearAllCaches();
        this.projectRenderService.ClearAllCaches();
        this.projectTagService.ClearAllCaches();
        this.projectTagAssignmentService.ClearAllCaches();
        this.publishedMocService.ClearAllCaches();
        this.publishedMocChangeHistoryService.ClearAllCaches();
        this.publishedMocImageService.ClearAllCaches();
        this.rebrickableSyncQueueService.ClearAllCaches();
        this.rebrickableTransactionService.ClearAllCaches();
        this.rebrickableUserLinkService.ClearAllCaches();
        this.renderPresetService.ClearAllCaches();
        this.sharedInstructionService.ClearAllCaches();
        this.sharedInstructionChangeHistoryService.ClearAllCaches();
        this.submodelService.ClearAllCaches();
        this.submodelChangeHistoryService.ClearAllCaches();
        this.submodelInstanceService.ClearAllCaches();
        this.submodelPlacedBrickService.ClearAllCaches();
        this.userAchievementService.ClearAllCaches();
        this.userBadgeService.ClearAllCaches();
        this.userBadgeAssignmentService.ClearAllCaches();
        this.userCollectionService.ClearAllCaches();
        this.userCollectionChangeHistoryService.ClearAllCaches();
        this.userCollectionPartService.ClearAllCaches();
        this.userCollectionSetImportService.ClearAllCaches();
        this.userFollowService.ClearAllCaches();
        this.userLostPartService.ClearAllCaches();
        this.userPartListService.ClearAllCaches();
        this.userPartListChangeHistoryService.ClearAllCaches();
        this.userPartListItemService.ClearAllCaches();
        this.userProfileService.ClearAllCaches();
        this.userProfileChangeHistoryService.ClearAllCaches();
        this.userProfileLinkService.ClearAllCaches();
        this.userProfileLinkTypeService.ClearAllCaches();
        this.userProfilePreferredThemeService.ClearAllCaches();
        this.userProfileStatService.ClearAllCaches();
        this.userSetListService.ClearAllCaches();
        this.userSetListChangeHistoryService.ClearAllCaches();
        this.userSetListItemService.ClearAllCaches();
        this.userSetOwnershipService.ClearAllCaches();
        this.userWishlistItemService.ClearAllCaches();
    }
}