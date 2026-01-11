using Foundation.Security.Database;
using System;
using System.Data;
using System.Linq;

namespace Foundation
{
    public class DataLoadHelper
    {
        public static SecurityUser CreateLocalUser(string accountName, string password, SecurityUser reportsTo = null, string firstName = null, string middleName = null, string lastName = null, string emailAddress = null, string cellPhoneNumber = null, string phoneNumber = null, bool active = true)
        {
            SecurityContext db = new SecurityContext();

            //
            // Check for an existing user by user name
            //
            SecurityUser securityUser = (from users in db.SecurityUsers
                                         where
                                         users.accountName.ToLower() == accountName.ToLower()
                                         select users).FirstOrDefault();

            if (securityUser == null)
            {
                //
                // Create a new user record
                //
                securityUser = new SecurityUser();

                securityUser.objectGuid = Guid.NewGuid();


                securityUser.accountName = accountName;
                securityUser.password = Foundation.Security.SecurityLogic.SecurePasswordHasher.Hash(password);

                securityUser.activeDirectoryAccount = false;
                securityUser.authenticationDomain = null;

                securityUser.mostRecentActivity = null;

                securityUser.firstName = firstName;
                securityUser.middleName = middleName;
                securityUser.lastName = lastName;
                securityUser.emailAddress = emailAddress;
                securityUser.cellPhoneNumber = cellPhoneNumber;
                securityUser.phoneNumber = phoneNumber;

                securityUser.failedLoginCount = 0;
                securityUser.lastLoginAttempt = null;

                securityUser.description = firstName + " " + (middleName + " ").Trim() + lastName;

                //
                // This uses an LDAP query to get the image from the first domain in the domain list.  It won't work perfectly in multi domain environments because 2nd and later domains have no LDAP credentials in the config
                //
                securityUser.image = null;

                securityUser.active = active;
                securityUser.deleted = false;

                db.SecurityUsers.Add(securityUser);

                db.SaveChanges();
            }

            return securityUser;
        }


        public static DateTime? GetNullableDateTime(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return System.Convert.ToDateTime(row[columnName]);
            }
            else
            {
                return null;
            }
        }

        public static DateTime GetDateTime(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return System.Convert.ToDateTime(row[columnName]);
            }
            else
            {
                throw new Exception("Expecting datetime data but found null for column " + columnName);
            }
        }


        public static int? GetNullableInteger(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return System.Convert.ToInt32(row[columnName]);
            }
            else
            {
                return null;
            }
        }

        public static int GetInteger(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return System.Convert.ToInt32(row[columnName]);
            }
            else
            {
                throw new Exception("Expecting integer data but found null for column " + columnName);
            }
        }


        public static string GetString(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false)
            {
                return row[columnName].ToString();
            }
            else
            {
                return null;
            }
        }

        public static float GetFloat(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return float.Parse(row[columnName].ToString());
            }
            else
            {
                return 0;
            }
        }


        public static Decimal GetDecimal(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return Decimal.Parse(row[columnName].ToString());
            }
            else
            {
                return 0;
            }
        }


        public static bool GetBoolean(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false)
            {
                string value = row[columnName].ToString().ToUpper().Trim();

                if (value == "TRUE" || value == "YES" || value == "1" || value == "-1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("Null value");
            }
        }


        public static bool? GetNullableBoolean(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false)
            {
                string value = row[columnName].ToString().ToUpper().Trim();

                if (value == "TRUE" || value == "YES" || value == "1" || value == "-1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        public static float? GetNullableFloat(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return float.Parse(row[columnName].ToString());
            }
            else
            {
                return null;
            }
        }


        public static Decimal? GetNullableDecimal(DataRow row, string columnName)
        {
            if (row.IsNull(columnName) == false && row[columnName].ToString().Length > 0)
            {
                return Decimal.Parse(row[columnName].ToString());
            }
            else
            {
                return null;
            }
        }

    }
}
