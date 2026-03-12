# Development Guidelines

In order to maintain consistency and continuity across language and projects in the  code base, the following
guidelines are to be conformed to by developers contributing to the code base.

These guidelines benefit the system by ensuring that all staff interacting with source files can expect a consistent 
experience, and this will reduce the time needed to learn or understand the business logic, and reduce the risk of error.


## What is this Code Base?

This is the K2 Research scheduling system, which is building on the K2 Research Foundation System

## What is the Foundation System?

The Foundation system is a set of C# programs that become the foundation for almost any database centric application.

It consists of 3 conceptual layers.  

The first layer is a code generation system that is found in the folders with the name prefix of 'CodeGeneration'.  
These produce baseline functioning code built from a system model defined in a database script generator program.  In this case, that file is found in the 
'SchedulerDatabaseGenerator' folder.  Running Code Generation on this produces 2 primary folders.  The first is the database scripts neeed to 
stand up the database.  The 2nd is the source code for C# and Angular to provide a working scaffolded system.  Note that these files produced need to 
manually cut and pasted into a container projeect that is manually managed.   In this case, the container projects are Scheduler.Server and Scheduler.Client

The second layer is a set of foundational c# libraries that are in the FoundationCommon, FoundationCore, and FoundationCore.Web folders.  These are 
feferenced by the systems built on Foundation code generation.

The third layer is a set of 2 standard foundation modules that define a Security and Auditing system that all Foundation Systems use.

Note a Foundation system is always built on the Security and Auditor modules, and can introduce more modules to suit its business needs.



Once that is in place, the manual work begins to create custom workflows that are built from new UIs and Server components to add specific business
functionality or custom screens to suit the final application.  The auto generated code frameworks should become the IO foundation wherever possible,
as they provide a robust data access layer for single table access.   Note that multi table operations above and beyond nav properties or child lists
most often need a customization to make them single step.


Please scan the above noted folders and see the source code to deep dive into how it works.  The Scheduler.Client contains customizations in the folders
generally including the name 'custom' in the in /src.app.components/ folder.  There are examples for resources, contacts, clients, offices, and more.

The Scheduler.Server project has most of its server side customizations in the 'controllers' and 'services' folders.

The basic use case for editing a the Scheduler systems's data structures using Foundation tooling is

- Create or update a class based on the database script generator to create or edit a schema.
- The person working on the project Runs the 'SchedulerTools' to output new database scripts which are used to build the Database ** NOTE THIS IS A MANUAL STEP TO BE DONE BY THE HUMAN WORKING WITH THE AGENT **
- The person processes the new database with the  'EF Core Power Tools' to produces an Entity Framework object model representing the database. ** NOTE THIS IS A MANUAL STEP TO BE DONE BY THE HUMAN WORKING WITH THE AGENT **
- The person working on the project Runs the 'SchedulerTools' to output new source files that are based on the EF Context and Script Generator, ** NOTE THIS IS A MANUAL STEP TO BE DONE BY THE HUMAN WORKING WITH THE AGENT **
  and then copies the output to the appropriate place in the final Scheduler projects.   App.module, App.Routing, and program.cs files are adjusted
  as well to integrate new files into the systems
 
 Once this is done, customization and extenstion can be beging to provide better UIS and custom workflows as ncecessary



## Conceptually, What is a Source File? 

A source file serves two distinct purposes:

1. The first is to provide a compiler with input to convert to machine language output.  This is the lowest common denominator for any source file ever.

2. The second and more important purpose is that it is a document that communicates the intention of its creator to its reader, who may or may not be a developer.


The source file is the lowest level of systems documentation, and in many cases it is the only documentation.

Therefore, a completed source file has to do more more than just compile.  

1. It is a human readable document that also happens to contain words that a compiler can comprehend.  
 
2. The compiler words should be wrapped alongside words (expressed as comments and variable/function names)  that a person can easily comprehend that tell the next person why the compiler words are there in the first place.
 
3. The compiler words used in the source file should be chosen by prioritizing clarity and simplicity over 'cleverness' or the use of language features that provide no benefit over a more traditional option beyond reducing character count.

Comments and white spacing are expected to be present and liberally used, even if other guidelines would measure it as excessive.

A source file has a life cycle.  It starts as an idea, and grows up into a mature document.  Here's how that works:

## Life of a Source File

A source file is created when a person has an idea that needs to be put into source code to make real.  

Here's how a source file grows up at Compactica.

### Initial Idea to Proof of Concept

The source File is created, and given a meaningful name.

- Neither the idea, nor the logic will be fully developed yet, and the source file may not even compile at the beginning of this stage.

- It is unlikely to be optimized for either computer or human readers at this point.

- Comments might exist in key places alongside the developer's thought process.

- Once the idea is validated, and things work enough to merit more effort we move to the next step:


### Completion of Proof of Concept

At this point, the file will compile, but the logic will need to be fully developed and tuned here to make it 'good' code.

This will include things like:

- Big functions and/or common code might be broken down into sub routines.

- Logic might be rearranged to be more organized or efficient.

- Common functionality might be broken out in base classes.

- Interfaces might be extracted.

- Comments and names are improved.

- **Mental Notes and thought processes you have as the initial developer are added as comments here to illustrate why you're building whatever it is the way you are building it.**  

- Performance Requirements are validated.

- Logging of significant events during processing are added at levels that are thoughtfully chosen.

- Database interactions might be optimized by tuning queries and adding indexes.

- Caches added where beneficial to minimize redundant repeated work.

At this point, the logic should be sound, and should pass technical test cases.  We can then move to the next step.

### Completion of Document

At this point, the logic contained in the source document is complete and efficient from the compiler's perspective, but it will be lacking in human readable content because that has not been the focus up to this point.  

- The narrative and style information will be added and tuned here.  

- The audience is the person that reads the file, not the compiler.

- The bigger the file, the more comments/documentation it should have in it.

**The objective of this step is to make the document easier to read and maintain by the next person who interacts with it.**

