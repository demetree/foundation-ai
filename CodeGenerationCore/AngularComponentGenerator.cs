using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Foundation.CodeGeneration.DatabaseGenerator;
using static Foundation.CodeGeneration.DatabaseGenerator.Database;
using static Foundation.CodeGeneration.DatabaseGenerator.Database.Table;
using static Foundation.StringUtility;

namespace Foundation.CodeGeneration
{
    public partial class AngularComponentGenerator : CodeGenerationBase
    {
        private const int SPACES_PER_TAB = 4;

        private class FieldConfiguration
        {
            public string camelCaseName;
            public string titleName;

            public Database.Table linkTable;
            public Database.Table.Field field;


            /// <summary>
            /// 
            /// Useful for tables that don't have a name field, but are used in FKs.  This will simply find the first string field on the table and return its 
            /// name.  It is better than nothing. and for tables without a name field, it's not possible to generically determine the best field to use.
            /// 
            /// Often, it'd be a concatenation of multiple fields...
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public string GetFirstStringFieldFromLinkTable()
            {
                if (linkTable != null)
                {
                    foreach (var field in linkTable.fields)
                    {
                        if (field.dataType == DataType.STRING_10 ||
                            field.dataType == DataType.STRING_100 ||
                            field.dataType == DataType.STRING_1000 ||
                            field.dataType == DataType.STRING_2000 ||
                            field.dataType == DataType.STRING_250 ||
                            field.dataType == DataType.STRING_50 ||
                            field.dataType == DataType.STRING_500 ||
                            field.dataType == DataType.STRING_850 ||
                            field.dataType == DataType.STRING_HTML_COLOR ||
                            field.dataType == DataType.TEXT)
                        {
                            return field.name;
                        }
                    }

                    //
                    // If we have not string name field. we will definitely have an id field, so use that.
                    //
                    return "id";
                }
                else
                {
                    return null;
                }
            }
        }


        protected static string BuildDefaultAngularListingComponentHTMLTemplateImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;


            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = StringUtility.CamelCase(entityName, false);
            string suffixableCamelCaseName = StringUtility.CamelCase(entityName, false);
            string pluralName = StringUtility.Pluralize(entityName);
            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);
            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            string pluralTitle = StringUtility.ConvertToHeader(pluralName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);

            //
            // Suppress active, deleted, objectGuid, and versionNumber fields because they're control fields that don't add value to non-admin users on listing screens
            //
            for (int i = 0; i < fieldsToDisplay.Count; i++)
            {
                if (fieldsToDisplay[i].camelCaseName == "active" ||
                    fieldsToDisplay[i].camelCaseName == "deleted" ||
                    fieldsToDisplay[i].camelCaseName == "versionNumber" ||
                    fieldsToDisplay[i].camelCaseName == "objectGuid")
                {
                    fieldsToDisplay.RemoveAt(i);
                    i--;
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<div class=\"page-container\">");

            sb.AppendLine($"    <div class=\"card mb-3 header-card shadow-sm\">");
            sb.AppendLine($"        <div class=\"card-header d-flex flex-wrap justify-content-between align-items-center gap-3\">");
            sb.AppendLine($"            <div class=\"header-left\">");
            sb.AppendLine($"                <!-- Back Button -->");
            sb.AppendLine($"                <button class=\"btn btn-sm btn-light back-button me-2\" (click)=\"goBack()\" ngbTooltip=\"Go Back\" *ngIf=\"canGoBack()\">");
            sb.AppendLine($"                    <i class=\"fa-solid fa-arrow-left\"></i>");
            sb.AppendLine($"                </button>");
            sb.AppendLine($"");
            sb.AppendLine($"                <!-- Title -->");
            sb.AppendLine($"                <div>");
            sb.AppendLine($"                    <h3 class=\"page-title mb-0\">{pluralTitle}</h3>");
            sb.AppendLine($"                    <small class=\"text-muted\">View {pluralTitle}</small>");
            sb.AppendLine($"                </div>");
            sb.AppendLine($"            </div>");
            sb.AppendLine($"");



            sb.AppendLine($"            <div class=\"header-right\">");

            //
            // This is a more robust version that handles loading state better.  Its a bit more verbose, but guards against almost any possible unusual condition.
            //
            sb.AppendLine($@"              <span class=""badge bg-secondary mx-2"" *ngIf=""loadingTotalCount || loadingFilteredCount"">
                <i class=""fa fa-spinner fa-spin me-1""></i> Loading...
              </span>

              <span *ngIf=""!(loadingTotalCount || loadingFilteredCount)"">
                <ng-container *ngIf=""total{entityName}Count$ | async as total; else falsyObservable"">
                  <span class=""badge bg-secondary mx-2"">

                    <!-- Filtered: some results -->
                    <ng-container *ngIf=""filtered{entityName}Count$ | async as filteredCount"">
                      <span *ngIf=""filterText && (filteredCount ?? 0) > 0"">
                        {{{{ filteredCount }}}} of {{{{ total }}}} {{{{ total !== 1 ? '{pluralTitle}' : '{titleName}' }}}}
                      </span>
                    </ng-container>
                    <!-- Filtered: 0 results -->
                    <span *ngIf=""total > 0 && filterText && (filtered{entityName}Count$ | async) === 0"">
                      No matching {pluralTitle}
                    </span>
                    <!-- No filter: normal count -->
                    <span *ngIf=""!filterText"">
                      {{{{ total }}}} {{{{ total !== 1 ? '{pluralTitle}' : '{titleName}' }}}}
                    </span>
                    <!-- No {pluralTitle} at all (even unfiltered) -->
                    <span *ngIf=""total === 0"">
                      No {pluralTitle}
                    </span>

                  </span>
                </ng-container>

                <!-- when observable is falsy, then decide whether to show a spinner or a loading spinner..-->
                <ng-template #falsyObservable>
                  <span class=""badge bg-secondary mx-2"" *ngIf=""(loadingTotalCount || loadingFilteredCount)"">
                    <i class=""fa fa-spinner fa-spin me-1""></i> Loading...
                  </span>
                  <span class=""badge bg-secondary mx-2"" *ngIf=""!(loadingTotalCount || loadingFilteredCount)"">
                    No {pluralTitle}
                  </span>
                </ng-template>

              </span>
");



            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine($"                <!-- Quick Search Input -->");

                sb.AppendLine($"                <div class=\"search-bar me-2 position-relative\">  ");//<div class=\"input-group input-group-sm\" style=\"max-width: 200px; position: relative;\">");
                sb.AppendLine($"                <i class=\"fa fa-search search-icon\"></i>");
                sb.AppendLine($"                <input type=\"text\"");
                sb.AppendLine($"                   class=\"form-control search-input\"");
                sb.AppendLine($"                   placeholder=\"Search {pluralName}...\"");
                sb.AppendLine($"                   [(ngModel)]=\"filterText\"");
                sb.AppendLine($"                   (ngModelChange)=\"onFilterChange()\">");
                sb.AppendLine($"                <button *ngIf=\"filterText\"");
                sb.AppendLine($"                    class=\"clear-search btn\"");
                sb.AppendLine($"                    (click)=\"filterText=''; onFilterChange()\"");
                sb.AppendLine($"                    title=\"Clear Search\">");
                sb.AppendLine($"                <i class=\"fa fa-times\"></i>");
                sb.AppendLine($"                </button>");
                sb.AppendLine($"            </div>");
            }

            sb.AppendLine($"");
            sb.AppendLine($"             <!-- Add/Edit Button -->");
            sb.AppendLine($"             <app-{angularName}-add-edit [navigateToDetailsAfterAdd]=\"false\"></app-{angularName}-add-edit>");

            sb.AppendLine($"          </div>");
            sb.AppendLine($"      </div>");
            sb.AppendLine($"    </div>");
            sb.AppendLine($"");

            if (addAuthorization == true)
            {
                sb.AppendLine($"  <app-{angularName}-table");
                sb.AppendLine($"    *ngIf=\"userIs{module}{entityName}Reader() == true\"");
                if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
                {
                    sb.AppendLine($"    [filterText]=\"filterText\"");
                }
                sb.AppendLine($"    [isSmallScreen]=\"isSmallScreen\">");
                sb.AppendLine($"  </app-{angularName}-table>");
            }
            else
            {

                sb.AppendLine($"  <app-{angularName}-table");
                if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
                {

                    sb.AppendLine($"    [filterText]=\"filterText\"");
                }
                sb.AppendLine($"    [isSmallScreen]=\"isSmallScreen\">");
                sb.AppendLine($"  </app-{angularName}-table>");
            }

            sb.AppendLine("</div>");

            return sb.ToString();
        }


        protected static string BuildDefaultAngularListingComponentSCSSImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable)
        {
            StringBuilder sb = new StringBuilder();


            sb.AppendLine(@".page-container {
  background-color: #f4f6f9;
  min-height: 100%;
  padding: 1rem;
}


/* Header Card */
.header-card {
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.08);

  .card-header {
    background-color: transparent;
    border: none;
    padding: 1rem 1.5rem;
  }

  .header-left {
    display: flex;
    align-items: center;

    .page-title {
      font-size: 1.5rem;
      font-weight: 600;
      margin: 0;
    }

    .text-muted {
      font-size: 0.875rem;
    }
  }

  .header-right {
    display: flex;
    align-items: center;
  }

  .back-button {
    padding: 0.25rem 0.5rem;
  }


  .search-bar {
    position: relative;
    min-width: 0;     // To help fix overflow
    flex: 1 1 250px;  // Allows shrinking but prefers 250px+
    max-width: 400px;

    .search-icon {
      position: absolute;
      top: 50%;
      left: 15px;
      transform: translateY(-50%);
      color: #6c757d;
    }


    .search-input {
      border: 1px solid #ced4da;
      border-radius: 4px;
      padding-left: 40px;
      width: 100%;
      height: 38px;

      &:focus {
        border-color: #0176d3;
        box-shadow: 0 0 0 0.2rem rgba(1, 118, 211, 0.25);
      }
    }

    .clear-search {
      position: absolute;
      top: 50%;
      right: 15px;
      transform: translateY(-50%);
      background: none;
      border: none;
      color: #6c757d;
      padding: 0;
    }
  }
}
");

            return sb.ToString();
        }


        protected static string BuildDefaultAngularListingComponentTypeScriptImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;

            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);

            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);
            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateCodeGenDisclaimer(entityName, angularName));

            sb.AppendLine("import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';");
            sb.AppendLine("import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';");
            sb.AppendLine("import { BreakpointObserver } from '@angular/cdk/layout';");
            sb.AppendLine("import { NavigationService } from '../../../utility-services/navigation.service';");
            sb.AppendLine("import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';");
            sb.AppendLine($"import {{ {entityName}Service, {entityName}Data }} from '../../../{module.ToLower()}-data-services/{angularName}.service';");
            sb.AppendLine($"import {{ {entityName}AddEditComponent }} from '../{angularName}-add-edit/{angularName}-add-edit.component';");
            sb.AppendLine($"import {{ {entityName}TableComponent }} from '../{angularName}-table/{angularName}-table.component';");
            sb.AppendLine("import { AlertService, MessageSeverity } from '../../../services/alert.service';");

            sb.AppendLine("");
            sb.AppendLine("@Component({");
            sb.AppendLine($"  selector: 'app-{angularName}-listing',");
            sb.AppendLine($"  templateUrl: './{angularName}-listing.component.html',");
            sb.AppendLine($"  styleUrls: ['./{angularName}-listing.component.scss']");
            sb.AppendLine("})");
            sb.AppendLine($"export class {entityName}ListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {{");
            sb.AppendLine($"  @ViewChild({entityName}AddEditComponent) addEdit{entityName}Component!: {entityName}AddEditComponent;");
            sb.AppendLine($"  @ViewChild({entityName}TableComponent) {camelCaseName}TableComponent!: {entityName}TableComponent;");
            sb.AppendLine("");
            sb.AppendLine($"  public {pluralName}: {entityName}Data[] | null = null;");
            sb.AppendLine($"  public isSmallScreen: boolean = false;");
            sb.AppendLine("");


            //
            // Needed for HTML template in all cases - but only will ever beused when there is an 'any string contains' parameter on the query object.
            //
            sb.AppendLine("  public filterText: string | null = null;");
            sb.AppendLine("");

            //
            // Record count state tracking
            //
            sb.AppendLine($@"  public total{entityName}Count$ : Observable<number> | null = null;
  public filtered{entityName}Count$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;
");

            sb.AppendLine($"  constructor(private {suffixableCamelCaseName}Service: {entityName}Service,");

            sb.AppendLine($"              private alertService: AlertService,");
            sb.AppendLine($"              private navigationService: NavigationService,");
            sb.AppendLine($"              private breakpointObserver: BreakpointObserver) {{ }}");
            sb.AppendLine("");
            sb.AppendLine("  ngOnInit(): void {");
            sb.AppendLine();
            sb.AppendLine("    this.breakpointObserver");
            sb.AppendLine("      .observe(['(max-width: 1100px)']) // this size is specified to try and find a balance so tablets and phone see cards, but wider screens get a table.");
            sb.AppendLine("      .subscribe((result) => {");
            sb.AppendLine("        this.isSmallScreen = result.matches;");
            sb.AppendLine("      });");
            sb.AppendLine();
            sb.AppendLine("    this.loadCounts();");
            sb.AppendLine("  }");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("  ngAfterViewInit(): void {");
            sb.AppendLine($"    //");
            sb.AppendLine($"    // Subscribe to the {suffixableCamelCaseName}Changed observable on the add/edit component so that when a {entityName} changes we can reload the list.");
            sb.AppendLine($"    //");
            sb.AppendLine($"    this.addEdit{entityName}Component.{suffixableCamelCaseName}Changed.subscribe({{");
            sb.AppendLine($"      next: (result: {entityName}Data[] | null) => {{");
            sb.AppendLine($"        this.{camelCaseName}TableComponent.loadData();");
            sb.AppendLine($"        this.loadCounts();");
            sb.AppendLine($"      }},");
            sb.AppendLine($"      error: (err: any) => {{");

            sb.AppendLine($"         this.alertService.showMessage(\"Error during {titleName} changed notification\", JSON.stringify(err), MessageSeverity.error);");

            sb.AppendLine($"      }}");
            sb.AppendLine($"    }});");
            sb.AppendLine("  }");
            sb.AppendLine("");
            sb.AppendLine($"  canDeactivate(): boolean {{");
            sb.AppendLine($"    //");
            sb.AppendLine($"    // Do not allow route changes when the modal is up.");
            sb.AppendLine($"    //");
            sb.AppendLine($"    if (this.addEdit{entityName}Component.modalIsDisplayed == true) {{");
            sb.AppendLine($"      return false;");
            sb.AppendLine($"    }} else {{");
            sb.AppendLine($"      return true;");
            sb.AppendLine($"    }}");
            sb.AppendLine($"  }}");
            sb.AppendLine($"");
            sb.AppendLine($"");

            sb.AppendLine($@"  private loadCounts(): void {{

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.total{entityName}Count$ = this.{camelCaseName}Service.Get{pluralName}RowCount({{
      active: true,
      deleted: false
    }}).pipe(
      map(c => Number(c ?? 0)),
      startWith(0),
      finalize(() => {{
        this.loadingTotalCount = false;
      }}),
      shareReplay(1)
    );

    if (this.filterText) {{

      this.filtered{entityName}Count$ = this.{camelCaseName}Service.Get{pluralName}RowCount({{
        active: true,
        deleted: false,
        anyStringContains: this.filterText || undefined
      }}).pipe(
        map(c => Number(c ?? 0)),
        startWith(0),
        finalize(() => {{
          this.loadingFilteredCount = false;
        }}),
        shareReplay(1)
      )
    }} else {{

      this.filtered{entityName}Count$ = this.total{entityName}Count$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }}

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.total{entityName}Count$.subscribe();

    if (this.filtered{entityName}Count$ != this.total{entityName}Count$) {{
      this.filtered{entityName}Count$.subscribe();
    }}
  }}

