import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd } from '@angular/router';
import { filter, map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AppTitleService {

    static appName = 'BMC';

    constructor(private titleService: Title, private router: Router) {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(() => {
                let route = this.router.routerState.root;
                while (route.firstChild) route = route.firstChild;
                return route;
            }),
            filter(route => route.outlet === 'primary'),
            map(route => route.snapshot.data)
        ).subscribe(data => {
            const title = data['title'];
            if (title) {
                this.titleService.setTitle(`${title} — ${AppTitleService.appName}`);
            }
        });
    }
}