- A narrative header comment is added to the top of the file to provide executive summary information about why the file exists, and what it's supposed to do.
    - When editing an existing file, narrative about the reason for the edits can be added at the discretion of the developer, if the changes are significant enough to benefit from it.

- XML Comments are placed at the top of each function describing it's function, and ideally its inputs and outputs

- Standardized line breaks and generous white spacing to help reader interpret sections by logical group is done is through the document


- Function names are reconsidered for sufficient clarity.  There is nothing wrong with long function names.
- Variable names are reviewed for clarity and conciseness.  Shorter is not better, and abbreviations are removed.
- Container variables such as arrays, lists, dictionaries, and hash sets are named with appropriate suffixes, such as 'List' or 'Array', etc..
- Namespace grouping might be considered for best fit in the library it lives in.
- Single character iterator variables like `int i` are acceptable only whey the are used in a single level loop construct.  Otherwise, rename them to something representing thir purpose like `parcelIndex`.
- Functions are reviewed in detail from a readability perspective, and white space of at least one line is placed between logical groups of code
    
    - Input
    - Processing
    - Output
    - Each group will have white space between lines so that they visually stand out.
    
- groups of code within functions are annotated with comment blocks to narrate their intent, even if it would seem obvious to some programmers.  
    - **Remember, the audience here is not just trained programmers.**
 

- the final line for a function that returns its output shouldn't be anything more than a simple return of a variable name.

- all unused code and commented code sections are removed.

- all debugging code is removed or disabled and clearly commented if it has ongoing utility.



- order of functions inside file is reviewed and set according to these general rules:
  
    - first are class constants.
    
    - next are variable groups.

    - next are helper classes, records and structs used by the primary class body.

    - Initialization functions next with constructors always being first, then any setup methods.
    
    - work functions are next, with the most important methods first, prioritized by visibility. 
        - public  methods should be first, with public static methods grouped together for clarity.
    
    - smaller helper type methods are placed last
    
    - destructors and disposers are placed at the end
    


## Code Grouping

For variable declarations, especially at the class level, when there are multiple variables involved in a single work concept, they should be 
grouped together.  There should be a a comment at the start of the group, explaining with the group is.  The variable names might even reflect membership
in that group where it makes sense.  

Code Grouping Should follow a basic pattern like this:

```
class Something
{
    //
    // Constants
    //
    private const int FREQUENCY = 1000;
    private const int DURATION = 10;


    //
    // Caches for query reduction
    //
    private static Dictionary<int, string> _exampleCache = null;

    //
    // State tracking for something or other
    //
    private int _requestCounter;
    private DateTime _lastRequestTime;

    //
    // Helper structures
    //
    private record HelperOne
    {
        public string name { get; set; }
        public int value { get; set ;}
    }

    //
    // Setup class with some things
    //
    public Something(int some, double thing)
    {
        //
        // Something will be done here
        //
    }
    
    ... And so on
}
```


## White Space

White space frames the document to make it easier to read.  That is why it is important.

White space is to be used liberally between groups of code.  

A group of code could be set of variables that contribute to a theme, or a section in a function.  It's something a bunch of lines of code represent that is unique, or important.  

It changes a wall of text into a group of paragraphs and sentences.

It is up to the programmer's discretion on what the groups are, but they are to be organized along the thinking process of the builder of the code.


## Accessibility Modifiers

All declarations of variables, properties, and functions at the class level should include the accessibility modifier appropriate for their intended use.  

Do not leave it off and assume the default is OK.

- `private`, `protected`, and `public` should be used on all variables, properties, and functions at the class level.

- `internal` should be generally avoided and `public` used instead, unless there is a specific need for it where `public` would be a problem.


## Variable Naming and Definition

All variable definitions are to be at the top of their scope, and not intermixed within scope logic.

One line per variable.

Variables should be initialized with their expected default value on their declaration line.


## Function Naming

Function names must use Pascal case.  

For example:

public int GetCount()

## Regions

The use of regions is acceptable within large file to allow named collapsible sections of code that merit grouping.  However, these rules must be followed:

- Use them around functions inside a class only, never in the header sections

- Before using them, ask yourself if this section of that can be grouped would be better off in another source file, perhaps in a new sub class.

- Do not use them around an entire source file.  A collapsed group or set oof groups is never the first thign a person nhould see when opening a file


### Function Scoped Locals

Camel case to be used.  

For example:

`int counter = 0;`

### Class Wide or Global Scoped

Camel case with an underscore prefix is to be used.  

For example:

`private int _totalRecords = 0;`

### Constants

Constants are to be defined either at the class or global level, and must use Constant Case / Macro Case.

For example:

`private const int MAXIMUM_COUNT = 100`


### Date and Time Data Types

In all code, variables that contain Date/Time values are to be stored in UTC terms.  All UTC, all the time.

- This is most commonly done with a `DateTime` type in C# that has a 'kind' of UTC.

- `DateTimeOffset` data types can be used as well, but there's usually no need for it.

- Maintaining this requires consideration on things like serialization so the appropriate UTC suffix of 'Z' is included with the serialization.  This usually needs special configuration of the serializer, so 
don't assume that the default options will just work.

- It is expected that all date times are in the UTC format during serialization of any kind.

- It is expected that writing or reading dates from disk, they are always in UTC terms.  

- SQL Server any many other databases do not store time zone indicators with the regular date time types, so for cross database consistency, all dates are to be in UTC terms all the time when at rest.

- Local time conversions should only be necessary when presenting the value to the users.


## Function Return Types

Return either a single value type, an object type, or a collection class of a value type or an object.  

**Do not return Tuples.**

## 'out' Parameters in Functions

These are perfectly fine to use for supplementary function output, especially when in a function that returns a bool type.  It makes for a clean design.

See the the `TryParse()` set of function in the .Net framework for a good usage example.

## 'ref' Parameters in Functions

Use them if there is a valid case, but usually there's not.  

