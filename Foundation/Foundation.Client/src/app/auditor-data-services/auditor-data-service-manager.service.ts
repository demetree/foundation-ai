import {Injectable} from '@angular/core';
import {AuditAccessTypeService} from  './audit-access-type.service';
import {AuditEventService} from  './audit-event.service';
import {AuditEventEntityStateService} from  './audit-event-entity-state.service';
import {AuditEventErrorMessageService} from  './audit-event-error-message.service';
import {AuditHostSystemService} from  './audit-host-system.service';
import {AuditModuleService} from  './audit-module.service';
import {AuditModuleEntityService} from  './audit-module-entity.service';
import {AuditPlanBService} from  './audit-plan-b.service';
import {AuditResourceService} from  './audit-resource.service';
import {AuditSessionService} from  './audit-session.service';
import {AuditSourceService} from  './audit-source.service';
import {AuditTypeService} from  './audit-type.service';
import {AuditUserService} from  './audit-user.service';
import {AuditUserAgentService} from  './audit-user-agent.service';
import {ExternalCommunicationService} from  './external-communication.service';
import {ExternalCommunicationRecipientService} from  './external-communication-recipient.service';


@Injectable({
  providedIn: 'root'
})
export class AuditorDataServiceManagerService  {

    constructor(public auditAccessTypeService: AuditAccessTypeService
              , public auditEventService: AuditEventService
              , public auditEventEntityStateService: AuditEventEntityStateService
              , public auditEventErrorMessageService: AuditEventErrorMessageService
              , public auditHostSystemService: AuditHostSystemService
              , public auditModuleService: AuditModuleService
              , public auditModuleEntityService: AuditModuleEntityService
              , public auditPlanBService: AuditPlanBService
              , public auditResourceService: AuditResourceService
              , public auditSessionService: AuditSessionService
              , public auditSourceService: AuditSourceService
              , public auditTypeService: AuditTypeService
              , public auditUserService: AuditUserService
              , public auditUserAgentService: AuditUserAgentService
              , public externalCommunicationService: ExternalCommunicationService
              , public externalCommunicationRecipientService: ExternalCommunicationRecipientService
) { }  


    public ClearAllCaches() {

        this.auditAccessTypeService.ClearAllCaches();
        this.auditEventService.ClearAllCaches();
        this.auditEventEntityStateService.ClearAllCaches();
        this.auditEventErrorMessageService.ClearAllCaches();
        this.auditHostSystemService.ClearAllCaches();
        this.auditModuleService.ClearAllCaches();
        this.auditModuleEntityService.ClearAllCaches();
        this.auditPlanBService.ClearAllCaches();
        this.auditResourceService.ClearAllCaches();
        this.auditSessionService.ClearAllCaches();
        this.auditSourceService.ClearAllCaches();
        this.auditTypeService.ClearAllCaches();
        this.auditUserService.ClearAllCaches();
        this.auditUserAgentService.ClearAllCaches();
        this.externalCommunicationService.ClearAllCaches();
        this.externalCommunicationRecipientService.ClearAllCaches();
    }
}