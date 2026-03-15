/*
   GENERATED FORM FOR THE POSTCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PostCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to post-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { PostCategoryService, PostCategoryData } from '../../../community-data-services/post-category.service';
import { PostCategoryAddEditComponent } from '../post-category-add-edit/post-category-add-edit.component';
import { PostCategoryTableComponent } from '../post-category-table/post-category-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-post-category-listing',
  templateUrl: './post-category-listing.component.html',
  styleUrls: ['./post-category-listing.component.scss']
})
export class PostCategoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(PostCategoryAddEditComponent) addEditPostCategoryComponent!: PostCategoryAddEditComponent;
  @ViewChild(PostCategoryTableComponent) postCategoryTableComponent!: PostCategoryTableComponent;

  public PostCategories: PostCategoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalPostCategoryCount$ : Observable<number> | null = null;
  public filteredPostCategoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private postCategoryService: PostCategoryService,
              private alertService: AlertService,
              private navigationService: NavigationService,
              private breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {

    this.breakpointObserver
      .observe(['(max-width: 1100px)']) // this size is specified to try and find a balance so tablets and phone see cards, but wider screens get a table.
      .subscribe((result) => {
        this.isSmallScreen = result.matches;
      });

    this.loadCounts();
  }


  ngAfterViewInit(): void {
    //
    // Subscribe to the postCategoryChanged observable on the add/edit component so that when a PostCategory changes we can reload the list.
    //
    this.addEditPostCategoryComponent.postCategoryChanged.subscribe({
      next: (result: PostCategoryData[] | null) => {
        this.postCategoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Post Category changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditPostCategoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalPostCategoryCount$ = this.postCategoryService.GetPostCategoriesRowCount({
      active: true,
      deleted: false
    }).pipe(
      map(c => Number(c ?? 0)),
      startWith(0),
      finalize(() => {
        this.loadingTotalCount = false;
      }),
      shareReplay(1)
    );

    if (this.filterText) {

      this.filteredPostCategoryCount$ = this.postCategoryService.GetPostCategoriesRowCount({
        active: true,
        deleted: false,
        anyStringContains: this.filterText || undefined
      }).pipe(
        map(c => Number(c ?? 0)),
        startWith(0),
        finalize(() => {
          this.loadingFilteredCount = false;
        }),
        shareReplay(1)
      )
    } else {

      this.filteredPostCategoryCount$ = this.totalPostCategoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalPostCategoryCount$.subscribe();

    if (this.filteredPostCategoryCount$ != this.totalPostCategoryCount$) {
      this.filteredPostCategoryCount$.subscribe();
    }
  }


  public goBack(): void {
    this.navigationService.goBack();
   }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public clearFilter() {
    this.filterText = '';
  }


  //
  // Update the counts when the filter change
  //
  public onFilterChange(): void {

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {
      this.postCategoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.postCategoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsCommunityPostCategoryReader(): boolean {
    return this.postCategoryService.userIsCommunityPostCategoryReader();
  }

  public userIsCommunityPostCategoryWriter(): boolean {
    return this.postCategoryService.userIsCommunityPostCategoryWriter();
  }
}
