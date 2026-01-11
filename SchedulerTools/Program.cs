using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;

namespace Foundation.Scheduler.CodeGeneration
{
    /// <summary>
    /// 
    /// The purpose of this program is to create baseline code for the Scheduler system
    /// 
    /// The entity model needs to be created using the EF Core Power Tools
    /// 
    /// Each time the schema changes, this needs to be rebuilt.
    /// 
    /// 
    /// General comments about code generation:
    /// 
    /// 1.) The schema creation will create script files for 4 types of databases.  The relevant ones should be copied into the Quarterback main project so that they are easy to find, and will be source controlled.
    ///     The copy in the Quarterback project should be updated whenever the Quarterback schema changes.
    /// 
    /// 2.) The application code generator creates 2 things.  
    /// 
    ///     The first is a bunch of API controllers that need to go into the Quarterback project.
    /// 
    ///     The second is the 'EntityExtensions' output, which needs to be added to this project because it is a bunch of partial class extensions to data classes that are in this project.
    ///     
    /// 3.) The database project needs to be referenced by the other project so that they gets the database context.
    /// 
    /// </summary>
    internal class Program
    {
        private static string _outputDirectory;

        static void Main()
        {
            bool exit = false;

            _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output");

            Console.Clear();

            while (!exit)
            {
                ShowMenu();

                // Read user input
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();  // Move to the next line after the key press

                // Handle the user's input
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        GenerateSchedulerDatabaseScriptCode();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateSchedulerApplicationCode();
                        break;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:

                        //
                        // This creates a petty harbour tenant, and configures the scheduler database to have an instance for Petty Harbour usage
                        //
                        ConfigurePettyHarbour();

                        break;


                    case ConsoleKey.X:
                        exit = true;
                        Console.WriteLine("Exiting...");
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Invalid selection, please try again.");
                        ShowMenu();
                        break;
                }
            }
        }

