using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Foundation.CodeGeneration;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
                        // This creates a Petty Harbour tenant, and configures the scheduler database to have an instance for Petty Harbour usage
                        //
                        ConfigurePettyHarbour();

                        break;

                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:

                        //
                        // Load 2025 rec committee data (bookings + financials) from Excel files
                        //
                        LoadPettyHarbourData();

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


                //
                // Tax Codes
                //
                TaxCode hstTaxCode = (from tc in context.TaxCodes where tc.code == "HST" && tc.tenantGuid == PHMCTenantGuid select tc).FirstOrDefault();

                if (hstTaxCode == null)
                {
                    hstTaxCode = new TaxCode();

                    hstTaxCode.tenantGuid = PHMCTenantGuid;
                    hstTaxCode.name = "HST (NL)";
                    hstTaxCode.description = "Harmonized Sales Tax - Newfoundland and Labrador (15%)";
                    hstTaxCode.code = "HST";
                    hstTaxCode.rate = 15.0m;
                    hstTaxCode.isDefault = true;
                    hstTaxCode.isExempt = false;
                    hstTaxCode.sequence = 1;
                    hstTaxCode.objectGuid = Guid.NewGuid();
                    hstTaxCode.active = true;
                    hstTaxCode.deleted = false;

                    context.TaxCodes.Add(hstTaxCode);

                    TaxCode exemptTaxCode = new TaxCode();

                    exemptTaxCode.tenantGuid = PHMCTenantGuid;
                    exemptTaxCode.name = "Exempt";
                    exemptTaxCode.description = "Tax exempt";
                    exemptTaxCode.code = "EXEMPT";
                    exemptTaxCode.rate = 0;
                    exemptTaxCode.isDefault = false;
                    exemptTaxCode.isExempt = true;
                    exemptTaxCode.sequence = 2;
                    exemptTaxCode.objectGuid = Guid.NewGuid();
                    exemptTaxCode.active = true;
                    exemptTaxCode.deleted = false;

                    context.TaxCodes.Add(exemptTaxCode);

                    context.SaveChanges();
                }
            }
        }

        private static void LoadPettyHarbourData()
        {
            PettyHarbourDataLoader.LoadAll();
        }

        private static void ShowMenu()
        {
            // Display the menu
            Console.WriteLine("=== Scheduler Code Generation Menu ===");
            Console.WriteLine();
            Console.WriteLine("1. Option 1 - Generation Scheduler Database Scripts.");
            Console.WriteLine("2. Option 2 - Generate Scheduler Application Entity Code");
            Console.WriteLine("3. Option 3 - Configure Petty Harbour Tenant");
            Console.WriteLine("4. Option 4 - Load Petty Harbour 2025 Data");
            Console.WriteLine("X. Option X - Exit");
            Console.WriteLine();
            Console.Write("Please select an option (1-4, or X): ");
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

            Console.WriteLine();
            Console.WriteLine("Scheduler application code created in folder: " + outputFolder);
            Console.WriteLine();

            //
            // Auto-deploy logic
            //
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .Build();

                var deploymentPaths = config.GetSection("DeploymentPaths").Get<Dictionary<string, string>>();

                if (deploymentPaths != null && deploymentPaths.Count > 0)
                {
                    bool deploySuccess = DeploymentUtility.PromptAndDeploy(_outputDirectory, deploymentPaths);

                    if (deploySuccess)
                    {
                        //
                        // If deployment was successful, try to automate the Angular module integration
                        //
                        string angularAppRoot = config["AngularAppRoot"];
                        string angularModuleFile = config["AngularModuleFile"] ?? "app.module.ts";
                        string angularRoutingFile = config["AngularRoutingFile"] ?? "app-routing.module.ts";

                        if (!string.IsNullOrEmpty(angularAppRoot))
                        {
                            Console.WriteLine("Automating Angular module integration...");
                            AngularAutomationUtility.IntegrateGeneratedCode(
                                Directory.GetCurrentDirectory(),
                                "Scheduler", // Module Name
                                outputFolder,
                                angularAppRoot,
                                angularModuleFile,
                                angularRoutingFile
                            );
                        }
                    }
                    else
                    {
                        Process.Start("explorer.exe", outputFolder);
                    }
                }
                else
                {
                    Process.Start("explorer.exe", outputFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading configuration or deploying: {ex.Message}");
                Process.Start("explorer.exe", outputFolder);
            }

        }
    }


    /// <summary>
    /// Loads 2025 PHMC Recreation Committee data from Excel spreadsheets into the Scheduler database.
    /// 
    /// Reads from two files in the 'Example Scheduler Data' folder:
    ///   - 2026-RecBookings.xlsx  → ScheduledEvents + EventCharges + EventCalendar links
    ///   - Rec_Finances.xls       → FinancialCategories + FinancialTransactions
    ///
    /// Prerequisites:
    ///   - Option 3 (ConfigurePettyHarbour) must have been run first to create the tenant, calendars, etc.
    ///   - The Excel files must exist in the 'Example Scheduler Data' folder relative to the repo root.
    /// 
    /// AI-generated code.
    /// </summary>
    internal class PettyHarbourDataLoader
    {
        // Same tenant GUID as ConfigurePettyHarbour
        private static readonly Guid PHMCTenantGuid = new Guid("D58F56C6-E3FB-4D3B-80B3-7053C66491E3");

        // Path to the data files (relative to repo root)
        private static readonly string DataFolder = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Example Scheduler Data");


        /// <summary>
        /// Main entry point for loading all PHMC 2025 data.
        /// </summary>
        public static void LoadAll()
        {
            // Required for ExcelDataReader to read .xls files (legacy BIFF format)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.WriteLine();
            Console.WriteLine("=== Loading Petty Harbour 2025 Data ===");
            Console.WriteLine();

            string bookingsFile = Path.GetFullPath(Path.Combine(DataFolder, "2026-RecBookings.xlsx"));
            string financesFile = Path.GetFullPath(Path.Combine(DataFolder, "Rec_Finances.xls"));

            if (!File.Exists(bookingsFile))
            {
                Console.WriteLine($"ERROR: Bookings file not found: {bookingsFile}");
                return;
            }

            if (!File.Exists(financesFile))
            {
                Console.WriteLine($"ERROR: Finances file not found: {financesFile}");
                return;
            }

            using (SchedulerContext context = new SchedulerContext())
            {
                //
                // Resolve required reference data
                //
                Currency cad = (from c in context.Currencies
                                where c.tenantGuid == PHMCTenantGuid && c.code == "CAD"
                                select c).FirstOrDefault();

                if (cad == null)
                {
                    // Try to find any default currency for this tenant
                    cad = (from c in context.Currencies
                           where c.tenantGuid == PHMCTenantGuid && c.isDefault
                           select c).FirstOrDefault();
                }

                if (cad == null)
                {
                    Console.WriteLine("ERROR: No currency found for PHMC tenant. Run Option 3 first and ensure a CAD currency exists.");
                    return;
                }

                Calendar recCalendar = (from c in context.Calendars
                                       where c.tenantGuid == PHMCTenantGuid && c.name == "Recreation"
                                       select c).FirstOrDefault();

                if (recCalendar == null)
                {
                    Console.WriteLine("ERROR: Recreation calendar not found. Run Option 3 first.");
                    return;
                }

                // EventStatus: use "Completed" for past events, "Planned" for future
                EventStatus completedStatus = (from es in context.EventStatuses
                                               where es.name == "Completed"
                                               select es).FirstOrDefault();

                EventStatus plannedStatus = (from es in context.EventStatuses
                                             where es.name == "Planned"
                                             select es).FirstOrDefault();

                if (completedStatus == null || plannedStatus == null)
                {
                    Console.WriteLine("ERROR: EventStatus seed data not found. Ensure database is properly initialized.");
                    return;
                }

                Console.WriteLine($"  Currency: {cad.name} (id={cad.id})");
                Console.WriteLine($"  Calendar: {recCalendar.name} (id={recCalendar.id})");
                Console.WriteLine();

                //
                // Step 1: Create fiscal periods for 2025 and 2026
                //
                Console.WriteLine("--- Step 1: Loading Fiscal Periods ---");
                int periodCount = LoadFiscalPeriods(context, 2025, 2026);
                Console.WriteLine($"  {periodCount} fiscal periods loaded.");
                Console.WriteLine();

                //
                // Step 2: Load financial categories from the Events Code sheet
                //
                Console.WriteLine("--- Step 2: Loading Financial Categories ---");
                Dictionary<string, int> categoryIdByCode = LoadFinancialCategories(context, financesFile, cad.id);
                Console.WriteLine($"  {categoryIdByCode.Count} categories loaded.");
                Console.WriteLine();

                //
                // Step 3: Load income transactions
                //
                Console.WriteLine("--- Step 3: Loading Income Transactions ---");
                int incomeCount = LoadFinancialTransactions(context, financesFile, "Income", true, categoryIdByCode, cad.id);
                Console.WriteLine($"  {incomeCount} income transactions loaded.");
                Console.WriteLine();

                //
                // Step 4: Load expense transactions
                //
                Console.WriteLine("--- Step 4: Loading Expense Transactions ---");
                int expenseCount = LoadFinancialTransactions(context, financesFile, "Expenses", false, categoryIdByCode, cad.id);
                Console.WriteLine($"  {expenseCount} expense transactions loaded.");
                Console.WriteLine();

                //
                // Step 5: Load bookings as ScheduledEvents
                //
                Console.WriteLine("--- Step 5: Loading Bookings ---");
                int bookingCount = LoadBookings(context, bookingsFile, recCalendar.id, completedStatus.id, plannedStatus.id, cad.id);
                Console.WriteLine($"  {bookingCount} bookings loaded.");
                Console.WriteLine();

                Console.WriteLine("=== Data loading complete ===");
                Console.WriteLine();
            }
        }


        /// <summary>
        /// Reads the 'Events Code' sheet from Rec_Finances.xls and creates FinancialCategory records.
        /// Returns a dictionary mapping category codes to their database IDs.
        ///
        /// Sheet layout (columns 0-1 are always empty):
        ///   Column 2: Category name (e.g., "Rec Centre Rental - Baby")
        ///   Column 3: Code number (e.g., 11)
        ///
        /// Revenue categories appear first, followed by an "Expenses" header row,
        /// then expense categories.
        ///
        /// AI-generated code.
        /// </summary>
        private static Dictionary<string, int> LoadFinancialCategories(
            SchedulerContext context,
            string financesFilePath,
            int currencyId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            //
            // Check if categories already exist for this tenant
            //
            int existingCount = context.FinancialCategories
                .Where(fc => fc.tenantGuid == PHMCTenantGuid)
                .Count();

            if (existingCount > 0)
            {
                Console.WriteLine($"  Financial categories already exist ({existingCount} found). Loading IDs...");

                var existing = context.FinancialCategories
                    .Where(fc => fc.tenantGuid == PHMCTenantGuid)
                    .ToList();

                foreach (var fc in existing)
                {
                    if (!string.IsNullOrEmpty(fc.code))
                    {
                        result[fc.code] = fc.id;
                    }
                }

                return result;
            }

            DataSet ds = ReadExcelFile(financesFilePath);

            //
            // Find the 'Events Code' sheet
            //
            DataTable eventsCodeSheet = null;

            foreach (DataTable dt in ds.Tables)
            {
                if (dt.TableName.Contains("Events", StringComparison.OrdinalIgnoreCase) ||
                    dt.TableName.Contains("Code", StringComparison.OrdinalIgnoreCase))
                {
                    eventsCodeSheet = dt;
                    break;
                }
            }

            if (eventsCodeSheet == null)
            {
                Console.WriteLine("  WARNING: Could not find 'Events Code' sheet. Skipping categories.");
                return result;
            }

            Console.WriteLine($"  Reading sheet: {eventsCodeSheet.TableName} ({eventsCodeSheet.Rows.Count} rows)");

            //
            // Parse: column 2 = category name, column 3 = code number.
            // Revenue entries appear first, then an "Expenses" section header, then expenses.
            //
            int sequence = 1;
            bool isExpenseSection = false;

            foreach (DataRow row in eventsCodeSheet.Rows)
            {
                string col2 = GetCellString(row, 2);
                string col3 = GetCellString(row, 3);

                if (string.IsNullOrWhiteSpace(col2) && string.IsNullOrWhiteSpace(col3))
                {
                    continue;
                }

                //
                // Detect the section boundary
                //
                if (col2.Equals("Expenses", StringComparison.OrdinalIgnoreCase))
                {
                    isExpenseSection = true;
                    continue;
                }

                if (col2.Equals("Revenue", StringComparison.OrdinalIgnoreCase))
                {
                    isExpenseSection = false;
                    continue;
                }

                // Need both a name and a code
                if (string.IsNullOrWhiteSpace(col2) || string.IsNullOrWhiteSpace(col3))
                {
                    continue;
                }

                string categoryName = col2.Trim();
                string categoryCode = col3.Replace(".0", "").Trim();

                // Normalize to integer string
                if (double.TryParse(categoryCode, out double codeNum))
                {
                    categoryCode = ((int)codeNum).ToString();
                }

                if (result.ContainsKey(categoryCode))
                {
                    continue;
                }

                bool isRevenue = !isExpenseSection;

                FinancialCategory fc = new FinancialCategory();

                fc.tenantGuid = PHMCTenantGuid;
                fc.code = categoryCode;
                fc.name = categoryName;
                fc.description = $"{(isRevenue ? "Revenue" : "Expense")}: {categoryName}";
                fc.isRevenue = isRevenue;
                fc.isTaxApplicable = false;
                fc.sequence = sequence++;
                fc.versionNumber = 0;
                fc.objectGuid = Guid.NewGuid();
                fc.active = true;
                fc.deleted = false;

                context.FinancialCategories.Add(fc);
                context.SaveChanges();

                result[categoryCode] = fc.id;

                Console.WriteLine($"    + {(isRevenue ? "Revenue" : "Expense")} [{categoryCode}] {categoryName}");
            }

            return result;
        }


        /// <summary>
        /// Loads financial transactions from either the Income or Expenses sheet.
        ///
        /// IMPORTANT: The two sheets have completely different column layouts:
        ///
        ///   Income sheet (6 columns, data starts at row 6):
        ///     Col 0: Date
        ///     Col 1: Code (maps to FinancialCategory)
        ///     Col 2: Description
        ///     Col 3: Total amount
        ///
        ///   Expenses sheet (12 columns, data starts at row 5):
        ///     Col 0: Code (maps to FinancialCategory)
        ///     Col 1: Date
        ///     Col 2: Expense (pre-tax amount)
        ///     Col 3: HST (tax amount, sometimes NULL)
        ///     Col 4: Amount Paid (total = expense + HST)
        ///     Col 7: Description
        ///
        /// AI-generated code.
        /// </summary>
        private static int LoadFinancialTransactions(
            SchedulerContext context,
            string financesFilePath,
            string sheetName,
            bool isRevenue,
            Dictionary<string, int> categoryIdByCode,
            int currencyId)
        {
            // Check if transactions already exist
            int existingCount = context.FinancialTransactions
                .Where(ft => ft.tenantGuid == PHMCTenantGuid && ft.isRevenue == isRevenue)
                .Count();

            if (existingCount > 0)
            {
                Console.WriteLine($"  {sheetName} transactions already exist ({existingCount} found). Skipping.");
                return 0;
            }

            DataSet ds = ReadExcelFile(financesFilePath);

            DataTable sheet = null;

            foreach (DataTable dt in ds.Tables)
            {
                if (dt.TableName.Contains(sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    sheet = dt;
                    break;
                }
            }

            if (sheet == null)
            {
                Console.WriteLine($"  WARNING: Could not find '{sheetName}' sheet.");
                return 0;
            }

            Console.WriteLine($"  Reading sheet: {sheet.TableName} ({sheet.Rows.Count} rows, {sheet.Columns.Count} columns)");

            //
            // Pre-load fiscal periods for this tenant so we can match by date
            //
            List<FiscalPeriod> fiscalPeriodList = context.FiscalPeriods
                .Where(fp => fp.tenantGuid == PHMCTenantGuid)
                .OrderBy(fp => fp.startDate)
                .ToList();

            //
            // Column indices differ between sheets.  Set up per-sheet mappings.
            //
            int codeCol;
            int dateCol;
            int amountCol;         // Pre-tax (or total if no tax column)
            int taxCol;            // -1 means no separate tax column
            int totalCol;          // -1 means calculate from amount + tax
            int descriptionCol;    // -1 means search remaining columns

            if (isRevenue)
            {
                // Income: Date(0) | Code(1) | Description(2) | Total(3)
                dateCol = 0;
                codeCol = 1;
                descriptionCol = 2;
                amountCol = 3;
                taxCol = -1;
                totalCol = -1;
            }
            else
            {
                // Expenses: Code(0) | Date(1) | Expense(2) | HST(3) | AmountPaid(4) | ... | Description(7)
                codeCol = 0;
                dateCol = 1;
                amountCol = 2;
                taxCol = 3;
                totalCol = 4;
                descriptionCol = 7;
            }

            int loadedCount = 0;
            int skippedCount = 0;
            int batchSize = 0;

            for (int rowIndex = 0; rowIndex < sheet.Rows.Count; rowIndex++)
            {
                DataRow row = sheet.Rows[rowIndex];

                //
                // Read the code cell — skip blank rows and headers
                //
                string codeStr = GetCellString(row, codeCol);

                if (string.IsNullOrWhiteSpace(codeStr))
                {
                    continue;
                }

                // Skip header-like rows
                if (codeStr.Contains("Code", StringComparison.OrdinalIgnoreCase) ||
                    codeStr.Contains("Total", StringComparison.OrdinalIgnoreCase) ||
                    codeStr.Contains("Event", StringComparison.OrdinalIgnoreCase) ||
                    codeStr.Contains("Revenue", StringComparison.OrdinalIgnoreCase) ||
                    codeStr.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
                    codeStr.Contains("PHRC", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                //
                // Parse the category code
                //
                string categoryCode = codeStr.Replace(".0", "").Trim();
                int categoryId = 0;

                if (!categoryIdByCode.TryGetValue(categoryCode, out categoryId))
                {
                    // Try to find a close match
                    if (double.TryParse(categoryCode, out double codeNum))
                    {
                        string intCode = ((int)codeNum).ToString();

                        if (!categoryIdByCode.TryGetValue(intCode, out categoryId))
                        {
                            // Create a catch-all category for unmatched codes
                            if (!categoryIdByCode.ContainsKey($"misc-{categoryCode}"))
                            {
                                FinancialCategory miscCategory = new FinancialCategory();

                                miscCategory.tenantGuid = PHMCTenantGuid;
                                miscCategory.code = categoryCode;
                                miscCategory.name = $"Category {categoryCode}";
                                miscCategory.description = $"{(isRevenue ? "Revenue" : "Expense")}: Auto-created for code {categoryCode}";
                                miscCategory.isRevenue = isRevenue;
                                miscCategory.isTaxApplicable = false;
                                miscCategory.sequence = 900 + int.Parse(categoryCode.Length > 3 ? categoryCode.Substring(0, 3) : categoryCode, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                                miscCategory.versionNumber = 0;
                                miscCategory.objectGuid = Guid.NewGuid();
                                miscCategory.active = true;
                                miscCategory.deleted = false;

                                context.FinancialCategories.Add(miscCategory);
                                context.SaveChanges();

                                categoryIdByCode[$"misc-{categoryCode}"] = miscCategory.id;
                                categoryIdByCode[categoryCode] = miscCategory.id;
                                categoryId = miscCategory.id;

                                Console.WriteLine($"    Auto-created category [{categoryCode}]");
                            }
                            else
                            {
                                categoryId = categoryIdByCode[$"misc-{categoryCode}"];
                            }
                        }
                    }
                    else
                    {
                        skippedCount++;
                        continue;
                    }
                }

                //
                // Parse the date
                //
                DateTime transactionDate = DateTime.UtcNow;
                string dateStr = GetCellString(row, dateCol);

                if (!string.IsNullOrEmpty(dateStr))
                {
                    if (!TryParseFlexibleDate(dateStr, out transactionDate))
                    {
                        transactionDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    }
                }

                //
                // Parse amount (pre-tax or total depending on sheet)
                //
                decimal amount = 0;
                string amountStr = GetCellString(row, amountCol);

                if (!string.IsNullOrEmpty(amountStr))
                {
                    amountStr = amountStr.Replace("$", "").Replace(",", "").Trim();

                    if (!decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount))
                    {
                        skippedCount++;
                        continue;
                    }
                }

                if (amount == 0)
                {
                    skippedCount++;
                    continue;
                }

                amount = Math.Abs(amount);

                //
                // Parse tax amount (Expenses only — Income has no separate tax column)
                //
                decimal taxAmount = 0;

                if (taxCol >= 0)
                {
                    string taxStr = GetCellString(row, taxCol);

                    if (!string.IsNullOrEmpty(taxStr))
                    {
                        taxStr = taxStr.Replace("$", "").Replace(",", "").Trim();
                        decimal.TryParse(taxStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out taxAmount);
                        taxAmount = Math.Abs(taxAmount);
                    }
                }

                //
                // Calculate total (use the explicit total column if available, otherwise amount + tax)
                //
                decimal totalAmount = amount + taxAmount;

                if (totalCol >= 0)
                {
                    string totalStr = GetCellString(row, totalCol);

                    if (!string.IsNullOrEmpty(totalStr))
                    {
                        totalStr = totalStr.Replace("$", "").Replace(",", "").Trim();

                        if (decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedTotal))
                        {
                            totalAmount = Math.Abs(parsedTotal);
                        }
                    }
                }

                //
                // Parse description
                //
                string description = "";

                if (descriptionCol >= 0 && descriptionCol < row.Table.Columns.Count)
                {
                    description = GetCellString(row, descriptionCol);
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    description = $"{(isRevenue ? "Income" : "Expense")} - Row {rowIndex + 1}";
                }

                //
                // Match the transaction date to a fiscal period
                //
                int? fiscalPeriodId = null;

                foreach (FiscalPeriod period in fiscalPeriodList)
                {
                    if (transactionDate >= period.startDate && transactionDate <= period.endDate)
                    {
                        fiscalPeriodId = period.id;
                        break;
                    }
                }

                //
                // Create the transaction
                //
                FinancialTransaction ft = new FinancialTransaction();

                ft.tenantGuid = PHMCTenantGuid;
                ft.financialCategoryId = categoryId;
                ft.fiscalPeriodId = fiscalPeriodId;
                ft.transactionDate = transactionDate;
                ft.description = description.Length > 500 ? description.Substring(0, 500) : description;
                ft.amount = amount;
                ft.taxAmount = taxAmount;
                ft.totalAmount = totalAmount;
                ft.isRevenue = isRevenue;
                ft.currencyId = currencyId;
                ft.versionNumber = 0;
                ft.objectGuid = Guid.NewGuid();
                ft.active = true;
                ft.deleted = false;

                context.FinancialTransactions.Add(ft);
                loadedCount++;
                batchSize++;

                // Save in batches for performance
                if (batchSize >= 50)
                {
                    context.SaveChanges();
                    batchSize = 0;
                }
            }

            // Save any remaining
            if (batchSize > 0)
            {
                context.SaveChanges();
            }

            if (skippedCount > 0)
            {
                Console.WriteLine($"  ({skippedCount} rows skipped due to empty/unparseable data)");
            }

            return loadedCount;
        }


        /// <summary>
        /// Loads bookings from 2026-RecBookings.xlsx as ScheduledEvents with EventCharges.
        /// </summary>
        private static int LoadBookings(
            SchedulerContext context,
            string bookingsFilePath,
            int calendarId,
            int completedStatusId,
            int plannedStatusId,
            int currencyId)
        {
            // Check if bookings already exist
            int existingCount = context.ScheduledEvents
                .Where(se => se.tenantGuid == PHMCTenantGuid && se.externalId != null && se.externalId.StartsWith("PHMC-BOOKING-"))
                .Count();

            if (existingCount > 0)
            {
                Console.WriteLine($"  Bookings already exist ({existingCount} found). Skipping.");
                return 0;
            }

            // Resolve ChargeType and ChargeStatus for EventCharges
            ChargeType rentalChargeType = context.ChargeTypes
                .Where(ct => ct.tenantGuid == PHMCTenantGuid && ct.isRevenue)
                .FirstOrDefault();

            if (rentalChargeType == null)
            {
                // Create a basic rental charge type
                rentalChargeType = new ChargeType();

                rentalChargeType.tenantGuid = PHMCTenantGuid;
                rentalChargeType.name = "Hall Rental";
                rentalChargeType.description = "Recreation centre hall rental fee";
                rentalChargeType.isRevenue = true;
                rentalChargeType.isTaxable = false;
                rentalChargeType.currencyId = currencyId;
                rentalChargeType.sequence = 1;
                rentalChargeType.versionNumber = 0;
                rentalChargeType.objectGuid = Guid.NewGuid();
                rentalChargeType.active = true;
                rentalChargeType.deleted = false;

                context.ChargeTypes.Add(rentalChargeType);
                context.SaveChanges();

                Console.WriteLine($"  Created ChargeType: {rentalChargeType.name} (id={rentalChargeType.id})");
            }

            ChargeStatus paidStatus = context.ChargeStatuses
                .Where(cs => cs.name == "Invoiced" || cs.name == "Paid")
                .FirstOrDefault();

            ChargeStatus pendingStatus = context.ChargeStatuses
                .Where(cs => cs.name == "Pending")
                .FirstOrDefault();

            if (paidStatus == null || pendingStatus == null)
            {
                Console.WriteLine("ERROR: ChargeStatus seed data not found.");
                return 0;
            }

            DataSet ds = ReadExcelFile(bookingsFilePath);

            if (ds.Tables.Count == 0)
            {
                Console.WriteLine("ERROR: No sheets found in bookings file.");
                return 0;
            }

            DataTable sheet = ds.Tables[0]; // First sheet

            Console.WriteLine($"  Reading sheet: {sheet.TableName} ({sheet.Rows.Count} rows)");

            //
            // Bookings layout:
            //   Col 0: Date
            //   Col 1: Name (contact)
            //   Col 2: Event (description, sometimes includes time)
            //   Col 3: Payment (fee amount + status)
            //   Col 4: Contact Info (email/phone)
            //   Col 5: Rental Agreement (signed/pending)
            //   Col 6: DD Refunded
            //
            int loadedCount = 0;
            bool headerSkipped = false;

            foreach (DataRow row in sheet.Rows)
            {
                // Skip header
                if (!headerSkipped)
                {
                    string col0 = GetCellString(row, 0);

                    if (col0.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
                        col0.Contains("2025", StringComparison.OrdinalIgnoreCase) ||
                        col0.Contains("2026", StringComparison.OrdinalIgnoreCase))
                    {
                        headerSkipped = true;

                        // If it looks like a year, it might be data
                        if (!col0.Contains("Date", StringComparison.OrdinalIgnoreCase))
                        {
                            // Fall through to process this row
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (!TryParseFlexibleDate(col0, out _))
                    {
                        continue; // Skip non-date header rows
                    }
                    else
                    {
                        headerSkipped = true;
                    }
                }

                string dateStr = GetCellString(row, 0);
                string contactName = GetCellString(row, 1);
                string eventDesc = GetCellString(row, 2);
                string paymentStr = GetCellString(row, 3);
                string contactInfo = GetCellString(row, 4);
                string rentalAgreement = GetCellString(row, 5);
                string ddRefunded = GetCellString(row, 6);

                // Skip completely empty rows
                if (string.IsNullOrWhiteSpace(dateStr) && string.IsNullOrWhiteSpace(eventDesc))
                {
                    continue;
                }

                // Parse the date
                DateTime eventDate;

                if (!TryParseFlexibleDate(dateStr, out eventDate))
                {
                    Console.WriteLine($"    SKIP: Could not parse date '{dateStr}' for event '{eventDesc}'");
                    continue;
                }

                //
                // Build the event name from contact + event description
                //
                string eventName = !string.IsNullOrEmpty(eventDesc) ? eventDesc : contactName;

                if (string.IsNullOrWhiteSpace(eventName))
                {
                    eventName = $"Booking on {eventDate:yyyy-MM-dd}";
                }

                // Truncate to field limit
                if (eventName.Length > 200)
                {
                    eventName = eventName.Substring(0, 200);
                }

                // Determine status based on date
                bool isPast = eventDate < DateTime.UtcNow;
                int statusId = isPast ? completedStatusId : plannedStatusId;

                // Try to extract time from event description
                DateTime startTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 10, 0, 0, DateTimeKind.Utc);
                DateTime endTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 14, 0, 0, DateTimeKind.Utc);

                // Build notes from all the extra columns
                string notes = "";

                if (!string.IsNullOrEmpty(contactName))
                {
                    notes += $"Contact: {contactName}\n";
                }

                if (!string.IsNullOrEmpty(contactInfo))
                {
                    notes += $"Contact Info: {contactInfo}\n";
                }

                if (!string.IsNullOrEmpty(rentalAgreement))
                {
                    notes += $"Rental Agreement: {rentalAgreement}\n";
                }

                if (!string.IsNullOrEmpty(ddRefunded))
                {
                    notes += $"DD Refunded: {ddRefunded}\n";
                }

                if (!string.IsNullOrEmpty(paymentStr))
                {
                    notes += $"Payment: {paymentStr}\n";
                }

                // Create the ScheduledEvent
                ScheduledEvent se = new ScheduledEvent();

                se.tenantGuid = PHMCTenantGuid;
                se.name = eventName;
                se.description = eventDesc;
                se.startDateTime = startTime;
                se.endDateTime = endTime;
                se.eventStatusId = statusId;
                se.location = "Recreation Centre";
                se.notes = notes.Trim();
                se.externalId = $"PHMC-BOOKING-{loadedCount + 1:D4}";
                se.versionNumber = 0;
                se.objectGuid = Guid.NewGuid();
                se.active = true;
                se.deleted = false;

                context.ScheduledEvents.Add(se);
                context.SaveChanges();

                // Link to the Recreation calendar
                EventCalendar ec = new EventCalendar();

                ec.tenantGuid = PHMCTenantGuid;
                ec.scheduledEventId = se.id;
                ec.calendarId = calendarId;
                ec.objectGuid = Guid.NewGuid();
                ec.active = true;
                ec.deleted = false;

                context.EventCalendars.Add(ec);

                // Try to parse a rental fee from the payment column
                decimal fee = ParseFeeFromPaymentColumn(paymentStr);

                if (fee > 0)
                {
                    bool isPaid = paymentStr != null &&
                                  (paymentStr.Contains("paid", StringComparison.OrdinalIgnoreCase) ||
                                   paymentStr.Contains("etransfer", StringComparison.OrdinalIgnoreCase) ||
                                   paymentStr.Contains("e-transfer", StringComparison.OrdinalIgnoreCase));

                    EventCharge charge = new EventCharge();

                    charge.tenantGuid = PHMCTenantGuid;
                    charge.scheduledEventId = se.id;
                    charge.chargeTypeId = rentalChargeType.id;
                    charge.chargeStatusId = isPaid ? paidStatus.id : pendingStatus.id;
                    charge.quantity = 1;
                    charge.unitPrice = fee;
                    charge.extendedAmount = fee;
                    charge.taxAmount = 0;
                    charge.currencyId = currencyId;
                    charge.isAutomatic = false;
                    charge.isDeposit = false;
                    charge.notes = paymentStr;
                    charge.versionNumber = 0;
                    charge.objectGuid = Guid.NewGuid();
                    charge.active = true;
                    charge.deleted = false;

                    context.EventCharges.Add(charge);
                }

                context.SaveChanges();
                loadedCount++;

                Console.WriteLine($"    + [{eventDate:yyyy-MM-dd}] {eventName}");
            }

            return loadedCount;
        }


        /// <summary>
        /// Creates monthly fiscal period records for each year in the given range.
        /// Periods are aligned to calendar month boundaries (Jan 1 – Dec 31).
        /// Skips any year that already has periods for this tenant.
        ///
        /// AI-generated code.
        /// </summary>
        private static int LoadFiscalPeriods(SchedulerContext context, int startYear, int endYear)
        {
            int createdCount = 0;

            string[] monthNames = new string[]
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            for (int year = startYear; year <= endYear; year++)
            {
                //
                // Check if periods already exist for this year
                //
                int existingCount = context.FiscalPeriods
                    .Where(fp => fp.tenantGuid == PHMCTenantGuid && fp.fiscalYear == year)
                    .Count();

                if (existingCount > 0)
                {
                    Console.WriteLine($"  Fiscal periods for {year} already exist ({existingCount} found). Skipping.");
                    continue;
                }

                //
                // Create 12 monthly periods aligned with calendar months
                //
                for (int month = 1; month <= 12; month++)
                {
                    DateTime periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

                    FiscalPeriod fp = new FiscalPeriod();

                    fp.tenantGuid = PHMCTenantGuid;
                    fp.name = $"{monthNames[month - 1]} {year}";
                    fp.description = $"{monthNames[month - 1]} {year} fiscal period";
                    fp.startDate = periodStart;
                    fp.endDate = periodEnd;
                    fp.periodType = "Month";
                    fp.fiscalYear = year;
                    fp.periodNumber = month;
                    fp.isClosed = false;
                    fp.sequence = ((year - startYear) * 12) + month;
                    fp.versionNumber = 0;
                    fp.objectGuid = Guid.NewGuid();
                    fp.active = true;
                    fp.deleted = false;

                    context.FiscalPeriods.Add(fp);
                    createdCount++;

                    Console.WriteLine($"    + {fp.name}");
                }

                context.SaveChanges();
            }

            return createdCount;
        }


        #region Helper Methods

        /// <summary>
        /// Reads an Excel file (.xlsx or .xls) and returns the data as a DataSet.
        /// </summary>
        private static DataSet ReadExcelFile(string filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    return reader.AsDataSet();
                }
            }
        }


        /// <summary>
        /// Safely gets a cell value as a trimmed string.
        /// </summary>
        private static string GetCellString(DataRow row, int columnIndex)
        {
            if (columnIndex >= row.Table.Columns.Count)
            {
                return "";
            }

            object val = row[columnIndex];

            if (val == null || val == DBNull.Value)
            {
                return "";
            }

            //
            // Handle OLE Automation dates from Excel.
            // Excel stores dates as doubles (e.g., 45717.0 = 2025-02-25).  Category
            // codes are small integers (< 1000), so we use a value threshold to
            // distinguish dates from numeric codes regardless of column position.
            //
            if (val is double d && d > 30000)
            {
                try
                {
                    DateTime dt = DateTime.FromOADate(d);
                    return dt.ToString("yyyy-MM-dd");
                }
                catch
                {
                    return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return val.ToString().Trim();
        }


        /// <summary>
        /// Tries to parse a date from various formats found in the rec committee data.
        /// Handles: yyyy-MM-dd, MM/dd/yyyy, dd-MMM-yy, Month Day, Year, etc.
        /// </summary>
        private static bool TryParseFlexibleDate(string dateStr, out DateTime result)
        {
            result = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(dateStr))
            {
                return false;
            }

            dateStr = dateStr.Trim();

            // Handle OLE date as number string
            if (double.TryParse(dateStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double oaDate))
            {
                if (oaDate > 1 && oaDate < 100000) // Reasonable OLE date range
                {
                    try
                    {
                        result = DateTime.FromOADate(oaDate);
                        result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
                        return true;
                    }
                    catch
                    {
                        // Fall through to string parsing
                    }
                }
            }

            // Try standard formats
            string[] formats = new[]
            {
                "yyyy-MM-dd",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "M/d/yyyy",
                "d-MMM-yy",
                "dd-MMM-yy",
                "MMM d, yyyy",
                "MMMM d, yyyy",
                "MMMM d yyyy",
                "MMM dd, yyyy",
                "d MMM yyyy",
                "dd MMM yyyy",
            };

            if (DateTime.TryParseExact(dateStr, formats, System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.AllowWhiteSpaces | System.Globalization.DateTimeStyles.AssumeUniversal, out result))
            {
                result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
                return true;
            }

            // Last resort: generic parse
            if (DateTime.TryParse(dateStr, System.Globalization.CultureInfo.InvariantCulture,
                                  System.Globalization.DateTimeStyles.AllowWhiteSpaces | System.Globalization.DateTimeStyles.AssumeUniversal, out result))
            {
                result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Extracts a dollar amount from the payment column text.
        /// Handles formats like: "$150", "$225 - paid", "$400 deposit paid", etc.
        /// </summary>
        private static decimal ParseFeeFromPaymentColumn(string paymentStr)
        {
            if (string.IsNullOrWhiteSpace(paymentStr))
            {
                return 0;
            }

            // Look for dollar amounts
            string cleaned = paymentStr.Trim();

            // Remove common non-numeric suffixes
            int dollarIndex = cleaned.IndexOf('$');

            if (dollarIndex >= 0)
            {
                string afterDollar = cleaned.Substring(dollarIndex + 1);

                // Take characters until we hit something that's not a digit, comma, or period
                string numericPart = "";

                foreach (char c in afterDollar)
                {
                    if (char.IsDigit(c) || c == '.' || c == ',')
                    {
                        numericPart += c;
                    }
                    else
                    {
                        break;
                    }
                }

                numericPart = numericPart.Replace(",", "");

                if (decimal.TryParse(numericPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal fee))
                {
                    return fee;
                }
            }

            // Try parsing the whole thing as a number
            string numbersOnly = cleaned.Replace("$", "").Replace(",", "");

            // Take the first number we find
            string firstNumber = "";
            bool foundDigit = false;

            foreach (char c in numbersOnly)
            {
                if (char.IsDigit(c) || c == '.')
                {
                    firstNumber += c;
                    foundDigit = true;
                }
                else if (foundDigit)
                {
                    break;
                }
            }

            if (decimal.TryParse(firstNumber, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0;
        }

        #endregion
    }
}


