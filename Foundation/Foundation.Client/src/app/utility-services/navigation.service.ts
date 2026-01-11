import { Injectable } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class NavigationService {
  private history: string[] = []; // Stack to track route history

  constructor(private router: Router) {
    // Listen for navigation events and track the history
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        const currentUrl = event.url;

        // Prevent adding the same route consecutively to avoid circular navigation
        if (this.history.length === 0 || this.history[this.history.length - 1] !== currentUrl) {
          this.history.push(currentUrl);
        }
      });
  }

  // Method to navigate back to the previous route
  goBack(): void {
    // Pop the current route
    this.history.pop();

    if (this.history.length > 0) {
      const previousUrl = this.history[this.history.length - 1];
      this.router.navigateByUrl(previousUrl);
    } else {
      // Fallback if no history is available
      this.router.navigate(['/']);
    }
  }

  // Method to check if there is a previous route to go back to
  canGoBack(): boolean {
    return this.history.length > 1; // True if there's at least one previous route
  }

  // Method to get the current route history for debugging
  getHistory(): string[] {
    return [...this.history]; // Return a copy of the history array
  }
}
