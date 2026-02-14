
/*

To use this to retain the state of components so that when a route is returned to it has the same state as the time before, setup the route with a data.persistComponent = true value like this:
{ path: 'styles', title: 'Shoe Wall Styles', component: StylesPageComponent, data: { persistComponent: true } },



Some reading here - https://stackoverflow.com/questions/41280471/how-to-implement-routereusestrategy-shoulddetach-for-specific-routes-in-angular

https://angular.io/api/router/RouteReuseStrategy

*/
import { RouteReuseStrategy, ActivatedRouteSnapshot, DetachedRouteHandle, RouterModule, Routes, UrlSegment, ActivatedRoute } from "@angular/router";

interface RouteStorageObject {
  snapshot: ActivatedRouteSnapshot;
  handle: DetachedRouteHandle;
}

export class RouteReuseService implements RouteReuseStrategy {

  storedRoutes: { [key: string]: RouteStorageObject } = {};


  /**
 * Decides when the route should be stored
 * If the route should be stored, I believe the boolean is indicating to a controller whether or not to fire this.store
 * _When_ it is called though does not particularly matter, just know that this determines whether or not we store the route
 * @param route This is, at least as I understand it, the route that the user is currently on, and we would like to know if we want to store it
 * @returns boolean indicating that we want to (true) or do not want to (false) store that route
 */
  shouldDetach(route: ActivatedRouteSnapshot): boolean {

    if (!route.routeConfig || route.routeConfig.loadChildren) {
      return false;
    }

    // check to see if the route has a persistComponent value.  If it does, return true so the state is kept.  Otherwise, return false so the detachment happens
    if (route.routeConfig.data && route.routeConfig.data["persistComponent"] == true) {

      // this route should be detached so it can be reattached later.  This will retain the state of the component at the route.
      return true;

    } else {

      // Do not detatch and store this route
      return false;
    }
  }



  /**
  * Constructs object of type `RouteStorageObject` to store, and then stores it for later attachment.
  *
  * This should not fire unless the route change's 'shouldDetach' returned true.
  * 
  * @param route This is stored for later comparison to requested routes, see `this.shouldAttach`
  * @param handle Later to be retrieved by this.retrieve, and offered up to whatever controller is using this class
  */
  store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle): void {
    let storedRoute: RouteStorageObject = {
      snapshot: route,
      handle: handle
    };

    //console.log("store:", storedRoute, "into: ", this.storedRoutes);

    // routes are stored by path - the key is the path name, and the handle is stored under it so that you can only ever have one object stored for a single path
    this.storedRoutes[route.routeConfig?.path!] = storedRoute;
  }

  /**
   * Determines whether or not there is a stored route and, if there is, whether or not it should be rendered in place of requested route
   * @param route The route the user requested
   * @returns boolean indicating whether or not to render the stored route
   */
  shouldAttach(route: ActivatedRouteSnapshot): boolean {

    // this will be true if the route has been stored before
    let canAttach: boolean = !!route.routeConfig && !!this.storedRoutes[route.routeConfig?.path!];

    // this decides whether the route already stored should be rendered in place of the requested route, and is the return value
    // at this point we already know that the paths match because the storedResults key is the route.routeConfig.path
    // so, if the route.params and route.queryParams also match, then we should reuse the component
    if (canAttach) {

      //console.log("param comparison:");
      //console.log(this.compareObjects(route.params, this.storedRoutes[route.routeConfig?.path!].snapshot.params));
      //console.log("query param comparison");
      //console.log(this.compareObjects(route.queryParams, this.storedRoutes[route.routeConfig?.path!].snapshot.queryParams));

      let paramsMatch: boolean = this.compareObjects(route.params, this.storedRoutes[route.routeConfig?.path!].snapshot.params);
      let queryParamsMatch: boolean = this.compareObjects(route.queryParams, this.storedRoutes[route.routeConfig?.path!].snapshot.queryParams);

      //console.log("deciding to attach...", route, "does it match?", this.storedRoutes[route.routeConfig?.path!].snapshot, "return: ", paramsMatch && queryParamsMatch);
      return paramsMatch && queryParamsMatch;

    } else {
      return false;
    }
  }


  /** 
 * Finds the locally stored instance of the requested route, if it exists, and returns it
 * @param route New route the user has requested
 * @returns DetachedRouteHandle object which can be used to render the component
 */
  retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {

    // return null if the path does not have a routerConfig OR if there is no stored route for that routerConfig
    if (!route.routeConfig || !this.storedRoutes[route.routeConfig?.path!]) return null;

    //console.log("retrieving", "return: ", this.storedRoutes[route.routeConfig?.path!]);

    /** returns handle when the route.routeConfig.path is already stored */
    return this.storedRoutes[route.routeConfig?.path!].handle;
  }


  /**
   * Determines whether or not the current route should be reused
   * @param future The route the user is going to, as triggered by the router
   * @param curr The route the user is currently on
   * @returns boolean basically indicating true if the user intends to leave the current route
   */
  shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {

    //console.log("deciding to reuse", "future", future.routeConfig, "current", curr.routeConfig, "return: ", future.routeConfig === curr.routeConfig);
    return future.routeConfig === curr.routeConfig;

  }


  /** 
  * This nasty bugger finds out whether the objects are _traditionally_ equal to each other, like you might assume someone else would have put this function in vanilla JS already
  * One thing to note is that it uses coercive comparison (==) on properties which both objects have, not strict comparison (===)
  * Another important note is that the method only tells you if `compare` has all equal parameters to `base`, not the other way around
  * @param base The base object which you would like to compare another object to
  * @param compare The object to compare to base
  * @returns boolean indicating whether or not the objects have all the same properties and those properties are ==
  */
  private compareObjects(base: any, compare: any): boolean {

    // loop through all properties in base object
    for (let baseProperty in base) {

      // determine if comparrison object has that property, if not: return false
      if (compare.hasOwnProperty(baseProperty)) {
        switch (typeof base[baseProperty]) {
          // if one is object and other is not: return false
          // if they are both objects, recursively call this comparison function
          case 'object':
            if (typeof compare[baseProperty] !== 'object' || !this.compareObjects(base[baseProperty], compare[baseProperty])) { return false; } break;
          // if one is function and other is not: return false
          // if both are functions, compare function.toString() results
          case 'function':
            if (typeof compare[baseProperty] !== 'function' || base[baseProperty].toString() !== compare[baseProperty].toString()) { return false; } break;
          // otherwise, see if they are equal using coercive comparison
          default:
            if (base[baseProperty] != compare[baseProperty]) { return false; }
        }
      } else {
        return false;
      }
    }

    // returns true only after false HAS NOT BEEN returned through all loops
    return true;
  }
}