Value types are best passed back directly as the result of a function, or as a field on an object that is returned, or outputted.

## Async Functions

Use them when there is valid reason to do so.  Much of the Compactica code base has a valid use for it, but some does not.

When creating an `async` function, then the name of the function must end with the word `Async` so that it is clear to the user that it must be invoked accordingly.

## Concurrency

When building logic that can be executed by multiple threads, follow these patterns:

- For collection type variables, always use a thread safe type from `System.Collections.Concurrent`, or one of the Compactica derived subclasses thereof.  There are lots of examples in the code base of most of these classes in use.
    - `ExpiringCache` and `ThreadSafeList` are heavily used Compactica custom class based on `System.Collections.Concurrent` classes.
    - `ConcurrentQueue` and `ConcurrentDictionary` are heaving used classes directly from the `System.Collections.Concurrent` namespace.

- For bool types, use the `ThreadSafeBool` class in the Compactica base library.

- For non bool basic value types, Use the `Interlocked` class to safely interact with variables across threads.

- Don't use `volatile` variables.

- Use `lock(object)` only in very simple scenarios.  Usually there's a better way to do things than with a lock, such as with thread safe collections.

- For more complex locking, especially in Async functions, always use a `SemaphoreSlim` configured appropriately for the locking rules desired.  Lots of examples of this are in the code base.

- **Do not use any locking through the `Monitor` class**

## White Space Between Functions

There should always be 2 blank lines between the bottom of one function and the start of another.

## Commenting

Comments should be used throughout the code.

- // is to be used instead of /* */ 
- Use the triple slash helper in Visual Studio to frame up XML comments at the top of functions.  Type /// at the top of function line to do that.

Comments should be created a the following levels:

- Top of the file
- Top of each function
- Inside each function to describe its intention
    - Small functions that are 10 lines or less may not need more than a single line, or maybe the function header comment is clear enough
    - Larger functions that perform broader functions, should comment each step in it's operation, likely bound to groups  of processing. For example:
        - Setup of some task - Note anything unique
        - Gathering required Data - Narrate use of parameters for sub functions calls if there are complex parameters.  Explain why you're getting whatever you're getting
        - Processing of data - Add a description of what what you're doing to it
        - Setting up for output
        - Returning output
            

- The comments at the top of the file can narrate the history of a file, at the discretion of the developer to provide context to the next developer if changes
 were made, and why they were made.  This is a great attachment point for git history review as well as it provides insights to reconcile deltas that might be found.

- Comments should be led by white space of at least one line, unless it's the first line in a function or class.

- Comments on their own lines should be made to stand out visually by having leading and trailing blank comment lines like this:

```
//
// Initialize some integers
//
int i = 0;
int j = 0;
int k = 0;
```

## Code Organization


## Braces

Should **always** follow a control structure such as an `if`, `for`, or `while`.  

They should **always** be on their own lines, with an exception for `TS` or `JS` that the opening brace can be on the end of the control structure line.

In `C#`
```
if (counter == 10)
{
    counter = 0;
}
else
{
    counter++;
}
```

Exception for `JS` and `TS`
```
if (counter == 10) {
    counter = 0;
} else {
    counter++;
}
```

When inside braces, always increase the alignment of the code by one tab level.

## Examples

Review core Compactica business logic.  For example, the 'LocalDatabaseLotManager.cs' in RollerOps is decent.

Also review the code generated by the code generation frameworks.  This includes the Web API data controllers, or TypeScript data services or components.

This example function has an acceptable structure and level of narrative.

```
        //
        /// <summary>
        /// 
        /// Helper method for ProjectLotParcelLiftPassSummarizationOptimized.
        /// 
        /// It builds up a list of Project Lot Parcel Lift Passes that need summarization, and it attaches some child objects onto each that are needed for calculations
        /// and data interchange processing.
        /// 
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="parcelLiftPassesToSummarize"></param>
        /// <param name="readFromDiskContext"></param>
        /// <param name="cutOffTime"></param>
        /// <returns></returns>
        private async Task<List<ProjectLotParcelLiftPass>> GetParcelLiftPassesToSummarize(int batchSize, RollerOpsContext readFromDiskContext)
        {
            List<ProjectLotParcelLiftPass> parcelLiftPassToSummarizeList = null;

            //
            // This is the optimized version that breaks up the work to better optimize the use of DB indexes.  It does not use the '.Include'
            //

            // Step 1 - Get the lift passes
            parcelLiftPassToSummarizeList = await (from plplp in readFromDiskContext.ProjectLotParcelLiftPasses
                                                 where plplp.summarized == false &&          // Get un-summarized parcel lift passes.
                                                 plplp.canSummarize == true
                                                 orderby plplp.id
                                                 select plplp)
                                                    .Take(batchSize)                  // process one batch at a time
                                                    .AsNoTracking()
                                                    .ToListAsync();

            //
            // Step 2 - Get the ProjectLotParcelLiftPassProjectRollerData records for the lift passes that need summarization.  Key them by the parcel lift pass id.
            //
            List<long> plplpIds = (from x in parcelLiftPassToSummarizeList select x.id).ToList();
            List<ProjectLotParcelLiftPassProjectRollerData> projectLotParcelLiftPassProjectRollerDataList = await (from plplpprd in readFromDiskContext.ProjectLotParcelLiftPassProjectRollerDatas
                                                                                                                   where plplpIds.Contains(plplpprd.projectLotParcelLiftPassId)
                                                                                                                   select plplpprd)
                                                                                                                    .AsNoTracking()
                                                                                                                    .ToListAsync();

            Dictionary<long, List<ProjectLotParcelLiftPassProjectRollerData>> liftPassToRollerDataMap = (from plplpprd in projectLotParcelLiftPassProjectRollerDataList
                                                                                                         group plplpprd by plplpprd.projectLotParcelLiftPassId into g
                                                                                                         select new
                                                                                                         {
                                                                                                             projectLotParcelLiftPassId = g.Key,
                                                                                                             projectLotParcelLiftPassProjectRollerDatas = g.ToList()
                                                                                                         })
                                                                                                         .ToDictionary(x => x.projectLotParcelLiftPassId, x => x.projectLotParcelLiftPassProjectRollerDatas);


            //
            // Step 3 -> map the dictionary data into the main output list.
            //
            foreach (ProjectLotParcelLiftPass plplp in parcelLiftPassToSummarizeList)
            {
                if (liftPassToRollerDataMap.TryGetValue(plplp.id, out List<ProjectLotParcelLiftPassProjectRollerData> values) == true)
                {
                    plplp.ProjectLotParcelLiftPassProjectRollerDatas = values;
                }
            }

            //
            // Step 4, get a list of all of the unique ProjectRollerDatas we need, and then get them from the database.
            //
            List<long> prdIds = (from x in projectLotParcelLiftPassProjectRollerDataList select x.projectRollerDataId).Distinct().ToList();
            Dictionary<long, ProjectRollerData> allProjectRollerDataNeededMap = await (from prd in readFromDiskContext.ProjectRollerDatas
                                                                                       where prdIds.Contains(prd.id)
                                                                                       select prd)
                                                                                        .AsNoTracking()
                                                                                        .ToDictionaryAsync(x => x.id, x => x);

            //
            // Step 5 - Map all of the roller data records from the map into the output
            // 
            foreach (ProjectLotParcelLiftPass plplp in parcelLiftPassToSummarizeList)
            {
                foreach (var plplprd in plplp.ProjectLotParcelLiftPassProjectRollerDatas)
                {
                    if (allProjectRollerDataNeededMap.TryGetValue(plplprd.projectRollerDataId, out ProjectRollerData value) == true)
                    {
                        plplprd.projectRollerData = value;
                    }
                }
            }

            //
            // Step 6 Get the project lot parcel list
            //
            foreach (ProjectLotParcelLiftPass plplp in parcelLiftPassToSummarizeList)
            {
                plplp.projectLotParcelLift = await GetProjectLotParcelLift(readFromDiskContext, plplp.projectLotParcelLiftId);
            }

            //
            // Step 7 Get the project roller pass
            //
            foreach (ProjectLotParcelLiftPass plplp in parcelLiftPassToSummarizeList)
            {
                plplp.projectRollerPass = await GetProjectRollerPass(readFromDiskContext, plplp.projectRollerPassId);
            }


            return parcelLiftPassToSummarizeList;
        }
```

