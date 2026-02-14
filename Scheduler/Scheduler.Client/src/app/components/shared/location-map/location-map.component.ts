import {
    Component,
    Input,
    Output,
    EventEmitter,
    OnChanges,
    OnDestroy,
    AfterViewInit,
    SimpleChanges,
    ElementRef,
    ViewChild
} from '@angular/core';
import * as L from 'leaflet';
import { GeocodingService, GeocodeResult } from '../../../services/geocoding.service';


/**
 * Reusable map component for displaying and editing latitude/longitude coordinates.
 *
 * Uses Leaflet with OpenStreetMap tiles (free, no API key).
 *
 * Usage:
 *   <app-location-map
 *     [latitude]="entity.latitude"
 *     [longitude]="entity.longitude"
 *     [editable]="true"
 *     [height]="'300px'"
 *     [showResolveButton]="true"
 *     [addressLine1]="form.get('addressLine1')?.value"
 *     [city]="form.get('city')?.value"
 *     [stateProvinceId]="form.get('stateProvinceId')?.value"
 *     [postalCode]="form.get('postalCode')?.value"
 *     [countryId]="form.get('countryId')?.value"
 *     (coordinatesChanged)="onCoordsChanged($event)">
 *   </app-location-map>
 *
 * When editable, clicking the map or dragging the marker emits updated coordinates.
 * When coordinates are null, shows a prompt message over a default world view.
 */
@Component({
    selector: 'app-location-map',
    templateUrl: './location-map.component.html',
    styleUrls: ['./location-map.component.scss']
})
export class LocationMapComponent implements AfterViewInit, OnChanges, OnDestroy {

    /** Latitude value (nullable). */
    @Input() latitude: number | null | undefined = null;

    /** Longitude value (nullable). */
    @Input() longitude: number | null | undefined = null;

    /** Whether the user can click/drag to change coordinates. */
    @Input() editable: boolean = false;

    /** CSS height for the map container. */
    @Input() height: string = '300px';

    /** Whether to show the "Resolve from Address" button. */
    @Input() showResolveButton: boolean = false;

    /** Address fields for geocoding resolution (optional). */
    @Input() addressLine1: string | null | undefined = null;
    @Input() city: string | null | undefined = null;
    @Input() stateProvinceId: number | null | undefined = null;
    @Input() postalCode: string | null | undefined = null;
    @Input() countryId: number | null | undefined = null;

    /** Emits when the user changes coordinates (click or drag). */
    @Output() coordinatesChanged = new EventEmitter<{ latitude: number; longitude: number }>();

    @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef<HTMLDivElement>;

    /** Whether valid coordinates are provided. */
    public hasCoordinates: boolean = false;

    /** Whether the geocoding resolve is in progress. */
    public isResolving: boolean = false;

    /** Message to display after a resolve attempt. */
    public resolveMessage: string | null = null;

    /** Whether the last resolve attempt was successful. */
    public resolveSuccess: boolean = false;

    /** The Leaflet map instance. */
    private map: L.Map | null = null;

    /** The current marker on the map. */
    private marker: L.Marker | null = null;

    /** Whether the view has been initialized (AfterViewInit fired). */
    private viewReady: boolean = false;

    /** Unique id suffix to avoid DOM id collisions when multiple instances exist. */
    private static instanceCounter: number = 0;
    public mapId: string;


    /** Default icon fix — Leaflet's default icon paths break when bundled by Webpack. */
    private defaultIcon = L.icon({
        iconUrl: 'assets/leaflet/marker-icon.png',
        iconRetinaUrl: 'assets/leaflet/marker-icon-2x.png',
        shadowUrl: 'assets/leaflet/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });


    constructor(private geocodingService: GeocodingService) {
        LocationMapComponent.instanceCounter++;
        this.mapId = 'location-map-' + LocationMapComponent.instanceCounter;
    }


    ngAfterViewInit(): void {
        this.viewReady = true;
        this.initializeMap();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (!this.viewReady) {
            return;
        }

        // Re-evaluate whether we have valid coordinates
        const latChanged = changes['latitude'];
        const lngChanged = changes['longitude'];

        if (latChanged || lngChanged) {
            this.updateMapView();
        }
    }


    ngOnDestroy(): void {
        if (this.map) {
            this.map.remove();
            this.map = null;
        }
    }


    /**
     * Creates the Leaflet map instance and sets the initial view.
     */
    private initializeMap(): void {
        if (!this.mapContainer) {
            return;
        }

        // Create the map
        this.map = L.map(this.mapContainer.nativeElement, {
            zoomControl: true,
            attributionControl: true
        });

        // Add OpenStreetMap tile layer
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }).addTo(this.map);

        // Wire up click handler for editable mode
        if (this.editable) {
            this.map.on('click', (event: L.LeafletMouseEvent) => {
                this.setMarker(event.latlng.lat, event.latlng.lng);
                this.coordinatesChanged.emit({
                    latitude: Math.round(event.latlng.lat * 1_000_000) / 1_000_000,
                    longitude: Math.round(event.latlng.lng * 1_000_000) / 1_000_000
                });
            });
        }

        // Set initial view
        this.updateMapView();
    }