");

            sb.AppendLine($"  public goBack(): void {{");
            sb.AppendLine($"    this.navigationService.goBack();");
            sb.AppendLine($"   }}");
            sb.AppendLine($"");
            sb.AppendLine($"");
            sb.AppendLine($"  public canGoBack(): boolean {{");
            sb.AppendLine($"    return this.navigationService.canGoBack();");
            sb.AppendLine($"  }}");
            sb.AppendLine($"");
            sb.AppendLine($"");

            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine($"  public clearFilter() {{");
                sb.AppendLine($"    this.filterText = '';");
                sb.AppendLine($"  }}");
                sb.AppendLine($"");
                sb.AppendLine($"");

                sb.AppendLine($@"  //
  // Update the counts when the filter change
  //
  public onFilterChange(): void {{

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {{
      this.{camelCaseName}TableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }}, 500);           // 500 millisecond debounce
  }}");

                sb.AppendLine($"");
                sb.AppendLine($"");
            }


            if (addAuthorization == true)
            {
                sb.AppendLine();
                sb.AppendLine($@"  public userIs{module}{entityName}Reader(): boolean {{
    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Reader();
  }}

  public userIs{module}{entityName}Writer(): boolean {{
    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer();
  }}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GenerateCodeGenDisclaimer(string entityName, string angularName)
        {
            return $@"/*
   GENERATED FORM FOR THE {entityName.ToUpper()} TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from {entityName} table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to {angularName}-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/";
        }

        protected static string BuildDefaultAngularTableComponentHTMLTemplateImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;


            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = StringUtility.CamelCase(entityName, false);
            string suffixableCamelCaseName = StringUtility.CamelCase(entityName, false);
            string pluralName = StringUtility.Pluralize(entityName);
            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);
            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            string pluralTitle = StringUtility.ConvertToHeader(pluralName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);

            //
            // Suppress active, deleted, objectGuid, and versionNumber fields because they're control fields that don't add value to non-admin users on Table screens
            //
            for (int i = 0; i < fieldsToDisplay.Count; i++)
            {
                if (fieldsToDisplay[i].camelCaseName == "active" ||
                    fieldsToDisplay[i].camelCaseName == "deleted" ||
                    fieldsToDisplay[i].camelCaseName == "versionNumber" ||
                    fieldsToDisplay[i].camelCaseName == "objectGuid")
                {
                    fieldsToDisplay.RemoveAt(i);
                    i--;
                }
            }

            StringBuilder sb = new StringBuilder();

            // Add the add edit component with the instruction not to show the add button
            sb.AppendLine($"<app-{angularName}-add-edit [showAddButton]=\"false\"></app-{angularName}-add-edit>");

            if (addAuthorization == true)
            {
                sb.AppendLine($"<div class=\"row\" *ngIf=\"userIs{module}{entityName}Reader() == true\">");
            }
            else
            {
                sb.AppendLine("<div class=\"row\">");
            }

            sb.AppendLine("    <div class=\"col\" *showSpinner=\"isLoading$ | async\">");
            sb.AppendLine("      <!-- Virtual Scrolling Table for Desktop -->");
            sb.AppendLine("      <div class=\"card shadow-sm virtual-table-container\" *ngIf=\"!isSmallScreen\">");
            sb.AppendLine("         <!-- Fixed Header (outside viewport) -->");
            sb.AppendLine("      <div class=\"table-header-fixed\">");
            sb.AppendLine("        <table class=\"custom-table mb-0\">");
            sb.AppendLine("        <thead>");
            sb.AppendLine("          <tr>");

            if (addAuthorization == true)
            {
                sb.AppendLine($"            <th class=\"header-action-button\" style=\"width: 1rem;\" *ngIf=\"userIs{module}{entityName}Writer() == true\"></th>");
                sb.AppendLine($"            <th class=\"header-action-button\" style=\"width: 1rem;\" *ngIf=\"userIs{module}{entityName}Writer() == true\"></th>");
            }
            else
            {
                sb.AppendLine("            <th class=\"header-action-button\" style=\"width: 1rem;\"></th>");
                sb.AppendLine("            <th class=\"header-action-button\" style=\"width: 1rem;\"></th>");
            }


            //
            // Dynamic column definition which replaces the need for the commented th block below.
            // 
            sb.AppendLine($@"
              <!-- Dynamic columns built with rules from the columns input -->
              <th class=""sortable""
                  scope=""col""
                  *ngFor=""let col of columns""
                  (click)=""sortBy(col.key)""
                  [style.width]=""col.width || 'auto'"">
                {{{{ col.label }}}}
                <span *ngIf=""sortColumn === col.key""
                      [ngClass]=""sortDirection === 'asc' ? 'arrow-up' : 'arrow-down'"">
                </span>
              </th>");


            sb.AppendLine("<!-- Commented block containing alternate means of defining columns by manually specifying them all.  This could be useful for people making new components based on this one, so leaving in as a resource.");

            //
            // Create TH tags for each property we want to display in the default component template
            //
            bool addressHeaderWritten = false;
            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                //
                // Special handling for tables with standard address fields.  They get all the address fields in one column.
                //
                if (scriptGenTable.HasAddressFields() == false || fc.field.IsPartOfAddressFields() == false)
                {
                    if (fc.field.isForeignKeyDataType() == false)
                    {
                        sb.AppendLine($"            <th class=\"sortable\" scope=\"col\" (click)=\"sortBy('{fc.field.name}')\">");
                        sb.AppendLine($"                {StringUtility.ConvertToHeader(fc.titleName)}");
                        sb.AppendLine($"                <span *ngIf=\"sortColumn === '{fc.field.name}'\" [ngClass]=\"sortDirection === 'asc' ? 'arrow-up' : 'arrow-down'\"></span>");
                        sb.AppendLine($"            </th>");
                    }
                    else
                    {
                        // Take the ' Id' suffix off of the field name when creating the title for the column
                        sb.AppendLine($"            <th class=\"sortable\" scope=\"col\" (click)=\"sortBy('{GetLinkFieldName(fc)}.name')\">");
                        sb.AppendLine($"                {StringUtility.ConvertToHeader(fc.titleName.Substring(0, fc.titleName.Length - 3))}");
                        sb.AppendLine($"                <span *ngIf=\"sortColumn === '{GetLinkFieldName(fc)}.name'\" [ngClass]=\"sortDirection === 'asc' ? 'arrow-up' : 'arrow-down'\"></span>");
                        sb.AppendLine($"            </th>");
                    }
                }
                else
                {
                    //
                    // Add one header for all the address fields.
                    //
                    if (addressHeaderWritten == false)
                    {
                        sb.AppendLine($"            <th class=\"\" >Address</th>");
                        addressHeaderWritten = true;
                    }
                }
            }

            sb.AppendLine("End of commented block for alternate definition option -->");

            sb.AppendLine("          </tr>");
            sb.AppendLine("        </thead>");
            sb.AppendLine("      </table>");
            sb.AppendLine("      </div>");
            sb.AppendLine("      <cdk-virtual-scroll-viewport itemSize=\"48\" class=\"viewport\" minBufferPx=\"400\" maxBufferPx=\"800\">");
            sb.AppendLine("        <table class=\"custom-table mb-0\">");

            sb.AppendLine("        <tbody>");
            sb.AppendLine("            <!-- Only visible rows are rendered -->");
            //
            // If we have active or deleted fields, then add an ngClass to the TR to change it's background color when rows are inactive or deleted,
            //
            bool haveActiveField = ((from x in scriptGenTable.fields where x.name == "active" select x).Count() == 1);
            bool haveDeletedField = ((from x in scriptGenTable.fields where x.name == "deleted" select x).Count() == 1);

            if (haveActiveField == true || haveDeletedField == true)
            {
                //
                // Create the TR with the ngClass for inactive and deleted rows
                //
                sb.AppendLine($"          <tr *cdkVirtualFor=\"let {camelCaseName} of filtered{pluralName}; trackBy: get{entityName}Id\"");
                sb.AppendLine($"              [ngClass]=\"{{");

                if (haveActiveField == true)
                {
                    sb.AppendLine($"                 'row-inactive': {camelCaseName}.active == false,");
                }

                if (haveDeletedField == true)
                {
                    sb.AppendLine($"                 'row-deleted': {camelCaseName}.deleted == true ");
                }
                sb.AppendLine("                 }\">");
            }
            else
            {
                //
                // Create a plain TR
                //
                sb.AppendLine($"          <tr *cdkVirtualFor=\"let {camelCaseName} of filtered{pluralName}; trackBy: get{entityName}Id\">");
            }

            //
            // Now build the cells
            //

            //
            // First add an edit link
            //
            if (addAuthorization == true)
            {
                sb.AppendLine($"            <td class=\"header-action-button\" *ngIf=\"userIs{module}{entityName}Writer() == true\">");
            }
            else
            {
                sb.AppendLine("            <td class=\"header-action-button\" >");
            }
            sb.AppendLine("              <button class=\"btn btn-sm\"");
            sb.AppendLine($"                      (click)=\"handleEdit({camelCaseName})\"");
            sb.AppendLine("                      ngbTooltip=\"Edit\" ");
            sb.AppendLine("                      placement=\"top\">");
            sb.AppendLine("                <i class=\"fa fa-pen\"></i>");
            sb.AppendLine("              </button>");
            sb.AppendLine("            </td>");


            //
            // Next add a delete button, which could also have an undelete mode.
            //
            if (addAuthorization == true)
            {
                sb.AppendLine($"            <td class=\"header-action-button\" *ngIf=\"userIs{module}{entityName}Writer() == true\">");
            }
            else
            {
                sb.AppendLine("            <td class=\"header-action-button\" >");
            }
            if (haveDeletedField == true)
            {
                //
                // Have deleted field.  Add conditional delete and undelete.
                //
                sb.AppendLine("              <button class=\"btn btn-sm\"");
                sb.AppendLine($"                      (click)=\"handleDelete({camelCaseName})\"");
                sb.AppendLine($"                      ngbTooltip=\"Delete\" ");
                sb.AppendLine($"                      placement=\"top\"");
                sb.AppendLine($"                      *ngIf=\"{camelCaseName}.deleted == false\">");
                sb.AppendLine("                <i class=\"fa fa-trash\"></i>");
                sb.AppendLine("              </button>");
                sb.AppendLine("              <button class=\"btn btn-sm\"");
                sb.AppendLine($"                      (click)=\"handleUndelete({camelCaseName})\"");
                sb.AppendLine($"                      ngbTooltip=\"Undelete\" ");
                sb.AppendLine($"                      placement=\"top\"");
                sb.AppendLine($"                      *ngIf=\"{camelCaseName}.deleted == true\">");
                sb.AppendLine($"                <i class=\"fa fa-trash-arrow-up\"></i>");
                sb.AppendLine($"              </button>");
            }
            else
            {
                //
                // No deleted field, so we can't do an undelete.
                //
                sb.AppendLine("              <button class=\"btn btn-sm\"");
                sb.AppendLine($"                      (click)=\"handleDelete({camelCaseName})\"");
                sb.AppendLine($"                      ngbTooltip=\"Delete\" ");
                sb.AppendLine($"                      placement=\"top\">");
                sb.AppendLine("                <i class=\"fa fa-trash\"></i>");
                sb.AppendLine("              </button>");
            }
            sb.AppendLine("            </td>");


            //
            // Dyanamic cell code that replaces the need for the commented block below
            //
            sb.AppendLine($@"
              <!-- Dynamic cells built with rules from the columns input -->
              <td *ngFor=""let col of columns""
                  class=""text-truncate""
                  [style.width]=""col.width || 'auto'"">

                <!-- Custom rendering -->
                <ng-container [ngSwitch]=""col.template"">

                  <!-- handle booleans -->
                  <boolean-icon *ngSwitchCase=""'boolean'"" [value]=""getNestedValue({camelCaseName}, col.key)"" />

                  <!-- handle internal links -->
                  <a *ngSwitchCase=""'link'""
                     [routerLink]=""col.linkPath ? buildLink({camelCaseName}, col.linkPath) : null""
                     class=""btn btn-link text-left"">
                    {{{{ getNestedValue({camelCaseName}, col.key) }}}}
                  </a>

                  <!-- handle dates -->
                  <span *ngSwitchCase=""'date'"">
                    {{{{ getNestedValue({camelCaseName}, col.key) | date:'yyyy-MM-dd HH:mm:ss Z' }}}}
                  </span>

                  <!-- handle colors -->
                  <span *ngSwitchCase=""'color'"" class=""d-inline-flex align-items-center gap-2"">
                    <!-- Color swatch -->
                    <span class=""color-swatch""
                            [style.background-color]=""getNestedValue({camelCaseName}, col.key) || '#cccccc'""
                            title=""{{{{ getNestedValue({camelCaseName}, col.key) || 'No color set' }}}}"">
                    </span>
                    <!-- Hex value -->
                    <!-- <span class=""text-muted small"">
                        {{{{ getNestedValue({camelCaseName}, col.key) || '—' }}}}
                    </span> -->
                  </span>

                  <!-- handle anything else -->
                  <span *ngSwitchDefault>
                    {{{{ getNestedValue({camelCaseName}, col.key) }}}}
                  </span>
                </ng-container>
              </td>");


            sb.AppendLine("<!-- Commented block containing alternate means of defining cells by manually specifying them all.  This could be useful for people making new components based on this one, so leaving in as a resource.");


            //
            // Now add the rest of the fields.
            //
            bool currentEntityTypeDetailLinkAdded = false;
            bool addressCellWritten = false;

            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                //
                // Special handling for tables with standard address fields.  They get all the address fields in one column.
                //
                if (scriptGenTable.HasAddressFields() == false || fc.field.IsPartOfAddressFields() == false)
                {
                    //
                    // for FK fields, use the FK sub object's first string property, and make it a link to it's detail screen
                    // 
                    if (fc.field.isForeignKeyDataType() == true)
                    {
                        //
                        // This is a foreign key field
                        //

                        //
                        // Use the .name property because that is guaranteed to be one on the minimal anonymous object that is referenced here.
                        //
                        sb.AppendLine($"            <td class=\"text-truncate\"><a [routerLink]=\"['/{fc.linkTable.name.ToLower()}', {camelCaseName}.{fc.camelCaseName}]\" class=\"btn btn-link text-left\" ngbTooltip=\"Open {ConvertToHeader(fc.linkTable.name)} details\">{{{{{camelCaseName}.{GetLinkFieldName(fc)}?.name}}}}</a></td>");
                    }
                    else
                    {
                        //
                        // This is not a foreign key field
                        //

                        //
                        // Use the property value as the text shown, and always make the first field a link to the details
                        //
                        // Route number and data type properties through standard angular filters.
                        //

                        // Special handling for boolean properties to use a custom visualization component
                        if (fc.field.isBooleanDataType() == true)
                        {
                            // use bool visualizer component
                            sb.AppendLine($"            <td class=\"text-truncate\"><boolean-icon [value]=\"{camelCaseName}.{fc.camelCaseName}\"/></td>");
                        }
                        else
                        {
                            //
                            // Non bool.  Display text, or format to appropriate data type/
                            //

                            string dataFilter = "";

                            if (fc.field.isDateTimeDataType() == true)
                            {
                                dataFilter = " | date:'YYYY-MM-dd HH:mm:ss OOOO'";
                            }
                            else if (fc.field.isDateDataType() == true)
                            {
                                dataFilter = " | date:'YYYY-MM-dd'";
                            }
                            else if (fc.field.isNumericDataType() == true)
                            {
                                dataFilter = " | bigNumberFormat : '1.0-2'";        // this is a custom pipe that support big numbers
                            }

                            if (currentEntityTypeDetailLinkAdded == false)
                            {
                                sb.AppendLine($"            <td class=\"text-truncate\"><a [routerLink]=\"['/{suffixableCamelCaseName.ToLower()}', {camelCaseName}.id]\" class=\"btn btn-link text-left\" ngbTooltip=\"Open {titleName} details\">{{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}</a></td>");

                                currentEntityTypeDetailLinkAdded = true;
                            }
                            else
                            {
                                if (fc.field.isEmailAddress() == true)
                                {
                                    sb.AppendLine($"            <td class=\"text-truncate\"><a href=\"mailto:{{{{{camelCaseName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Send Email to {{{{{camelCaseName}?.{fc.camelCaseName}}}}}\">{{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}</a></td>");
                                }
                                else if (fc.field.isPhoneNumber() == true)
                                {
                                    sb.AppendLine($"            <td class=\"text-truncate\"><a href=\"tel:{{{{{camelCaseName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Call {{{{{camelCaseName}?.{fc.camelCaseName}}}}}\">{{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}</a></td>");
                                }

                                else
                                {
                                    sb.AppendLine($"            <td class=\"text-truncate\">{{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}</td>");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (addressCellWritten == false)
                    {
                        //
                        // Put in the standard address fields.
                        //
                        if (scriptGenTable.HasField("stateId") == true)
                        {
                            sb.AppendLine($"            <td class=\"text-truncate\">");
                            sb.AppendLine($"              <small>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address1\">{{{{{camelCaseName}.address1}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address2\">{{{{{camelCaseName}.address2}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address3\">{{{{{camelCaseName}.address3}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.stateId\">{{{{{camelCaseName}.state?.name}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.postalCode\">{{{{{camelCaseName}.postalCode}}}}</span>");
                            sb.AppendLine($"              </small>");
                            sb.AppendLine($"            </td>");
                        }
                        else
                        {
                            sb.AppendLine($"            <td class=\"text-truncate\">");
                            sb.AppendLine($"              <small>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address1\">{{{{{camelCaseName}.address1}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address2\">{{{{{camelCaseName}.address2}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address3\">{{{{{camelCaseName}.address3}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.state\">{{{{{camelCaseName}.state}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.postalCode\">{{{{{camelCaseName}.postalCode}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.country\">{{{{{camelCaseName}.country}}}}</span>");
                            sb.AppendLine($"              </small>");
                            sb.AppendLine($"            </td>");
                        }

                        addressCellWritten = true;
                    }
                }
            }
            sb.AppendLine("End of commented block for alternate definition option -->");

            sb.AppendLine("                 </tr>");
            sb.AppendLine("             </tbody>");
            sb.AppendLine("         </table>");
            sb.AppendLine("         </cdk-virtual-scroll-viewport>");
            sb.AppendLine("      </div>");
            sb.AppendLine();
            sb.AppendLine("      <!-- Cards for Mobile -->");
            sb.AppendLine($"      <cdk-virtual-scroll-viewport *ngIf=\"isSmallScreen\" itemSize=\"320\" class=\"mobile-viewport\" minBufferPx=\"600\" maxBufferPx=\"1200\">");


            if (haveActiveField == true || haveDeletedField == true)
            {
                //
                // Create the for loop with the ngClass for inactive and deleted rows
                //
                sb.AppendLine($"        <div *cdkVirtualFor=\"let {camelCaseName} of filtered{pluralName}; trackBy: get{entityName}Id\" class=\"card my-2\"");
                sb.AppendLine($"              [ngClass]=\"{{");

                if (haveActiveField == true)
                {
                    sb.AppendLine($"                 'row-inactive': {camelCaseName}.active == false,");
                }

                if (haveDeletedField == true)
                {
                    sb.AppendLine($"                 'row-deleted': {camelCaseName}.deleted == true ");
                }
                sb.AppendLine("                 }\">");
            }
            else
            {
                //
                // Create a plain for with no classes on it
                //
                sb.AppendLine($"        <div *cdkVirtualFor=\"let {camelCaseName} of filtered{pluralName}; trackBy: get{entityName}Id\" class=\"card my-2\">");
            }


            sb.AppendLine($@"

        <!-- Prominent field first, if defined on the columns input -->
        <div class=""card-header py-3"" *ngIf=""prominentColumn"">
          <strong class=""d-block text-truncate fs-5"">
            

            <ng-container [ngSwitch]=""prominentColumn.template"">
              <boolean-icon *ngSwitchCase=""'boolean'"" [value]=""getNestedValue({camelCaseName}, prominentColumn.key)"" />
              <a *ngSwitchCase=""'link'""
                 [routerLink]=""prominentColumn.linkPath ? buildLink({camelCaseName}, prominentColumn.linkPath) : null""
                 class=""text-primary"">
                {{{{ getNestedValue({camelCaseName}, prominentColumn.key) }}}}
              </a>
              <span *ngSwitchCase=""'date'"">
                {{{{ getNestedValue({camelCaseName}, prominentColumn.key) | date:'yyyy-MM-dd HH:mm:ss Z' }}}}
              </span>
              <span *ngSwitchDefault>
                {{{{ getNestedValue({camelCaseName}, prominentColumn.key) }}}}
              </span>
            </ng-container>


          </strong>
        </div>

        <!-- Main card body built with rules from the columns input  -->
        <div class=""card-body small"">
          <div class=""row g-2"">
            <ng-container *ngFor=""let col of mobileColumns"">
              <!-- Skip if it's the prominent one (already shown above) -->
              <ng-container *ngIf=""col !== prominentColumn"">
                <div class=""col-5 text-muted"">{{{{ col.label }}}}:</div>
                <div class=""col-7 text-end fw-medium"">

                  <ng-container [ngSwitch]=""col.template"">

                    <!-- boolean handling -->
                    <boolean-icon *ngSwitchCase=""'boolean'"" [value]=""getNestedValue({camelCaseName}, col.key)"" />

                    <!-- internal link handling -->
                    <a *ngSwitchCase=""'link'""
                       [routerLink]=""col.linkPath ? buildLink({camelCaseName}, col.linkPath) : null""
                       class=""text-primary"">
                      {{{{ getNestedValue({camelCaseName}, col.key)}}}}
                    </a>

                    <!-- data handling -->
                    <span *ngSwitchCase=""'date'"">
                      {{{{ getNestedValue({camelCaseName}, col.key) | date:'yyyy-MM-dd HH:mm:ss Z' }}}}
                    </span>

                    <!-- color handling -->
                    <span *ngSwitchCase=""'color'"" class=""d-inline-flex align-items-center gap-2 justify-content-end"">
                        <span class=""color-swatch""
                            [style.background-color]=""getNestedValue({camelCaseName}, col.key) || '#cccccc'""
                            title=""{{{{ getNestedValue({camelCaseName}, col.key) || 'No color set' }}}}"">
                        </span>
                        <!--
                        <span class=""small"">
                        {{{{ getNestedValue({camelCaseName}, col.key) || '—' }}}}
                        </span>
                        -->
                    </span>

                    <!-- handle everything else -->
                    <span *ngSwitchDefault>
                      {{{{ getNestedValue({camelCaseName}, col.key)}}}}
                    </span>
                  </ng-container>

                </div>
              </ng-container>
            </ng-container>
          </div>
        </div>
");

            sb.AppendLine($@"
        <!-- Action buttons -->
        <div class=""card-footer bg-transparent border-0 pt-0"">
          <div class=""btn-group w-100"" role=""group"">
            <button class=""btn btn-sm btn-outline-primary flex-fill"" [routerLink]=""['/{camelCaseName.ToLower()}', {camelCaseName}.id]"" ngbTooltip=""View"">
              <i class=""fa fa-magnifying-glass""></i> View
            </button>
            <button class=""btn btn-sm btn-outline-secondary"" (click)=""handleEdit({camelCaseName})"" {(addAuthorization == true ? $"*ngIf=\"userIs{module}{entityName}Writer()\"" : "")} ngbTooltip=""Edit"">
              <i class=""fa fa-pen""></i> Edit
            </button>");


            //
            // If we have a deleted field, add both delete and undelete
            //
            if (haveDeletedField == true)
            {
                sb.AppendLine($@"
            <button class=""btn btn-sm btn-outline-danger"" (click)=""handleDelete({camelCaseName})"" {(addAuthorization == true ? $"*ngIf=\"userIs{module}{entityName}Writer() && {camelCaseName}.deleted == false\"" : $"*ngIf=\"{camelCaseName}.deleted == false\"")} ngbTooltip=""Delete"">
              <i class=""fa fa-trash""></i> Delete
            </button>
            <button class=""btn btn-sm btn-outline-warning"" (click)=""handleUndelete({camelCaseName})"" {(addAuthorization == true ? $"*ngIf=\"userIs{module}{entityName}Writer() && {camelCaseName}.deleted == true\"" : $"*ngIf=\"{camelCaseName}.deleted == true\"")} ngbTooltip=""Undelete"">
              <i class=""fa fa-trash-arrow-upp""></i> UnDelete
            </button>");

            }
            else
            {
                //
                // Add just a delete button
                //
                sb.AppendLine($@"
            <button class=""btn btn-sm btn-outline-danger"" (click)=""handleDelete({camelCaseName})"" {(addAuthorization == true ? $"*ngIf=\"userIs{module}{entityName}Writer()\"" : "")} ngbTooltip=""Delete"">
              <i class=""fa fa-trash""></i> Delete
            </button>");
            }
            sb.AppendLine($@"
          </div>
        </div>
      </div>
");

            sb.AppendLine("<!-- Commented block containing alternate means of defining card by manually specifying fields.  This could be useful for people making new components based on this one, so leaving in as a resource.");


            sb.AppendLine($"           <div class=\"card-header d-flex justify-content-between align-items-center\">");
            sb.AppendLine($"             <div>");

            sb.Append($"               <strong>");


            //
            // write strong text for each displayName field if we have display names.  Otherwise, expect there to be a name field or an id field.
            //
            if (scriptGenTable.displayNameFieldList.Count > 0)
            {
                for (int i = 0; i < scriptGenTable.displayNameFieldList.Count; i++)
                {
                    var dnf = scriptGenTable.displayNameFieldList[i];

                    if (i != 0)
                    {
                        sb.Append($"{{{{{camelCaseName}.{dnf.name} != null ? \", \" + {camelCaseName}.{dnf.name} : \"\"}}}}");
                    }
                    else
                    {
                        sb.Append($"{{{{{camelCaseName}.{dnf.name}}}}}");
                    }
                }
            }
            else
            {
                bool hasNameField = false;

                foreach (var fc in fieldsToDisplay)
                {
                    if (fc.camelCaseName == "name")
                    {
                        hasNameField = true;
                        break;
                    }
                }

                if (hasNameField == true)
                {
                    sb.Append($"{{{{{camelCaseName}.name}}}}");
                }
                else
                {
                    //
                    // It'll have an id at least...
                    //
                    sb.Append($"{{{{{camelCaseName}.id}}}}");
                }
            }

            sb.AppendLine($"</strong>");

            //
            // Add the first foreign key table's name as the sub text in a small tag
            //
            if (scriptGenTable.foreignKeys.Count > 0)
            {
                var fkField = GetMostSensibleForeignFieldForSmallText(scriptGenTable);

                // Not sure if this will work on both .Net Core and .Net Framework entity objects.  it should work on Core at least.  If it doesn't work for Framework, then add a separate platform specification write for that platform.
                sb.AppendLine($"               <small class=\"text-muted d-block\">{{{{{camelCaseName}.{GetLinkFieldName(fkField)}?.name}}}}</small>");
            }

            sb.AppendLine($"             </div>");
            sb.AppendLine($"             <div>");
            sb.AppendLine($"                <button class=\"btn btn-sm\"");
            sb.AppendLine($"                        [routerLink]=\"['/{suffixableCamelCaseName.ToLower()}', {camelCaseName}.id]\"");
            sb.AppendLine($"                        ngbTooltip=\"{titleName} Details\">");
            sb.AppendLine($"                <i class=\"fa fa-magnifying-glass\"></i>");
            sb.AppendLine($"                </button>");
            sb.AppendLine($"                <button class=\"btn btn-sm\"");
            sb.AppendLine($"                        (click)=\"handleEdit({camelCaseName})\"");
            sb.AppendLine($"                        ngbTooltip=\"Edit {titleName}\">");
            sb.AppendLine($"                <i class=\"fa fa-pen\"></i>");
            sb.AppendLine($"                </button>");

            if (haveDeletedField == true)
            {
                //
                // Have deleted field.  Add conditional delete and undelete.
                //
                sb.AppendLine("                <button class=\"btn btn-sm\"");
                sb.AppendLine($"                      (click)=\"handleDelete({camelCaseName})\"");
                sb.AppendLine($"                      ngbTooltip=\"Delete {titleName}\" ");
                sb.AppendLine($"                      placement=\"top\"");
                sb.AppendLine($"                      *ngIf=\"{camelCaseName}.deleted == false\">");
                sb.AppendLine("                  <i class=\"fa fa-trash\"></i>");
                sb.AppendLine("                </button>");
                sb.AppendLine("                <button class=\"btn btn-sm\"");
                sb.AppendLine($"                      (click)=\"handleUndelete({camelCaseName})\"");
                sb.AppendLine($"                      ngbTooltip=\"Undelete {titleName}\" ");
                sb.AppendLine($"                      placement=\"top\"");
                sb.AppendLine($"                      *ngIf=\"{camelCaseName}.deleted == true\">");
                sb.AppendLine($"                 <i class=\"fa fa-trash-arrow-up\"></i>");
                sb.AppendLine($"                </button>");
            }
            else
            {
                //
                // No deleted field, so we can't do an undelete.
                //
                sb.AppendLine("                <button class=\"btn btn-sm\"");
                sb.AppendLine($"                     (click)=\"handleDelete({camelCaseName})\"");
                sb.AppendLine($"                     ngbTooltip=\"Delete {titleName}\" ");
                sb.AppendLine($"                     placement=\"top\">");
                sb.AppendLine("                  <i class=\"fa fa-trash\"></i>");
                sb.AppendLine("                </button>");
            }



            sb.AppendLine($"             </div>");
            sb.AppendLine($"             </div>");
            sb.AppendLine($"             <div class=\"card-body\">");

            //
            // Now add the rest of the fields.
            //
            addressCellWritten = false;         // reset address written to false flag.
            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                //
                // Special handling for tables with standard address fields.  They get all the address fields in one column.
                //
                if (scriptGenTable.HasAddressFields() == false || fc.field.IsPartOfAddressFields() == false)
                {
                    //
                    // for FK fields, use the FK sub object's first string property, and make it a link to it's detail screen
                    // 
                    if (fc.field.isForeignKeyDataType() == true)
                    {
                        //
                        // This is a foreign key field
                        //

                        //
                        // Use the .name property because that is guaranteed to be one on the minimal anonymous object that is referenced here.
                        //
                        sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.{fc.camelCaseName} != null\">");
                        sb.AppendLine($"              <strong>{StringUtility.TitleCase(fc.titleName.Substring(0, fc.titleName.Length - 3))}: </strong>");
                        sb.AppendLine($"              <a [routerLink]=\"['/{fc.linkTable.name.ToLower()}', {camelCaseName}.{fc.camelCaseName}]\" class=\"text-left p-0 m-0\" ngbTooltip=\"Open {ConvertToHeader(fc.linkTable.name)} details\">{{{{{camelCaseName}.{GetLinkFieldName(fc)}?.name}}}}</a>");
                        sb.AppendLine($"            </p>");
                    }
                    else
                    {
                        //
                        // This is not a foreign key field
                        //

                        //
                        // Use the property value as the text shown, and always make the first field a link to the details
                        //
                        // Route number and data type properties through standard angular filters.
                        //

                        // Special handling for boolean properties to use a custom visualization component
                        if (fc.field.isBooleanDataType() == true)
                        {
                            // use bool visualizer component
                            sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.{fc.camelCaseName} != null\">");
                            sb.AppendLine($"              <strong>{fc.titleName}: </strong>");
                            sb.AppendLine($"              <boolean-icon [value]=\"{camelCaseName}.{fc.camelCaseName}\"/>");
                            sb.AppendLine($"            </p>");
                        }
                        else
                        {
                            //
                            // Non bool.  Display text, or format to appropriate data type.
                            //
                            string dataFilter = "";

                            if (fc.field.isDateTimeDataType() == true)
                            {
                                dataFilter = " | date:'YYYY-MM-dd HH:mm:ss OOOO'";
                            }
                            else if (fc.field.isDateDataType() == true)
                            {
                                dataFilter = " | date:'YYYY-MM-dd'";
                            }
                            else if (fc.field.isNumericDataType() == true)
                            {
                                dataFilter = " | bigNumberFormat : '1.0-2'";        // this is a custom pipe that support big numbers
                            }

                            if (fc.field.isEmailAddress() == true)
                            {
                                sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.{fc.camelCaseName} != null\">");
                                sb.AppendLine($"              <strong>{fc.titleName}: </strong>");
                                sb.AppendLine($"              <a href=\"mailto:{{{{{camelCaseName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Send Email to {{{{{camelCaseName}?.{fc.camelCaseName}}}}}\">{{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}</a>");
                                sb.AppendLine($"            </p>");

                            }
                            else if (fc.field.isPhoneNumber() == true)
                            {
                                sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.{fc.camelCaseName} != null\">");
                                sb.AppendLine($"              <strong>{fc.titleName}: </strong>");
                                sb.AppendLine($"              <a href=\"tel:{{{{{camelCaseName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Call {{{{{camelCaseName}?.{fc.camelCaseName}}}}}\">{{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}</a>");
                                sb.AppendLine($"            </p>");
                            }
                            else
                            {
                                sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.{fc.camelCaseName} != null\">");
                                sb.AppendLine($"              <strong>{fc.titleName}: </strong>");
                                sb.AppendLine($"              {{{{{camelCaseName}.{fc.camelCaseName}{dataFilter}}}}}");
                                sb.AppendLine($"            </p>");
                            }
                        }
                    }
                }
                else
                {
                    if (addressCellWritten == false)
                    {
                        //
                        // Put in the standard address fields.
                        //
                        if (scriptGenTable.HasField("stateId") == true)
                        {

                            sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.address1\">");
                            sb.AppendLine($"              <strong>Address: </strong>");
                            sb.AppendLine($"              <small>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address1\">{{{{{camelCaseName}.address1}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address2\">{{{{{camelCaseName}.address2}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address3\">{{{{{camelCaseName}.address3}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.stateId\">{{{{{camelCaseName}.state?.name}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.postalCode\">{{{{{camelCaseName}.postalCode}}}}</span>");
                            sb.AppendLine($"              </small>");
                            sb.AppendLine($"            </p>");
                        }
                        else
                        {
                            sb.AppendLine($"            <p class=\"{angularName}-info\" *ngIf=\"{camelCaseName}.address1\">");
                            sb.AppendLine($"              <strong>Address: </strong>");
                            sb.AppendLine($"              <small>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address1\">{{{{{camelCaseName}.address1}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address2\">{{{{{camelCaseName}.address2}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.address3\">{{{{{camelCaseName}.address3}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.state\">{{{{{camelCaseName}.state}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.postalCode\">{{{{{camelCaseName}.postalCode}}}}</span>");
                            sb.AppendLine($"                <span *ngIf=\"{camelCaseName}.country\">{{{{{camelCaseName}.country}}}}</span>");
                            sb.AppendLine($"              </small>");
                            sb.AppendLine($"            </p>");

                        }
                        addressCellWritten = true;
                    }
                }
            }

            sb.AppendLine($"            </div>");
            sb.AppendLine($"         </div>");

            sb.AppendLine("End of commented block for alternate definition option  -->");

            sb.AppendLine($"      </cdk-virtual-scroll-viewport>");
            sb.AppendLine($"   </div>");
            sb.AppendLine($"</div>");

            return sb.ToString();
        }


        protected static string BuildDefaultAngularTableComponentSCSSImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable)
        {
            string entityName;


            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            bool haveActiveField = ((from x in scriptGenTable.fields where x.name == "active" select x).Count() == 1);
            bool haveDeletedField = ((from x in scriptGenTable.fields where x.name == "deleted" select x).Count() == 1);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@".virtual-table-container {
  height: calc(100vh - 220px);
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 5px rgba(0,0,0,0.08);
  background: white;
  display: flex;
  flex-direction: column;
}