### Imports in C#


Sequence of imports at top of files should be as follows:

This sequence is logical because it goes from most generic to least generic.

    1.  System always first
    1.  System sub namespaces next
    2.  Microsoft namespaces
    3.  3rd Party Reference namespaces
    4.  Foundation namespaces
    5.  Compactica namespaces

Do not include any unused namespaces.



## Property Syntactic Sugar

In C#, It is acceptable to use one line property shorthand for simple object properties rather than fully implementing getters and setters for properties that don't need custom logic in their setters or getters.

However, if the property needs any logic in either the setter or the getter, the full traditional property syntax should be used


For example:

**Simple Property 'Sugared' Version:**

```
// Example of syntactic sugar: Auto-property
// This declares a simple property with automatic getter and setter.
// The compiler creates a private backing field behind the scenes.
public class Person
{
    public string Name { get; set; }
}
```

**Simple Property 'Desugared' Version:** 

Note that this level of definition is needless in this context, because there is no logic in the setter or getter, but if there was, then this is the pattern to use with logic embedded in the getter and setter as necessary.

```
// Desugared version: Traditional property with explicit backing field
// This explicitly defines the private field and getter/setter methods.
public class PersonDesugared
{
    // Private backing field to store the value.
    private string _name;

    // Public property with getter and setter.
    public string Name
    {
        // Getter returns the backing field value.
        get
        {
            // get logic would be here

            return _name;
        }
        // Setter assigns to the backing field.
        set
        {
            // set logic would be here

            _name = value;
        }
    }
}
```


### Use of Tuples

Do not use tuples.  

Create a struct, record, or class instead.


## Use of 'var' type in C#

Limit use `var` data types in C# because they take away from clarity.  Only use them when the return type is complex, or there is readability improvement to its use in complex data types.

Don't use them only because you can.

`var` is always OK to use in `TS` and `JS`, as it is more of a language requirement/convention there.  That said, clarity there too is encouraged.


## Use of 'dynamic` type in C#

Nope.  Don't use them.


## Use of 'anonymous' types

This is acceptable when doing things like creating a simple object as the output of a Linq expression where you want just a few fields, and the use case doesn't justify the definition of a struct, record, or class.

Don't use them as return types.  Create an appropriate class to define your output and create an instance of that to return instead.


## Use of 'object' type in C#

Only use this when there is a valid reason to do so, such as the definition of an object to put a lock on, or when you truly need to reference something agnostically, without a strongly typed option.


## Use of 'any' type in TS

Try to avoid as much as possible.  Create classes or interfaces as need be to avoid them.


### Ternary Operators

These are to be avoided.  Use a regular `if` instead.  This applies in the majority of cases.  This is primarily because they break the 'one line is one thing' rule.

An exception might be when used in a wall of 1:1 mapping assignments to do things like null checks on the right side of the assignment.  In this case, it'll be more readable than interrupting a wall of assignments with a multi line if in the middle of them.


## Double Negatives

Try not to use double negatives in comparisons.  It's tougher to read.  

Use a test for positive `(state == true)` instead of not negative `(state != false)`.  

If the expected value is false though, then use `(state == false)` instead of `(state != true)`

# Style Requirements 

There are many ways to write code that a compiler will accept.  At Compactica, our guidelines prioritize clarity over brevity.

The reason for the use of these styles is because they improve code readability, and do not affect code efficiency.

## One Line is One Thing

**This is the cardinal rule.**

A single line of code should do exactly one thing, and the thing it does should be very clear.  
- If appropriate the one thing that the line does should be explained by a comment.

