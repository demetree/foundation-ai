using Foundation.Messaging.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Ensures that expected notification types exist in the database at application startup.
    /// Missing types are created automatically; existing types are left untouched.
    /// 
    /// This prevents runtime errors when code references a notification type by name
    /// (e.g., "Mention", "SystemAlert") that hasn't been seeded in the database.
    /// 
    /// AI-developed as part of Notification Improvements, March 2026.
    /// 
    /// </summary>
    public static class NotificationTypeSeeder
    {
        /// <summary>
        /// The list of notification types that the system expects to exist.
        /// Add new entries here when adding new notification categories.
        /// </summary>
        private static readonly (string Name, string Description)[] ExpectedTypes = new[]
        {
            ("Mention", "User was @mentioned in a conversation message"),
            ("SystemAlert", "System-generated alert for threshold or condition monitoring"),
            ("DirectMessage", "Notification for a new direct message conversation"),
            ("EntityUpdate", "Notification for updates to a linked business entity")
        };


        /// <summary>
        /// Checks the database for expected notification types and creates any that are missing.
        /// Safe to call multiple times — idempotent.
        /// </summary>
        public static async Task EnsureNotificationTypesExistAsync()
        {
            using (MessagingContext db = new MessagingContext())
            {
                List<string> existingNames = await db.NotificationTypes
                    .Where(t => t.active == true && t.deleted == false)
                    .Select(t => t.name)
                    .ToListAsync();

                bool anyCreated = false;

                foreach (var (name, description) in ExpectedTypes)
                {
                    if (existingNames.Contains(name) == false)
                    {
                        db.NotificationTypes.Add(new NotificationType
                        {
                            name = name,
                            description = description,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        });

                        anyCreated = true;
                    }
                }

                if (anyCreated)
                {
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