/* Fixed header (outside viewport) */
.table-header-fixed {
  background: white;
  z-index: 30;
  border-bottom: 2px solid #dee2e6;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

/* Viewport — the only thing that scrolls */
.viewport {
  flex: 1;
  overflow-y: auto;
  overflow-x: auto;         /* to allow scrolling if the content exceeds width.  Note headers won't move with content, but will accept that compromise for not making data unreachable. */
  scrollbar-gutter: stable; /* eliminates scrollbar jump */

  /* Hack for consistnent column sizing between header and body tables*/
  padding-right: 17px;
  margin-right: -17px;
}

.mobile-viewport {
  height: calc(100vh - 180px); /* or whatever fits your layout */
  display: block;
}

/* Base table — shared by header and body */
.custom-table {
  table-layout: fixed;
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  margin: 0;
}

/* Header table keeps Bootstrap look */
.table-header-fixed .custom-table {
  border-collapse: collapse;
}

/* Inactive (deactivated) rows */
tr.row-inactive,
.card.row-inactive {
  font-style: italic;
  opacity: 0.75; // Slightly faded for extra clarity
}

/* Deleted rows */
tr.row-deleted,
.card.row-deleted {
  text-decoration: line-through;
  opacity: 0.6;
  color: #999; // Muted color
}

/* Combine both if deleted takes precedence */
tr.row-deleted.row-inactive,
.card.row-deleted.row-inactive {
  font-style: normal; // Strikethrough wins over italic
}

.card.row-inactive {
  font-style: italic;
  opacity: 0.75;
}

.card.row-deleted {
  text-decoration: line-through;
  opacity: 0.6;
  background-color: #f8f9fa; // Light gray background
}



/* Body table — completely clean (no Bootstrap magic) */
.viewport .custom-table {
  background: transparent;
}

  /* Manual striping & hover (exactly what Bootstrap does) */
  .viewport .custom-table tbody tr {
    background-color: white;
  }

    .viewport .custom-table tbody tr:nth-child(odd) {
      background-color: #f8f9fa;
    }

    .viewport .custom-table tbody tr:hover {
      background-color: #e9ecef !important;
    }

/* Perfect alignment — header and body use IDENTICAL rules */
.custom-table th,
.custom-table td {
  padding: 0.75rem !important;
  border: none !important;
  box-sizing: border-box;
  white-space: nowrap;
  overflow: hidden;
  vertical-align: middle;
}

/* Action button columns — identical in header and body */
.header-action-button {
  width: 45px !important;
  min-width: 45px !important;
  max-width: 45px !important;
  padding: 0.5rem 0.25rem !important;
  text-align: center;
}

/* Sortable header styling */
.sortable {
  cursor: pointer;
  position: relative;
  padding-right: 1.5rem;
  white-space: nowrap;
}

  .sortable:hover {
    background-color: #f8f9fa;
  }

/* Sort arrows */
.arrow-up::after,
.arrow-down::after {
  content: '';
  display: inline-block;
  margin-left: 5px;
  width: 0;
  height: 0;
  vertical-align: middle;
}

.arrow-up::after {
  border-left: 5px solid transparent;
  border-right: 5px solid transparent;
  border-bottom: 5px solid #000;
}

.arrow-down::after {
  border-left: 5px solid transparent;
  border-right: 5px solid transparent;
  border-top: 5px solid #000;
}

/* CDK wrapper — never adds unwanted padding */
.cdk-virtual-scroll-content-wrapper {
  width: 100% !important;
}


/* Nicer custom scrollbar */
.viewport::-webkit-scrollbar {
  width: 10px;
}

.viewport::-webkit-scrollbar-track {
  background: transparent;
}

.viewport::-webkit-scrollbar-thumb {
  background: rgba(0,0,0,0.2);
  border-radius: 5px;
}

.viewport::-webkit-scrollbar-thumb:hover {
  background: rgba(0,0,0,0.4);
}

.color-swatch {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 1px solid #ced4da;        /* Matches form-control border for consistency */
  border-radius: 4px;               /* Slight rounding for modern look */
  flex-shrink: 0;                   /* Prevents shrinking in flex layouts */
  box-shadow: 0 1px 2px rgba(0,0,0,0.1); /* Subtle depth */
}

/* Slightly larger swatch on desktop for better visibility */
@media (min-width: 768px) {
  .color-swatch {
    width: 24px;
    height: 24px;
  }
}

/* Ensure the text doesn't get truncated awkwardly when next to the swatch */
td .color-swatch,
.col-7 .color-swatch {
  vertical-align: middle;
}

");


            return sb.ToString();
        }


        protected static string BuildDefaultAngularTableComponentTypeScriptImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;

            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);

            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);
            string angularName = StringUtility.ConvertToAngularComponentName(entityName);


            bool haveDeletedField = ((from x in scriptGenTable.fields where x.name == "deleted" select x).Count() == 1);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateCodeGenDisclaimer(entityName, angularName));

            sb.AppendLine("import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';");
            sb.AppendLine("import { BehaviorSubject } from 'rxjs';");
            sb.AppendLine($"import {{ {entityName}Service, {entityName}Data, {entityName}QueryParameters }} from '../../../{module.ToLower()}-data-services/{angularName}.service';");
            sb.AppendLine($"import {{ {entityName}AddEditComponent }} from '../{angularName}-add-edit/{angularName}-add-edit.component';");
            if (addAuthorization == true)
            {
                sb.AppendLine("import { AuthService } from '../../../services/auth.service';");
            }
            sb.AppendLine("import { AlertService, MessageSeverity } from '../../../services/alert.service';");
            sb.AppendLine("import { ConfirmationService } from '../../../services/confirmation-service';");
            sb.AppendLine("import { TableColumn } from '../../../utility/foundation.utility';");

            sb.AppendLine("");
            sb.AppendLine("@Component({");
            sb.AppendLine($"  selector: 'app-{angularName}-table',");
            sb.AppendLine($"  templateUrl: './{angularName}-table.component.html',");
            sb.AppendLine($"  styleUrls: ['./{angularName}-table.component.scss'],");
            sb.AppendLine($"  changeDetection: ChangeDetectionStrategy.OnPush");
            sb.AppendLine("})");
            sb.AppendLine($"export class {entityName}TableComponent implements OnInit, OnChanges, AfterViewInit {{");
            sb.AppendLine($"  @ViewChild({entityName}AddEditComponent) addEdit{entityName}Component!: {entityName}AddEditComponent;");
            sb.AppendLine("");

            sb.AppendLine($"  @Input() {pluralName}: {entityName}Data[] | null = null; // Optional prefiltered data");
            sb.AppendLine($"  @Input() isSmallScreen: boolean = false;");

            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine($"  @Input() filterText: string | null = null; // Optional filter text ");
            }

            sb.AppendLine($"  @Input() queryParams: Partial<{entityName}QueryParameters> = {{ }} // Optional query parameters");
            sb.AppendLine();
            sb.AppendLine($"  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior");
            sb.AppendLine($"  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior");

            if (haveDeletedField == true)
            {
                sb.AppendLine($"  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior");
            }

            sb.AppendLine();
            sb.AppendLine($"  @Output() edit = new EventEmitter<{entityName}Data>(); // Emitted for custom edit handling");
            sb.AppendLine($"  @Output() delete = new EventEmitter<{entityName}Data>(); // Emitted for custom delete handling");

            if (haveDeletedField == true)
            {
                sb.AppendLine($"  @Output() undelete = new EventEmitter<{entityName}Data>(); // Emitted for custom undelete handling");
            }




            sb.AppendLine();
            sb.AppendLine($"  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit");


            sb.AppendLine();
            sb.AppendLine($"  public filtered{pluralName}: {entityName}Data[] | null = null;        // Stores the filtered/sorted data");
            sb.AppendLine();
            sb.AppendLine($"  // Sorting properties");
            sb.AppendLine($"  public sortColumn: string | null = null;");
            sb.AppendLine($"  public sortDirection: 'asc' | 'desc' = 'asc';");
            sb.AppendLine();

            sb.AppendLine("");

            sb.AppendLine("  private isLoadingSubject = new BehaviorSubject<boolean>(true);");
            sb.AppendLine("  public isLoading$ = this.isLoadingSubject.asObservable();");
            sb.AppendLine();
            sb.AppendLine("  private isManagingData: boolean = false; // Tracks if component is managing data loading");
            sb.AppendLine();

            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine("  private debounceTimeout: any;");
                sb.AppendLine("");
            }


            sb.AppendLine($@"
  //
  // Error state tracking to help suppress endless loop scenarios in case of server error or request path errors
  //
  private inErrorState: boolean = false;
  private errorResetTimeout: any;

");

            sb.AppendLine($"  constructor(private {suffixableCamelCaseName}Service: {entityName}Service,");

            if (addAuthorization == true)
            {
                sb.AppendLine($"              private authService: AuthService,");
            }

            sb.AppendLine($"              private alertService: AlertService,");
            sb.AppendLine($"              private confirmationService: ConfirmationService) {{ }}");
            sb.AppendLine("");
            sb.AppendLine("  ngOnInit(): void {");
            sb.AppendLine();
            sb.AppendLine("    // If the parent has not provided custom columns, build the defaults with entitlement checks");
            sb.AppendLine("    if (this.columns.length === 0) {");
            sb.AppendLine("      this.buildDefaultColumns();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    if (!this.{pluralName}) {{");
            sb.AppendLine();
            sb.AppendLine($"        this.isManagingData = true; // Component is managing data loading");
            sb.AppendLine($"        this.loadData(); // Load data on initialization");
            sb.AppendLine();
            sb.AppendLine($"    }} else {{");
            sb.AppendLine();
            sb.AppendLine($"        this.applyFiltersAndSort();");
            sb.AppendLine($"        this.isLoadingSubject.next(false);");
            sb.AppendLine();
            sb.AppendLine($"    }}");
            sb.AppendLine($"  }}");
            sb.AppendLine();

            sb.AppendLine("  ngAfterViewInit(): void {");
            sb.AppendLine($"");
            sb.AppendLine($"    //");
            sb.AppendLine($"    // Subscribe to the {suffixableCamelCaseName}Changed observable on the add/edit component so that when a {entityName} changes we can reload the list, if component is available and not disabled..");
            sb.AppendLine($"    //");
            sb.AppendLine($"    if (this.addEdit{entityName}Component && !this.disableDefaultEdit) {{");
            sb.AppendLine($"        this.addEdit{entityName}Component.{suffixableCamelCaseName}Changed.subscribe({{");
            sb.AppendLine($"        next: (result: {entityName}Data[] | null) => {{");
            sb.AppendLine($"            this.loadData();");
            sb.AppendLine($"        }},");
            sb.AppendLine($"        error: (err: any) => {{");

            sb.AppendLine($"             this.alertService.showMessage(\"Error during {titleName} changed notification\", JSON.stringify(err), MessageSeverity.error);");

            sb.AppendLine($"        }}");
            sb.AppendLine($"        }});");
            sb.AppendLine("     }");
            sb.AppendLine("  }");
            sb.AppendLine();


            sb.AppendLine($"  ngOnChanges(changes: SimpleChanges): void {{");
            sb.AppendLine();

            // Don't react to input changes when in error state, because it's most likely an immediate retry will just retrigger the same error again.
            //
            // Note that if there's a server error, or a code error with a route being called, it's not likely that this will fix itself, but just in
            // case a brief server condition triggered this, we'll reset the error state flag every 10 seconds.
            //
            sb.AppendLine($@"
    if (this.inErrorState == true) {{
      //
      // Circuit breaker for endless loop prevention.
      //
      return;
    }}
");
            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine($"    //");
                sb.AppendLine($"    // Reset the whole page - note that this only makes sense when this component is managing the loading of data.  Don't use the filterText input property when you are providing your own data via the data input.");
                sb.AppendLine($"    //");
                sb.AppendLine($"    if (changes['filterText'] && this.isManagingData == true)");
                sb.AppendLine($"    {{");
                sb.AppendLine($"       clearTimeout(this.debounceTimeout);");
                sb.AppendLine($"       this.debounceTimeout = setTimeout(() => {{");
                sb.AppendLine();
                sb.AppendLine($"         if (this.isManagingData)");
                sb.AppendLine($"         {{");
                sb.AppendLine($"             this.loadData();");
                sb.AppendLine($"         }}");
                sb.AppendLine($"         else");
                sb.AppendLine($"         {{");
                sb.AppendLine($"             this.applyFiltersAndSort();");
                sb.AppendLine($"         }}");
                sb.AppendLine();
                sb.AppendLine($"       }}, 200); // 200ms debounce delay");
                sb.AppendLine($"    }}");
                sb.AppendLine();
                sb.AppendLine($"    if (changes['queryParams'])");
                sb.AppendLine($"    {{");
                sb.AppendLine($"        this.loadData()");
                sb.AppendLine($"    }}");
            }
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();


            List<FieldConfiguration> fieldsForAllusers = new List<FieldConfiguration>();
            List<FieldConfiguration> additionalfieldsForWriters = new List<FieldConfiguration>();
            List<FieldConfiguration> additionalfieldsForAdmins = new List<FieldConfiguration>();


            //
            // Create lists of the fields to display to help only show the admin type fields to privileged users.
            //
            // TODO - Data visiblity fields are not considered here yet, but they should be added.  Do that when needed.
            //
            // Readers see regular fields
            // Writers see regular fields plus version number
            // Admins see regular fields plus version number, active state, deleted state
            //
            // NOTE: CSS styling makes deleted rows stike through, and inactive rows italicized, so the active/deleted columns aren't really needed on list views
            //
            foreach (FieldConfiguration fieldConfiguration in fieldsToDisplay)
            {
                if (fieldConfiguration.field.name == "active")
                {
                    //additionalfieldsForWriters.Add(fieldConfiguration);
                    additionalfieldsForAdmins.Add(fieldConfiguration);
                }
                else if (fieldConfiguration.field.name == "deleted")
                {
                    additionalfieldsForAdmins.Add(fieldConfiguration);
                }
                else if (fieldConfiguration.field.name == "versionNumber")
                {
                    additionalfieldsForWriters.Add(fieldConfiguration);
                    additionalfieldsForAdmins.Add(fieldConfiguration);
                }
                else
                {
                    fieldsForAllusers.Add(fieldConfiguration);
                }
            }


            sb.AppendLine($@" /**
   * Construct the default column array based on user entitlements.
   */
  private buildDefaultColumns(): void {{

    //
    // Add the fields to the column config.  Refinements to widths and such can be made easily by custom users by tweaking the input as need be.
    //
    // This is the default set to start with something for customization
    //
    // Note width purposely set to undefined so the reader knows the property is there for use.  It is to be overridden as appropriate by users 
    // providing custom column specs.
    //
    // Start with the common columns that everyone sees
    //
    const defaultColumns: TableColumn[] = [");

            bool firstStringFound = false;
            foreach (FieldConfiguration field in fieldsForAllusers)
            {
                //
                // Make the first string 'Prominent' for mobile mode, and linkable to the element's detail page.
                //
                if (firstStringFound == false && IsString(field) == true)
                {
                    sb.AppendLine($"    {{ key: '{field.camelCaseName}', label: '{field.titleName}', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/{camelCaseName.ToLower()}', 'id']  }},");

                    firstStringFound = true;
                }
                else
                {
                    sb.AppendLine("    " + GenerateTableColumnAdditionLine(field, true));
                }
            }

            sb.AppendLine($@"
    ];");

            if (addAuthorization == true)
            {
                sb.AppendLine($@"

    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.{camelCaseName}Service.userIs{module}{entityName}Writer();
    const isAdmin = this.authService.is{module}Administrator; 

    if (isAdmin) {{");

                foreach (FieldConfiguration field in additionalfieldsForAdmins)
                {
                    sb.AppendLine("     defaultColumns.push(" + GenerateTableColumnAdditionLine(field, false) + ");");
                }

                sb.AppendLine($@"
    }}
    else if (isWriter) {{");

                foreach (FieldConfiguration field in additionalfieldsForWriters)
                {
                    sb.AppendLine("     defaultColumns.push(" + GenerateTableColumnAdditionLine(field, false) + ");");
                }

                sb.AppendLine($@"    }}
");
            }
            else
            {
                //
                // No auth mode just gets all fields
                //
                foreach (FieldConfiguration field in additionalfieldsForAdmins)
                {
                    sb.AppendLine("     defaultColumns.push(" + GenerateTableColumnAdditionLine(field, false) + ");");
                }
                foreach (FieldConfiguration field in additionalfieldsForWriters)
                {
                    sb.AppendLine("     defaultColumns.push(" + GenerateTableColumnAdditionLine(field, false) + ");");
                }
            }


            sb.AppendLine($@"    
    // Assign the built array as the active columns
    this.columns = defaultColumns;
  }}

");

            sb.AppendLine($"  public sortBy(column: string) : void {{");
            sb.AppendLine();
            sb.AppendLine($"    if (this.sortColumn === column) {{");
            sb.AppendLine($"        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';");
            sb.AppendLine($"    }} else {{");
            sb.AppendLine($"        this.sortColumn = column;");
            sb.AppendLine($"        this.sortDirection = 'asc';");
            sb.AppendLine($"    }}");
            sb.AppendLine();
            sb.AppendLine($"    this.applyFiltersAndSort();");
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("  public loadData(): void {");
            sb.AppendLine();

            sb.AppendLine("    if (!this.isManagingData) {");
            sb.AppendLine("      return; // Skip if parent is providing data");
            sb.AppendLine("    }");


            if (addAuthorization == true)
            {
                sb.AppendLine();
                sb.AppendLine($@"    if (this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Reader() == false) {{
      this.alertService.showMessage(this.authService.currentUser?.userName + "" does not have the permission to read from {Pluralize(titleName)}"", '', MessageSeverity.info);
      return;
    }}");
            }

            sb.AppendLine();


            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine($"    //");
                sb.AppendLine($"    // Server side filtering using the any string contains parameter");
                sb.AppendLine($"    //");
                sb.AppendLine($"    const {camelCaseName}QueryParams = {{");
                sb.AppendLine($"        ...this.queryParams,");
                sb.AppendLine($"        anyStringContains: this.filterText || undefined");
                sb.AppendLine($"    }};");
            }
            else
            {
                sb.AppendLine($"    const {camelCaseName}QueryParams = this.queryParams;");
            }

            sb.AppendLine();

            sb.AppendLine("    //");
            sb.AppendLine("    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.");
            sb.AppendLine("    //");
            sb.AppendLine($"    this.{suffixableCamelCaseName}Service.Get{entityName}List({suffixableCamelCaseName}QueryParams).subscribe({{");
            sb.AppendLine($"      next: ({entityName}List) => {{");
            sb.AppendLine($"        if ({entityName}List) {{");
            sb.AppendLine($"          this.{pluralName} = {entityName}List;");
            sb.AppendLine("        } else {");
            sb.AppendLine($"          this.{pluralName} = [];");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        //");
            sb.AppendLine("        // Apply the sort.  Filtering of data done already done in the data we receive from the service.");
            sb.AppendLine("        //");
            sb.AppendLine("        this.applyFiltersAndSort();");
            sb.AppendLine();
            sb.AppendLine("        //");
            sb.AppendLine("        // Clear the loading spinner");
            sb.AppendLine("        //");
            sb.AppendLine("        this.isLoadingSubject.next(false);");
            sb.AppendLine();
            sb.AppendLine("        //");
            sb.AppendLine("        // Reset the error state");
            sb.AppendLine("        //");
            sb.AppendLine("        this.inErrorState = false;");
            sb.AppendLine();
            sb.AppendLine("      },");
            sb.AppendLine("      error: (err) => {");

            sb.AppendLine();
            sb.AppendLine("        //");
            sb.AppendLine("        // Clear the loading spinner");
            sb.AppendLine("        //");
            sb.AppendLine("        this.isLoadingSubject.next(false);");
            sb.AppendLine();
            sb.AppendLine("        //");
            sb.AppendLine("        // Turn the error state on");
            sb.AppendLine("        //");
            sb.AppendLine("        this.setErrorState();");
            sb.AppendLine();

            sb.AppendLine($"         this.alertService.showMessage(\"Error getting {titleName} data\", JSON.stringify(err), MessageSeverity.error);");

            sb.AppendLine("      }");
            sb.AppendLine("    });");
            sb.AppendLine("  }");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine(@"

  private setErrorState(): void {

    // Turn error state flag on
    this.inErrorState = true;

    // wipe any existing timeout
    clearTimeout(this.errorResetTimeout);

    // Reset error state after 15 seconds
    this.errorResetTimeout = setTimeout(() => {
      this.inErrorState = false;
    }, 15000); // Allow retry after 15s
  }

");

            sb.AppendLine($"   private applyFiltersAndSort(): void {{");
            sb.AppendLine($"");
            sb.AppendLine($"    if (!this.{pluralName}) {{");
            sb.AppendLine($"      this.filtered{pluralName} = null;");
            sb.AppendLine($"      return;");
            sb.AppendLine($"    }}");
            sb.AppendLine($"");
            sb.AppendLine($"    // Helper function to safely access nested properties used by sorting and filtering.");
            sb.AppendLine($"    const getNestedValue = (obj: any, path: string): any => {{");
            sb.AppendLine($"      return path.split('.').reduce((current, key) => {{");
            sb.AppendLine($"        return current && current[key] !== undefined ? current[key] : '';");
            sb.AppendLine($"      }}, obj);");
            sb.AppendLine($"    }};");
            sb.AppendLine($"");
            sb.AppendLine($"");
            sb.AppendLine($"    let result = [...this.{pluralName}];");
            sb.AppendLine($"");

            //
            // Do this check because without this on, there is no filter text variable, and realistically, no string fields to be able to filter by anyway.
            //
            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine($"    if (this.filterText) {{");
                sb.AppendLine($"");
                sb.AppendLine($"      const searchText = this.filterText.toLowerCase().trim();");
                sb.AppendLine($"");
                sb.AppendLine($"      if (searchText) {{");
                sb.AppendLine($"");
                sb.AppendLine($"        // Define fields to filter on, including nested properties");
                sb.AppendLine($"        const filterFields = [");

                foreach (FieldConfiguration fc in fieldsToDisplay)
                {
                    if (fc.field.name == "id" ||
                        fc.field.name == "active" ||
                        fc.field.name == "deleted" ||
                        fc.field.name == "versionNumber" ||
                        fc.field.name == "objectGuid")
                    {
                        continue;
                    }

                    if (fc.field.isForeignKeyDataType() == false)
                    {
                        sb.AppendLine($"                      '{fc.field.name}',");
                    }
                    else
                    {
                        sb.AppendLine($"                      '{GetLinkFieldName(fc)}.name',");
                    }
                }

                sb.AppendLine($"        ];");
                sb.AppendLine($"");

                sb.AppendLine($"        result = result.filter(({camelCaseName}) =>");
                sb.AppendLine($"");
                sb.AppendLine($"        filterFields.some((field) => {{");
                sb.AppendLine($"        const value = getNestedValue({camelCaseName}, field);");
                sb.AppendLine($"            return value && value.toString().toLowerCase().includes(searchText);");
                sb.AppendLine($"          }})");
                sb.AppendLine($"          );");
                sb.AppendLine($"      }}");
                sb.AppendLine($"    }}");
                sb.AppendLine($"");
            }

            sb.AppendLine($"    // Apply sorting");
            sb.AppendLine($"    if (this.sortColumn) {{");
            sb.AppendLine($"      result.sort((a, b) => {{");
            sb.AppendLine($"");
            sb.AppendLine($"        const aValue = getNestedValue(a, this.sortColumn!);");
            sb.AppendLine($"        const bValue = getNestedValue(b, this.sortColumn!);");
            sb.AppendLine($"");
            sb.AppendLine($"        if (typeof aValue === 'number' && typeof bValue === 'number') {{");
            sb.AppendLine($"          return this.sortDirection === 'asc' ? aValue - bValue : bValue - aValue;");
            sb.AppendLine($"        }}");
            sb.AppendLine($"");
            sb.AppendLine($"        const aStr = aValue ? aValue.toString() : '';");
            sb.AppendLine($"        const bStr = bValue ? bValue.toString() : '';");
            sb.AppendLine($"        const comparison = aStr.localeCompare(bStr, undefined, {{sensitivity: 'base' }});");
            sb.AppendLine($"        return this.sortDirection === 'asc' ? comparison : -comparison;");
            sb.AppendLine($"      }});");
            sb.AppendLine($"    }}");
            sb.AppendLine($"");
            sb.AppendLine($"    this.filtered{pluralName} = result;");
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"  public handleEdit({camelCaseName}: {entityName}Data): void {{");
            sb.AppendLine($"    if (this.disableDefaultEdit)");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        this.edit.emit({camelCaseName}); // Let parent handle edit");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    else if (this.addEdit{entityName}Component)");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        this.addEdit{entityName}Component.openModal({camelCaseName}); // Default edit behavior");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    else");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        this.alertService.showMessage(");
            sb.AppendLine($"          'Edit functionality unavailable',");
            sb.AppendLine($"          'Add/Edit component not initialized',");
            sb.AppendLine($"          MessageSeverity.warn");
            sb.AppendLine($"        );");
            sb.AppendLine($"    }}");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($"");

            sb.AppendLine($"  public handleDelete({camelCaseName}: {entityName}Data): void {{");
            sb.AppendLine($"    if (this.disableDefaultDelete)");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        this.delete.emit({camelCaseName}); // Let parent handle delete");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    else");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        this.confirmationService");
            sb.AppendLine($"          .confirm('Delete {entityName}', 'Are you sure you want to delete this {titleName}?')");
            sb.AppendLine($"          .then((result) => {{");
            sb.AppendLine($"              if (result)");
            sb.AppendLine($"              {{");
            sb.AppendLine($"                  this.delete{entityName}({camelCaseName});");
            sb.AppendLine($"              }}");
            sb.AppendLine($"          }})");
            sb.AppendLine($"          .catch(() => {{ }});");
            sb.AppendLine($"    }}");
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();


            sb.AppendLine($"  private delete{entityName}({suffixableCamelCaseName}Data: {entityName}Data): void {{");
            sb.AppendLine($"    this.{suffixableCamelCaseName}Service.Delete{entityName}({suffixableCamelCaseName}Data.id).subscribe({{");
            sb.AppendLine("      next: () => {");
            sb.AppendLine($"       this.{suffixableCamelCaseName}Service.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.");
            sb.AppendLine("        this.loadData(); // Reload the data list after deletion");
            sb.AppendLine("      },");
            sb.AppendLine("      error: (err) => {");
            sb.AppendLine($"         this.alertService.showMessage(\"Error deleting {titleName}\", JSON.stringify(err), MessageSeverity.error);");
            sb.AppendLine("      }");
            sb.AppendLine("    });");
            sb.AppendLine("  }");
            sb.AppendLine("");
            sb.AppendLine("");


            if (haveDeletedField == true)
            {
                sb.AppendLine($"  public handleUndelete({camelCaseName}: {entityName}Data): void {{");
                sb.AppendLine($"    if (this.disableDefaultUndelete)");
                sb.AppendLine($"    {{");
                sb.AppendLine($"        this.undelete.emit({camelCaseName}); // Let parent handle undelete");
                sb.AppendLine($"    }}");
                sb.AppendLine($"    else");
                sb.AppendLine($"    {{");
                sb.AppendLine($"        this.confirmationService");
                sb.AppendLine($"          .confirm('Undelete {entityName}', 'Are you sure you want to undelete this {titleName}?')");
                sb.AppendLine($"          .then((result) => {{");
                sb.AppendLine($"              if (result)");
                sb.AppendLine($"              {{");
                sb.AppendLine($"                  this.undelete{entityName}({camelCaseName});");
                sb.AppendLine($"              }}");
                sb.AppendLine($"          }})");
                sb.AppendLine($"          .catch(() => {{ }});");
                sb.AppendLine($"    }}");
                sb.AppendLine($"}}");
                sb.AppendLine("");
                sb.AppendLine("");


                sb.AppendLine($"  private undelete{entityName}({suffixableCamelCaseName}Data: {entityName}Data): void {{");
                sb.AppendLine();
                sb.AppendLine($"      var {suffixableCamelCaseName}ToSubmit = this.{suffixableCamelCaseName}Service.ConvertTo{entityName}SubmitData({suffixableCamelCaseName}Data); // Convert {entityName} data to post object for undeleting");
                sb.AppendLine($"      {camelCaseName}ToSubmit.deleted = false;");
                sb.AppendLine();
                sb.AppendLine($"      this.{suffixableCamelCaseName}Service.Put{entityName}({suffixableCamelCaseName}ToSubmit.id, {suffixableCamelCaseName}ToSubmit).subscribe({{");

                sb.AppendLine("      next: () => {");
                sb.AppendLine($"       this.{suffixableCamelCaseName}Service.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.");
                sb.AppendLine("        this.loadData(); // Reload the data list after un-deletion");
                sb.AppendLine("      },");
                sb.AppendLine("      error: (err) => {");

                sb.AppendLine($"         this.alertService.showMessage(\"Error undeleting {titleName}\", JSON.stringify(err), MessageSeverity.error);");


                sb.AppendLine("      }");
                sb.AppendLine("    });");
                sb.AppendLine("  }");
                sb.AppendLine("");
                sb.AppendLine("");
            }

            sb.AppendLine($"  public get{entityName}Id(index: number, {camelCaseName}: any): number {{");
            sb.AppendLine($"    return {camelCaseName}.id;");
            sb.AppendLine("  }");
            sb.AppendLine("");


            if (addAuthorization == true)
            {
                sb.AppendLine();
                sb.AppendLine($@"  public userIs{module}{entityName}Reader(): boolean {{
    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Reader();
  }}

  public userIs{module}{entityName}Writer(): boolean {{
    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer();
  }}

");
            }

            sb.AppendLine($@"

  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {{
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }}


  // Build routerLink arrays like ['/{camelCaseName}', {camelCaseName}Id]
  public buildLink(item: any, path: string[]): any[] {{
    //
    // Expect a starting item in the path array with a slash to indicate the route.  After that, the other items in path are expected to be properties of the item.  Tyically one, but more are technically supported.
    //
    // The example usage is ['/route', 'routeId'], where route is the name of the route, and 'routeId' is the property name on the item object that indexes the route.
    //
    return path.map(segment => segment.startsWith('/') ? segment : item[segment]);
  }}


  // Returns only columns that should appear on mobile
  get mobileColumns(): TableColumn[] {{
    return this.columns.filter(col => col.mobile !== 'hidden');
  }}


  // First ""prominent"" column for mobile view
  get prominentColumn(): TableColumn | null {{
    return this.columns.find(col => col.mobile === 'prominent') || null;
  }}
");

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GenerateTableColumnAdditionLine(FieldConfiguration field, bool includeComma = false)
        {
            string output;

            if (field.field.dataType == DataType.DATE ||
                                    field.field.dataType == DataType.DATETIME)
            {
                output = $"{{ key: '{field.camelCaseName}', label: '{field.titleName}', width: undefined, template: 'date' }}" + (includeComma ? "," : "");
            }
            else if (field.field.dataType == DataType.BOOL)
            {
                output = $"{{ key: '{field.camelCaseName}', label: '{field.titleName}', width: '120px', template: 'boolean' }}" + (includeComma ? "," : "");
            }
            else if (field.field.dataType == DataType.STRING_HTML_COLOR)
            {
                output = $"{{ key: '{field.camelCaseName}', label: '{field.titleName}', width: \"50px\", template: 'color' }}" + (includeComma ? "," : "");
            }
            else if (field.field.dataType == DataType.FOREIGN_KEY ||
                     field.field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER ||
                     field.field.dataType == DataType.FOREIGN_KEY_GUID ||
                     field.field.dataType == DataType.FOREIGN_KEY_STRING)
            {
                output = $"{{ key: '{GetLinkFieldName(field)}.name', label: '{ConvertToHeader(field.linkTable.name)}', width: undefined, template: 'link', linkPath: ['/{field.linkTable.name.ToLower()}', '{field.camelCaseName}'] }}" + (includeComma ? "," : ""); ;
            }
            else
            {
                output = $"{{ key: '{field.camelCaseName}', label: '{field.titleName}', width: undefined }}" + (includeComma ? "," : "");
            }


            if (field.field.hideOnDefaultLists == true)
            {
                output = "// " + output;
            }

            return output;
        }

        private static bool IsString(FieldConfiguration field)
        {
            if (field.field.dataType == DataType.STRING_10 ||
                field.field.dataType == DataType.STRING_100 ||
                field.field.dataType == DataType.STRING_1000 ||
                field.field.dataType == DataType.STRING_2000 ||
                field.field.dataType == DataType.STRING_250 ||
                field.field.dataType == DataType.STRING_50 ||
                field.field.dataType == DataType.STRING_500 ||
                field.field.dataType == DataType.STRING_850 ||
                field.field.dataType == DataType.STRING_HTML_COLOR ||
                field.field.dataType == DataType.STRING_PRIMARY_KEY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected static string BuildDefaultAngularAddEditComponentHTMLTemplateImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;

            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }

            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);
            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);

            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);

            StringBuilder sb = new StringBuilder();

            // Button to open the modal
            sb.AppendLine($"<div class=\"add-{angularName}\" *ngIf=\"showAddButton == true\">");
            sb.AppendLine($"    <button type=\"button\"");
            sb.AppendLine($"        class=\"btn btn-sm btn-add-{angularName} me-2\"");
            sb.AppendLine($"        (click)=\"openModal()\"");
            sb.AppendLine($"        ngbTooltip=\"Add {titleName}\" ");
            if (addAuthorization == true)
            {
                sb.AppendLine($"        *ngIf=\"userIs{module}{entityName}Writer() == true\"");
            }
            sb.AppendLine($"        placement=\"top\"");
            sb.AppendLine($"        style=\"min-width: 75px;\">Add</button>");
            sb.AppendLine($"</div>");

            sb.AppendLine("");
            sb.AppendLine("<!-- ng-bootstrap modal -->");
            sb.AppendLine($"<ng-template #{suffixableCamelCaseName}Modal let-modal>");
            sb.AppendLine($"   <div class=\"modal-header bg-dark text-light {angularName}-modal-header\">");
            sb.AppendLine($"      <h5 class=\"modal-title mb-0\" id=\"{suffixableCamelCaseName}ModalLabel\" ngbTooltip=\"{{{{objectGuid}}}}\">");
            sb.AppendLine($"      {{{{ isEditMode ? 'Edit {titleName}' : 'Add {titleName}' }}}}");
            sb.AppendLine($"      </h5>");
            sb.AppendLine($"      <button type=\"button\"");
            sb.AppendLine($"              class=\"btn btn-sm btn-close btn-close-white\"");
            sb.AppendLine($"              (click)=\"closeModal()\">");
            sb.AppendLine($"       </button>");
            sb.AppendLine($"   </div>");
            sb.AppendLine($"   <div class=\"modal-body {angularName}-modal-body\">");

            // Use reactive form with [formGroup] instead of ngForm
            sb.AppendLine($"      <form [formGroup]=\"{suffixableCamelCaseName}Form\" (ngSubmit)=\"submitForm()\">");

            // Generate form fields (updated in GenerateFormInputFields for reactive forms)
            GenerateFormInputFields(module, entityName, camelCaseName + "SubmitData", angularName, fieldsToDisplay, sb, true, addAuthorization, 3);

            sb.AppendLine($"         <div class=\"modal-footer {angularName}-modal-footer\">");
            sb.AppendLine($"            <button type=\"button\" class=\"btn btn-sm btn-secondary\" (click)=\"closeModal()\" ngbTooltip=\"Cancel the {titleName} changes and Close\">Cancel</button>");

            // Update disabled logic for reactive forms
            sb.AppendLine($"            <button type=\"submit\" class=\"btn btn-sm btn-primary\" [disabled]=\"isSaving || !{suffixableCamelCaseName}Form.valid || {suffixableCamelCaseName}Form.pristine{(addAuthorization ? $" || !userIs{module}{entityName}Writer()" : "")}\" ngbTooltip=\"Save the {titleName} changes and Close\">{{{{ isEditMode ? 'Save' : 'Create' }}}}</button>");
            sb.AppendLine($"         </div>");
            sb.AppendLine($"      </form>");
            sb.AppendLine($"   </div>");
            sb.AppendLine($"</ng-template>");

            return sb.ToString();
        }


        private static void GenerateFormInputFields(string module, string entityName, string dataObjectName, string angularName, List<FieldConfiguration> fieldsToDisplay, StringBuilder sb, bool addEditModeConditions, bool addAuthorization, int tabCount = 6)
        {
            string suffixableCamelCaseName = StringUtility.CamelCase(entityName, false);

            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                //
                // Add a red asterisk to the front of the label if the field is not nullable
                //
                string headerNameToUse = fc.titleName;

                if (fc.field.isForeignKeyDataType() == true)
                {
                    // Take the ' Id' off the end of the field name
                    headerNameToUse = headerNameToUse.Substring(0, headerNameToUse.Length - 3);
                }

                string optionalRequiredIndicator = "";
                if (fc.field.nullable == false)
                {
                    optionalRequiredIndicator = "<span class=\"text-danger\">*</span>";
                }



                //
                // Note the varying class on the wrapper div for boolean properties.
                //
                if (fc.field.isBooleanDataType() == true)
                {
                    //
                    // Handle boolean fields with different classes - form-check wrapper and my-2 for margins
                    //
                    if (fc.camelCaseName == "active" || fc.camelCaseName == "deleted")
                    {
                        // Special carve out for active and deleted fields. They should only be visible in edit mode.
                        sb.AppendLine(GetTabWhiteSpace(tabCount) + "<div class=\"form-check my-2\"" + (addEditModeConditions == true ? " *ngIf=\"isEditMode && !isFieldHidden('{fc.camelCaseName}')\"" : "*ngIf=\"!isFieldHidden('{fc.camelCaseName}')\"") + $" )>");
                    }
                    else
                    {
                        // Normal boolean field
                        sb.AppendLine(GetTabWhiteSpace(tabCount) + $"<div class=\"form-check my-2\" *ngIf=\"!isFieldHidden('{fc.camelCaseName}')\">");
                    }
                }
                else
                {
                    if (fc.camelCaseName == "versionNumber")
                    {
                        // Special carve out for versionNumber field. It should only be visible in edit mode.
                        sb.AppendLine(GetTabWhiteSpace(tabCount) + "<div class=\"form-group\"" + (addEditModeConditions == true ? " *ngIf=\"isEditMode && !isFieldHidden('{fc.camelCaseName}')\"" : "*ngIf=\"!isFieldHidden('{fc.camelCaseName}')\"") + $" )>");
                    }
                    else
                    {
                        // Regular field.
                        sb.AppendLine(GetTabWhiteSpace(tabCount) + $"<div class=\"form-group\" *ngIf=\"!isFieldHidden('{fc.camelCaseName}')\">");
                    }
                }

                //
                // Draw Label
                //
                if (fc.field.isEmailAddress() == true)
                {
                    if (addEditModeConditions == true)
                    {
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\" *ngIf=\"isEditMode\"><a href=\"mailto:{{{{{dataObjectName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Send Email to {{{{{dataObjectName}?.{fc.camelCaseName}}}}}\">{optionalRequiredIndicator}{headerNameToUse}</a></label>");
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\" *ngIf=\"!isEditMode\">{optionalRequiredIndicator}{headerNameToUse}</label>");
                    }
                    else
                    {
                        sb.AppendLine($"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\"><a href=\"mailto:{{{{{dataObjectName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Send Email to {{{{{dataObjectName}?.{fc.camelCaseName}}}}}\">{optionalRequiredIndicator}{headerNameToUse}</a></label>");
                    }
                }
                else if (fc.field.isPhoneNumber() == true)
                {
                    if (addEditModeConditions == true)
                    {
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\" *ngIf=\"isEditMode\"><a href=\"tel:{{{{{dataObjectName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Phone {{{{{dataObjectName}?.{fc.camelCaseName}}}}}\">{optionalRequiredIndicator}{headerNameToUse}</a></label>");
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\" *ngIf=\"!isEditMode\">{optionalRequiredIndicator}{headerNameToUse}</label>");
                    }
                    else
                    {
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\"><a href=\"tel:{{{{{dataObjectName}?.{fc.camelCaseName}}}}}\" ngbTooltip=\"Phone {{{{{dataObjectName}?.{fc.camelCaseName}}}}}\">{optionalRequiredIndicator}{headerNameToUse}</a></label>");
                    }
                }
                else if (fc.field.isBooleanDataType() == true)
                {
                    // Don't draw a label here.  It will be handled inside the boolean input writing
                }
                else
                {
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" id=\"label-{entityName}-{fc.camelCaseName}\">{optionalRequiredIndicator}{headerNameToUse}</label>");
                }

                //
                // Create input fields for the different field types
                //
                if (fc.field.isLargeTextDataType() == true)
                {
                    // Use a textarea for large text fields
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<textarea id=\"{entityName}-{fc.camelCaseName}\" formControlName=\"{fc.camelCaseName}\" placeholder=\"Enter {fc.titleName}\" class=\"form-control {angularName}-input\" {(addAuthorization ? $"[disabled]=\"!userIs{module}{entityName}Writer()\"" : "")}></textarea>");
                }
                else if (fc.field.isForeignKeyDataType() == true)
                {
                    // Use a select for foreign keys
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<select id=\"{entityName}-{fc.camelCaseName}\" formControlName=\"{fc.camelCaseName}\" class=\"form-select {angularName}-select\" {(addAuthorization ? $"[disabled]=\"!userIs{module}{entityName}Writer()\"" : "")}>");
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 2) + $"<option [ngValue]=\"null\">Select {ConvertToHeader(fc.linkTable.name)}</option>");

                    // Handle display name for foreign keys
                    string nameProperties;
                    if (fc.linkTable.displayNameFieldList.Count > 0)
                    {
                        nameProperties = "[";
                        for (int i = 0; i < fc.linkTable.displayNameFieldList.Count; i++)
                        {
                            Database.Table.Field displayNameField = fc.linkTable.displayNameFieldList[i];
                            if (i > 0)
                            {
                                nameProperties += ", ";
                            }
                            nameProperties += CamelCase(fc.linkTable.name, false) + "." + displayNameField.name;
                        }
                        nameProperties += "] | filterAndJoin";
                    }
                    else
                    {
                        nameProperties = CamelCase(fc.linkTable.name, false) + "." + fc.GetFirstStringFieldFromLinkTable();
                    }

                    sb.AppendLine(GetTabWhiteSpace(tabCount + 2) + $"<option *ngFor=\"let {CamelCase(fc.linkTable.name, false)} of {CamelCase(Pluralize(fc.linkTable.name), false)}$ | async\" [ngValue]=\"{CamelCase(fc.linkTable.name, false)}.id\">{{{{{nameProperties}}}}}</option>");

                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"</select>");
                }
                else if (fc.field.isHTMLColor() == true)
                {
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $@"<div class=""d-flex align-items-center gap-3"">
                    <!-- Native color picker input (type=""color"") -->
                    <input 
                        type=""color"" 
                        id=""{entityName}-{fc.camelCaseName}"" 
                        formControlName=""{fc.camelCaseName}"" 
                        class=""form-control form-control-color color-picker"" 
                        [disabled]=""!userIs{module}{entityName}Writer()""
                        aria-labelledby=""label-color""
                        title=""Choose Color"">
                    
                    <!-- Usually this isn't needed
                    <input 
                        type=""text"" 
                        [value]=""{suffixableCamelCaseName}Form.get('color')?.value || '#000000'"" 
                        readonly 
                        class=""form-control input-color-display""
                        style=""width: 120px;"">-->
                </div>");

                    // this HEX version of the colour strings adds no value.  Users can use an eye dropper to get the value.  99% of people don't ever care to see this.
                    //<!-- <small class=""form-text text-muted"">
                    //    Select a color. The value will be stored as a hex code (e.g., #0176d3).
                    //</small> -->")

                }
                else if (fc.field.isBooleanDataType() == true)
                {
                    // Use a checkbox for boolean values (no 'required' attribute to avoid forcing true)
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<input type=\"checkbox\" id=\"{entityName}-{fc.camelCaseName}\" formControlName=\"{fc.camelCaseName}\" class=\"form-check-input\" {(addAuthorization ? $"[disabled]=\"!userIs{module}{entityName}Writer()\"" : "")} />");

                    // draw label with special form-check-label class after the input for booleans
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<label for=\"{entityName}-{fc.camelCaseName}\" class=\"form-check-label\" id=\"label-{entityName}-{fc.camelCaseName}\">{optionalRequiredIndicator}{headerNameToUse}</label>");
                }
                else
                {
                    // Map other types to appropriate HTML input types
                    string htmlDataType;

                    if (fc.field.isTextDataType() == true)
                    {
                        htmlDataType = "text";
                    }
                    else if (fc.field.isNumericDataType() == true)
                    {
                        htmlDataType = "number";
                    }
                    else if (fc.field.isDateDataType() == true)
                    {
                        htmlDataType = "date";
                    }
                    else if (fc.field.isDateTimeDataType() == true)
                    {
                        htmlDataType = "datetime-local";
                    }
                    else if (fc.field.isEmailAddress() == true)
                    {
                        htmlDataType = "email";
                    }
                    else if (fc.field.dataType == DataType.URI)
                    {
                        htmlDataType = "url";
                    }
                    else
                    {
                        htmlDataType = "text";
                    }

                    if (fc.camelCaseName == "versionNumber")
                    {
                        // Special handling to disable the versionNumber field
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<input type=\"{htmlDataType}\" id=\"{entityName}-{fc.camelCaseName}\" formControlName=\"{fc.camelCaseName}\" [disabled]=\"true\" placeholder=\"Enter {fc.titleName}\" class=\"form-control {angularName}-input\" />");
                    }
                    else
                    {
                        // Regular field with aria-labelledby for accessibility
                        sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<input type=\"{htmlDataType}\" id=\"{entityName}-{fc.camelCaseName}\" formControlName=\"{fc.camelCaseName}\" placeholder=\"Enter {fc.titleName}\" class=\"form-control {angularName}-input\" {(addAuthorization ? $"[disabled]=\"!userIs{module}{entityName}Writer()\"" : "")} aria-labelledby=\"label-{fc.camelCaseName}\" />");
                    }
                }

                // Add validation feedback for required fields
                if (fc.field.nullable == false && !fc.field.isBooleanDataType() && fc.camelCaseName != "versionNumber")
                {
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"<div *ngIf=\"{suffixableCamelCaseName}Form.get('{fc.camelCaseName}')?.invalid && {suffixableCamelCaseName}Form.get('{fc.camelCaseName}')?.touched\" class=\"text-danger\">");
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 2) + $"{headerNameToUse} is required");
                    sb.AppendLine(GetTabWhiteSpace(tabCount + 1) + $"</div>");
                }

                sb.AppendLine(GetTabWhiteSpace(tabCount) + "</div>");
            }
        }


        private static string GetTabWhiteSpace(int tabCount)
        {
            StringBuilder output = new StringBuilder();

            int limit = tabCount * SPACES_PER_TAB;
            for (int i = 0; i < limit; i++)
            {
                output.Append(" ");
            }

            return output.ToString();
        }

        protected static string BuildDefaultAngularAddEditComponentSCSSImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable)
        {
            string entityName;

            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = StringUtility.CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);
            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);

            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($".add-{angularName} {{");
            sb.AppendLine("  display: flex;");
            sb.AppendLine("  align-items: center;");
            sb.AppendLine("  justify-content: flex-start;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine($".btn-add-{angularName} {{");
            sb.AppendLine($"  background-color: #0176d3;");
            sb.AppendLine($"  border-color: #0176d3;");
            sb.AppendLine($"  height: 37px;");
            sb.AppendLine($"  color: #fff;");
            sb.AppendLine($"  display: flex;");
            sb.AppendLine($"  align-items: center;");
            sb.AppendLine($"  justify-content: center;");
            sb.AppendLine($"  padding: 0 12px;");
            sb.AppendLine($"  width: 60px;");
            sb.AppendLine("}");

            sb.AppendLine("");
            sb.AppendLine($".{angularName}-modal-header {{");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine($".{angularName}-modal-body {{");
            sb.AppendLine("  padding: 15px;");
            sb.AppendLine("  font-size: 1rem;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".section-heading {");
            sb.AppendLine("  font-weight: bold;");
            sb.AppendLine("  margin-top: 10px;");
            sb.AppendLine("  margin-bottom: 10px;");
            sb.AppendLine("  color: black;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine($".{angularName}-input, .{angularName}-select {{");
            sb.AppendLine("  margin-bottom: 15px;");
            sb.AppendLine("  border-radius: 5px;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine($".{angularName}-input::placeholder, .{angularName}-select::placeholder {{");
            sb.AppendLine("  color: #6c757d;");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine($@"

// Styles for the color picker
.color-picker {{
  // Native color inputs are usually square and small; make it a reasonable size
  width: 60px;
  height: 42px;
  padding: 4px;
  cursor: pointer;
}}

// Style for the read-only hex display next to the picker
.input-color-display {{
  margin-bottom: 0; // Align properly with the picker
}}

");

            sb.AppendLine($".{angularName}-modal-footer {{");
            sb.AppendLine("  display: flex;");
            sb.AppendLine("  justify-content: flex-end;");
            sb.AppendLine("  padding-top: 10px;");
            sb.AppendLine("  padding-bottom: 0px;");
            sb.AppendLine("  border: none;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".text-danger {");
            sb.AppendLine("  color: #dc3545 !important;");
            sb.AppendLine("}");

            return sb.ToString();
        }


        protected static string BuildDefaultAngularAddEditComponentTypeScriptImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;

            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }

            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);

            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);
            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);
            List<Database.Table> distinctTableReferences = GetDistinctTableReferences(scriptGenTable);

            bool haveActiveField = fieldsToDisplay.Any(f => f.camelCaseName == "active");
            bool haveDeletedField = fieldsToDisplay.Any(f => f.camelCaseName == "deleted");


            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateCodeGenDisclaimer(entityName, angularName));


            // Imports
            sb.AppendLine("import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';");
            sb.AppendLine("import { FormBuilder, FormGroup, Validators } from '@angular/forms';");
            sb.AppendLine("import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';");
            sb.AppendLine("import { Router } from '@angular/router';");
            sb.AppendLine("import { Subject, finalize } from 'rxjs';");
            sb.AppendLine("import { AlertService, MessageSeverity } from '../../../services/alert.service';");
            sb.AppendLine($"import {{ {entityName}Service, {entityName}Data, {entityName}SubmitData }} from '../../../{module.ToLower()}-data-services/{angularName}.service';");
            sb.AppendLine("import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';");

            foreach (Database.Table table in distinctTableReferences)
            {
                sb.AppendLine($"import {{ {table.name}Service }} from '../../../{module.ToLower()}-data-services/{StringUtility.ConvertToAngularComponentName(table.name)}.service';");
            }

            if (addAuthorization == true)
            {
                sb.AppendLine("import { AuthService } from '../../../services/auth.service';");
            }

            sb.AppendLine("");

            GenerateFormInterface(entityName, suffixableCamelCaseName, fieldsToDisplay, sb, haveActiveField, haveDeletedField);

            // Component decorator
            sb.AppendLine("@Component({");
            sb.AppendLine($"  selector: 'app-{angularName}-add-edit',");
            sb.AppendLine($"  templateUrl: './{angularName}-add-edit.component.html',");
            sb.AppendLine($"  styleUrls: ['./{angularName}-add-edit.component.scss']");
            sb.AppendLine("})");

            // Class declaration
            sb.AppendLine($"export class {entityName}AddEditComponent {{");
            sb.AppendLine($"  @ViewChild('{suffixableCamelCaseName}Modal') {suffixableCamelCaseName}Modal!: TemplateRef<any>;");
            sb.AppendLine($"  @Output() {suffixableCamelCaseName}Changed = new Subject<{entityName}Data[]>();");
            sb.AppendLine($"  @Input() {camelCaseName}SubmitData: {entityName}SubmitData | null = null;");
            sb.AppendLine($"  @Input() navigateToDetailsAfterAdd: boolean = true;");
            sb.AppendLine($"  @Input() showAddButton: boolean = true;");
            sb.AppendLine("");




            sb.AppendLine($@"
  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<{entityName}FormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];

");


            //
            // Create the form group
            //
            GenerateFormGroup(suffixableCamelCaseName, fieldsToDisplay, sb, haveActiveField, haveDeletedField);

            sb.AppendLine("  private modalRef: NgbModalRef | undefined;");
            sb.AppendLine("  public isEditMode = false;");
            sb.AppendLine("  public objectGuid: string = \"\";");
            sb.AppendLine("  public modalIsDisplayed: boolean = false;");

            sb.AppendLine();
            sb.AppendLine("  public isSaving: boolean = false;");
            sb.AppendLine();

            // Observables for self-referencing and foreign keys
            sb.AppendLine($"  {Pluralize(camelCaseName)}$ = this.{suffixableCamelCaseName}Service.Get{entityName}List();");
            foreach (Database.Table table in distinctTableReferences)
            {
                sb.AppendLine($"  {CamelCase(Pluralize(table.name), false)}$ = this.{CamelCase(table.name, false)}Service.Get{table.name}List();");
            }

            // Constructor
            sb.AppendLine("");
            sb.AppendLine("  constructor(");
            sb.AppendLine($"    private modalService: NgbModal,");
            sb.AppendLine($"    private {suffixableCamelCaseName}Service: {entityName}Service,");
            foreach (Database.Table table in distinctTableReferences)
            {
                sb.AppendLine($"    private {CamelCase(table.name, false)}Service: {table.name}Service,");
            }
            if (addAuthorization == true)
            {
                sb.AppendLine($"    private authService: AuthService,");
            }
            sb.AppendLine("    private alertService: AlertService,");
            sb.AppendLine("    private router: Router,");
            sb.AppendLine("    private fb: FormBuilder) {");
            sb.AppendLine("  }");

            // openModal method
            sb.AppendLine("");
            sb.AppendLine($"  openModal({suffixableCamelCaseName}Data?: {entityName}Data) {{");
            sb.AppendLine();
            sb.AppendLine($"    if ({suffixableCamelCaseName}Data != null) {{");
            if (addAuthorization == true)
            {
                sb.AppendLine();
                sb.AppendLine($"      if (!this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Reader()) {{");
                sb.AppendLine($"        this.alertService.showMessage(");
                sb.AppendLine($"          `${{this.authService.currentUser?.userName}} does not have permission to read {Pluralize(titleName)}`,");
                sb.AppendLine($"          '',");
                sb.AppendLine($"          MessageSeverity.info");
                sb.AppendLine($"        );");
                sb.AppendLine($"        return;");
                sb.AppendLine($"      }}");
            }
            sb.AppendLine($"      this.{camelCaseName}SubmitData = this.{suffixableCamelCaseName}Service.ConvertTo{entityName}SubmitData({suffixableCamelCaseName}Data);");
            sb.AppendLine($"      this.isEditMode = true;");
            if (HasObjectGuidField(type) == true && !camelCaseName.Contains("ChangeHistory"))
            {
                sb.AppendLine($"      this.objectGuid = {camelCaseName}Data.objectGuid;");
            }
            sb.AppendLine();
            sb.AppendLine($"      this.buildFormValues({camelCaseName}Data);");
            sb.AppendLine();
            sb.AppendLine($"    }} else {{");
            if (addAuthorization == true)
            {
                sb.AppendLine();
                sb.AppendLine($"      if (!this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer()) {{");
                sb.AppendLine($"        this.alertService.showMessage(");
                sb.AppendLine($"          `${{this.authService.currentUser?.userName}} does not have permission to write {Pluralize(titleName)}`,");
                sb.AppendLine($"          '',");
                sb.AppendLine($"          MessageSeverity.info");
                sb.AppendLine($"        );");
                sb.AppendLine($"        return;");
                sb.AppendLine();
                sb.AppendLine($"      }}");
            }
            sb.AppendLine();
            sb.AppendLine($"      this.isEditMode = false;");
            sb.AppendLine();
            sb.AppendLine($"      this.buildFormValues(null);");
            sb.AppendLine($@"
      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {{
        this.{camelCaseName}Form.patchValue(this.preSeededData);
      }}
");
            sb.AppendLine($"    }}");
            sb.AppendLine("");
            sb.AppendLine($@"
    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {{
      const fieldName = this.hiddenFields[index];
      const control = this.{camelCaseName}Form.get(fieldName);
      if (control !== null) {{
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }}
    }}
");

            sb.AppendLine($"    this.modalRef = this.modalService.open(this.{suffixableCamelCaseName}Modal, {{");
            sb.AppendLine($"      size: 'xl',");
            sb.AppendLine($"      scrollable: true,");
            sb.AppendLine($"      backdrop: 'static',");
            sb.AppendLine($"      keyboard: true,");
            sb.AppendLine($"      windowClass: 'custom-modal'");
            sb.AppendLine($"    }});");
            sb.AppendLine($"    this.modalIsDisplayed = true;");
            sb.AppendLine($"  }}");

            // closeModal method
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"  closeModal() {{");
            sb.AppendLine($"    if (this.modalRef) {{");
            sb.AppendLine($"      this.modalRef.dismiss('cancel');");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    this.modalIsDisplayed = false;");
            sb.AppendLine($"  }}");

            // submitForm method
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"  submitForm() {{");
            sb.AppendLine();
            sb.AppendLine(@"    if (this.isSaving == true) {
      return;
    }
");

            if (addAuthorization == true)
            {
                sb.AppendLine($"    if (this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer() == false) {{");
                sb.AppendLine($"      this.alertService.showMessage(");
                sb.AppendLine($"        `${{this.authService.currentUser?.userName}} does not have permission to write {Pluralize(titleName)}`,");
                sb.AppendLine($"        '',");
                sb.AppendLine($"        MessageSeverity.info");
                sb.AppendLine($"      );");
                sb.AppendLine($"      return;");
                sb.AppendLine($"    }}");
            }
            sb.AppendLine($@"

    if (!this.{camelCaseName}Form.valid) {{
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.{camelCaseName}Form.markAllAsTouched();
      return;
    }}
");

            sb.AppendLine($"    this.isSaving = true;");
            sb.AppendLine();

            sb.AppendLine($@"    const formValue = this.{camelCaseName}Form.getRawValue();
");

            GenerateCreateSubmitDataCode(entityName, camelCaseName, fieldsToDisplay, sb, true);


            sb.AppendLine();
            sb.AppendLine($"      if (this.isEditMode) {{");
            sb.AppendLine($"        this.update{entityName}({camelCaseName}SubmitData);");
            sb.AppendLine($"      }} else {{");
            sb.AppendLine($"        this.add{entityName}({camelCaseName}SubmitData);");
            sb.AppendLine($"      }}");
            sb.AppendLine($"  }}");

            // add method
            sb.AppendLine("");
            sb.AppendLine($"  private add{entityName}({suffixableCamelCaseName}Data: {entityName}SubmitData) {{");
            bool haveVersionNumberField = scriptGenTable.fields.Any(f => f.name == "versionNumber");

            haveActiveField = scriptGenTable.fields.Any(f => f.name == "active");
            haveDeletedField = scriptGenTable.fields.Any(f => f.name == "deleted");
            if (haveVersionNumberField || haveActiveField || haveDeletedField)
            {
                sb.AppendLine($"    // Assign initial values to non-nullable control fields suitable for adding new data.");
                if (haveVersionNumberField)
                {
                    sb.AppendLine($"    {suffixableCamelCaseName}Data.versionNumber = 0;");
                }
                if (haveActiveField)
                {
                    sb.AppendLine($"    {suffixableCamelCaseName}Data.active = true;");
                }
                if (haveDeletedField)
                {
                    sb.AppendLine($"    {suffixableCamelCaseName}Data.deleted = false;");
                }
            }
            sb.AppendLine($@"    this.{suffixableCamelCaseName}Service.Post{entityName}({suffixableCamelCaseName}Data).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({{");
            sb.AppendLine($"      next: (new{entityName}) => {{");
            sb.AppendLine();
            sb.AppendLine($"        this.{suffixableCamelCaseName}Service.ClearAllCaches();");
            sb.AppendLine();
            sb.AppendLine($"        this.{suffixableCamelCaseName}Changed.next([new{entityName}]);");
            sb.AppendLine();
            sb.AppendLine($"        this.alertService.showMessage(\"{titleName} added successfully\", '', MessageSeverity.success);");
            sb.AppendLine();
            sb.AppendLine($"        this.closeModal();");
            sb.AppendLine();
            sb.AppendLine($"        if (this.navigateToDetailsAfterAdd) {{");
            sb.AppendLine($"          this.router.navigate(['/{entityName.ToLower()}', new{entityName}.id]);");
            sb.AppendLine($"        }}");
            sb.AppendLine($"      }},");
            sb.AppendLine($"      error: (err) => {{");
            GenerateSaveErrorMessage(titleName, sb);
            sb.AppendLine($"      }}");
            sb.AppendLine($"    }});");
            sb.AppendLine($"  }}");

            // update method
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"  private update{entityName}({suffixableCamelCaseName}Data: {entityName}SubmitData) {{");
            sb.AppendLine($@"    this.{suffixableCamelCaseName}Service.Put{entityName}({suffixableCamelCaseName}Data.id, {suffixableCamelCaseName}Data).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({{");
            sb.AppendLine($"      next: (updated{entityName}) => {{");
            sb.AppendLine();
            sb.AppendLine($"        this.{suffixableCamelCaseName}Service.ClearAllCaches();");
            sb.AppendLine();
            sb.AppendLine($"        this.{suffixableCamelCaseName}Changed.next([updated{entityName}]);");
            sb.AppendLine();
            sb.AppendLine($"        this.alertService.showMessage(\"{titleName} updated successfully\", '', MessageSeverity.success);");
            sb.AppendLine();
            sb.AppendLine($"        this.closeModal();");
            sb.AppendLine($"      }},");
            sb.AppendLine($"      error: (err) => {{");
            GenerateSaveErrorMessage(titleName, sb);
            sb.AppendLine($"      }}");
            sb.AppendLine($"    }});");
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();

            //
            // Add the generate build form values function - shared between add/edit and detail components
            //
            GenerateBuildFormValuesFunction(entityName, camelCaseName, fieldsToDisplay, sb, haveActiveField, haveDeletedField);


            sb.AppendLine($@"
  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {{
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {{
      return false;
    }}
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }}
");


            // Authorization methods
            if (addAuthorization == true)
            {
                sb.AppendLine("");
                sb.AppendLine($"  public userIs{module}{entityName}Reader(): boolean {{");
                sb.AppendLine($"    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Reader();");
                sb.AppendLine($"  }}");
                sb.AppendLine("");
                sb.AppendLine($"  public userIs{module}{entityName}Writer(): boolean {{");
                sb.AppendLine($"    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer();");
                sb.AppendLine($"  }}");
            }

            sb.AppendLine($"}}");

            return sb.ToString();
        }


        private static void GenerateFormInterface(string entityName, string suffixableCamelCaseName, List<FieldConfiguration> fieldsToDisplay, StringBuilder sb, bool haveActiveField, bool haveDeletedField)
        {
            sb.AppendLine($@"//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type=""number"" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//");

            sb.AppendLine($"interface {entityName}FormValues {{");

            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                if (fc.field.dataType == DataType.FOREIGN_KEY ||
                    fc.field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER)
                {
                    if (fc.field.nullable == true)
                    {
                        sb.AppendLine($"  {fc.camelCaseName}: number | bigint | null,       // For FK link number");
                    }
                    else
                    {
                        sb.AppendLine($"  {fc.camelCaseName}: number | bigint,       // For FK link number");
                    }
                }
                else
                {
                    if (fc.field.isNumericDataType() == true)
                    {
                        if (fc.field.nullable == true)
                        {
                            sb.AppendLine($"  {fc.camelCaseName}: string | null,     // Stored as string for form input, converted to number on submit.");
                        }
                        else
                        {
                            sb.AppendLine($"  {fc.camelCaseName}: string,     // Stored as string for form input, converted to number on submit.");
                        }

                    }
                    else if (fc.field.isBooleanDataType() == true)
                    {
                        if (fc.field.nullable == true)
                        {
                            sb.AppendLine($"  {fc.camelCaseName}: boolean | null,");
                        }
                        else
                        {
                            sb.AppendLine($"  {fc.camelCaseName}: boolean,");
                        }
                    }
                    else
                    {
                        if (fc.field.nullable == true)
                        {
                            sb.AppendLine($"  {fc.camelCaseName}: string | null,");
                        }
                        else
                        {
                            sb.AppendLine($"  {fc.camelCaseName}: string,");
                        }
                    }
                }
            }

            //
            // If active or deleted aren't in the fields to display, but are on the table, then add them to the form group so we can manage their mapping back for submission (unlikely this will happen..)
            //
            if (haveActiveField || haveDeletedField)
            {
                if (!fieldsToDisplay.Any(f => f.camelCaseName == "active"))
                {
                    sb.AppendLine($"  active: boolean,");
                }
                if (!fieldsToDisplay.Any(f => f.camelCaseName == "deleted"))
                {
                    sb.AppendLine($"  deleted: boolean");
                }
            }
            sb.AppendLine($"}};");
            sb.AppendLine();
        }


        private static void GenerateFormGroup(string suffixableCamelCaseName, List<FieldConfiguration> fieldsToDisplay, StringBuilder sb, bool haveActiveField, bool haveDeletedField)
        {
            sb.AppendLine($"  public {suffixableCamelCaseName}Form: FormGroup = this.fb.group({{");

            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                if (fc.field.dataType == DataType.FOREIGN_KEY ||
                    fc.field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER)
                {
                    string validators = fc.field.nullable == false ? ", Validators.required" : "";
                    sb.AppendLine($"        {fc.camelCaseName}: [null{validators}],");
                }
                else
                {
                    string defaultValue = fc.field.isBooleanDataType() ? (fc.camelCaseName == "active" ? "true" : "false") : "''";
                    string validators = fc.field.nullable == false && !fc.field.isBooleanDataType() && fc.camelCaseName != "versionNumber" ? ", Validators.required" : "";

                    sb.AppendLine($"        {fc.camelCaseName}: [{defaultValue}{validators}],");
                }
            }

            //
            // If active or deleted aren't in the fields to display, but are on the table, then add them to the form group so we can manage their mapping back for submission (unlikely this will happen..)
            //
            if (haveActiveField || haveDeletedField)
            {
                if (!fieldsToDisplay.Any(f => f.camelCaseName == "active"))
                {
                    sb.AppendLine($"        active: [true],");
                }
                if (!fieldsToDisplay.Any(f => f.camelCaseName == "deleted"))
                {
                    sb.AppendLine($"        deleted: [false]");
                }
            }
            sb.AppendLine($"      }});");
            sb.AppendLine();
        }

        private static bool HasObjectGuidField(Type type)
        {
            return type.GetProperty("objectGuid")?.PropertyType == typeof(Guid);
        }


        protected static string BuildDefaultAngularDetailComponentHTMLTemplateImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;


            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);
            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);

            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<div class=\"container-fluid p-1 m-0 {angularName}-details-container\">");
            sb.AppendLine($"    <div class=\"p-2 detail-background\">");
            sb.AppendLine($"        <div class=\"card mb-3 header-card shadow-sm\">");
            sb.AppendLine($"            <div class=\"{angularName}-details-header\">");
            sb.AppendLine($"                <div class=\"header-left\">");
            sb.AppendLine($"                    <button class=\"btn btn-sm btn-light back-button me-2\" (click)=\"goBack()\" ngbTooltip=\"Go Back\"  *ngIf=\"canGoBack()\"><i class=\"fa-solid fa-arrow-left\"></i></button>");
            sb.AppendLine($"                    <div>");
            sb.AppendLine($"                        <h3 class=\"page-title mb-0\">{titleName}</h3>");
            sb.AppendLine($"                        <small class=\"text-muted\">{{{{ isEditMode ? 'Edit {titleName}' : 'Add {titleName}'}}}}</small>");
            sb.AppendLine($"                    </div>");

            sb.AppendLine($"                </div>");
            sb.AppendLine($"            </div>");
            sb.AppendLine($"        </div>");
            sb.AppendLine($"");


            sb.AppendLine($"        <div class=\"card mb-3 header-card shadow-sm\">");

            if (addAuthorization == true)
            {
                sb.AppendLine($"            <div class=\"row flex-grow-1\" *ngIf=\"userIs{module}{entityName}Reader()\">");
            }
            else
            {
                sb.AppendLine($"            <div class=\"row flex-grow-1\">");
            }

            sb.AppendLine($"                <div class=\"col m-2 {angularName}-details-body\">");       // looks like crap when on a non-white background without the m-2.
            sb.AppendLine($"                    <div *showSpinner=\"isLoading$ | async\">");       // Done this way to keep the form in the DOM so that our view child binding will work.
            sb.AppendLine($"                        <form [class.hidden]=\"isLoading$ | async\" [formGroup]=\"{suffixableCamelCaseName}Form\" (ngSubmit)=\"submitForm()\" class=\"{angularName}-details-form\">");


            //
            // Use a common form builder function that is also used by the add/edit component
            //
            GenerateFormInputFields(module, entityName, camelCaseName + "Data", angularName, fieldsToDisplay, sb, false, addAuthorization, 7);

            sb.AppendLine($"                            <div class=\"{angularName}-details-footer\">");
            sb.AppendLine($"                                <button *ngIf=\"isEditMode\" type=\"button\" class=\"btn btm-sm btn-secondary\" [disabled]=\"{suffixableCamelCaseName}Form.pristine\" (click)=\"loadData(true)\" ngbTooltip=\"Reload {titleName}\">Reset</button>");
            sb.AppendLine($"                                <button type=\"submit\" class=\"btn btm-sm btn-primary\" [disabled]=\"isSaving || !{suffixableCamelCaseName}Form.valid || !{suffixableCamelCaseName}Form.dirty || {suffixableCamelCaseName}Form.pristine" + (addAuthorization == true ? $" || userIs{module}{entityName}Writer() == false " : "") + $"\" ngbTooltip=\"Save {titleName}\">{{{{ isEditMode ? 'Save' : 'Create' }}}}</button>");
            sb.AppendLine($"                            </div>");
            sb.AppendLine($"                        </form>");
            sb.AppendLine($"                    </div>");
            sb.AppendLine($"                </div>");
            sb.AppendLine($"            </div>");
            sb.AppendLine($"        </div>");



            //
            //
            // Look for links to this table in the table's database by checking all its tables foreign keys.
            //
            List<(ForeignKey, Table)> relatedTables = new List<(ForeignKey, Table)>();
            foreach (Table table in scriptGenTable.database.tables)
            {
                if (table != scriptGenTable)        // ignore self references
                {
                    foreach (ForeignKey fk in table.foreignKeys)
                    {
                        if (fk.targetTable == scriptGenTable)
                        {
                            // the targetTable we are searching links to the targetTable we are processing.  Add it to the list.
                            relatedTables.Add((fk, table));
                        }
                    }
                }
            }



            //
            // Add in NG bootstrap nav component with all the foreign keys as tables in the nav
            //
            if (relatedTables.Count > 0)
            {
                if (addAuthorization == true)
                {
                    sb.AppendLine($"        <div class=\"card mb-3 header-card shadow-sm\" *ngIf=\"isEditMode && userIs{module}{entityName}Reader() && (isLoading$ | async) == false\">");
                }
                else
                {
                    sb.AppendLine($"        <div class=\"card mb-3 header-card shadow-sm\">");
                }


                sb.AppendLine($"            <div class=\"row flex-grow-1\">");
                sb.AppendLine($"                <div class=\"col m-2 {angularName}-details-body\">");
                sb.AppendLine();
                sb.AppendLine($"                    <ul ngbNav #nav=\"ngbNav\" class=\"nav-tabs\">");


                foreach ((ForeignKey fk, Table table) relatedTableAndFK in relatedTables)
                {
                    string relatedTableName = relatedTableAndFK.table.name;
                    string relatedTableCamelCaseName = CamelCase(relatedTableAndFK.table.name, false);
                    string pluralRelatedName = Pluralize(relatedTableAndFK.table.name);
                    string pluralRelatedNameTitle = ConvertToHeader(Pluralize(relatedTableAndFK.table.name));
                    string fkFieldName = relatedTableAndFK.fk.field.name;

                    string angularNameForRelatedTable = StringUtility.ConvertToAngularComponentName(relatedTableName);


                    //
                    // Make a title for the tab from the FK field name.
                    //
                    string buttonTitle = fkFieldName;           // start with the name of the FK from the related targetTable



                    // pull off its id suffix if it has one
                    if (buttonTitle.EndsWith("Id") == true)
                    {
                        buttonTitle = buttonTitle.Substring(0, fkFieldName.Length - 2);
                    }

                    if (buttonTitle.ToUpper() == entityName.ToUpper())
                    {
                        //
                        // Make the button title come from the name of the related targetTable
                        //
                        buttonTitle = pluralRelatedNameTitle;
                    }
                    else
                    {
                        //
                        // Make the button title come from the FK field on the related targetTable - with the name of the related targetTable
                        //
                        buttonTitle = pluralRelatedNameTitle + " " + Pluralize(ConvertToHeader(buttonTitle));
                    }



                    if (addAuthorization == true)
                    {
                        sb.AppendLine($"                      <li ngbNavItem *ngIf=\"{relatedTableCamelCaseName}Service.userIs{module}{relatedTableName}Reader()\">");
                    }
                    else
                    {
                        sb.AppendLine($"                      <li ngbNavItem>");
                    }

                    sb.AppendLine($"                            <button ngbNavLink>{buttonTitle}");
                    sb.AppendLine($"                                <span class=\"badge bg-primary ms-2\">");


                    string sourceFieldNameWithoutId = relatedTableAndFK.fk.field.name;

                    if (sourceFieldNameWithoutId.EndsWith("Id") == true)
                    {
                        sourceFieldNameWithoutId = sourceFieldNameWithoutId.Substring(0, sourceFieldNameWithoutId.Length - 2);
                    }


                    string primaryNameToUsePascalCase;


                    if (sourceFieldNameWithoutId.Trim().ToUpper() == relatedTableAndFK.fk.targetTable.name.Trim().ToUpper())
                    {
                        primaryNameToUsePascalCase = relatedTableAndFK.fk.sourceTable.name;
                    }
                    else
                    {
                        //
                        // Pascal case name of the source table and the source field name is the outcome.
                        //
                        primaryNameToUsePascalCase = $"{relatedTableName}{CamelCaseToPascalCase(sourceFieldNameWithoutId)}";
                    }


                    sb.AppendLine($"                                    {{{{ ({camelCaseName}Data?.{Pluralize(CamelCaseToPascalCase(primaryNameToUsePascalCase))}Count$ | async) ?? '-' }}}}");

                    sb.AppendLine($"                                </span>");
                    sb.AppendLine($"                            </button>");
                    sb.AppendLine($"                            <ng-template ngbNavContent>");


                    //
                    //
                    //
                    sb.AppendLine($@"                            <div class=""d-flex flex-column gap-3 p-0"">
                                <div class=""flex-grow-1"">
                                    <app-{angularNameForRelatedTable}-table [queryParams]=""{{ {fkFieldName}: {camelCaseName}Data?.id }}""></app-{angularNameForRelatedTable}-table>
                                </div>
                                <div class=""d-flex justify-content-end"">
                                    <app-{angularNameForRelatedTable}-add-edit [preSeededData]=""{{ {fkFieldName}: {camelCaseName}Data?.id }}"" [hiddenFields]=""['{fkFieldName}']""></app-{angularNameForRelatedTable}-add-edit>
                                </div>
                            </div>");


                    sb.AppendLine($"                            </ng-template>");
                    sb.AppendLine($"                        </li>");
                }


                sb.AppendLine($"                    </ul>");
                sb.AppendLine($"                     <div [ngbNavOutlet]=\"nav\"></div>");
                sb.AppendLine($"                </div>");
                sb.AppendLine($"            </div>");
                sb.AppendLine($"        </div>");
            }



            sb.AppendLine($"    </div>");
            sb.AppendLine($"</div>");
            sb.AppendLine($"");

            return sb.ToString();
        }


        protected static string BuildDefaultAngularDetailComponentSCSSImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable)
        {
            string entityName;


            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = StringUtility.CamelCase(entityName, false);
            string pluralName = Pluralize(entityName);
            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);

            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@".header-card {
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.08);

  .card-header {
    background-color: transparent;
    border: none;
    padding: 1rem 1.5rem;
  }

  .header-left {
    display: flex;
    align-items: center;

    .page-title {
      font-size: 1.5rem;
      font-weight: 600;
      margin: 0;
    }

    .text-muted {
      font-size: 0.875rem;
    }
  }
}

// Styles for the color picker
.color-picker {
  // Native color inputs are usually square and small; make it a reasonable size
  width: 60px;
  height: 42px;
  padding: 4px;
  cursor: pointer;
}

// Style for the read-only hex display next to the picker
.input-color-display {
  margin-bottom: 0; // Align properly with the picker
}

");

            sb.AppendLine($"");
            sb.AppendLine($".{angularName}-details-header {{");
            sb.AppendLine($"    background-color: transparent;");
            sb.AppendLine($"    border: none;");
            sb.AppendLine($"    padding: 1rem 1.5rem;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($"/* Full height for container with navbar and footer */");
            sb.AppendLine($".{angularName}-details-container {{");
            sb.AppendLine($"  display: flex;");
            sb.AppendLine($"  flex-direction: column;");
            sb.AppendLine($"  height: 100%; /* Full height relative to viewport */");
            sb.AppendLine($"  overflow: hidden; /* Prevent internal scrolling and make outer container scrollable */");
            sb.AppendLine($"  padding-top: 3px;");
            sb.AppendLine($"  padding-left: 10px;");
            sb.AppendLine($"  padding-right: 10px;");
            sb.AppendLine($"  padding-bottom: 3px;");
            sb.AppendLine($"  background-color: #f4f6f9;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($"/* Body section that allows for scrolling */");
            sb.AppendLine($".{angularName}-details-body {{");
            sb.AppendLine($"  flex-grow: 1; /* Takes up remaining space in the viewport */");
            sb.AppendLine($"  overflow-y: auto; /* Enables vertical scrolling if needed */");
            sb.AppendLine($"  box-sizing: border-box; /* Ensure padding doesn't cause overflow */");
            sb.AppendLine($"  overflow-x: hidden;   /* hide any potential x overflow.  There should be none, but we want to make sure x scrollbars don't appear*/");
            sb.AppendLine($"  padding: 1rem 1.5rem;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($"/* Form styling */");
            sb.AppendLine($".{angularName}-details-form {{");
            sb.AppendLine($"  width: 100%; /* Ensure the form doesn't exceed the container width */");
            sb.AppendLine($"  max-width: 100%; /* Prevent unintended overflow */");
            sb.AppendLine($"  box-sizing: border-box; /* Ensure consistent sizing with padding and borders */");
            sb.AppendLine($"  padding: 5px;");
            sb.AppendLine($"  font-size: 1rem;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($"/* Footer stays at the bottom of the form but will only be visible when the page is scrolled */");
            sb.AppendLine($".{angularName}-details-footer {{");
            sb.AppendLine($"  display: flex;");
            sb.AppendLine($"  justify-content: flex-end;");
            sb.AppendLine($"  gap: 5px;");
            sb.AppendLine($"  border: none;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($".{angularName}-input, .{angularName}-select {{");
            sb.AppendLine($"  margin-bottom: 10px;");
            sb.AppendLine($"  border-radius: 5px;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($".{angularName}-input::placeholder, .{angularName}-select::placeholder {{");
            sb.AppendLine($"  color: #6c757d;");
            sb.AppendLine($"}}");
            sb.AppendLine($"");
            sb.AppendLine($".text-danger {{");
            sb.AppendLine($"  color: #dc3545 !important;");
            sb.AppendLine($"}}");
            sb.AppendLine();
            sb.AppendLine(".detail-background {");
            sb.AppendLine("  border-radius: 5px;");
            sb.AppendLine("  padding: 1rem;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".back-button {");
            sb.AppendLine("  margin-right: 3px;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".hidden {");
            sb.AppendLine("  display: none;");
            sb.AppendLine("}");

            return sb.ToString();
        }


        protected static string BuildDefaultAngularDetailComponentTypeScriptImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization)
        {
            string entityName;


            if (type.Name.EndsWith("Datum") == false)
            {
                entityName = type.Name;
            }
            else
            {
                entityName = type.Name.Replace("Datum", "Data");
            }


            string camelCaseName = CamelCase(entityName, false);
            string suffixableCamelCaseName = CamelCase(entityName, false);

            string pluralName = Pluralize(entityName);

            string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);
            string angularName = StringUtility.ConvertToAngularComponentName(entityName);

            List<FieldConfiguration> fieldsToDisplay = GetFieldsToDisplay(type, scriptGenTable);
            List<Database.Table> distinctTableReferences = GetDistinctTableReferences(scriptGenTable, true);

            bool haveActiveField = fieldsToDisplay.Any(f => f.camelCaseName == "active");
            bool haveDeletedField = fieldsToDisplay.Any(f => f.camelCaseName == "deleted");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GenerateCodeGenDisclaimer(entityName, angularName));

            sb.AppendLine("import { Component, OnInit, Input, Output } from '@angular/core';");
            sb.AppendLine("import { FormBuilder, FormGroup, Validators } from '@angular/forms';");
            sb.AppendLine("import { ActivatedRoute, Router } from '@angular/router';");
            sb.AppendLine("import { NavigationService } from '../../../utility-services/navigation.service';");
            sb.AppendLine("import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';");
            sb.AppendLine("import { AlertService, MessageSeverity } from '../../../services/alert.service';");
            sb.AppendLine($"import {{ {entityName}Service, {entityName}Data, {entityName}SubmitData }} from '../../../{module.ToLower()}-data-services/{angularName}.service';");

            // Don't need to reference the SubmitData class for these imports because we're only reading from them here.
            foreach (Database.Table table in distinctTableReferences)
            {
                sb.AppendLine($"import {{ {table.name}Service }} from '../../../{module.ToLower()}-data-services/{StringUtility.ConvertToAngularComponentName(table.name)}.service';");
            }

            if (addAuthorization == true)
            {
                sb.AppendLine("import { AuthService } from '../../../services/auth.service';");
            }
            sb.AppendLine("import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';");
            sb.AppendLine("import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';");


            GenerateFormInterface(entityName, suffixableCamelCaseName, fieldsToDisplay, sb, haveActiveField, haveDeletedField);

            sb.AppendLine("");
            sb.AppendLine("@Component({");
            sb.AppendLine($"  selector: 'app-{angularName}-detail',");
            sb.AppendLine($"  templateUrl: './{angularName}-detail.component.html',");
            sb.AppendLine($"  styleUrls: ['./{angularName}-detail.component.scss']");
            sb.AppendLine("})");
            sb.AppendLine("");
            sb.AppendLine($"export class {entityName}DetailComponent implements OnInit, CanComponentDeactivate {{");
            sb.AppendLine();


            sb.AppendLine($@"
  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<{entityName}FormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];

");

            //
            // Create the form 
            //
            GenerateFormGroup(suffixableCamelCaseName, fieldsToDisplay, sb, haveActiveField, haveDeletedField);

            sb.AppendLine();
            sb.AppendLine($"  public {suffixableCamelCaseName}Id: string | null = null;");
            sb.AppendLine($"  public {camelCaseName}Data: {entityName}Data | null = null;");
            sb.AppendLine();
            sb.AppendLine("  private isLoadingSubject = new BehaviorSubject<boolean>(true);");
            sb.AppendLine("  public isLoading$ = this.isLoadingSubject.asObservable();");
            sb.AppendLine();
            sb.AppendLine("  public isSaving = false;");
            sb.AppendLine();
            sb.AppendLine("  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'");
            sb.AppendLine();

            // Add an observable for self type, for tables that have self referencing foreign keys
            sb.AppendLine($"  {Pluralize(camelCaseName)}$ = this.{suffixableCamelCaseName}Service.Get{entityName}List();");

            // Add an observable for regular foreign keys
            foreach (Database.Table table in distinctTableReferences)
            {
                sb.AppendLine($"  public {CamelCase(Pluralize(table.name), false)}$ = this.{CamelCase(table.name, false)}Service.Get{table.name}List();");
            }
            sb.AppendLine();
            sb.AppendLine("  private destroy$ = new Subject<void>();");
            sb.AppendLine();

            //
            // Create the constructor
            //
            sb.AppendLine("  constructor(");
            sb.AppendLine($"    public {suffixableCamelCaseName}Service: {entityName}Service,");
            foreach (Database.Table table in distinctTableReferences)
            {
                // Add a service for each distinct table reference - make them public so we can use them in the template for think like security level checks.
                sb.AppendLine($"    public {CamelCase(table.name, false)}Service: {table.name}Service,");

            }

            if (addAuthorization == true)
            {
                sb.AppendLine($"    private authService: AuthService,");
            }

            sb.AppendLine("    private route: ActivatedRoute,");
            sb.AppendLine("    private router: Router,");
            sb.AppendLine("    private fb: FormBuilder,");
            sb.AppendLine("    private alertService: AlertService,");
            sb.AppendLine("    private navigationService: NavigationService) { ");
            sb.AppendLine();
            sb.AppendLine("    }");

            sb.AppendLine();


            //
            // Lifecycle management methods
            //
            sb.AppendLine($"  ngOnInit(): void {{");
            sb.AppendLine($"");
            sb.AppendLine($"    // Get the {suffixableCamelCaseName}Id from the route parameters");
            sb.AppendLine($"    this.{suffixableCamelCaseName}Id = this.route.snapshot.paramMap.get('{suffixableCamelCaseName}Id');");
            sb.AppendLine();

            sb.AppendLine($@"    if (this.{suffixableCamelCaseName}Id === 'new' ||
        this.{suffixableCamelCaseName}Id == null) {{
      //
      // Add mode
      //
      this.isEditMode = false;
      this.{suffixableCamelCaseName}Data = null;

      this.buildFormValues(null);");


            sb.AppendLine($@"
      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {{
        this.{camelCaseName}Form.patchValue(this.preSeededData);
      }}
");


            sb.AppendLine($@"
    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {{
      const fieldName = this.hiddenFields[index];
      const control = this.{camelCaseName}Form.get(fieldName);
      if (control !== null) {{
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }}
    }}
");

            sb.AppendLine($@"
      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New {titleName}';

    }} else {{

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit {titleName}';

      // Load the data from the server
      this.loadData(false);
    }}
");

            sb.AppendLine($"  }}");
            sb.AppendLine();

            sb.AppendLine(@"
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }");

            // 
            // Deactivation check
            //
            sb.AppendLine($"");
            sb.AppendLine($"");
            sb.AppendLine($"  public canDeactivate(): boolean {{");
            sb.AppendLine($"    if (this.{suffixableCamelCaseName}Form.dirty) {{");
            sb.AppendLine($"      return confirm('You have unsaved {titleName} changes. Are you sure you want to leave this page?');");
            sb.AppendLine($"    }}");
            sb.AppendLine($"    return true;");
            sb.AppendLine($"  }}");
            sb.AppendLine($"");
            sb.AppendLine($"");


            //
            // Query parameters for children tables
            //
            sb.AppendLine($@" public GetQueryParameters(): any {{

    if (this.{suffixableCamelCaseName}Id != null && this.{suffixableCamelCaseName}Id !== 'new') {{

      const id = parseInt(this.{suffixableCamelCaseName}Id, 10);

      if (!isNaN(id)) {{
        return {{ {suffixableCamelCaseName}Id: id }};
      }}
    }}

    return null;
  }}
");


            //
            // Data loading
            //

            sb.AppendLine($@"
/*
  * Loads the {entityName} data for the current {camelCaseName}Id.
  *
  * Fully respects the {entityName}Service caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {{

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);
");


            if (addAuthorization == true)
            {
                sb.AppendLine($@"
    //
    // Permission Check
    //
    if (!this.{camelCaseName}Service.userIs{module}{entityName}Reader()) {{

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${{userName}} does not have permission to read {pluralName}.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }}");
            }


            sb.AppendLine($@"
    //
    // Validate {camelCaseName}Id
    //
    if (!this.{camelCaseName}Id) {{

      this.alertService.showMessage('No {entityName} ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }}

    const {camelCaseName}Id = Number(this.{camelCaseName}Id);

    if (isNaN({camelCaseName}Id) || {camelCaseName}Id <= 0) {{

      this.alertService.showMessage(`Invalid {titleName} ID: ""${{this.{camelCaseName}Id}}""`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }}

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {{
      // This is the most targeted way: clear only this {entityName} + relations

      this.{camelCaseName}Service.ClearRecordCache({camelCaseName}Id, true);
    }}

    //
    // Subscribe with full next/error handling
    //
    this.{camelCaseName}Service.Get{entityName}({camelCaseName}Id, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({{

      next: ({camelCaseName}Data) => {{

        //
        // Success path — {camelCaseName}Data can legitimately be null if 404'd but request succeeded
        //
        if (!{camelCaseName}Data) {{

          this.handle{entityName}NotFound({camelCaseName}Id);

        }} else {{

          this.{camelCaseName}Data = {camelCaseName}Data;
          this.buildFormValues(this.{camelCaseName}Data);

          if (forceLoadAndDisplaySuccessAlert === true) {{
            this.alertService.showMessage(
              '{entityName} loaded successfully',
              '',
              MessageSeverity.success
            );
          }}
        }}

        this.isLoadingSubject.next(false);
      }},

      error: (error: any) => {{
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handle{entityName}LoadError(error, {camelCaseName}Id);
        this.isLoadingSubject.next(false);
      }}
    }});
  }}


  private handle{entityName}NotFound({camelCaseName}Id: number): void {{

    this.{camelCaseName}Data = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `{entityName} #${{{camelCaseName}Id}} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }}


  private handle{entityName}LoadError(error: any, {camelCaseName}Id: number): void {{

    let message = 'Failed to load {titleName}.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {{
      switch (error.status) {{
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this {titleName}.';
          title = 'Forbidden';
          break;
        case 404:
          message = `{titleName} #${{{camelCaseName}Id}} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${{error.status || 'unknown'}}: ${{error.statusText || 'Request failed'}}`;
      }}
    }} else {{
      message = error?.message || message;
    }}

    console.error(`{titleName} load failed (ID: ${{{camelCaseName}Id}})`, error);

    //
    // Reset UI to safe state
    //
    this.{camelCaseName}Data = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }}
");

            //
            // Add the generate build form values function - shared between add/edit and detail components
            //
            GenerateBuildFormValuesFunction(entityName, camelCaseName, fieldsToDisplay, sb, haveActiveField, haveDeletedField);

            //
            // Navigation related methods
            //
            sb.AppendLine();
            sb.AppendLine($"  public goBack(): void {{");
            sb.AppendLine($"    this.navigationService.goBack();");
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"  public canGoBack(): boolean {{");
            sb.AppendLine($"    return this.navigationService.canGoBack();");
            sb.AppendLine($"  }}");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine($@"  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {{
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {{
      return false;
    }}
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }}

");


            //
            // submitForm method
            //
            sb.AppendLine($"  public submitForm() {{");
            sb.AppendLine($"");

            sb.AppendLine($@"    if (this.isSaving == true) {{
      return;
    }}
");

            if (addAuthorization == true)
            {
                sb.AppendLine($@"    if (this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer() == false) {{
      this.alertService.showMessage(this.authService.currentUser?.userName + "" does not have the permission to write to {Pluralize(titleName)}"", 'Access Denied', MessageSeverity.info);
      return;
    }}");
            }

            sb.AppendLine($@"
    if (!this.{camelCaseName}Form.valid) {{
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.{camelCaseName}Form.markAllAsTouched();
      return;
    }}

    this.isSaving = true;

    const formValue = this.{camelCaseName}Form.getRawValue();
");

            //
            // Create the submit data object - shared by details and add/edit components
            //
            GenerateCreateSubmitDataCode(entityName, camelCaseName, fieldsToDisplay, sb, false);

            sb.AppendLine($@"

    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.{camelCaseName}Service.Put{entityName}({camelCaseName}SubmitData.id, {camelCaseName}SubmitData)
      : this.{camelCaseName}Service.Post{entityName}({camelCaseName}SubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({{
      next: (saved{entityName}Data) => {{

        this.{camelCaseName}Service.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {{
          //
          // Navigate to the newly created {titleName}'s detail page
          //
          this.{camelCaseName}Form.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.{camelCaseName}Form.markAsUntouched();

          this.router.navigate(['/{pluralName.ToLower()}', saved{entityName}Data.id]);
          this.alertService.showMessage('{titleName} added successfully', '', MessageSeverity.success);
        }} else {{

          //
          // Rebuild the form with the new data
          //
          this.{camelCaseName}Data = saved{entityName}Data;
          this.buildFormValues(this.{camelCaseName}Data);

          this.alertService.showMessage(""{titleName} saved successfully"", '', MessageSeverity.success);
        }}
      }},
      error: (err) => {{

            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {{
                errorMessage = err.message || 'An unexpected error occurred.';
            }}
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {{
                if (err.status === 403)
                {{
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this {titleName}.';
                }}
                else
                {{
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the {titleName}.';
                }}
            }}
            // Fallback for unexpected error formats
            else {{
                errorMessage = 'An unexpected error occurred.';
            }}

            this.alertService.showMessage('{titleName} could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }}
    }});
  }}");

            if (addAuthorization == true)
            {
                sb.AppendLine();
                sb.AppendLine($@"  public userIs{module}{entityName}Reader(): boolean {{
    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Reader();
  }}

  public userIs{module}{entityName}Writer(): boolean {{
    return this.{suffixableCamelCaseName}Service.userIs{module}{entityName}Writer();
  }}");
            }

            sb.AppendLine($"}}");

            return sb.ToString();
        }

        private static void GenerateCreateSubmitDataCode(string entityName, string camelCaseName, List<FieldConfiguration> fieldsToDisplay, StringBuilder sb, bool useSubmitSuffixOnDataObject)
        {
            sb.AppendLine($@"

    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const {camelCaseName}SubmitData: {entityName}SubmitData = {{
        id: this.{camelCaseName}{(useSubmitSuffixOnDataObject == true ? "Submit" : "")}Data?.id || 0,");

            //
            // This properly configures the DTO for each data type, and respects both nullable and non-nullable fields.
            //
            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                if (fc.field.isBooleanDataType() == true)
                {
                    // Convert back to boolean data type, handling nulls.
                    if (fc.field.nullable == true)
                    {
                        sb.AppendLine($"        {fc.camelCaseName}: formValue.{fc.camelCaseName} == true ? true : formValue.{fc.camelCaseName} == false ? false : null,");
                    }
                    else
                    {
                        sb.AppendLine($"        {fc.camelCaseName}: !!formValue.{fc.camelCaseName},");
                    }
                }
                else if (fc.field.isDateTimeDataType() == true)
                {
                    //
                    // Convert datetimes from datetime-local format to ISO 8601,  handling nulls.
                    //
                    if (fc.field.nullable == true)
                    {
                        sb.AppendLine($"        {fc.camelCaseName}: formValue.{fc.camelCaseName} ? dateTimeLocalToIsoUtc(formValue.{fc.camelCaseName}.trim()) : null,");
                    }
                    else
                    {
                        sb.AppendLine($"        {fc.camelCaseName}: dateTimeLocalToIsoUtc(formValue.{fc.camelCaseName}!.trim())!,");        // validator makes sure that this has a value.
                    }
                }
                else if (fc.field.isNumericDataType() == true ||
                         fc.field.dataType == DataType.FOREIGN_KEY ||
                         fc.field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER)
                {
                    if (fc.field.name == "versionNumber")
                    {
                        sb.AppendLine($"        versionNumber: this.{camelCaseName}{(useSubmitSuffixOnDataObject == true ? "Submit" : "")}Data?.versionNumber ?? 0,");       // always use the version number from the source data object.  (UI guards against changes, but just in case).
                    }
                    else
                    {
                        if (fc.field.nullable == true)
                        {
                            sb.AppendLine($"        {fc.camelCaseName}: formValue.{fc.camelCaseName} ? Number(formValue.{fc.camelCaseName}) : null,");
                        }
                        else
                        {
                            sb.AppendLine($"        {fc.camelCaseName}: Number(formValue.{fc.camelCaseName}),"); // Expect a string that can convert to a number here
                        }
                    }
                }
                else
                {
                    // everything else becomes a string
                    if (fc.field.nullable == true)
                    {
                        sb.AppendLine($"        {fc.camelCaseName}: formValue.{fc.camelCaseName}?.trim() || null,");
                    }
                    else
                    {
                        sb.AppendLine($"        {fc.camelCaseName}: formValue.{fc.camelCaseName}!.trim(),");
                    }
                }
            }

            sb.AppendLine($@"   }};");
        }

        private static void GenerateBuildFormValuesFunction(string entityName, string camelCaseName, List<FieldConfiguration> fieldsToDisplay, StringBuilder sb, bool haveActiveField, bool haveDeletedField)
        {
            sb.AppendLine($@"
  private buildFormValues({camelCaseName}Data: {entityName}Data | null) {{

    if ({camelCaseName}Data == null) {{
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.{camelCaseName}Form.reset({{");

            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                if (fc.field.isBooleanDataType() == true)
                {
                    // make active fields default to true, all other bools go to false.
                    sb.AppendLine($"        {fc.camelCaseName}: {((fc.camelCaseName == "active" ? "true" : "false"))},");
                }
                else if (fc.field.isDateTimeDataType() == true)
                {
                    // DateTimes init as strings for datetime-local inputs.  This Just here to note that.
                    sb.AppendLine($"        {fc.camelCaseName}: '',");
                }
                else if (fc.field.dataType == DataType.FOREIGN_KEY ||
                         fc.field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER)
                {
                    // Foreign key fields init to null
                    sb.AppendLine($"        {fc.camelCaseName}: null,");
                }
                else
                {
                    // everything else becomes a string
                    sb.AppendLine($"        {fc.camelCaseName}: '',");
                }
            }

            //
            // If active or deleted aren't on the fields to display, put them on now
            //
            if (haveActiveField || haveDeletedField)
            {
                if (!fieldsToDisplay.Any(f => f.camelCaseName == "active"))
                {
                    sb.AppendLine($"        active: true,");
                }
                if (!fieldsToDisplay.Any(f => f.camelCaseName == "deleted"))
                {
                    sb.AppendLine($"        deleted: false,");
                }
            }


            sb.AppendLine($@"   }}, {{ emitEvent: false}});

    }}
    else {{

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.{camelCaseName}Form.reset({{");

            foreach (FieldConfiguration fc in fieldsToDisplay)
            {
                if (fc.field.isBooleanDataType() == true)
                {
                    // make active fields default to true, all other bools go to false.
                    sb.AppendLine($"        {fc.camelCaseName}: {camelCaseName}Data.{fc.camelCaseName} ?? {((fc.camelCaseName == "active" ? "true" : "false"))},");
                }
                else if (fc.field.isDateTimeDataType() == true)
                {
                    // Convert DateTimes from ISO 8601 on the data object to datatime-local format
                    sb.AppendLine($"        {fc.camelCaseName}: isoUtcStringToDateTimeLocal({camelCaseName}Data.{fc.camelCaseName}) ?? '',");
                }
                else if (fc.field.isNumericDataType() == true)
                {
                    // Convert numbers to strings
                    sb.AppendLine($"        {fc.camelCaseName}: {camelCaseName}Data.{fc.camelCaseName}?.toString() ?? '',");
                }
                else if (fc.field.dataType == DataType.FOREIGN_KEY ||
                         fc.field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER)
                {
                    // Keep foreign key number values as numbers or null
                    sb.AppendLine($"        {fc.camelCaseName}: {camelCaseName}Data.{fc.camelCaseName},");
                }
                else
                {
                    // everything else becomes a string
                    sb.AppendLine($"        {fc.camelCaseName}: {camelCaseName}Data.{fc.camelCaseName} ?? '',");
                }
            }

            sb.AppendLine($@"      }}, {{ emitEvent: false}});
    }}

    this.{camelCaseName}Form.markAsPristine();
    this.{camelCaseName}Form.markAsUntouched();
  }}");
        }


        private static void GenerateSaveErrorMessage(string titleName, StringBuilder sb)
        {
            sb.AppendLine($"            let errorMessage: string;");
            sb.AppendLine();
            sb.AppendLine($"            // Check if err is an Error object (e.g., new Error('message'))");
            sb.AppendLine($"            if (err instanceof Error) {{");
            sb.AppendLine($"                errorMessage = err.message || 'An unexpected error occurred.';");
            sb.AppendLine($"            }}");
            sb.AppendLine($"            // Check if err is a ServerError object with status and error properties");
            sb.AppendLine($"            else if (err.status && err.error)");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                if (err.status === 403)");
            sb.AppendLine($"                {{");
            sb.AppendLine($"                    errorMessage = err.error?.message ||");
            sb.AppendLine($"                                   'You do not have permission to save this {titleName}.';");
            sb.AppendLine($"                }}");
            sb.AppendLine($"                else");
            sb.AppendLine($"                {{");
            sb.AppendLine($"                    errorMessage = err.error?.message ||");
            sb.AppendLine($"                                   err.error?.error_description ||");
            sb.AppendLine($"                                   err.error?.detail ||");
            sb.AppendLine($"                                   'An error occurred while saving the {titleName}.';");
            sb.AppendLine($"                }}");
            sb.AppendLine($"            }}");
            sb.AppendLine($"            // Fallback for unexpected error formats");
            sb.AppendLine($"            else {{");
            sb.AppendLine($"                errorMessage = 'An unexpected error occurred.';");
            sb.AppendLine($"            }}");
            sb.AppendLine();
            sb.AppendLine($"            this.alertService.showMessage('{titleName} could not be saved',");
            sb.AppendLine($"                                          errorMessage,");
            sb.AppendLine($"                                          MessageSeverity.error);");
        }

        private static string GetLinkFieldName(FieldConfiguration fc)
        {
            string data = fc.camelCaseName.Substring(0, fc.camelCaseName.Length - 2);

            // Add an underscore to the front of reserved words, to match what the EF context generator does.
            if (IsReservedWord(data) == true)
            {
                data = $"_{data}";
            }

            return data;
        }

        private static string GetLinkFieldName(Database.Table.ForeignKey fkField)
        {
            string data = fkField.field.name.Substring(0, fkField.field.name.Length - 2);

            // Add an underscore to the front of reserved words, to match what the EF context generator does.
            if (IsReservedWord(data) == true)
            {
                data = $"_{data}";
            }

            return data;
        }


        private static Database.Table.ForeignKey GetMostSensibleForeignFieldForSmallText(Database.Table scriptGenTable)
        {
            Database.Table.ForeignKey output = null;

            //
            // First see if there is an fk with the name of table name + Type.  
            //
            foreach (var fk in scriptGenTable.foreignKeys)
            {
                if (fk.field.name == CamelCase(scriptGenTable.name, false) + "TypeId")
                {
                    return fk;
                }
            }


            //
            // First see if there is an fk with the name of table name + Status.  
            //
            foreach (var fk in scriptGenTable.foreignKeys)
            {
                if (fk.field.name == CamelCase(scriptGenTable.name, false) + "StatusId")
                {
                    return fk;
                }
            }

            //
            // Just return the first.
            //
            if (scriptGenTable.foreignKeys.Count > 0)
            {
                return scriptGenTable.foreignKeys[0];
            }

            return output;
        }


        private static List<FieldConfiguration> GetFieldsToDisplay(Type type, DatabaseGenerator.Database.Table scriptGenTable)
        {
            List<FieldConfiguration> output = new List<FieldConfiguration>();

            foreach (PropertyInfo prop in type.GetProperties())
            {
                // jump over the id field on the list getter function.  It adds no value as a parameter here and will likely class with the individual getter function, which only has ID as a param below.
                if (prop.Name == "id")
                {
                    continue;
                }

                //
                // Don't create a password column
                //
                if (prop.Name == "password")
                {
                    continue;
                }

                //
                // Don't include tenant guid or object guid columns
                //
                if (prop.Name == "tenantGuid" || prop.Name == "objectGuid")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                Database.Table.Field field = scriptGenTable.GetFieldByName(prop.Name);

                if (field != null)
                {
                    FieldConfiguration fc = new FieldConfiguration();

                    fc.field = field;

                    fc.titleName = StringUtility.ConvertToHeader(field.name);
                    fc.camelCaseName = field.name;

                    if (field.dataType == DataType.FOREIGN_KEY ||
                        field.dataType == DataType.FOREIGN_KEY_GUID ||
                        field.dataType == DataType.FOREIGN_KEY_STRING ||
                        field.dataType == DataType.FOREIGN_KEY_BIG_INTEGER)
                    {

                        foreach (var fk in scriptGenTable.foreignKeys)
                        {
                            if (fk.field.name == field.name)
                            {
                                fc.linkTable = fk.targetTable;
                                break;
                            }
                        }
                    }
                    else
                    {
                        fc.linkTable = null;
                    }

                    output.Add(fc);
                }
            }

            return output;
        }


        private static List<Database.Table> GetDistinctTableReferences(Database.Table scriptGenTable, bool includeTablesThatLinkToThisOne = false)
        {
            List<Database.Table> output = new List<Database.Table>();

            foreach (var fk in scriptGenTable.foreignKeys)
            {
                if (output.Contains(fk.targetTable) == false &&
                    fk.targetTable.name != scriptGenTable.name)
                {
                    output.Add(fk.targetTable);
                }
            }

            if (includeTablesThatLinkToThisOne == true)
            {
                //
                //
                // Look for links to this targetTable in the targetTable's database by checking all its tables foreign keys.
                //
                foreach (Table table in scriptGenTable.database.tables)
                {
                    if (table != scriptGenTable)
                    {
                        foreach (ForeignKey fk in table.foreignKeys)
                        {
                            if (fk.targetTable == scriptGenTable)
                            {
                                // the targetTable we are searching links to the targetTable we are processing.  Add it to the list.

                                if (output.Contains(table) == false)
                                {
                                    output.Add(table);
                                }
                            }
                        }
                    }
                }
            }


            return output;
        }

        public static void BuildAngularComponentImplementationFromEntityFrameworkContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp", bool addAuthorization = true)
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            System.IO.Directory.CreateDirectory(filePath + moduleName);
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\angular");
            System.IO.Directory.CreateDirectory(filePath + moduleName + $"\\angular\\{moduleName.ToLower()}-data-components");



            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;



                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;

                    Type type = propertyType.GenericTypeArguments[0];

                    // Get the table spec from the script generator
                    DatabaseGenerator.Database.Table scriptGenTable = null;
                    foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                    {
                        if (tbl.name == type.Name)
                        {
                            scriptGenTable = tbl;
                            break;
                        }
                    }

                    //
                    // If table name ends with 'Statu' or 'Campu' then look again with an s on the end.
                    //
                    if (scriptGenTable == null && (type.Name.EndsWith("Statu") || type.Name.EndsWith("Campu")))
                    {
                        var realName = type.Name + "s";

                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            if (tbl.name == realName)
                            {
                                scriptGenTable = tbl;
                                break;
                            }
                        }
                    }

                    //
                    // Datum is the plural for Date in the new EFCorePowerTools pluralizer.
                    //
                    if (scriptGenTable == null && type.Name.EndsWith("Datum"))
                    {
                        var realName = type.Name.Replace("Datum", "Data");

                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            if (tbl.name == realName)
                            {
                                scriptGenTable = tbl;
                                break;
                            }
                        }
                    }

                    //
                    // Generate the Angular components to interact with the data services
                    //
                    if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                    {
                        string angularName = StringUtility.ConvertToAngularComponentName(scriptGenTable.name);

                        string entityDirectory = filePath + moduleName + $"\\angular\\{moduleName.ToLower()}-data-components\\" + angularName;

                        System.IO.Directory.CreateDirectory(entityDirectory);


                        #region Listing component
                        //
                        // Create the folder for the list component files
                        //
                        string listComponentFolder = entityDirectory + "\\" + angularName + "-listing";

                        System.IO.Directory.CreateDirectory(listComponentFolder);

                        //
                        // Create the default list component HTML template code
                        //
                        string listingHTMLTemplateCode = BuildDefaultAngularListingComponentHTMLTemplateImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the listing component HTML template code
                        //
                        // This complains with paths over 256 characters...
                        System.IO.File.WriteAllText("\\\\?\\" + listComponentFolder + "\\" + angularName + "-listing.component.html", listingHTMLTemplateCode);



                        //
                        // Create the default list component SCSS code
                        //
                        string listingSCSSCode = BuildDefaultAngularListingComponentSCSSImplementation(moduleName, type, scriptGenTable);

                        //
                        // Write out the listing SCSS
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + listComponentFolder + "\\" + angularName + "-listing.component.scss", listingSCSSCode);


                        //
                        // Create the default list component typescript template code
                        //
                        string listingTypeScriptCode = BuildDefaultAngularListingComponentTypeScriptImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the TypeScript
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + listComponentFolder + "\\" + angularName + "-listing.component.ts", listingTypeScriptCode);


                        #endregion


                        #region Add/Edit Component

                        //
                        // Now create the Add/Edit component
                        //
                        string addEditComponentFolder = entityDirectory + "\\" + angularName + "-add-edit";

                        System.IO.Directory.CreateDirectory(addEditComponentFolder);

                        //
                        // Create the default Add/Edit component HTML template code
                        //
                        string addEditHTMLTemplateCode = BuildDefaultAngularAddEditComponentHTMLTemplateImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the listing component HTML template code
                        //
                        // Prefix is to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + addEditComponentFolder + "\\" + angularName + "-add-edit.component.html", addEditHTMLTemplateCode);



                        //
                        // Create the default Add/Edit component SCSS code
                        //
                        string addEditSCSSCode = BuildDefaultAngularAddEditComponentSCSSImplementation(moduleName, type, scriptGenTable);

                        //
                        // Write out the Add/Edit component SCSS
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + addEditComponentFolder + "\\" + angularName + "-add-edit.component.scss", addEditSCSSCode);


                        //
                        // Create the default Add/Edit component typescript template code
                        //
                        string addEditTypeScriptCode = BuildDefaultAngularAddEditComponentTypeScriptImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the TypeScript
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + addEditComponentFolder + "\\" + angularName + "-add-edit.component.ts", addEditTypeScriptCode);

                        #endregion


                        #region Detail Component

                        //
                        // Now create the Detail component
                        //
                        string detailComponentFolder = entityDirectory + "\\" + angularName + "-detail";

                        System.IO.Directory.CreateDirectory(detailComponentFolder);

                        //
                        // Create the default detail component HTML template code
                        //
                        string detailHTMLTemplateCode = BuildDefaultAngularDetailComponentHTMLTemplateImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the listing component HTML template code
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + detailComponentFolder + "\\" + angularName + "-detail.component.html", detailHTMLTemplateCode);


                        //
                        // Create the default detail component SCSS code
                        //
                        string detailSCSSCode = BuildDefaultAngularDetailComponentSCSSImplementation(moduleName, type, scriptGenTable);

                        //
                        // Write out the detail component SCSS
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + detailComponentFolder + "\\" + angularName + "-detail.component.scss", detailSCSSCode);


                        //
                        // Create the default detail component typescript template code
                        //
                        string detailTypeScriptCode = BuildDefaultAngularDetailComponentTypeScriptImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the TypeScript
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + detailComponentFolder + "\\" + angularName + "-detail.component.ts", detailTypeScriptCode);


                        #endregion


                        #region Table component
                        //
                        // Create the folder for the table component files
                        //

                        string tableComponentFolder = entityDirectory + "\\" + angularName + "-table";

                        System.IO.Directory.CreateDirectory(tableComponentFolder);

                        //
                        // Create the default table component HTML template code
                        //
                        string tableHTMLTemplateCode = BuildDefaultAngularTableComponentHTMLTemplateImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the table component HTML template code
                        //
                        // This complains with paths over 256 characters...
                        System.IO.File.WriteAllText("\\\\?\\" + tableComponentFolder + "\\" + angularName + "-table.component.html", tableHTMLTemplateCode);



                        //
                        // Create the default table component SCSS code
                        //
                        string tableSCSSCode = BuildDefaultAngularTableComponentSCSSImplementation(moduleName, type, scriptGenTable);

                        //
                        // Write out the table SCSS
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + tableComponentFolder + "\\" + angularName + "-table.component.scss", tableSCSSCode);


                        //
                        // Create the default table component typescript template code
                        //
                        string tableTypeScriptCode = BuildDefaultAngularTableComponentTypeScriptImplementation(moduleName, type, scriptGenTable, addAuthorization);

                        //
                        // Write out the TypeScript
                        //
                        // Prefix is to disable path parsing to allow long paths
                        //
                        System.IO.File.WriteAllText("\\\\?\\" + tableComponentFolder + "\\" + angularName + "-table.component.ts", tableTypeScriptCode);


                        #endregion
                    }
                }
            }

            System.IO.File.WriteAllText(filePath + moduleName + $"\\angular\\{moduleName}.data-component.list.for.app.modules.ts.txt", GenerateAppComponentListing(moduleName, contextType, database, addAuthorization));


            System.IO.File.WriteAllText(filePath + moduleName + $"\\angular\\foundation.utility.ts", GenerateFoundationUtilityCode());

            return;
        }

        private static string GenerateFoundationUtilityCode()
        {
            return @"//
// Utility functions and interfaces used by Foundation components
//
//

/**
* This interface is be used to define a column that is used on a Foundation table component.  Each table component
* predefines their default set of these, but users of the components can override that set any way they want to change
* things like content, width, position, etc..
*
*/
export interface TableColumn {

  // Property path in the object, e.g. 'startTime' or 'auditUser.name'.  Supports nested objects
  key: string;

  // Display heeader
  label: string;

  // Optional fixed width (px) or percentage width.  Using undefined makes it auto size.
  width?: string;

  // Cell template data type 
  template?: 'boolean' | 'date' | 'link' | 'default';

  // Link target for link template types.  First element is the route, starting with a slash, the second element is the name of the property that contains the index to the route.  eg ['/auditevent', 'auditEventId']
  linkPath?: string[];

  // Show on mobile cards (default is shown if no value.  'prominent' to make it show in the mobile card header, 'hidden' to hide it.)
  mobile?: 'hidden' | 'prominent'
}



/**
* Converts an ISO UTC date-time string (e.g. ""1999-06-30T00:00:00.000000Z"")
* into the format required by <input type=""datetime-local""> (""YYYY-MM-DDTHH:mm"").
* If the input is null, undefined, or empty, returns null.
* This handles the fact that the server sends strings, not Date objects.
*/
export function isoUtcStringToDateTimeLocal(value: string | null | undefined): string | null {

  if (!value || typeof value !== 'string') {
    return null;
  }

  // The native Date parser understands ISO strings with Z correctly.
  const date = new Date(value);

  // Guard against invalid dates
  if (isNaN(date.getTime())) {
    return null;
  }

  // Build the exact format the browser input expects.
  // We intentionally use local time components because datetime-local is a local control.
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
}


/**
 * Covert the datetime-local string back to a UTC ISO string that the server expects.
 */
export function dateTimeLocalToIsoUtc(value: string | null | undefined): string | null {

  if (!value) {
    return null;
  }

  const date = new Date(value);

  if (isNaN(date.getTime())) {
    return null;
  }

  //
  // toISOString() always returns UTC with Z and milliseconds
  //
  return date.toISOString();
}";
        }

        private static string GenerateAppComponentListing(string moduleName, Type contextType, DatabaseGenerator.Database database, bool addAuthentication = true)
        {
            StringBuilder importSb = new StringBuilder();
            StringBuilder declarationsParamSb = new StringBuilder();


            StringBuilder routingImportsSb = new StringBuilder();
            StringBuilder routingSb = new StringBuilder();

            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;

                    if (entityName.EndsWith("Datum") == true)
                    {
                        entityName = entityName.Replace("Datum", "Data");
                    }



                    string camelCaseName = CamelCase(entityName, false);
                    string suffixableCamelCaseName = StringUtility.CamelCase(entityName, false);

                    // Fix nonsense with some pluralization
                    if (camelCaseName.EndsWith("Statu") == true || camelCaseName.EndsWith("Campu") == true)
                    {
                        camelCaseName += "s";
                    }



                    string pluralName = Pluralize(entityName);
                    string titleName = StringUtility.ConvertToHeader(suffixableCamelCaseName);


                    string angularName = StringUtility.ConvertToAngularComponentName(entityName);
                    if (angularName.EndsWith("statu") == true || angularName.EndsWith("campu") == true)
                    {
                        angularName += "s";
                    }

                    Type type = propertyType.GenericTypeArguments[0];

                    // Get the table spec from the script generator
                    DatabaseGenerator.Database.Table scriptGenTable = null;
                    foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                    {
                        if (tbl.name == type.Name)
                        {
                            scriptGenTable = tbl;
                            break;
                        }
                    }

                    //
                    // If table name ends with 'Statu' or 'Campu' then look again with an s on the end.  This is for the stupidity in the .Net Framework's plural determination.
                    //
                    if (scriptGenTable == null && (type.Name.EndsWith("Statu") || type.Name.EndsWith("Campu")))
                    {
                        var realName = type.Name + "s";

                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            if (tbl.name == realName)
                            {
                                scriptGenTable = tbl;
                                break;
                            }
                        }
                    }


                    //
                    // Datum is the plural for Date in the new EFCorePowerTools pluralizer.
                    //
                    if (scriptGenTable == null && type.Name.EndsWith("Datum"))
                    {
                        var realName = type.Name.Replace("Datum", "Data");

                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            if (tbl.name == realName)
                            {
                                scriptGenTable = tbl;

                                // recalculate the angular name to be a function of the adjusted name.  This is for situations like the 'ProjectRollerData' table that becomes an entity of name 'ProjectRollerDatum', and we want to use the real table name as the service name
                                angularName = StringUtility.ConvertToAngularComponentName(realName);
                                break;
                            }
                        }
                    }


                    if (scriptGenTable != null)
                    {
                        string entity;

                        if (type.Name.EndsWith("Datum") == false)
                        {
                            entity = type.Name;
                        }
                        else
                        {
                            entity = type.Name.Replace("Datum", "Data");
                        }

                        string plural = Pluralize(entity);


                        //
                        // For the component import 
                        //
                        importSb.AppendLine("import { " + entity + $"ListingComponent }} from './{moduleName.ToLower()}-data-components/" + angularName + "/" + angularName + "-listing/" + angularName + "-listing.component';");
                        importSb.AppendLine("import { " + entity + $"AddEditComponent }} from './{moduleName.ToLower()}-data-components/" + angularName + "/" + angularName + "-add-edit/" + angularName + "-add-edit.component';");
                        importSb.AppendLine("import { " + entity + $"DetailComponent }} from './{moduleName.ToLower()}-data-components/" + angularName + "/" + angularName + "-detail/" + angularName + "-detail.component';");
                        importSb.AppendLine("import { " + entity + $"TableComponent }} from './{moduleName.ToLower()}-data-components/" + angularName + "/" + angularName + "-table/" + angularName + "-table.component';");

                        //
                        // for the declarations
                        //
                        declarationsParamSb.AppendLine(entity + "ListingComponent,");
                        declarationsParamSb.AppendLine(entity + "AddEditComponent,");
                        declarationsParamSb.AppendLine(entity + "DetailComponent,");
                        declarationsParamSb.AppendLine(entity + "TableComponent,");

                        //
                        // For case insensitive routing
                        //
                        // This is made case insensitive, and it depends on the caseInsensitiveMatcher function
                        //
                        routingImportsSb.AppendLine("import { " + entity + $"ListingComponent }} from './{moduleName.ToLower()}-data-components/" + angularName + "/" + angularName + "-listing/" + angularName + "-listing.component';");
                        routingImportsSb.AppendLine("import { " + entity + $"DetailComponent }} from './{moduleName.ToLower()}-data-components/" + angularName + "/" + angularName + "-detail/" + angularName + "-detail.component';");

                        if (addAuthentication == true)
                        {
                            routingSb.AppendLine($"  {{path: '{pluralName.ToLower()}', component: {entityName}ListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: '{Pluralize(titleName)}' }},");
                            //routingSb.AppendLine($"  {{path: '{entityName.ToLower()}/:{suffixableCamelCaseName}Id', component: {entityName}DetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: '{titleName}' }},");

                            /*

                            { path: 'crews/new', component: CrewDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Crew' },
                              { path: 'crews/:crewId', component: CrewDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew' },
                              { path: 'crew/:crewId', component: CrewDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew' },
                              { path: 'crew', redirectTo: 'crews' }, 

                             */

                            routingSb.AppendLine($"  {{path: '{pluralName.ToLower()}/new', component: {entityName}DetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create {titleName}' }},");
                            routingSb.AppendLine($"  {{path: '{pluralName.ToLower()}/:{suffixableCamelCaseName}Id', component: {entityName}DetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit {titleName}' }},");
                            routingSb.AppendLine($"  {{path: '{entityName.ToLower()}/:{suffixableCamelCaseName}Id', component: {entityName}DetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit {titleName}' }},");
                            routingSb.AppendLine($"  {{path: '{entityName.ToLower()}',  redirectTo: '{pluralName.ToLower()}'}},");

                        }
                        else
                        {
                            routingSb.AppendLine($"  {{path: '{pluralName.ToLower()}', component: {entityName}ListingComponent, canDeactivate: [UnsavedChangesGuard], title: '{Pluralize(titleName)}' }},");
                            //routingSb.AppendLine($"  {{path: '{entityName.ToLower()}/:{suffixableCamelCaseName}Id', component: {entityName}DetailComponent, canDeactivate: [UnsavedChangesGuard], title: '{titleName}' }},");

                            routingSb.AppendLine($"  {{path: '{pluralName.ToLower()}/new', component: {entityName}DetailComponent, canDeactivate: [UnsavedChangesGuard], title: 'Create {titleName}' }},");
                            routingSb.AppendLine($"  {{path: '{pluralName.ToLower()}/:{suffixableCamelCaseName}Id', component: {entityName}DetailComponent, canDeactivate: [UnsavedChangesGuard], title: 'Edit {titleName}' }},");
                            routingSb.AppendLine($"  {{path: '{entityName.ToLower()}/:{suffixableCamelCaseName}Id', component: {entityName}DetailComponent, canDeactivate: [UnsavedChangesGuard], title: 'Edit {titleName}' }},");
                            routingSb.AppendLine($"  {{path: '{entityName.ToLower()}',  redirectTo: '{pluralName.ToLower()}'}},");


                        }
                    }
                }
            }

            StringBuilder outputSb = new StringBuilder();

            outputSb.AppendLine(("These are the import lines to add to the top of app.module.ts file to import the auto generated component files.  This has listing and Add/Edit components"));
            outputSb.AppendLine();
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// Beginning of imports for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.Append(importSb.ToString());
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// End of imports for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.AppendLine();
            outputSb.AppendLine();


            outputSb.AppendLine(("These are the import lines to add to declarations section of the the app.module.ts to reference the auto generated component objects"));
            outputSb.AppendLine();
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// Beginning of declarations for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.Append(declarationsParamSb.ToString());
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// End of declarations for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.AppendLine();
            outputSb.AppendLine();
            outputSb.AppendLine();


            outputSb.AppendLine(("These are the import lines to add to the top of routing module file to import the auto generated component files.  List is just listing components."));
            outputSb.AppendLine();
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// Beginning of imports for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.Append(routingImportsSb.ToString());
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// End of imports for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.AppendLine();
            outputSb.AppendLine();
            outputSb.AppendLine();


            outputSb.AppendLine(("These are the lines to add to routing module to reference case insensitively reference the auto generated component objects"));
            outputSb.AppendLine();
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// Beginning of routes for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.Append(routingSb.ToString());
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// End of routes for {moduleName} Data Components ");
            outputSb.AppendLine(@"//");
            outputSb.AppendLine();
            outputSb.AppendLine();


            return outputSb.ToString();
        }
    }
}