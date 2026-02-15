/*

   GENERATED SERVICE FOR THE BMC TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BMC table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import {Injectable} from '@angular/core';
import {BrickCategoryService} from  './brick-category.service';
import {BrickColourService} from  './brick-colour.service';
import {BrickConnectionService} from  './brick-connection.service';
import {BrickElementService} from  './brick-element.service';
import {BrickPartService} from  './brick-part.service';
import {BrickPartChangeHistoryService} from  './brick-part-change-history.service';
import {BrickPartColourService} from  './brick-part-colour.service';
import {BrickPartConnectorService} from  './brick-part-connector.service';
import {BrickPartRelationshipService} from  './brick-part-relationship.service';
import {BuildManualService} from  './build-manual.service';
import {BuildManualChangeHistoryService} from  './build-manual-change-history.service';
import {BuildManualPageService} from  './build-manual-page.service';
import {BuildManualStepService} from  './build-manual-step.service';
import {BuildStepAnnotationService} from  './build-step-annotation.service';
import {BuildStepAnnotationTypeService} from  './build-step-annotation-type.service';
import {BuildStepPartService} from  './build-step-part.service';
import {ColourFinishService} from  './colour-finish.service';
import {ConnectorTypeService} from  './connector-type.service';
import {ExportFormatService} from  './export-format.service';
import {LegoMinifigService} from  './lego-minifig.service';
import {LegoSetService} from  './lego-set.service';
import {LegoSetMinifigService} from  './lego-set-minifig.service';
import {LegoSetPartService} from  './lego-set-part.service';
import {LegoSetSubsetService} from  './lego-set-subset.service';
import {LegoThemeService} from  './lego-theme.service';
import {PartTypeService} from  './part-type.service';
import {PlacedBrickService} from  './placed-brick.service';
import {PlacedBrickChangeHistoryService} from  './placed-brick-change-history.service';
import {ProjectService} from  './project.service';
import {ProjectCameraPresetService} from  './project-camera-preset.service';
import {ProjectChangeHistoryService} from  './project-change-history.service';
import {ProjectExportService} from  './project-export.service';
import {ProjectReferenceImageService} from  './project-reference-image.service';
import {ProjectRenderService} from  './project-render.service';
import {ProjectTagService} from  './project-tag.service';
import {ProjectTagAssignmentService} from  './project-tag-assignment.service';
import {RenderPresetService} from  './render-preset.service';
import {SubmodelService} from  './submodel.service';
import {SubmodelChangeHistoryService} from  './submodel-change-history.service';
import {SubmodelPlacedBrickService} from  './submodel-placed-brick.service';
import {UserCollectionService} from  './user-collection.service';
import {UserCollectionChangeHistoryService} from  './user-collection-change-history.service';
import {UserCollectionPartService} from  './user-collection-part.service';
import {UserCollectionSetImportService} from  './user-collection-set-import.service';
import {UserWishlistItemService} from  './user-wishlist-item.service';


@Injectable({
  providedIn: 'root'
})
export class BMCDataServiceManagerService  {

    constructor(public brickCategoryService: BrickCategoryService
              , public brickColourService: BrickColourService
              , public brickConnectionService: BrickConnectionService
              , public brickElementService: BrickElementService
              , public brickPartService: BrickPartService
              , public brickPartChangeHistoryService: BrickPartChangeHistoryService
              , public brickPartColourService: BrickPartColourService
              , public brickPartConnectorService: BrickPartConnectorService
              , public brickPartRelationshipService: BrickPartRelationshipService
              , public buildManualService: BuildManualService
              , public buildManualChangeHistoryService: BuildManualChangeHistoryService
              , public buildManualPageService: BuildManualPageService
              , public buildManualStepService: BuildManualStepService
              , public buildStepAnnotationService: BuildStepAnnotationService
              , public buildStepAnnotationTypeService: BuildStepAnnotationTypeService
              , public buildStepPartService: BuildStepPartService
              , public colourFinishService: ColourFinishService
              , public connectorTypeService: ConnectorTypeService
              , public exportFormatService: ExportFormatService
              , public legoMinifigService: LegoMinifigService
              , public legoSetService: LegoSetService
              , public legoSetMinifigService: LegoSetMinifigService
              , public legoSetPartService: LegoSetPartService
              , public legoSetSubsetService: LegoSetSubsetService
              , public legoThemeService: LegoThemeService
              , public partTypeService: PartTypeService
              , public placedBrickService: PlacedBrickService
              , public placedBrickChangeHistoryService: PlacedBrickChangeHistoryService
              , public projectService: ProjectService
              , public projectCameraPresetService: ProjectCameraPresetService
              , public projectChangeHistoryService: ProjectChangeHistoryService
              , public projectExportService: ProjectExportService
              , public projectReferenceImageService: ProjectReferenceImageService
              , public projectRenderService: ProjectRenderService
              , public projectTagService: ProjectTagService
              , public projectTagAssignmentService: ProjectTagAssignmentService
              , public renderPresetService: RenderPresetService
              , public submodelService: SubmodelService
              , public submodelChangeHistoryService: SubmodelChangeHistoryService
              , public submodelPlacedBrickService: SubmodelPlacedBrickService
              , public userCollectionService: UserCollectionService
              , public userCollectionChangeHistoryService: UserCollectionChangeHistoryService
              , public userCollectionPartService: UserCollectionPartService
              , public userCollectionSetImportService: UserCollectionSetImportService
              , public userWishlistItemService: UserWishlistItemService
) { }  


    public ClearAllCaches() {

        this.brickCategoryService.ClearAllCaches();
        this.brickColourService.ClearAllCaches();
        this.brickConnectionService.ClearAllCaches();
        this.brickElementService.ClearAllCaches();
        this.brickPartService.ClearAllCaches();
        this.brickPartChangeHistoryService.ClearAllCaches();
        this.brickPartColourService.ClearAllCaches();
        this.brickPartConnectorService.ClearAllCaches();
        this.brickPartRelationshipService.ClearAllCaches();
        this.buildManualService.ClearAllCaches();
        this.buildManualChangeHistoryService.ClearAllCaches();
        this.buildManualPageService.ClearAllCaches();
        this.buildManualStepService.ClearAllCaches();
        this.buildStepAnnotationService.ClearAllCaches();
        this.buildStepAnnotationTypeService.ClearAllCaches();
        this.buildStepPartService.ClearAllCaches();
        this.colourFinishService.ClearAllCaches();
        this.connectorTypeService.ClearAllCaches();
        this.exportFormatService.ClearAllCaches();
        this.legoMinifigService.ClearAllCaches();
        this.legoSetService.ClearAllCaches();
        this.legoSetMinifigService.ClearAllCaches();
        this.legoSetPartService.ClearAllCaches();
        this.legoSetSubsetService.ClearAllCaches();
        this.legoThemeService.ClearAllCaches();
        this.partTypeService.ClearAllCaches();
        this.placedBrickService.ClearAllCaches();
        this.placedBrickChangeHistoryService.ClearAllCaches();
        this.projectService.ClearAllCaches();
        this.projectCameraPresetService.ClearAllCaches();
        this.projectChangeHistoryService.ClearAllCaches();
        this.projectExportService.ClearAllCaches();
        this.projectReferenceImageService.ClearAllCaches();
        this.projectRenderService.ClearAllCaches();
        this.projectTagService.ClearAllCaches();
        this.projectTagAssignmentService.ClearAllCaches();
        this.renderPresetService.ClearAllCaches();
        this.submodelService.ClearAllCaches();
        this.submodelChangeHistoryService.ClearAllCaches();
        this.submodelPlacedBrickService.ClearAllCaches();
        this.userCollectionService.ClearAllCaches();
        this.userCollectionChangeHistoryService.ClearAllCaches();
        this.userCollectionPartService.ClearAllCaches();
        this.userCollectionSetImportService.ClearAllCaches();
        this.userWishlistItemService.ClearAllCaches();
    }
}