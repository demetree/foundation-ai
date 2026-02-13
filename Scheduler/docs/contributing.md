# Scheduler — Contributing

This guide covers Scheduler-specific development patterns.  For general code style, naming conventions, commenting standards, and formatting rules, see the root [README.md](file:///d:/source/repos/scheduler/README.md).

---

## What Not to Edit

> [!CAUTION]
> The following folders contain **auto-generated code** produced by Foundation Code Generation tools.  Manual edits will be **overwritten** the next time code generation runs.

**Server:**
- `Scheduler.Server/DataControllers/` — all 138 auto-generated CRUD controllers

**Client:**
- `Scheduler.Client/src/app/scheduler-data-services/` — auto-generated HTTP data services
- `Scheduler.Client/src/app/scheduler-data-components/` — auto-generated CRUD UI components

If you need to customize behavior for an entity, create custom files in the appropriate `*-custom` component folder or custom controller/service — never modify the auto-generated ones.


---

## Adding a New Entity

When a new data entity needs to be introduced, the process is:

1. **Define the entity** in the `SchedulerDatabaseGenerator` script definitions
2. **Run `SchedulerTools`** to produce new database scripts *(manual step — done by a team member)*
3. **Apply the database scripts** to SQL Server *(manual step)*
4. **Run EF Core Power Tools** to regenerate the Entity Framework model in `SchedulerDatabase` *(manual step)*
5. **Run `SchedulerTools`** again to produce new auto-generated source files for controllers, services, and components *(manual step)*
6. **Copy generated files** into the appropriate project folders:
   - Server data controller → `Scheduler.Server/DataControllers/`
   - Client data service → `Scheduler.Client/src/app/scheduler-data-services/`
   - Client data component → `Scheduler.Client/src/app/scheduler-data-components/`
7. **Register the controller** in `Program.cs` within the `controllers.Add(typeof(...))` section
8. **Register the Angular module** in `app.module.ts` (imports and declarations)
9. **Add routes** in `app-routing.module.ts`

After these steps, you have baseline CRUD.  Custom screens and logic are built on top.


---

## Creating Custom Components (Client)

### Component Group Pattern

Each entity follows a consistent structure of component sub-folders.  For example, to add a custom UI for a `Vehicle` entity:

```
components/
└── vehicle-custom/
    ├── vehicle-custom-listing/        ← Route entry point
    │   ├── vehicle-custom-listing.component.ts
    │   ├── vehicle-custom-listing.component.html
    │   └── vehicle-custom-listing.component.scss
    ├── vehicle-custom-table/          ← Data table with pagination
    ├── vehicle-custom-detail/         ← Single record view with tabs
    ├── vehicle-custom-add-edit/       ← Create/edit form
    ├── vehicle-overview-tab/          ← Detail tab: summary info
    └── vehicle-assignments-tab/       ← Detail tab: related events
```

**Key patterns to follow:**
- The **listing** component is the route entry point and contains the table + an add/edit button
- The **table** component handles pagination, filtering, and row click navigation to the detail view
- The **detail** component uses a tabbed layout (Bootstrap `ngb-tabset` or similar) with one sub-component per tab
- The **add-edit** component is typically opened as a modal dialog

Study existing groups like `resource-custom/` (18 sub-components) or `office-custom/` (13 sub-components) for comprehensive examples.


### Registering Components

After creating your component files:

1. Add the component class to the `declarations` array in `app.module.ts`
2. Add routes in `app-routing.module.ts`
3. Add navigation links in the sidebar component if needed


---

## Creating Custom Controllers (Server)

Custom controllers go in `Scheduler.Server/Controllers/` and extend the Foundation base classes.

### Template

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Foundation;
using Foundation.Scheduler.Database;


namespace Foundation.Scheduler.Controllers
{
    //
    // Custom controller for [EntityName] business logic
    //
    // Provides endpoints beyond the auto-generated CRUD for [EntityName].
    //
    [Route("api/[controller]")]
    [ApiController]
    public class MyCustomController : Foundation.Web.Controllers.FoundationControllerBase
    {
        private readonly ILogger<MyCustomController> _logger;
        private readonly SchedulerContext _context;


        public MyCustomController(ILogger<MyCustomController> logger,
                                  SchedulerContext context)
            : base()
        {
            _logger = logger;
            _context = context;
        }


        //
        /// <summary>
        ///
        /// Description of what this endpoint does.
        ///
        /// </summary>
        [HttpGet("MyAction")]
        public async Task<IActionResult> MyActionAsync()
        {
            // Implementation here

            return Ok();
        }
    }
}
```

### Registration

Add the controller to the explicit controller list in `Program.cs`:

```csharp
//
// Custom Scheduler controllers
//
controllers.Add(typeof(MyCustomController));
```


---

## Creating Custom Services (Server)

Custom services go in `Scheduler.Server/Services/`.

### Template

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Foundation.Scheduler.Database;


namespace Scheduler.Server.Services
{
    //
    /// <summary>
    ///
    /// Description of the service's responsibility.
    ///
    /// </summary>
    public class MyCustomService
    {
        private readonly ILogger<MyCustomService> _logger;


        public MyCustomService(ILogger<MyCustomService> logger)
        {
            _logger = logger;
        }


        //
        /// <summary>
        ///
        /// Description of this method.
        ///
        /// </summary>
        public async Task DoWorkAsync()
        {
            // Implementation here
        }
    }
}
```

### Registration

Register in `Program.cs` with the appropriate lifetime:

```csharp
// Scoped — one instance per HTTP request (most common)
builder.Services.AddScoped<Scheduler.Server.Services.MyCustomService>();

// Singleton — one instance for the application lifetime
// builder.Services.AddSingleton<Scheduler.Server.Services.MyCustomService>();
```


---

## Creating Angular Services (Client)

Custom services extend `SecureEndpointBase` for authenticated API calls.

### Template

```typescript
import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';


@Injectable({
    providedIn: 'root'
})
export class MyCustomService extends SecureEndpointBase {

    constructor(http: HttpClient,
                alertService: AlertService,
                authService: AuthService,
                @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);
    }


    /**
     * Description of what this method does.
     */
    public getMyData(): Observable<any> {

        //
        // Build authentication headers
        //
        const headers = this.authService.GetAuthenticationHeaders();

        return this.http.get<any>(`${this.baseUrl}api/MyCustom/MyAction`, {
            headers: headers
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.getMyData());
            })
        );
    }
}
```

**Key patterns:**
- Always use `this.authService.GetAuthenticationHeaders()` for authenticated calls
- Use `this.handleError(error, retryFn)` for automatic token-refresh retry on 401
- Register with `providedIn: 'root'` for singleton services, or add to a module's `providers` array


---

## AI-Developed Code Labeling

Per the project guidelines, any code significantly developed with AI assistance **must** be clearly labeled:

- **File header**: add a note like `AI-Developed — This file was significantly developed with AI assistance.`
- **Function header**: note the AI involvement in the XML/JSDoc comment
- **Inline**: add comments within AI-developed logic sections as appropriate

See `conflict-detection.service.ts` and `RecurrenceExpansionService.cs` for examples of this labeling in practice.