        private static void ConfigurePettyHarbour()
        {
            Guid PHMCTenantGuid = Guid.Parse("d58f56c6-e3fb-4d3b-80b3-7053c66491e3");
            //
            // Create the tenant in the security module if it doesn't exist
            //
            Security.Database.SecurityTenant phmcTenant;

            using (SecurityContext sc = new SecurityContext())
            { 
                phmcTenant = (from st in sc.SecurityTenants where st.objectGuid == PHMCTenantGuid select st).FirstOrDefault();

                if (phmcTenant == null)
                {
                    phmcTenant = new SecurityTenant();

                    phmcTenant.name = "Petty Harbour Maddox Cove";
                    phmcTenant.description = "The town of Petty Harbour Maddox Cove";

                    phmcTenant.objectGuid = PHMCTenantGuid;

                    phmcTenant.active = true;
                    phmcTenant.deleted = false;

                    sc.SecurityTenants.Add(phmcTenant);

                    sc.SaveChanges();
                }
            }

            //
            // Setup the Scheduler
            //
            using (SchedulerContext context = new SchedulerContext())
            {

                Country canada = (from c in context.Countries where c.name == "Canada" select c).FirstOrDefault();

                if (canada == null)
                {
                    canada = new Country();

                    canada.name = "Canada";
                    canada.description = "Canada";
                    canada.abbreviation = "CA";

                    canada.sequence = 1;

                    canada.objectGuid = Guid.NewGuid();
                    canada.active = true;
                    canada.deleted = false;

                    context.Countries.Add(canada);

                    context.SaveChanges();  
                }


                StateProvince nlProvince = (from sp in context.StateProvinces where sp.abbreviation == "NL" && 
                                                                                    sp.countryId == canada.id 
                                            select sp).FirstOrDefault();

                if (nlProvince == null)
                {
                    nlProvince = new StateProvince();

                    nlProvince.name = "Newfoundland";
                    nlProvince.description = "Newfoundland and Labrador";
                    nlProvince.abbreviation = "NL";
                    nlProvince.countryId = canada.id;
                    nlProvince.sequence = 1;

                    nlProvince.objectGuid = Guid.NewGuid();
                    nlProvince.active = true;
                    nlProvince.deleted = false;

                    context.StateProvinces.Add(nlProvince);

                    context.SaveChanges();
                }



                Database.TimeZone nlTimeZone = (from tz in context.TimeZones
                                            where tz.ianaTimeZone == "America/St_Johns"
                                                select tz).FirstOrDefault();

                if (nlTimeZone == null)
                {
                    nlTimeZone = new Database.TimeZone();

                    nlTimeZone.name = "St. John's";
                    nlTimeZone.description = "St. John's Newfoundland, Canada";
                    nlTimeZone.ianaTimeZone = "America/St_Johns";
                    nlTimeZone.abbreviation = "NDT";
                    nlTimeZone.standardUTCOffsetHours = -3.5f;
                    nlTimeZone.dstUTCOffsetHours = -2.5f;
                    nlTimeZone.supportsDaylightSavings = true;

                    nlTimeZone.abbreviationDaylightSavings = "NST";

                    nlTimeZone.sequence = 1;

                    //nlTimeZone.tenantGuid = PHMCTenantGuid;
                    nlTimeZone.objectGuid = Guid.NewGuid();
                    nlTimeZone.active = true;
                    nlTimeZone.deleted = false;

                    context.TimeZones.Add(nlTimeZone);

                    context.SaveChanges();
                }



                TenantProfile tenantProfile = (from tp in context.TenantProfiles where tp.objectGuid == PHMCTenantGuid &&
                                                                                       tp.tenantGuid == PHMCTenantGuid
                                               select tp).FirstOrDefault();

                if (tenantProfile == null)
                {
                    tenantProfile = new TenantProfile();

                    tenantProfile.name = "Petty Harbour Maddox Cove";
                    tenantProfile.description = "The town of Petty Harbour Maddox Cove";

                    tenantProfile.addressLine1 = "35 Main Road";
                    tenantProfile.postalCode = "A0A 3H0";
                    tenantProfile.city = "Petty Harbour";

                    tenantProfile.stateProvinceId = nlProvince.id;
                    tenantProfile.countryId = nlProvince.countryId;

                    tenantProfile.phoneNumber = " 709 368 3959";

                    tenantProfile.website = "www.pettyharbourmaddoxcove.ca";

                    tenantProfile.versionNumber = 0;

                    tenantProfile.tenantGuid = PHMCTenantGuid;
                    tenantProfile.objectGuid = PHMCTenantGuid;      // make this the same as the tenant guid
                    tenantProfile.active = true;
                    tenantProfile.deleted = false;


                    context.TenantProfiles.Add(tenantProfile);

                    context.SaveChanges();
                }


                //
                // Prioirites
                //
                Priority highPriority = (from p in context.Priorities where p.name == "High" && p.tenantGuid == PHMCTenantGuid select p).FirstOrDefault();
                Priority mediumPriority = (from p in context.Priorities where p.name == "Medium" && p.tenantGuid == PHMCTenantGuid select p).FirstOrDefault();
                Priority lowPriority = (from p in context.Priorities where p.name == "Low" && p.tenantGuid == PHMCTenantGuid select p).FirstOrDefault();

                if (highPriority == null)
                {
                    //
                    // Buld 3 priorities if high does not exist. 
                    //
                    highPriority = new Priority();
                    mediumPriority = new Priority();
                    lowPriority = new Priority();

                    highPriority.name = "High";
                    highPriority.description = "High Priority";
                    highPriority.sequence = 1;
                    highPriority.color = "#2BF164";
                    highPriority.tenantGuid = PHMCTenantGuid;
                    highPriority.active = true;
                    highPriority.deleted = false;
                    highPriority.objectGuid = Guid.NewGuid();


                    mediumPriority.name = "Medium";
                    mediumPriority.description = "Medium Priority";
                    mediumPriority.color = "#F1A82C";
                    mediumPriority.sequence = 2;
                    mediumPriority.tenantGuid = PHMCTenantGuid;
                    mediumPriority.active = true;
                    mediumPriority.deleted = false;
                    mediumPriority.objectGuid = Guid.NewGuid();


                    lowPriority.name = "Low";
                    lowPriority.description = "Low Priority";
                    lowPriority.sequence = 3;
                    lowPriority.color = "#98948F";
                    lowPriority.tenantGuid = PHMCTenantGuid;
                    lowPriority.active = true;
                    lowPriority.deleted = false;
                    lowPriority.objectGuid = Guid.NewGuid();

                    context.Priorities.Add(highPriority);
                    context.Priorities.Add(mediumPriority);
                    context.Priorities.Add(lowPriority);

                    context.SaveChanges();
                }



                //
                // Calendars
                //
                Calendar recCommitteeCalendar = (from c in context.Calendars where c.name == "Recreation" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                

                if (recCommitteeCalendar == null)
                {
                    recCommitteeCalendar = new Calendar();

                    recCommitteeCalendar.name = "Recreation";
                    recCommitteeCalendar.description = "Recreation Events";


                    recCommitteeCalendar.iconId = 10;           // room
                    recCommitteeCalendar.color = "#fb0909";

                    recCommitteeCalendar.isDefault = false;
                    recCommitteeCalendar.versionNumber = 0;

                    recCommitteeCalendar.tenantGuid = PHMCTenantGuid;
                    recCommitteeCalendar.objectGuid = Guid.NewGuid();
                    recCommitteeCalendar.active = true;
                    recCommitteeCalendar.deleted = false;


                    context.Calendars.Add(recCommitteeCalendar);
                }

                Calendar communityCalendar = (from c in context.Calendars where c.name == "Community" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                if (communityCalendar == null)
                {
                    communityCalendar = new Calendar();

                    communityCalendar.name = "Community";
                    communityCalendar.description = "Community Events";


                    communityCalendar.iconId = 8;           // vehicle
                    communityCalendar.color = "#06b131";

                    communityCalendar.isDefault = true;
                    communityCalendar.versionNumber = 0;

                    communityCalendar.tenantGuid = PHMCTenantGuid;
                    communityCalendar.objectGuid = Guid.NewGuid();
                    communityCalendar.active = true;
                    communityCalendar.deleted = false;


                    context.Calendars.Add(communityCalendar);

                    context.SaveChanges();
                }


                Calendar councilCalendar = (from c in context.Calendars where c.name == "Council" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                if (councilCalendar == null)
                {
                    councilCalendar = new Calendar();

                    councilCalendar.name = "Council";
                    councilCalendar.description = "Council Meetings";


                    councilCalendar.iconId = 8;           // vehicle
                    councilCalendar.color = "#06b131";

                    councilCalendar.isDefault = true;
                    councilCalendar.versionNumber = 0;

                    councilCalendar.tenantGuid = PHMCTenantGuid;
                    councilCalendar.objectGuid = Guid.NewGuid();
                    councilCalendar.active = true;
                    councilCalendar.deleted = false;

                    context.Calendars.Add(councilCalendar);

                    context.SaveChanges();
                }



                //
                // Resource Types
                //
                
                ResourceType townWorkerResourceType = (from rt in context.ResourceTypes where rt.name == "Worker" && rt.tenantGuid == PHMCTenantGuid select rt).FirstOrDefault();
                if (townWorkerResourceType == null)
                {
                    townWorkerResourceType = new ResourceType();

                    townWorkerResourceType.name = "Worker";
                    townWorkerResourceType.description = "Town Worker";

                    townWorkerResourceType.sequence = 1;       

                    townWorkerResourceType.iconId = 4;
                    townWorkerResourceType.color = "#2889f0";

                    townWorkerResourceType.tenantGuid = PHMCTenantGuid;
                    townWorkerResourceType.objectGuid = Guid.NewGuid();
                    townWorkerResourceType.active = true;
                    townWorkerResourceType.deleted = false;


                    context.ResourceTypes.Add(townWorkerResourceType);

                    context.SaveChanges();
                }


                ResourceType councilorResourceType = (from rt in context.ResourceTypes where rt.name == "Councilor" && rt.tenantGuid == PHMCTenantGuid select rt).FirstOrDefault();
                if (councilorResourceType == null)
                {
                    councilorResourceType = new ResourceType();

                    councilorResourceType.name = "Councilor";
                    councilorResourceType.description = "Town Councilor";

                    councilorResourceType.sequence = 2;

                    councilorResourceType.iconId = 3;
                    councilorResourceType.color = "#f29a36";

                    councilorResourceType.tenantGuid = PHMCTenantGuid;
                    councilorResourceType.objectGuid = Guid.NewGuid();
                    councilorResourceType.active = true;
                    councilorResourceType.deleted = false;


                    context.ResourceTypes.Add(councilorResourceType);

                    context.SaveChanges();
                }


                ResourceType recCommitteeResourceType = (from rt in context.ResourceTypes where rt.name == "Rec Committee" && rt.tenantGuid == PHMCTenantGuid select rt).FirstOrDefault();
                
                
                if (recCommitteeResourceType == null)
                {
                    recCommitteeResourceType = new ResourceType();

                    recCommitteeResourceType.name = "Rec Committee";
                    recCommitteeResourceType.description = "Rec Committee Volunteer";

                    recCommitteeResourceType.sequence = 3;

                    recCommitteeResourceType.iconId = 10;
                    recCommitteeResourceType.color = "#febebe";

                    recCommitteeResourceType.tenantGuid = PHMCTenantGuid;
                    recCommitteeResourceType.objectGuid = Guid.NewGuid();
                    recCommitteeResourceType.active = true;
                    recCommitteeResourceType.deleted = false;


                    context.ResourceTypes.Add(recCommitteeResourceType);

                    context.SaveChanges();
                }

                ResourceType mayorResourceType = (from rt in context.ResourceTypes where rt.name == "Mayor" && rt.tenantGuid == PHMCTenantGuid select rt).FirstOrDefault();
                if (mayorResourceType == null)
                {
                    mayorResourceType = new ResourceType();

                    mayorResourceType.name = "Mayor";
                    mayorResourceType.description = "Town Mayor";

                    mayorResourceType.sequence = 4;

                    mayorResourceType.iconId = 1;
                    mayorResourceType.color = "#f9f10b";


                    mayorResourceType.tenantGuid = PHMCTenantGuid;
                    mayorResourceType.objectGuid = Guid.NewGuid();
                    mayorResourceType.active = true;
                    mayorResourceType.deleted = false;


                    context.ResourceTypes.Add(mayorResourceType);

                    context.SaveChanges();
                }



                ResourceType vehicleResourceType = (from rt in context.ResourceTypes where rt.name == "Vehicle" && rt.tenantGuid == PHMCTenantGuid select rt).FirstOrDefault();


                if (vehicleResourceType == null)
                {
                    vehicleResourceType = new ResourceType();

                    vehicleResourceType.name = "Vehicle";
                    vehicleResourceType.description = "Vehicle";

                    vehicleResourceType.sequence = 5;

                    vehicleResourceType.iconId = 8;             // vehcile
                    vehicleResourceType.color = "#2a8479";

                    vehicleResourceType.tenantGuid = PHMCTenantGuid;
                    vehicleResourceType.objectGuid = Guid.NewGuid();
                    vehicleResourceType.active = true;
                    vehicleResourceType.deleted = false;


                    context.ResourceTypes.Add(vehicleResourceType);

                    context.SaveChanges();
                }


                //
                // Assignment Roles
                //
                AssignmentRole staffRole = (from ar in context.AssignmentRoles where ar.name == "Staff" && ar.tenantGuid == PHMCTenantGuid select ar).FirstOrDefault();
                if (staffRole == null)
                {
                    staffRole = new AssignmentRole();

                    staffRole.name = "Staff";
                    staffRole.description = "Staff Member";

                    staffRole.sequence = 1;

                    staffRole.iconId = 28;          // wrench
                    staffRole.color = "#6389e3";


                    staffRole.tenantGuid = PHMCTenantGuid;
                    staffRole.objectGuid = Guid.NewGuid();
                    staffRole.active = true;
                    staffRole.deleted = false;


                    context.AssignmentRoles.Add(staffRole);

                    context.SaveChanges();
                }


                AssignmentRole volunteerRole = (from ar in context.AssignmentRoles where ar.name == "Volunteer" && ar.tenantGuid == PHMCTenantGuid select ar).FirstOrDefault();
                if (volunteerRole == null)
                {
                    volunteerRole = new AssignmentRole();

                    volunteerRole.name = "Volunteer";
                    volunteerRole.description = "Volunteer";

                    volunteerRole.sequence = 2;

                    volunteerRole.iconId = 29;          // screwdriver
                    volunteerRole.color = "#00b825";

                    volunteerRole.tenantGuid = PHMCTenantGuid;
                    volunteerRole.objectGuid = Guid.NewGuid();
                    volunteerRole.active = true;
                    volunteerRole.deleted = false;

                    context.AssignmentRoles.Add(volunteerRole);

                    context.SaveChanges();
                }


                //
                // Client Types
                //
                ClientType phmcClientType = (from ct in context.ClientTypes where ct.name == "Petty Harbour Maddox Cove" && ct.tenantGuid == PHMCTenantGuid select ct).FirstOrDefault();
                if (phmcClientType == null)
                {
                    phmcClientType = new ClientType();

                    phmcClientType.name = "Petty Harbour Maddox Cove";
                    phmcClientType.description = "Petty Harbour Maddox Cove";

                    phmcClientType.sequence = 1;

                    phmcClientType.iconId = 14;          // Home
                    phmcClientType.color = "#393ed5";

                    phmcClientType.tenantGuid = PHMCTenantGuid;
                    phmcClientType.objectGuid = Guid.NewGuid();
                    phmcClientType.active = true;
                    phmcClientType.deleted = false;

                    context.ClientTypes.Add(phmcClientType);

                    context.SaveChanges();
                }


                ClientType externalClientType = (from ct in context.ClientTypes where ct.name == "External" && ct.tenantGuid == PHMCTenantGuid select ct).FirstOrDefault();
                if (externalClientType == null)
                {
                    externalClientType = new ClientType();

                    externalClientType.name = "External";
                    externalClientType.description = "External Client - Not Petty Harbour Maddox Cove";

                    externalClientType.sequence = 2;

                    externalClientType.iconId = 73;          // Cloud
                    externalClientType.color = "#8a51b8";

                    externalClientType.tenantGuid = PHMCTenantGuid;
                    externalClientType.objectGuid = Guid.NewGuid();
                    externalClientType.active = true;
                    externalClientType.deleted = false;

                    context.ClientTypes.Add(externalClientType);

                    context.SaveChanges();
                }


                SchedulingTargetType residentialTargetType = (from stt in context.SchedulingTargetTypes where stt.name == "Residential" && stt.tenantGuid == PHMCTenantGuid select stt).FirstOrDefault();
                if (residentialTargetType == null)
                {
                    residentialTargetType = new SchedulingTargetType();

                    residentialTargetType.name = "Residential";
                    residentialTargetType.description = "Residential Address";

                    residentialTargetType.sequence = 2;

                    residentialTargetType.iconId = 14;          // Home
                    residentialTargetType.color = "#d0a225";

                    residentialTargetType.tenantGuid = PHMCTenantGuid;
                    residentialTargetType.objectGuid = Guid.NewGuid();
                    residentialTargetType.active = true;
                    residentialTargetType.deleted = false;

                    context.SchedulingTargetTypes.Add(residentialTargetType);

                    context.SaveChanges();
                }


                SchedulingTargetType municipalTargetType = (from stt in context.SchedulingTargetTypes where stt.name == "Municipal" && stt.tenantGuid == PHMCTenantGuid select stt).FirstOrDefault();
                if (municipalTargetType == null)
                {
                    municipalTargetType = new SchedulingTargetType();

                    municipalTargetType.name = "Municipal";
                    municipalTargetType.description = "Municipal Location";

                    municipalTargetType.sequence = 2;

                    municipalTargetType.iconId = 25;          // Location
                    municipalTargetType.color = "#8d3434";

                    municipalTargetType.tenantGuid = PHMCTenantGuid;
                    municipalTargetType.objectGuid = Guid.NewGuid();
                    municipalTargetType.active = true;
                    municipalTargetType.deleted = false;

                    context.SchedulingTargetTypes.Add(municipalTargetType);

                    context.SaveChanges();
                }

                SchedulingTargetType communityTargetType = (from stt in context.SchedulingTargetTypes where stt.name == "Community" && stt.tenantGuid == PHMCTenantGuid select stt).FirstOrDefault();
                if (communityTargetType == null)
                {
                    communityTargetType = new SchedulingTargetType();

                    communityTargetType.name = "Community";
                    communityTargetType.description = "Community Location";

                    communityTargetType.sequence = 3;

                    communityTargetType.iconId = 2;          // People
                    communityTargetType.color = "#56d7a6";

                    communityTargetType.tenantGuid = PHMCTenantGuid;
                    communityTargetType.objectGuid = Guid.NewGuid();
                    communityTargetType.active = true;
                    communityTargetType.deleted = false;

                    context.SchedulingTargetTypes.Add(communityTargetType);

                    context.SaveChanges();
                }



                SchedulingTargetType roadTargetType = (from stt in context.SchedulingTargetTypes where stt.name == "Road" && stt.tenantGuid == PHMCTenantGuid select stt).FirstOrDefault();
                if (roadTargetType == null)
                {
                    roadTargetType = new SchedulingTargetType();

                    roadTargetType.name = "Road";
                    roadTargetType.description = "Road";

                    roadTargetType.sequence = 4;

                    roadTargetType.iconId = 51;             // Route
                    roadTargetType.color = "#267d5c";

                    roadTargetType.tenantGuid = PHMCTenantGuid;
                    roadTargetType.objectGuid = Guid.NewGuid();
                    roadTargetType.active = true;
                    roadTargetType.deleted = false;

                    context.SchedulingTargetTypes.Add(roadTargetType);

                    context.SaveChanges();
                }



                SchedulingTargetType wharfTargetType = (from stt in context.SchedulingTargetTypes where stt.name == "Wharf" && stt.tenantGuid == PHMCTenantGuid select stt).FirstOrDefault();
                if (wharfTargetType == null)
                {
                    wharfTargetType = new SchedulingTargetType();

                    wharfTargetType.name = "Wharf";
                    wharfTargetType.description = "Wharf";

                    wharfTargetType.sequence = 5;

                    wharfTargetType.iconId = 52;             // ship / marine
                    wharfTargetType.color = "#1d2bf7";

                    wharfTargetType.tenantGuid = PHMCTenantGuid;
                    wharfTargetType.objectGuid = Guid.NewGuid();
                    wharfTargetType.active = true;
                    wharfTargetType.deleted = false;

                    context.SchedulingTargetTypes.Add(wharfTargetType);

                    context.SaveChanges();
                }


                RateType standardRateType = (from stt in context.RateTypes where stt.name == "Standard" && stt.tenantGuid == PHMCTenantGuid select stt).FirstOrDefault();
                if (standardRateType == null)
                {
                    standardRateType = new RateType();

                    standardRateType.name = "Standard";
                    standardRateType.description = "Standard Rate";

                    standardRateType.sequence = 1;

                    standardRateType.color = "#1cb02d";

                    standardRateType.tenantGuid = PHMCTenantGuid;
                    standardRateType.objectGuid = Guid.NewGuid();
                    standardRateType.active = true;
                    standardRateType.deleted = false;

                    context.RateTypes.Add(standardRateType);

                    context.SaveChanges();
                }


                Currency canadianCurrency = (from c in context.Currencies where c.name == "Canadian" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                if (canadianCurrency == null)
                {
                    canadianCurrency = new Currency();

                    canadianCurrency.name = "Canadian";
                    canadianCurrency.description = "Canadian Dollars";
                    canadianCurrency.code = "CAD";

                    canadianCurrency.isDefault = true;
                    canadianCurrency.sequence = 1;

                    canadianCurrency.color = "#1cb02d";

                    canadianCurrency.tenantGuid = PHMCTenantGuid;
                    canadianCurrency.objectGuid = Guid.NewGuid();
                    canadianCurrency.active = true;
                    canadianCurrency.deleted = false;

                    context.Currencies.Add(canadianCurrency);

                    context.SaveChanges();
                }



                //
                // Crews - Garbage Crew, Rec Committee, Town Council etc..
                //
                Crew garbageCollectionCrew = (from c in context.Crews where c.name == "Garbage" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                if (garbageCollectionCrew == null)
                {
                    garbageCollectionCrew = new Crew();

                    garbageCollectionCrew.name = "Garbage";
                    garbageCollectionCrew.description = "Garbage Collection Crew";

                    garbageCollectionCrew.iconId = 71;      // trash
                    garbageCollectionCrew.color = "#03a800";

                    garbageCollectionCrew.tenantGuid = PHMCTenantGuid;
                    garbageCollectionCrew.objectGuid = Guid.NewGuid();
                    garbageCollectionCrew.active = true;
                    garbageCollectionCrew.deleted = false;

                    context.Crews.Add(garbageCollectionCrew);

                    context.SaveChanges();
                }

                Crew townCouncilCrew = (from c in context.Crews where c.name == "Council" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                if (townCouncilCrew == null)
                {
                    townCouncilCrew = new Crew();

                    townCouncilCrew.name = "Council";
                    townCouncilCrew.description = "Town Council";

                    townCouncilCrew.iconId = 3;      // Supervisor
                    townCouncilCrew.color = "#3632f5";

                    townCouncilCrew.tenantGuid = PHMCTenantGuid;
                    townCouncilCrew.objectGuid = Guid.NewGuid();
                    townCouncilCrew.active = true;
                    townCouncilCrew.deleted = false;

                    context.Crews.Add(townCouncilCrew);

                    context.SaveChanges();
                }



                Crew recCommitteeCrew = (from c in context.Crews where c.name == "Rec Committee" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();
                if (recCommitteeCrew == null)
                {
                    recCommitteeCrew = new Crew();

                    recCommitteeCrew.name = "Rec Committee";
                    recCommitteeCrew.description = "Recreation Committee";

                    recCommitteeCrew.iconId = 46;      // heat
                    recCommitteeCrew.color = "#f63131";

                    recCommitteeCrew.tenantGuid = PHMCTenantGuid;
                    recCommitteeCrew.objectGuid = Guid.NewGuid();
                    recCommitteeCrew.active = true;
                    recCommitteeCrew.deleted = false;

                    context.Crews.Add(recCommitteeCrew);

                    context.SaveChanges();
                }


                //
                // Add DK and WK to the Contacts
                //
                Contact dkContact = (from c in context.Contacts where c.firstName == "Demetree" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();

                if (dkContact == null)
                {
                    dkContact = new Contact();

                    dkContact.firstName = "Demetree";
                    dkContact.lastName = "Kallergis";
                    dkContact.birthDate = new DateOnly(1976, 3, 1);
                    dkContact.email = "demetree@gmail.com";
                    dkContact.mobile = "289 253 8439";
                    dkContact.contactTypeId = (from ct in context.ContactTypes where ct.name == "Technical Contact" select ct.id).FirstOrDefault();

                    dkContact.tenantGuid = PHMCTenantGuid;
                    dkContact.objectGuid = Guid.NewGuid();
                    dkContact.active = true;
                    dkContact.deleted = false;

                    context.Contacts.Add(dkContact);

                    context.SaveChanges();
                }

                Contact wkContact = (from c in context.Contacts where c.firstName == "Wendy" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();

                if (wkContact == null)
                {
                    wkContact = new Contact();

                    wkContact.firstName = "Wendy";
                    wkContact.lastName = "Kallergis";
                    wkContact.birthDate = new DateOnly(1974, 4, 4);
                    wkContact.email = "wendyjames74@yahoo.ca";
                    wkContact.mobile = "905 516 9135";
                    wkContact.contactTypeId = (from ct in context.ContactTypes where ct.name == "Volunteer" select ct.id).FirstOrDefault();

                    wkContact.tenantGuid = PHMCTenantGuid;
                    wkContact.objectGuid = Guid.NewGuid();
                    wkContact.active = true;
                    wkContact.deleted = false;

                    context.Contacts.Add(wkContact);
                    context.SaveChanges();
                }

                Client phmcClient = (from c in context.Clients where c.name == "Petty Harbour Maddox Cove" && c.tenantGuid == PHMCTenantGuid select c).FirstOrDefault();

                if (phmcClient == null) 
                {
                    phmcClient = new Client();

                    phmcClient.name = "Petty Harbour Maddox Cove";
                    phmcClient.description = "Town of Petty Harbour Maddox Cove";
                    phmcClient.clientTypeId = phmcClientType.id;
                    phmcClient.currencyId = canadianCurrency.id;
                    phmcClient.timeZoneId = nlTimeZone.id;

                    phmcClient.stateProvinceId = nlProvince.id;
                    phmcClient.countryId = canada.id;

                    phmcClient.addressLine1 = "35 Main Road";
                    phmcClient.city = "Petty Harbour";
                    phmcClient.postalCode = "A0A 3H0";
                    phmcClient.phone = "709 368 3959";
                    phmcClient.email = "cao@pettyharbourmaddoxcove.ca";  

                    phmcClient.notes = "Emergency After-Hours Contact: 709-685-2453";
                    
                    phmcClient.tenantGuid = PHMCTenantGuid;
                    phmcClient.objectGuid = Guid.NewGuid();
                    phmcClient.active = true;
                    phmcClient.deleted = false;

                    context.Clients.Add(phmcClient);
                    context.SaveChanges();
                }


                //
                // Add my house to the scheduling targets
                //
                SchedulingTarget homeTarget = (from st in context.SchedulingTargets where st.name == "33 Skinner's Hill" && st.tenantGuid == PHMCTenantGuid select st).FirstOrDefault();

                if (homeTarget == null)
                {
                    homeTarget = new SchedulingTarget();

                    homeTarget.name = "33 Skinner's Hill";
                    homeTarget.clientId = phmcClient.id;

                    homeTarget.schedulingTargetTypeId = residentialTargetType.id;
                    homeTarget.timeZoneId = nlTimeZone.id;

                    homeTarget.tenantGuid = PHMCTenantGuid;
                    homeTarget.objectGuid = Guid.NewGuid();
                    homeTarget.active = true;
                    homeTarget.deleted = false;

                    context.SchedulingTargets.Add(homeTarget);
                    context.SaveChanges();
                }


                //
                // Add me and wendy as contacts to our house's schedulng target
                //
                SchedulingTargetContact wkStc = (from stc in context.SchedulingTargetContacts where stc.contactId == wkContact.id && stc.schedulingTargetId == homeTarget.id select stc).FirstOrDefault();

                if (wkStc == null)
                {
                    wkStc = new SchedulingTargetContact();

                    wkStc.contactId = wkContact.id;
                    wkStc.schedulingTargetId = homeTarget.id;
                    wkStc.relationshipTypeId = (from rt in context.RelationshipTypes where rt.name == "Owner" select rt.id).FirstOrDefault();

                    wkStc.tenantGuid = PHMCTenantGuid;
                    wkStc.objectGuid = Guid.NewGuid();
                    wkStc.active = true;
                    wkStc.deleted = false;


                    context.SchedulingTargetContacts.Add(wkStc);

                    context.SaveChanges();
                }

                SchedulingTargetContact dkStc = (from stc in context.SchedulingTargetContacts where stc.contactId == dkContact.id && stc.schedulingTargetId == homeTarget.id select stc).FirstOrDefault();

                if (dkStc == null)
                {
                    dkStc = new SchedulingTargetContact();

                    dkStc.contactId = dkContact.id;
                    dkStc.schedulingTargetId = homeTarget.id;
                    dkStc.relationshipTypeId = (from rt in context.RelationshipTypes where rt.name == "Owner" select rt.id).FirstOrDefault();

                    dkStc.tenantGuid = PHMCTenantGuid;
                    dkStc.objectGuid = Guid.NewGuid();
                    dkStc.active = true;
                    dkStc.deleted = false;


                    context.SchedulingTargetContacts.Add(dkStc);

                    context.SaveChanges();
                }


                //
                // Add Wendy as a resource, and then make her part of the rec comittee crew
                //
                Resource wkResource = (from r in context.Resources where r.name == "Wendy Kallergis" && r.tenantGuid == PHMCTenantGuid select r).FirstOrDefault();
                if (wkResource == null)
                {
                    wkResource = new Resource();

                    wkResource.name = "Wendy Kallergis";
                    wkResource.description = "Wendy Kallergis";
                    wkResource.resourceTypeId = recCommitteeResourceType.id;

                    wkResource.timeZoneId = nlTimeZone.id;
                    wkResource.tenantGuid = PHMCTenantGuid;
                    wkResource.objectGuid = Guid.NewGuid();
                    wkResource.active = true;
                    wkResource.deleted = false;

                    context.Resources.Add(wkResource);
                    context.SaveChanges();
                }


                ResourceContact wkRC = (from rc in context.ResourceContacts where rc.resourceId == wkResource.id && rc.contactId == wkContact.id select rc).FirstOrDefault();

                if (wkRC == null)
                {
                    wkRC = new ResourceContact();

                    wkRC.resourceId = wkResource.id;
                    wkRC.contactId = wkContact.id;
                    wkRC.relationshipTypeId = (from rt in context.RelationshipTypes where rt.name == "Self" select rt.id).FirstOrDefault();
                    wkRC.isPrimary = true;

                    wkRC.tenantGuid = PHMCTenantGuid;
                    wkRC.objectGuid = Guid.NewGuid();
                    wkRC.active = true;
                    wkRC.deleted = false;

                    context.ResourceContacts.Add(wkRC);
                    context.SaveChanges();
                }

                //
                // Put wendy on rec committee crew
                //
                CrewMember wendyRecCrewMember = (from cm in context.CrewMembers where cm.crewId == recCommitteeCrew.id && cm.resourceId == wkResource.id select cm).FirstOrDefault();

                if (wendyRecCrewMember == null)
                {
                    wendyRecCrewMember = new CrewMember();

                    wendyRecCrewMember.crewId = recCommitteeCrew.id;
                    wendyRecCrewMember.resourceId = wkResource.id;
                    wendyRecCrewMember.assignmentRoleId = volunteerRole.id;
                    wendyRecCrewMember.sequence = 1;

                    wendyRecCrewMember.tenantGuid = PHMCTenantGuid;
                    wendyRecCrewMember.objectGuid = Guid.NewGuid();
                    wendyRecCrewMember.active = true;
                    wendyRecCrewMember.deleted = false;

                    context.CrewMembers.Add(wendyRecCrewMember);

                    context.SaveChanges();
                }
            }
        }

        private static void ShowMenu()
        {
            // Display the menu
            Console.WriteLine("=== Scheduler Code Generation Menu ===");
            Console.WriteLine();
            Console.WriteLine("1. Option 1 - Generation Scheduler Database Scripts.");
            Console.WriteLine("2. Option 2 - Generate Scheduler Application Entity Code");
            Console.WriteLine("3. Option 3 - Configure Petty Harbour Tenant");
            Console.WriteLine("X. Option X - Exit");
            Console.WriteLine();
            Console.Write("Please select an option (1-3, or X): ");
        }


        static void GenerateSchedulerDatabaseScriptCode()
        {
            string outputFolder = Path.Combine(_outputDirectory, "SchedulerDatabaseScripts");

            Foundation.Scheduler.Database.SchedulerDatabaseGenerator SchedulerDatabaseGenerator = new Foundation.Scheduler.Database.SchedulerDatabaseGenerator();
            SchedulerDatabaseGenerator.GenerateDatabaseCreationScriptsInFolder(outputFolder);

            Console.WriteLine();
            Console.WriteLine("Scheduler database scripts created in folder: " + outputFolder);
            Console.WriteLine();

            Process.Start("explorer.exe", outputFolder);
        }



        static void GenerateSchedulerApplicationCode()
        {

            //
            // This depends upon the SchedulerContext class, and its child data classes to be created in this project, presumably with the ADO.Net 'Code First From Database' tool.  Namespace adjustments and file moves may be necessary.
            //
            string outputFolder = Path.Combine(_outputDirectory, "SchedulerEntityCode");

            Foundation.Scheduler.Database.SchedulerDatabaseGenerator SchedulerDatabaseGenerator = new Foundation.Scheduler.Database.SchedulerDatabaseGenerator();

            //
            // Create the WebAPI controllers - Note that we are ignoring the foundation services for Scheduler controllers
            //
            Foundation.CodeGeneration.CodeGeneratorUtility.GenerateTemplateCodeFromEntityFrameworkContext("SchedulerContext", "Scheduler", typeof(Foundation.Scheduler.Database.SchedulerContext), SchedulerDatabaseGenerator.database, outputFolder);


            //
            // Create Angular data services to interact with the WebAPI.  Roller Ops does not use authorization.
            //
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Scheduler", typeof(Foundation.Scheduler.Database.SchedulerContext), SchedulerDatabaseGenerator.database, outputFolder);

            //
            // Create Angular Components to interact with the data services  Roller Ops does not use authorization.
            //
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("Scheduler", typeof(Foundation.Scheduler.Database.SchedulerContext), SchedulerDatabaseGenerator.database, outputFolder);


            Console.WriteLine();
            Console.WriteLine("Scheduler application code created in folder: " + outputFolder);
            Console.WriteLine();

            Process.Start("explorer.exe", outputFolder);

        }
    }
}