Do not pack multiple operations on one line, with the notable exception of a for loop, where the initialization, evaluation, and operation are acceptable on one line, as per convention.

This includes things such as ternary operations, null coalescing operators, pre or post increment operators, and the like, whose usage should be minimized, or heavily supplemented by narrative
comments explaining the embedded logic if they are used.


**Do not do this**
**This is because it does more than one thing on one line and is therefore harder to read than it needs to be**
```
ServerStatistics statistics = _systemStatsCache.TryGetValue("stats", out var cachedStats) ? cachedStats : await _rollerOps.GenerateStatistics(false);
```

**This is how it should be done**
Note that here it is easy to also add the results to the cache, which is something that can't be done in the 'bad' example above.
```
ServerStatistics statistics = null;

//
// Try and get the statistics from the cache.  Fall back to generating them if they're not cached.
//
if (_systemStatsCache.TryGetValue("stats", out var cachedStats) == true)
{
    statistics = cachedStats;
}
else
{
    //
    // Generate new statistics because they can't be found in the cache
    //
    statistics = await _rollerOps.GenerateStatistics(false);

    //
    // put the results into the cache for next time.
    //
    _systemStatsCache.Add("stats", statistics);
}

```


## If Statements:

All evaluation tests should have a value on the left and right hand side of the expression.  Do not use a single expression shorthand, **even if it resolves to a bool**.

- Avoid the use of the negation `!` operator where possible.  Use `== false` instead because it is more readable.

All if statements, regardless of whether or not they execute a single line following their evaluation must be wrapped in braces.



**This pattern should not be used.  This violates both of the above requirements.**

```

            if (_isDisposed)
                return;
```

**This is the style that should be used:**

```
            if (_isDisposed == true)
            {
                return;
            }

```
## Space before and After Operators

For any expression that has a left side and a right side, there should always be a single space on either side of the operator.

Spaces must also be after `if`, and `for`, `while', and within their braces, as shown in the examples.

For example: 

**These are correct**
```
	// correct
	i = 1;

	// correct
	if (i == 1)

	// correct
	for (int i = 0; i < 10; i++)

```

**These are incorrect**
```
	// Bad
	i=1;

	// Bad
	if (i==1)

	// Bad
	for(int i=0;i<10;i++)

```

## Line Breaks After Equals Sign in assignment 

It is ugly to have a line break after the equal sign on an assignment expression, especially when followed by unaligned lines.  It does not read easily, so don't do it.

**Do not do this:**

```

            bool overallStatus = 
                statistics.dataCollectorRunning &&
                statistics.dataInterchangeRunning;
```

**It should be this:**

```

            bool overallStatus = statistics.dataCollectorRunning &&
                                 statistics.dataInterchangeRunning;

```

# Function Definitions with Multiple Parameters

When you have a function whose definition contains parameters that cause the length of the line to exceed the width of a regular screen, then the parameters after the first should be on their own lines, aligned with the column of the first character of the first parameter.

This is bad:

```
        public async Task<bool> CreateLotParcelLiftStiffnessPNG(System.IO.Stream stream, DateTime? minimumDateTime = null, DateTime? maximumDateTime = null, ParcelImageValueMode imageValueMode = DEFAULT_PARCEL_IMAGE_VALUE_MODE, int minimumPassesForColoring = MINIMUM_PARCEL_LIFT_PASS_COUNT_FOR_MV_COLOR_ASSIGNMENT)
```

This is good:

```

        public async Task<bool> CreateLotParcelLiftStiffnessPNG(System.IO.Stream stream, 
                                                                DateTime? minimumDateTime = null, 
                                                                DateTime? maximumDateTime = null,
                                                                ParcelImageValueMode imageValueMode = DEFAULT_PARCEL_IMAGE_VALUE_MODE,
                                                                int minimumPassesForColoring = MINIMUM_PARCEL_LIFT_PASS_COUNT_FOR_MV_COLOR_ASSIGNMENT)
```

# Invoking Functions With Multiple Parameters

When calling a function that has a lot of parameters, it is better to use the named parameter pattern because it is clearer.

Note in the following example that that this function wraps its parameters on multiple lines, and the function invoked in the function uses the named parameter pattern.

```
        async Task<Compactica.LotProcessing.Data.Lot> ILotDataProvider.GetLotAsync(Guid lotGuid,
                                                                                   DateTime? minimumDateTime,
                                                                                   DateTime? maximumDateTime,
                                                                                   bool includeLifts,
                                                                                   bool includeParcels,
                                                                                   ILotDataProvider.ParcelLiftPassOptions parcelLiftPassOptions,
                                                                                   bool includeParcelLiftPassRollerDataGuids)
        {
            List<Compactica.LotProcessing.Data.Lot> outputLots = await LoadLotDataAsync(lotGuid: lotGuid,
                                                                                        minimumDateTime: minimumDateTime,
                                                                                        maximumDateTime: maximumDateTime,
                                                                                        includeLifts: includeLifts,
                                                                                        includeParcels: includeParcels,
                                                                                        parcelLiftPassOptions: parcelLiftPassOptions,
                                                                                        includeParcelLiftPassRollerDataGuids: includeParcelLiftPassRollerDataGuids);

            if (outputLots != null && outputLots.Count == 1)
            {
                return outputLots[0];
            }
            else
            {
                return null;
            }
        }


