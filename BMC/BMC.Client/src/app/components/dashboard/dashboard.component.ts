import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

interface DashboardStat {
    icon: string;
    label: string;
    value: string | number;
    color: string;
    route?: string;
}

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {

    stats: DashboardStat[] = [];
    recentProjects: any[] = [];
    isLoading = true;

    constructor(private http: HttpClient, private authService: AuthService) { }

    ngOnInit() {
        this.loadDashboardData();
    }

    loadDashboardData() {
        this.isLoading = true;

        const headers = this.authService.GetAuthenticationHeaders();

        // Load parts count
        this.http.get<any>('/api/BrickParts/RowCount', { headers }).subscribe({
            next: (count) => {
                this.stats.push({
                    icon: 'fas fa-puzzle-piece',
                    label: 'Total Parts',
                    value: count ?? 0,
                    color: '#ffa726',
                    route: '/parts'
                });
            },
            error: () => {
                this.stats.push({ icon: 'fas fa-puzzle-piece', label: 'Total Parts', value: '—', color: '#ffa726' });
            }
        });

        // Load categories count
        this.http.get<any>('/api/BrickCategories/RowCount', { headers }).subscribe({
            next: (count) => {
                this.stats.push({
                    icon: 'fas fa-layer-group',
                    label: 'Categories',
                    value: count ?? 0,
                    color: '#ff8f00',
                });
            },
            error: () => {
                this.stats.push({ icon: 'fas fa-layer-group', label: 'Categories', value: '—', color: '#ff8f00' });
            }
        });

        // Load colours count
        this.http.get<any>('/api/BrickColours/RowCount', { headers }).subscribe({
            next: (count) => {
                this.stats.push({
                    icon: 'fas fa-palette',
                    label: 'Colours',
                    value: count ?? 0,
                    color: '#e65100',
                    route: '/colours'
                });
            },
            error: () => {
                this.stats.push({ icon: 'fas fa-palette', label: 'Colours', value: '—', color: '#e65100' });
            }
        });

        // Load projects count
        this.http.get<any>('/api/Projects/RowCount', { headers }).subscribe({
            next: (count) => {
                this.stats.push({
                    icon: 'fas fa-project-diagram',
                    label: 'Projects',
                    value: count ?? 0,
                    color: '#ffb74d',
                    route: '/projects'
                });
                this.isLoading = false;
            },
            error: () => {
                this.stats.push({ icon: 'fas fa-project-diagram', label: 'Projects', value: '—', color: '#ffb74d' });
                this.isLoading = false;
            }
        });
    }
}
