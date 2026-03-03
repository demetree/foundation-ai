import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// My Lost Parts Service
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface LostPart {
    id: number;
    brickPartId: number;
    partName: string;
    partNum: string;
    partImageUrl: string | null;
    partCategory: string | null;
    brickColourId: number;
    colourName: string;
    colourHex: string | null;
    isTransparent: boolean;
    legoSetId: number | null;
    setNum: string | null;
    setName: string | null;
    setImageUrl: string | null;
    lostQuantity: number;
    rebrickableInvPartId: number | null;
}

export interface LostPartsStats {
    totalRecords: number;
    totalLostParts: number;
    uniqueParts: number;
    setsAffected: number;
    uniqueColours: number;
}

export interface AffectedSet {
    id: number;
    setNum: string;
    name: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class MyLostPartsService {
    private readonly baseUrl = '/api/my-lost-parts';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/my-lost-parts — all lost parts */
    getAll(search?: string, setId?: number, sort?: string): Observable<LostPart[]> {
        let params = new HttpParams();
        if (search) params = params.set('search', search);
        if (setId) params = params.set('setId', setId.toString());
        if (sort) params = params.set('sort', sort);

        return this.http.get<LostPart[]>(
            this.baseUrl,
            { headers: this.headers, params }
        );
    }

    /** GET /api/my-lost-parts/stats */
    getStats(): Observable<LostPartsStats> {
        return this.http.get<LostPartsStats>(
            `${this.baseUrl}/stats`,
            { headers: this.headers }
        );
    }

    /** POST /api/my-lost-parts — report a lost part */
    report(brickPartId: number, brickColourId: number, lostQuantity: number = 1, legoSetId?: number): Observable<any> {
        return this.http.post(
            this.baseUrl,
            { brickPartId, brickColourId, lostQuantity, legoSetId },
            { headers: this.headers }
        );
    }

    /** PUT /api/my-lost-parts/{id} */
    update(id: number, lostQuantity: number): Observable<any> {
        return this.http.put(
            `${this.baseUrl}/${id}`,
            { lostQuantity },
            { headers: this.headers }
        );
    }

    /** DELETE /api/my-lost-parts/{id} */
    remove(id: number): Observable<any> {
        return this.http.delete(
            `${this.baseUrl}/${id}`,
            { headers: this.headers }
        );
    }

    /** GET /api/my-lost-parts/affected-sets */
    getAffectedSets(): Observable<AffectedSet[]> {
        return this.http.get<AffectedSet[]>(
            `${this.baseUrl}/affected-sets`,
            { headers: this.headers }
        );
    }
}
