//
// This is a tweaked version of the basic crew service to get crews with member data.  It only provides read list and single get operations.
//
// It returns a crew object with a new members sub list, but is otherwise the same as the regular crew object.
//
// Use the crews service for any other actions on the crew entity.
//

import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { CrewQueryParameters } from '../scheduler-data-services/crew.service'
import { CrewMemberData } from '../scheduler-data-services/crew-member.service'

const SHARE_REPLAY_CACHE_SIZE = 1;

//
// This models the data returned from the server.  It has a function to convert to an object suitable for saving with.
//
// Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CrewWithMembersData {
  id!: bigint | number;
  name!: string;
  description!: string | null;
  versionNumber!: bigint | number;
  objectGuid!: string;
  active!: boolean;
  deleted!: boolean;
  crewMembers!: CrewMemberData[] | null;
}


@Injectable({
  providedIn: 'root'
})
export class CrewWithMembersService extends SecureEndpointBase {

  private listCache: Map<string, Observable<Array<CrewWithMembersData>>>;
  private recordCache: Map<string, Observable<CrewWithMembersData>>;


  constructor(http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    private utilityService: UtilityService,
    @Inject('BASE_URL') private baseUrl: string) {
    super(http, alertService, authService);

    this.listCache = new Map<string, Observable<Array<CrewWithMembersData>>>();
    this.recordCache = new Map<string, Observable<CrewWithMembersData>>();
  }


  public ClearListCaches(config: CrewQueryParameters | null = null) {

    const configHash = this.getConfigHash(config);

    if (this.listCache.has(configHash)) {
      this.listCache.delete(configHash);
    }
  }


  public ClearRecordCache(id: bigint | number, includeRelations: boolean = true) {

    const configHash = this.utilityService.hashCode(`_${id}_${includeRelations}`);

    if (this.recordCache.has(configHash)) {
      this.recordCache.delete(configHash);
    }
  }


  public ClearAllCaches() {
    this.listCache.clear();
    this.recordCache.clear();
  }



  public GetCrewWithMembers(id: bigint | number, includeRelations: boolean = true): Observable<CrewWithMembersData> {

    const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

    if (this.recordCache.has(configHash) == false) {

      const crew$ = this.requestCrew(id, includeRelations).pipe(
        shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
        catchError((error) => {
          this.recordCache.delete(configHash);

          //this.alertService.showHttpErrorMessage("Unable to get Crew", error);

          return throwError(() => error);
        })
      );

      this.recordCache.set(configHash, crew$);

      return crew$;
    }

    return this.recordCache.get(configHash) as Observable<CrewWithMembersData>;
  }

  private requestCrew(id: bigint | number, includeRelations: boolean = true): Observable<CrewWithMembersData> {

    let queryParams = new HttpParams();


    const authenticationHeaders = this.authService.GetAuthenticationHeaders();

    return this.http.get<CrewWithMembersData>(this.baseUrl + 'api/CrewWithMembers/' + id.toString(), { params: queryParams, headers: authenticationHeaders }).pipe(
      catchError(error => {
        return this.handleError(error, () => this.requestCrew(id, includeRelations));
      }));
  }

  public GetCrewList(config: CrewQueryParameters | any = null): Observable<Array<CrewWithMembersData>> {

    const configHash = this.getConfigHash(config);

    if (!this.listCache.has(configHash)) {
      const crewList$ = this.requestCrewList(config).pipe(
        shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
        catchError((error) => {
          this.listCache.delete(configHash);

          //this.alertService.showHttpErrorMessage("Unable to get Crew list", error);

          return throwError(() => error);
        })
      );

      this.listCache.set(configHash, crewList$);

      return crewList$;
    }

    return this.listCache.get(configHash) as Observable<Array<CrewWithMembersData>>;
  }


  private requestCrewList(config: CrewQueryParameters | any): Observable<Array<CrewWithMembersData>> {

    let queryParams = new HttpParams();

    if (config != null) {

      for (const property in config) {
        if (config[property] != null) {
          queryParams = queryParams.append(property, config[property].toString());
        }
      }
    }

    const authenticationHeaders = this.authService.GetAuthenticationHeaders();

    return this.http.get<Array<CrewWithMembersData>>(this.baseUrl + 'api/CrewsWithMembers', { params: queryParams, headers: authenticationHeaders }).pipe(
      catchError(error => {
        return this.handleError(error, () => this.requestCrewList(config));
      }));
  }





  private getConfigHash(config: CrewQueryParameters | any): string {

    if (!config) {
      return '_';
    }

    // Normalize the config object, excluding null and undefined properties
    const normalizedConfig = Object.keys(config)
      .sort() // Ensure consistent property order
      .reduce((obj: any, key: string) => {
        if (config[key] != null) { // Exclude null and undefined
          obj[key] = config[key];
        }
        return obj;
      }, {});

    if (Object.keys(normalizedConfig).length > 0) {
      return this.utilityService.hashCode(JSON.stringify(normalizedConfig));
    }

    return '_';
  }
}