```


# C# Project Settings

For each C# project, `Implicit Usings` and `Nullable` types should be disabled.  

- 'ImplicitUsings' makes references less clear, so it is to be disabled.
- 'Nullable' just complicates the use of object types with needless further annotation, so it is to be disabled.

In project files, configure the settings as follows:

```
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
```

# 3rd Party Packages in C# or TS

Do not introduce new 3rd party dependencies into either C# or TypeScript code without approval by the senior team.

# 3rd Party Web APIs

Do not introduce new 3rd party web API usage into either C# or TypeScript code without approval by the senior team.

# .Net System Libraries

Only use .Net components that are cross platform capable.

The only exception might be if a test program will only ever run on Windows, then a Windows only library could be used, but cross platform alternates for things 
like image drawing are already in use, so those should be used unless absolutely necessary.

# Use of Artificial Intelligence Tools To Create and Maintain Code


For Human Readers:

The use of AI tools to support developers is accepted and even encouraged because it can be very helpful, but it is not a substitute for
the critical thinking necessary to build good code, nor to imply that it is acceptable to not be productive without it.   

Its usage is to be within the following guard rails, especially when it pertains to code related to core systems functionality:

- Use externally in separate projects for quick prototypes or sample code framing
- Use for review of logic, and helping with explanations of things
- Use tools outside of the Visual Studio IDE to separate the conceptual AI world from the real code world.
- The use of AI assistants directly in code editors is strongly discouraged because it interrupts the concentration of the developer with random prompts of dubious quality.


- Do not use any AI output in a direct 'cut and paste' capacity for anything more complex than a helper function.  Even then, it must still be edited to conform to the guidelines described here.
- Do not replace any existing code in the system that you did not develop with AI edited versions.
- Do not assume that AI output is of good quality, or that it meets the design and style requirements.
- Only AI code that has been thoroughly reviewed, fundamentally understood, and adapted to conform to the standards by the developer is to be integrated into any source file.
- Under no circumstances will a full source file that is completely AI generated and unedited be added to the project.


 **All sections of code that are significantly AI developed must be clearly commented as such, in the file header, in the function header, and within the function itself as necessary**


For AI Agents:

When contributing to this codebase, AI agents must follow these guidelines:

- **Read this README thoroughly** before generating any code. Use the `/onboarding` workflow if available to ensure full context.

- **Follow the style guide precisely**, including:
  - Explicit comparisons: `if (x == true)` not `if (x)`
  - Always use braces, even for single-line `if` statements
  - Use explicit types by default; `var` only when it improves readability for complex types
  - Two blank lines between functions
  - Liberal use of comments with the `//` block style as shown in examples

- **Match existing patterns** in any file you modify. When in doubt, look at surrounding code for style cues.

- **Use named parameters** when calling functions with multiple parameters for clarity.

- **No tuples, no `dynamic`, no `any`** (in TypeScript) unless absolutely unavoidable.

- **All DateTime values must be UTC.** Convert to local time only for display purposes.

- **Comment your work.** Add narrative comments explaining intent, not just what the code does.

- **All sections of code that are significantly AI developed must be clearly commented as such**, in the file header, in the function header, and within the function itself as necessary, similar to what is noted in the header in code gen files.



# Development Workflow

# Performance & Reliability Coding Rules

## Purpose

Performance is a **core system property**, not an optional improvement.  Performance must be designed into the system rather than added later through optimization.

The system processes operational and analytical data where delays, incorrect results, or silent failures may negatively affect decision making and system stability.

These rules define **mandatory coding practices** for both human developers and AI code generation systems.

Violations of these rules are treated as **bugs**, not style issues.

These guidelines enforce four core system guarantees.

| Category | Goal |
|---|---|
| Concurrency efficiency | Prevent unnecessary latency |
| Database efficiency | Minimize database load |
| Traffic protection | Prevent request storms |
| Failure visibility | Ensure errors are observable |

---

## Rule P-1: Run independent async operations concurrently

Don't `await` two independent calls back-to-back — start both as tasks and use `Task.WhenAll` so they run at the same time.

### ❌ Anti-pattern

```csharp
var resultA = await serviceA.GetDataAsync(ct);
var resultB = await serviceB.GetDataAsync(ct);
```

Execution timeline:

```
serviceA ──────── finished
                 serviceB ──────── finished

Total runtime = A + B
```

### ✅ Correct pattern

```csharp
Task<DataA> taskA = serviceA.GetDataAsync(ct);
Task<DataB> taskB = serviceB.GetDataAsync(ct);

await Task.WhenAll(taskA, taskB);

DataA resultA = await taskA;
DataB resultB = await taskB;
```

Execution timeline:

```
serviceA ──────── finished
serviceB ──────── finished

Total runtime = max(A, B)
```

### When sequential execution is correct

Sequential awaits are appropriate when:

- the second operation depends on the result of the first
- operations must run in strict order
- concurrency could introduce race conditions

Concurrency must never break logical correctness.

---

## Rule P-2: Avoid multiple database queries when one will do

Never fire a `CountAsync` just to log a number and then immediately retrieve the record — that is two round-trips to the database where one is enough.

### ❌ Anti-pattern

```csharp
int count = await query.CountAsync(ct);

var item = await query
    .OrderByDescending(x => x.CreatedAt)
    .FirstOrDefaultAsync(ct);
```

Database round trips:
- Query 1 → `COUNT(*)`
- Query 2 → `SELECT TOP 1`

### ✅ Correct pattern

```csharp
var item = await query
    .OrderByDescending(x => x.CreatedAt)
    .FirstOrDefaultAsync(ct);

bool exists = item != null;
```

One query returns both the record and the existence check.

### When CountAsync is appropriate

Use `CountAsync` only when the actual count value is required, such as pagination metadata, analytics, or reporting queries.

---

## Rule P-3: Heavy endpoints must be rate-limited

Any endpoint that does real work needs a `[RateLimit]` attribute — if it queries the database or crunches data, it is heavy.

An endpoint is heavy if it performs multiple database queries, aggregates large datasets, or triggers computational work beyond simple lookups.

Rate limiting protects the system from accidental traffic spikes, automated request loops, denial-of-service amplification, and misconfigured integrations.

```csharp
[RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
```

### Suggested limits

| Endpoint type | Suggested limit |
|---|---|
| Lightweight lookup | `ThirtyPerMinute` |
| Medium computation | `ThirtyPerMinute` |
| Heavy computation | `TenPerMinute` |

When the expected cost is unknown, prefer the more conservative limit.

### When rate limiting may not be required

