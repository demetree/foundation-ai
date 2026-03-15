//
// Login Geo Map Component
//
// Interactive world map visualization showing the geographic origin of login attempts.
// Uses Leaflet.js with OpenStreetMap tiles served through the TileProxy.
// Includes a country summary table with continent grouping below the map.
//
// Following Foundation Premium UI Patterns.
// AI-assisted development - February 2026
//

import { Component, Input, OnInit, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';

import * as L from 'leaflet';

import { LoginAttemptData } from '../../../security-data-services/login-attempt.service';


//
// Geographic location data extracted from login attempts
//
interface GeoLoginGroup {
    ipAddress: string;
    latitude: number;
    longitude: number;
    countryCode: string;
    countryName: string;
    city: string;
    totalAttempts: number;
    successCount: number;
    failureCount: number;
    failureRate: number;
}


//
// Country summary for the table
//
interface CountrySummary {
    countryCode: string;
    countryName: string;
    continent: string;
    totalAttempts: number;
    successCount: number;
    failureCount: number;
    failureRate: number;
    ipCount: number;
}


//
// Continent grouping for collapsible display
//
interface ContinentGroup {
    name: string;
    countries: CountrySummary[];
    totalAttempts: number;
    isExpanded: boolean;
}


//
// Map from country code to continent name
//
const COUNTRY_TO_CONTINENT: { [key: string]: string } = {
    // North America
    'US': 'North America', 'CA': 'North America', 'MX': 'North America',
    'GT': 'North America', 'CU': 'North America', 'HT': 'North America',
    'DO': 'North America', 'HN': 'North America', 'NI': 'North America',
    'SV': 'North America', 'CR': 'North America', 'PA': 'North America',
    'JM': 'North America', 'TT': 'North America', 'BS': 'North America',
    'BB': 'North America', 'BZ': 'North America', 'PR': 'North America',

    // South America
    'BR': 'South America', 'AR': 'South America', 'CO': 'South America',
    'PE': 'South America', 'VE': 'South America', 'CL': 'South America',
    'EC': 'South America', 'BO': 'South America', 'PY': 'South America',
    'UY': 'South America', 'GY': 'South America', 'SR': 'South America',

    // Europe
    'GB': 'Europe', 'FR': 'Europe', 'DE': 'Europe', 'IT': 'Europe',
    'ES': 'Europe', 'PT': 'Europe', 'NL': 'Europe', 'BE': 'Europe',
    'SE': 'Europe', 'NO': 'Europe', 'DK': 'Europe', 'FI': 'Europe',
    'PL': 'Europe', 'CZ': 'Europe', 'AT': 'Europe', 'CH': 'Europe',
    'IE': 'Europe', 'RO': 'Europe', 'HU': 'Europe', 'SK': 'Europe',
    'HR': 'Europe', 'BG': 'Europe', 'RS': 'Europe', 'UA': 'Europe',
    'RU': 'Europe', 'GR': 'Europe', 'TR': 'Europe', 'LT': 'Europe',
    'LV': 'Europe', 'EE': 'Europe', 'SI': 'Europe', 'LU': 'Europe',
    'MT': 'Europe', 'CY': 'Europe', 'IS': 'Europe', 'AL': 'Europe',
    'BA': 'Europe', 'ME': 'Europe', 'MK': 'Europe', 'MD': 'Europe',

    // Asia
    'CN': 'Asia', 'JP': 'Asia', 'KR': 'Asia', 'IN': 'Asia',
    'ID': 'Asia', 'TH': 'Asia', 'VN': 'Asia', 'PH': 'Asia',
    'MY': 'Asia', 'SG': 'Asia', 'PK': 'Asia', 'BD': 'Asia',
    'LK': 'Asia', 'NP': 'Asia', 'MM': 'Asia', 'KH': 'Asia',
    'LA': 'Asia', 'TW': 'Asia', 'HK': 'Asia', 'MO': 'Asia',
    'MN': 'Asia', 'KZ': 'Asia', 'UZ': 'Asia', 'KG': 'Asia',
    'TJ': 'Asia', 'TM': 'Asia', 'AZ': 'Asia', 'GE': 'Asia',
    'AM': 'Asia', 'AF': 'Asia', 'IQ': 'Asia', 'IR': 'Asia',

    // Middle East
    'SA': 'Middle East', 'AE': 'Middle East', 'IL': 'Middle East',
    'JO': 'Middle East', 'LB': 'Middle East', 'KW': 'Middle East',
    'QA': 'Middle East', 'BH': 'Middle East', 'OM': 'Middle East',
    'YE': 'Middle East', 'SY': 'Middle East',

    // Africa
    'ZA': 'Africa', 'NG': 'Africa', 'EG': 'Africa', 'KE': 'Africa',
    'ET': 'Africa', 'GH': 'Africa', 'TZ': 'Africa', 'UG': 'Africa',
    'CM': 'Africa', 'CI': 'Africa', 'SN': 'Africa', 'MA': 'Africa',
    'TN': 'Africa', 'DZ': 'Africa', 'LY': 'Africa', 'SD': 'Africa',
    'AO': 'Africa', 'MZ': 'Africa', 'ZW': 'Africa', 'RW': 'Africa',

    // Oceania
    'AU': 'Oceania', 'NZ': 'Oceania', 'FJ': 'Oceania',
    'PG': 'Oceania', 'NC': 'Oceania', 'WS': 'Oceania'
};


@Component({
    selector: 'app-login-geo-map',
    templateUrl: './login-geo-map.component.html',
    styleUrls: ['./login-geo-map.component.scss']
})
export class LoginGeoMapComponent implements OnInit, AfterViewInit, OnDestroy {

    //
    // Input: Login attempt data from parent
    //
    @Input() attempts: LoginAttemptData[] = [];

    //
    // Map and marker state
    //
    private map: L.Map | null = null;
    private markersLayer: L.LayerGroup = L.layerGroup();

    //
    // Computed data
    //
    geoGroups: GeoLoginGroup[] = [];
    continentGroups: ContinentGroup[] = [];
    hasGeoData: boolean = false;
    geoDataCount: number = 0;
    totalAttemptCount: number = 0;

    //
    // Map container reference
    //
    @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef;


    constructor() { }


    ngOnInit(): void {
        this.processAttempts();
    }


    ngAfterViewInit(): void {
        //
        // Initialize map after the view is rendered so the container element exists
        //
        setTimeout(() => this.initializeMap(), 100);
    }


    ngOnDestroy(): void {
        if (this.map != null) {
            this.map.remove();
            this.map = null;
        }
    }


    //
    // Process login attempts and extract geo data
    //
    private processAttempts(): void {
        if (this.attempts == null || this.attempts.length === 0) {
            return;
        }

        this.totalAttemptCount = this.attempts.length;

        //
        // Group attempts by IP address and extract geo fields
        // The geo fields come from the IpAddressLocation nav property via the API
        //
        const ipMap = new Map<string, GeoLoginGroup>();

        for (const attempt of this.attempts) {
            //
            // Check if this attempt has geo data from the IpAddressLocation nav property
            //
            const geoData = attempt.ipAddressLocation;

            if (geoData == null) {
                continue;
            }

            const lat = geoData.latitude;
            const lon = geoData.longitude;
            const countryCode = geoData.countryCode;
            const countryName = geoData.countryName;
            const city = geoData.city;

            if (lat == null || lon == null) {
                continue;
            }

            const ip = attempt.ipAddress || 'Unknown';
            let group = ipMap.get(ip);

            if (group == null) {
                group = {
                    ipAddress: ip,
                    latitude: lat,
                    longitude: lon,
                    countryCode: countryCode || '',
                    countryName: countryName || 'Unknown',
                    city: city || '',
                    totalAttempts: 0,
                    successCount: 0,
                    failureCount: 0,
                    failureRate: 0
                };
                ipMap.set(ip, group);
            }

            group.totalAttempts++;

            if (this.isSuccess(attempt) == true) {
                group.successCount++;
            } else {
                group.failureCount++;
            }
        }

        //
        // Compute failure rates
        //
        for (const group of ipMap.values()) {
            group.failureRate = group.totalAttempts > 0
                ? Math.round((group.failureCount / group.totalAttempts) * 100)
                : 0;
        }

        this.geoGroups = Array.from(ipMap.values());
        this.hasGeoData = this.geoGroups.length > 0;
        this.geoDataCount = this.geoGroups.reduce((sum, g) => sum + g.totalAttempts, 0);

        //
        // Build country and continent summaries
        //
        this.buildCountrySummaries();
    }


    //
    // Build country summary table data with continent grouping
    //
    private buildCountrySummaries(): void {
        //
        // Group by country
        //
        const countryMap = new Map<string, CountrySummary>();

        for (const group of this.geoGroups) {
            const code = group.countryCode || 'XX';
            let country = countryMap.get(code);

            if (country == null) {
                country = {
                    countryCode: code,
                    countryName: group.countryName || 'Unknown',
                    continent: COUNTRY_TO_CONTINENT[code] || 'Other',
                    totalAttempts: 0,
                    successCount: 0,
                    failureCount: 0,
                    failureRate: 0,
                    ipCount: 0
                };
                countryMap.set(code, country);
            }

            country.totalAttempts += group.totalAttempts;
            country.successCount += group.successCount;
            country.failureCount += group.failureCount;
            country.ipCount++;
        }

        //
        // Compute failure rates for countries
        //
        for (const country of countryMap.values()) {
            country.failureRate = country.totalAttempts > 0
                ? Math.round((country.failureCount / country.totalAttempts) * 100)
                : 0;
        }

        //
        // Group by continent
        //
        const continentMap = new Map<string, ContinentGroup>();

        for (const country of countryMap.values()) {
            let continent = continentMap.get(country.continent);

            if (continent == null) {
                continent = {
                    name: country.continent,
                    countries: [],
                    totalAttempts: 0,
                    isExpanded: true
                };
                continentMap.set(country.continent, continent);
            }

            continent.countries.push(country);
            continent.totalAttempts += country.totalAttempts;
        }

        //
        // Sort countries within each continent by attempt count, continents by attempt count
        //
        for (const continent of continentMap.values()) {
            continent.countries.sort((a, b) => b.totalAttempts - a.totalAttempts);
        }

        this.continentGroups = Array.from(continentMap.values())
            .sort((a, b) => b.totalAttempts - a.totalAttempts);
    }


    //
    // Initialize the Leaflet map
    //
    private initializeMap(): void {
        if (this.mapContainer == null) {
            return;
        }

        if (this.hasGeoData == false) {
            return;
        }

        //
        // Fix Leaflet default icon paths (webpack compatibility)
        //
        delete (L.Icon.Default.prototype as any)._getIconUrl;
        L.Icon.Default.mergeOptions({
            iconRetinaUrl: 'assets/marker-icon-2x.png',
            iconUrl: 'assets/marker-icon.png',
            shadowUrl: 'assets/marker-shadow.png'
        });

        //
        // Create the map centered on the world
        //
        this.map = L.map(this.mapContainer.nativeElement, {
            center: [20, 0],
            zoom: 2,
            minZoom: 2,
            maxZoom: 12,
            attributionControl: false
        });

        //
        // Add OpenStreetMap tile layer
        // Uses the TileProxy endpoint to avoid CORS issues and cache tiles server-side
        //
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(this.map);

        //
        // Add circle markers for each geo group
        //
        this.addMarkers();

        //
        // Fit bounds to show all markers
        //
        if (this.geoGroups.length > 0) {
            const bounds = L.latLngBounds(
                this.geoGroups.map(g => [g.latitude, g.longitude] as L.LatLngTuple)
            );
            this.map.fitBounds(bounds, { padding: [30, 30], maxZoom: 6 });
        }

        //
        // Force a resize after the map is in the DOM
        //
        setTimeout(() => {
            if (this.map != null) {
                this.map.invalidateSize();
            }
        }, 200);
    }


    //
    // Add circle markers to the map for each geo group
    //
    private addMarkers(): void {
        if (this.map == null) {
            return;
        }

        this.markersLayer.clearLayers();

        //
        // Calculate max attempts for proportional sizing
        //
        const maxAttempts = Math.max(...this.geoGroups.map(g => g.totalAttempts), 1);

        for (const group of this.geoGroups) {
            //
            // Size: proportional to attempt count (minimum 6, maximum 30)
            //
            const radius = 6 + (group.totalAttempts / maxAttempts) * 24;

            //
            // Color: green for mostly success, red for mostly failure, orange for mixed
            //
            let fillColor = '#28a745';

            if (group.failureRate > 50) {
                fillColor = '#dc3545';
            } else if (group.failureRate > 20) {
                fillColor = '#fd7e14';
            }

            const marker = L.circleMarker([group.latitude, group.longitude], {
                radius: radius,
                fillColor: fillColor,
                color: '#ffffff',
                weight: 2,
                opacity: 0.9,
                fillOpacity: 0.7
            });

            //
            // Popup with details
            //
            const popupContent = this.buildPopupContent(group);
            marker.bindPopup(popupContent, { maxWidth: 280 });

            this.markersLayer.addLayer(marker);
        }

        this.markersLayer.addTo(this.map);
    }


    //
    // Build HTML popup content for a marker
    //
    private buildPopupContent(group: GeoLoginGroup): string {
        const locationParts: string[] = [];

        if (group.city) {
            locationParts.push(group.city);
        }

        if (group.countryName) {
            locationParts.push(group.countryName);
        }

        const locationStr = locationParts.length > 0 ? locationParts.join(', ') : 'Unknown';

        const flagEmoji = group.countryCode ? this.getFlagEmoji(group.countryCode) : '';

        let html = '<div class="geo-popup">';
        html += '<div class="geo-popup-title">' + flagEmoji + ' ' + locationStr + '</div>';
        html += '<div class="geo-popup-ip"><strong>IP:</strong> ' + group.ipAddress + '</div>';
        html += '<div class="geo-popup-stats">';
        html += '<div><strong>Attempts:</strong> ' + group.totalAttempts + '</div>';
        html += '<div class="text-success"><strong>Success:</strong> ' + group.successCount + '</div>';
        html += '<div class="text-danger"><strong>Failed:</strong> ' + group.failureCount + '</div>';

        if (group.failureRate > 0) {
            html += '<div><strong>Fail Rate:</strong> ' + group.failureRate + '%</div>';
        }

        html += '</div></div>';

        return html;
    }


    //
    // Toggle continent group expansion
    //
    toggleContinent(continent: ContinentGroup): void {
        continent.isExpanded = !continent.isExpanded;
    }


    //
    // Get a flag emoji from a two-letter country code
    //
    getFlagEmoji(countryCode: string): string {
        if (countryCode == null || countryCode.length !== 2) {
            return '';
        }

        const codePoints = countryCode
            .toUpperCase()
            .split('')
            .map(char => 127397 + char.charCodeAt(0));

        return String.fromCodePoint(...codePoints);
    }


    //
    // Get failure rate badge class
    //
    getFailureRateBadgeClass(rate: number): string {
        if (rate <= 20) {
            return 'bg-success';
        }
        if (rate <= 50) {
            return 'bg-warning text-dark';
        }
        return 'bg-danger';
    }


    //
    // Success determination helper (matches parent component logic)
    //
    private isSuccess(attempt: LoginAttemptData): boolean {
        const success = attempt.success;

        if (success === true) {
            return true;
        }
        if (success === false) {
            return false;
        }

        //
        // Fallback heuristic for historical data without success field
        //
        const value = (attempt.value || '').toLowerCase();

        const failureIndicators = ['fail', 'error', 'invalid', 'denied', 'locked', 'expired', 'disabled', 'not found', 'unauthorized'];

        for (const indicator of failureIndicators) {
            if (value.includes(indicator) == true) {
                return false;
            }
        }

        const successIndicators = ['success', 'ok', 'authenticated', 'granted'];

        for (const indicator of successIndicators) {
            if (value.includes(indicator) == true) {
                return true;
            }
        }

        return true;
    }
}
