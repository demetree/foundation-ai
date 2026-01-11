import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';


//
// Components wanting the unsaved change confirmation need to implement this interface.
//
export interface CanComponentDeactivate {
  canDeactivate: () => boolean | Observable<boolean> | Promise<boolean>;
}

@Injectable({
  providedIn: 'root',
})
export class UnsavedChangesGuard implements CanDeactivate<CanComponentDeactivate> {
  canDeactivate(
    component: CanComponentDeactivate
  ): boolean | Observable<boolean> | Promise<boolean> {
    return component.canDeactivate ? component.canDeactivate() : true;
  }
}