    /**
     * Updates the map center and marker based on current latitude/longitude inputs.
     */
    private updateMapView(): void {
        if (!this.map) {
            return;
        }

        this.hasCoordinates = this.latitude != null && this.longitude != null
            && !isNaN(this.latitude) && !isNaN(this.longitude);

        if (this.hasCoordinates) {
            const lat = this.latitude as number;
            const lng = this.longitude as number;

            this.map.setView([lat, lng], 14);
            this.setMarker(lat, lng);
        } else {
            // Default world view
            this.map.setView([20, 0], 2);
            this.removeMarker();
        }

        // Fix tile rendering issues (common when map container is initially hidden)
        setTimeout(() => {
            this.map?.invalidateSize();
        }, 100);
    }


    /**
     * Places or moves the marker to the given coordinates.
     */
    private setMarker(lat: number, lng: number): void {
        if (!this.map) {
            return;
        }

        if (this.marker) {
            this.marker.setLatLng([lat, lng]);
        } else {
            this.marker = L.marker([lat, lng], {
                icon: this.defaultIcon,
                draggable: this.editable
            }).addTo(this.map);

            // Wire up drag end handler
            if (this.editable) {
                this.marker.on('dragend', () => {
                    const position = this.marker?.getLatLng();
                    if (position) {
                        this.coordinatesChanged.emit({
                            latitude: Math.round(position.lat * 1_000_000) / 1_000_000,
                            longitude: Math.round(position.lng * 1_000_000) / 1_000_000
                        });
                    }
                });
            }
        }
    }


    /**
     * Removes the marker from the map.
     */
    private removeMarker(): void {
        if (this.marker && this.map) {
            this.map.removeLayer(this.marker);
            this.marker = null;
        }
    }


    /**
     * Resolves the current address inputs into coordinates via the geocoding API.
     * Updates the marker and emits coordinatesChanged on success.
     */
    public resolveFromAddress(): void {
        if (this.isResolving) {
            return;
        }

        //
        // Check that at least one address field has a value
        //
        const hasAnyAddress = this.addressLine1 || this.city || this.postalCode
            || this.stateProvinceId || this.countryId;

        if (!hasAnyAddress) {
            this.resolveMessage = 'Please fill in at least one address field first.';
            this.resolveSuccess = false;
            this.clearResolveMessage();
            return;
        }

        this.isResolving = true;
        this.resolveMessage = null;

        this.geocodingService.resolveAddress({
            addressLine1: this.addressLine1 || undefined,
            city: this.city || undefined,
            stateProvinceId: this.stateProvinceId ?? null,
            postalCode: this.postalCode || undefined,
            countryId: this.countryId ?? null
        }).subscribe({
            next: (result: GeocodeResult) => {
                this.isResolving = false;

                //
                // Update the map with the resolved coordinates
                //
                if (this.map) {
                    this.map.setView([result.latitude, result.longitude], 15);
                    this.setMarker(result.latitude, result.longitude);
                }

                //
                // Emit the new coordinates so the parent form can update
                //
                this.coordinatesChanged.emit({
                    latitude: result.latitude,
                    longitude: result.longitude
                });

                //
                // Show a brief success message with confidence indicator
                //
                const confidencePercent = Math.round(result.confidence * 100);
                this.resolveMessage = `Resolved (${confidencePercent}% confidence)`;
                this.resolveSuccess = true;
                this.clearResolveMessage();
            },
            error: (error: any) => {
                this.isResolving = false;

                if (error?.status === 404) {
                    this.resolveMessage = 'Address could not be resolved. Try providing more details.';
                } else if (error?.status === 429) {
                    this.resolveMessage = 'Geocoding is busy, please try again in a moment.';
                } else {
                    this.resolveMessage = 'An error occurred while resolving the address.';
                }

                this.resolveSuccess = false;
                this.clearResolveMessage(8000);
            }
        });
    }


    /**
     * Clears the resolve message after a delay.
     */
    private clearResolveMessage(delayMs: number = 5000): void {
        setTimeout(() => {
            this.resolveMessage = null;
        }, delayMs);
    }


    /**
     * Public method to force a map resize (useful when the map container
     * becomes visible after being hidden, e.g. in a tab or accordion).
     */
    public invalidateSize(): void {
        if (this.map) {
            setTimeout(() => {
                this.map?.invalidateSize();
            }, 100);
        }
    }
}