Rate limiting may be unnecessary when the endpoint is internal service-to-service, upstream infrastructure already enforces throttling, or the operation is extremely lightweight.

---

## Rule P-4: Avoid unnecessary collection re-enumeration

Don't `ToList()` from EF and then `Select()` in memory — push the projection into the query so the database does it once.

### ❌ Anti-pattern

```csharp
List<Entity> entities = await db.Entities
    .Where(...)
    .ToListAsync(ct);

List<ViewModel> result = entities
    .Select(x => new ViewModel
    {
        Id    = x.Id,
        Value = x.Value
    })
    .ToList();
```

Problems: unnecessary memory allocation, extra iteration, inefficient data flow.

### ✅ Correct pattern

```csharp
List<ViewModel> result = await db.Entities
    .Where(...)
    .Select(x => new ViewModel
    {
        Id    = x.Id,
        Value = x.Value
    })
    .ToListAsync(ct);
```

Projection occurs directly in the query and the collection is materialized once.

### When in-memory projection is acceptable

Projection after materialization is acceptable when the transformation cannot be translated to SQL, complex business logic is required, or external services are involved.

---

## Rule P-5: Do not hide failures

Do not swallow exceptions by returning empty or default objects as successful results.

Failures must either propagate to the caller, be logged and rethrown with context, or return a clearly marked partial result.

### ❌ Anti-pattern

```csharp
private async Task<ResultModel> CalculateAsync(...)
{
    try
    {
        return await calculator.ComputeAsync(...);
    }
    catch (Exception ex)
    {
        logger.LogError(ex);
        return new ResultModel();
    }
}
```

The caller receives incorrect data while the operation actually failed.

### ✅ Correct pattern

Allow exceptions to propagate.

```csharp
private async Task<ResultModel> CalculateAsync(...)
{
    return await calculator.ComputeAsync(...);
}
```

### Acceptable exception handling

Exceptions may be caught when additional diagnostic context is added before rethrowing, the exception is translated into a domain-specific error, or partial results are intentionally returned and clearly documented.

Silent failure is not acceptable.

---

## Rule P-6: Query data only where it is required

Don't query data before you know you need it — move database calls inside the block where they are actually used.

### ❌ Anti-pattern

```csharp
var parent = await db.Parents
    .FirstOrDefaultAsync(x => x.Id == id, ct);

var latest = await db.Children
    .Where(x => x.ParentId == id)
    .MaxAsync(x => x.CreatedAt, ct);

var snapshot = await db.Snapshots
    .FirstOrDefaultAsync(..., ct);

if (snapshot != null && snapshot.Timestamp >= latest)
{
    return snapshot;
}
```

Several queries execute even when unnecessary.

### ✅ Correct pattern

```csharp
var snapshot = await db.Snapshots
    .FirstOrDefaultAsync(..., ct);

if (snapshot != null)
{
    var latest = await db.Children
        .Where(x => x.ParentId == id)
        .MaxAsync(x => x.CreatedAt, ct);

    if (snapshot.Timestamp >= latest)
    {
        return snapshot;
    }
}
```

Queries execute only when required.

---

## Rule P-7: Cache results of expensive DB operations and guard against concurrent duplicate execution

Endpoints that perform any meaningful database work should cache their results in a RAM cache with an application-appropriate expiration to eliminate redundant work on repeated calls.

For concurrent-safe caching, use a `SemaphoreSlim` keyed on the cache key.  Upon attaining the semaphore, **check the cache again** before executing the query — this double-check eliminates duplicate DB calls from multiple requests that arrived simultaneously before the cache was populated.

### ❌ Anti-pattern

```csharp
// No cache — every request hits the database
public async Task<SummaryData> GetSummaryAsync(Guid projectGuid, CancellationToken ct)
{
    return await db.ProjectMetricSnapshots
        .Where(s => s.projectGuid == projectGuid)
        .FirstOrDefaultAsync(ct);
}
```

### ✅ Correct pattern

```csharp
//
// Semaphore keyed per project to prevent concurrent duplicate queries
//
private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _summaryLockMap = new();

public async Task<SummaryData> GetSummaryAsync(Guid projectGuid, CancellationToken ct)
{
    string cacheKey = $"summary:{projectGuid}";

    //
    // Fast path — return from cache if available
    //
    if (_summaryCache.TryGetValue(cacheKey, out SummaryData cached) == true)
    {
        return cached;
    }

    //
    // Obtain a per-project semaphore to serialize concurrent callers for the same key
    //
    SemaphoreSlim semaphore = _summaryLockMap.GetOrAdd(projectGuid, _ => new SemaphoreSlim(1, 1));

    await semaphore.WaitAsync(ct);

    try
    {
        //
        // Double-check: another caller may have populated the cache while we were waiting
        //
        if (_summaryCache.TryGetValue(cacheKey, out cached) == true)
        {
            return cached;
        }

        //
        // Cache was empty — execute the query and store the result
        //
        SummaryData result = await db.ProjectMetricSnapshots
            .Where(s => s.projectGuid == projectGuid)
            .FirstOrDefaultAsync(ct);

        _summaryCache.Add(cacheKey, result, expiry: TimeSpan.FromMinutes(5));

        return result;
    }
    finally
    {
        semaphore.Release();
    }
}
```

### Cache expiration guidance

Choose an expiry that reflects how stale the data can be for the use case:

| Data type | Suggested expiry |
|---|---|
| Snapshot / aggregated metrics | 1–5 minutes |
| Project configuration / settings | 5–15 minutes |
| Reference data (rarely changes) | 30–60 minutes |
| Real-time sensor data | No cache or ≤ 30 seconds |

Use `ExpiringCache` from the Compactica base library where available — it wraps this pattern consistently.

---

## Common Performance Anti-Patterns

Avoid the following patterns unless there is a clear architectural reason.

