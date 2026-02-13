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
import {BrickPartService} from  './brick-part.service';
import {BrickPartChangeHistoryService} from  './brick-part-change-history.service';
import {BrickPartColourService} from  './brick-part-colour.service';
import {BrickPartConnectorService} from  './brick-part-connector.service';
import {ConnectorTypeService} from  './connector-type.service';
import {PlacedBrickService} from  './placed-brick.service';
import {PlacedBrickChangeHistoryService} from  './placed-brick-change-history.service';
import {ProjectService} from  './project.service';
import {ProjectChangeHistoryService} from  './project-change-history.service';


@Injectable({
  providedIn: 'root'
})
export class BMCDataServiceManagerService  {

    constructor(public brickCategoryService: BrickCategoryService
              , public brickColourService: BrickColourService
              , public brickConnectionService: BrickConnectionService
              , public brickPartService: BrickPartService
              , public brickPartChangeHistoryService: BrickPartChangeHistoryService
              , public brickPartColourService: BrickPartColourService
              , public brickPartConnectorService: BrickPartConnectorService
              , public connectorTypeService: ConnectorTypeService
              , public placedBrickService: PlacedBrickService
              , public placedBrickChangeHistoryService: PlacedBrickChangeHistoryService
              , public projectService: ProjectService
              , public projectChangeHistoryService: ProjectChangeHistoryService
) { }  


    public ClearAllCaches() {

        this.brickCategoryService.ClearAllCaches();
        this.brickColourService.ClearAllCaches();
        this.brickConnectionService.ClearAllCaches();
        this.brickPartService.ClearAllCaches();
        this.brickPartChangeHistoryService.ClearAllCaches();
        this.brickPartColourService.ClearAllCaches();
        this.brickPartConnectorService.ClearAllCaches();
        this.connectorTypeService.ClearAllCaches();
        this.placedBrickService.ClearAllCaches();
        this.placedBrickChangeHistoryService.ClearAllCaches();
        this.projectService.ClearAllCaches();
        this.projectChangeHistoryService.ClearAllCaches();
    }
}