| Pattern | Avoid |
|---|---|
| Sequential async calls | `await A(); await B();` |
| Double enumeration | `ToList() → Select() → ToList()` |
| Redundant database queries | `CountAsync() + FirstOrDefaultAsync()` |
| Hidden failures | `catch → return default` |
| Preloading unused data | Query data before the condition that determines if it is needed |
| No caching on repeated queries | Re-querying the DB on every request for data that doesn't change frequently |

---

## AI Code Generation Checklist

Every AI-generated change must be reviewed against this checklist.

**Concurrency**
- [ ] Are independent async calls executed concurrently?
- [ ] Are unnecessary sequential awaits avoided?

**Database efficiency**
- [ ] Are unnecessary database round trips avoided?
- [ ] Is projection performed inside the query when possible?
- [ ] Is data fetched only when required?
- [ ] Does the query avoid loading unnecessary columns or entities?
- [ ] Is the result cached with an appropriate expiry to avoid repeated identical queries?
- [ ] Is a semaphore double-check used where concurrent callers could trigger duplicate DB calls?

**Traffic protection**
- [ ] Does any heavy endpoint include rate limiting?
- [ ] Could the endpoint accidentally trigger excessive load?

**Failure visibility**
- [ ] Are exceptions allowed to propagate?
- [ ] Are silent default returns avoided?

**Memory & iteration**
- [ ] Are collections enumerated only once when possible?
- [ ] Are unnecessary `.ToList()` calls avoided?

---

## Design Principles

When generating or reviewing code, prefer designs that are:

| Property | Meaning |
|---|---|
| Observable | Failures are visible |
| Predictable | Performance remains stable under load |
| Efficient | Database and CPU work are minimized |
| Safe | Traffic spikes cannot overload the system |

---

## AI Code Generation Rule

When generating code, prefer solutions that:

- minimize database round trips
- execute independent work concurrently
- avoid unnecessary memory allocations
- surface failures rather than hiding them

If multiple implementations are possible, prefer the one that:

1. performs fewer database queries
2. performs fewer iterations
3. reduces blocking or sequential async execution

---

## Git Long File Name Handling - DO THIS FIRST

To handle long file names in Git on Windows, ensure that Git is configured to support long paths. You can enable this by running the following command in your Git Bash or command prompt:
```bash

git config --global core.longpaths true

```

Make windows support long filenames

 Direct Registry Edit (Works on Windows 11 Home)

Press Win + R, type regedit, and press Enter.

Navigate to:textHKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem

Find the DWORD value named LongPathsEnabled.

If it exists, double-click it and set its value to 1.
If it does not exist, right-click in the right pane → New → DWORD (32-bit) Value → name it LongPathsEnabled → set value to 1.



## Branching Strategy
- **`master`**: Represents the current released version of the Compactica product.
- **`develop`**: Integration branch for ongoing development.
- **Feature branches**: Created off `develop` for tasks (e.g., `feature/task-description`).
- **Hotfix branches**: Created off `master` for critical production fixes (e.g., `hotfix/bug-description`).

## Process
1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/task-description
   ```


## Development Workflow

Commit and push changes to the feature branch.

Create a pull request to merge into develop.

PRs require at least one approval and passing CI checks.

After merging into develop, the feature branch is deleted.

Periodically, develop is merged into master for a new release via a PR.

Only admins can merge or push to master.

Bringing your local branch up to current develop level is to follow a rebase process

Refer to CONTRIBUTING.MD for more information

## Restrictions

Direct pushes to master are restricted to admins.

PRs to develop and master require approvals and passing tests.




  # GENERAL CODE STRUCTURE

    A single line of code should contain a piece of work in the objective of the function.  Avoid doing multiple things in one line because it can affect the ability to quickly interpret the goal.

    Remember that the audience is the programmer, and their time is valuable.




  # GENERAL NAMING PRACTICES FOR ALL LANGUAGES:

    - variable, function, and classes should be named so their purpose is easily understood.  The priority is on clarity and readability, not compactness. 

        - A notable exception is index variables for non-nested loops can use the 'int i' pattern because that is sufficiently clear in and of itself, or within very simple LINQ 
          expressions when naming a working variable name,
        



    - private class level variables should be prefixed with an underscore.

    - function level variables should be camel case named

    - constants should be capitalized.

    - when contributing to an existing source code file, you should add code, comments, indenting and white space using the style that is already in place.

    - personal details such as your name or initials should only be added to comments sparingly, and if personal attribution is relevant to the work.  Version history can tell the same story with more detail.

    - ticket numbers related to code changes should be used sparingly as well, and should be justifiable in the context of work being done.

    - code indenting and white space styles can be compliant to the generally accepted patterns for each language used, however, that is secondary to the Compactica style guidelines.






  # COMPILER OUTPUT:

    - If the compiler generates a warning for code that is being added, then it should be addressed and fixed.

    - Code analysis should be done on a periodic basis (when efficiencies are looking to be found) using the Visual Studio analysis and profiling tools to identify possible weak spots, and areas for improvement, 
      in performance and memory usage

    

  # Source Control Practices:
    
    Here is a link to a GIT pattern that we will use at Compactica:

    https://barro.github.io/2016/02/a-succesful-git-branching-model-considered-harmful/

    Basics:
        
        1.) all work will be done in a branch from the master branch.
            - each package of work assigned will be done in its own branch,
            - multiple check-ins to the branch may occur while work is in progress.
            - for long lived branches, the current state of master can be fetched as necessary if the foundation changes enough to merit it.

        2.) When a branch is complete, a pull request will be created, and reviewed, and feedback may be provided for review and updating in the working branch.
    
        3.) Once PR is approved, work done in the work branch will be merged into the master branch.

	# Branch Naming:
	
	Basic pattern for bug fix things is this, grouped either by a fix or a feature as the category of the branch.  use hotfix or feature, followed by a slash, then your initials, then the date of the branch creation, and then a description of it.  For example:
 
		hotfix/dk-aug-7-2025-improvements-to-bcdc-health-check
 
		feature/dk-aug-9-2025-new-thing-being-added

 
