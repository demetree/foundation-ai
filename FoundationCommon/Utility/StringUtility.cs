using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Foundation
{
    public static partial class StringUtility
    {
        public const string JSON_DATE_FORMAT = "yyyy:MM:ddTHH:mm:ss.fffZ";      // To format UTC dates like JSON requires


        public static DataTable ConvertCSVtoDataTable(string csvData)
        {
            byte[] data = Encoding.ASCII.GetBytes(csvData);

            return ConvertCSVtoDataTable(data);
        }


        //
        // The purpose of these two functions is to get a list of stringified properties suitable for feeding into the database script generator's insert creator
        //
        public static Dictionary<string, string> GetStringDictionaryFromObject<T>(T dataObject, bool ignoreIdProperty = true)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                if (ignoreIdProperty == true && prop.Name == "id")
                {
                    continue;
                }

                if (prop.PropertyType.FullName.StartsWith("System.") == true)       // Only include properties that are of types that come from the System name space.  This means that all the built in number and string and such types will come in but custom types will not,  which is what we want.
                {

                    object value = prop.GetValue(dataObject);

                    if (value != null)
                    {
                        if (value.GetType() == typeof(DateTime))
                        {
                            DateTime dateValue = (DateTime)value;

                            output.Add(prop.Name, dateValue.ToString(Foundation.StringUtility.JSON_DATE_FORMAT));
                        }
                        else if (value.GetType() == typeof(bool))
                        {
                            //
                            // Bool becomes 1/0 for database insertion purposes
                            //
                            bool boolValue = (bool)(value);

                            if (boolValue == true)
                            {
                                output.Add(prop.Name, "1");
                            }
                            else
                            {
                                output.Add(prop.Name, "0");
                            }
                        }
                        else
                        {
                            output.Add(prop.Name, value.ToString());
                        }
                    }
                    else
                    {
                        output.Add(prop.Name, null);
                    }
                }
            }

            return output;
        }

        //
        // The purpose of these two functions is to get a list of stringified properties suitable for feeding into the database script generator's insert creator
        //
        public static List<Dictionary<string, string>> GetListOfStringDictionariesFromListOfObjects<T>(List<T> dataObjectList, bool ignoreIdProperty = true)
        {
            List<Dictionary<string, string>> output = new List<Dictionary<string, string>>();

            var props = typeof(T).GetProperties();

            foreach (var dataObject in dataObjectList)
            {
                Dictionary<string, string> outputLine = new Dictionary<string, string>();

                foreach (var prop in props)
                {
                    if (ignoreIdProperty == true && prop.Name == "id")
                    {
                        continue;
                    }

                    if (prop.PropertyType.FullName.StartsWith("System.") == true &&
                        prop.PropertyType.FullName.StartsWith("System.Collections") == false)       // Only include properties that are of types that come from the System name space.  This means that all the built in number and string and such types will come in but custom types will not,  which is what we want.
                    {
                        object value = prop.GetValue(dataObject);

                        if (value != null)
                        {
                            if (value.GetType() == typeof(DateTime))
                            {
                                DateTime dateValue = (DateTime)(value);

                                outputLine.Add(prop.Name, dateValue.ToString(Foundation.StringUtility.JSON_DATE_FORMAT));
                            }
                            else if (value.GetType() == typeof(bool))
                            {
                                //
                                // Bool becomes 1/0 for database insertion purposes
                                //
                                bool boolValue = (bool)(value);

                                if (boolValue == true)
                                {
                                    outputLine.Add(prop.Name, "1");
                                }
                                else
                                {
                                    outputLine.Add(prop.Name, "0");
                                }
                            }
                            else
                            {
                                outputLine.Add(prop.Name, value.ToString());
                            }
                        }
                        else
                        {
                            outputLine.Add(prop.Name, null);
                        }
                    }
                }

                output.Add(outputLine);
            }

            return output;
        }


        //
        // Turn a string into title case.  Note that this does not add spaces between camcel case words.  Use ConvertToHeader for that
        //
        public static string TitleCase(string data)
        {
            if (data != null)
            {
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

                // If string is all upper case, then go to lower case to let the title case recapitalize it as necessary.
                if (IsAllUpper(data) == true)
                {
                    data = data.ToLower();
                }

                return ti.ToTitleCase(data);
            }
            else
            {
                return null;
            }
        }

        public static bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }


        public static string CamelCase(string data, bool renameReservedWords = true)
        {
            string output;

            if (!string.IsNullOrEmpty(data) && data.Length > 1)
            {
                output = char.ToLowerInvariant(data[0]) + data.Substring(1);
            }
            else
            {
                output = data;
            }

            if (renameReservedWords == true)
            {
                // add an underscore prefix to C# reserved words.
                // C# reserved words that require renaming

                if (IsReservedWord(output) == true)
                {
                    output = $"_{output}";
                }
            }

            return output;
        }



        /// <summary>
        /// 
        ///  Converts a string in Camel Case to Pascal Case
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CamelCaseToPascalCase(string data)
        {
            string output;

            if (!string.IsNullOrEmpty(data) && data.Length > 1)
            {
                output = char.ToUpperInvariant(data[0]) + data.Substring(1);
            }
            else
            {
                output = data;
            }

            return output;
        }


        public static string SnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && i > 0)
                {
                    result.Append('_').Append(char.ToLower(input[i]));
                }
                else
                {
                    result.Append(char.ToLower(input[i]));
                }
            }

            return result.ToString();
        }


        public static bool IsReservedWord(string str)
        {
            if (str == null)
            {
                return false;
            }

            HashSet<string> reservedWords = new HashSet<string>
                {
                    "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
                    "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
                    "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
                    "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
                    "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
                    "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
                    "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint",
                    "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
                };

            if (reservedWords.Contains(str))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string UnCamelCaseAndMakeTitle(string data)
        {
            string output = "";
            int counter = 0;

            bool hasLowerCase = false;

            foreach (Char chr in data)
            {
                if (char.IsLower(chr) == true)
                {
                    hasLowerCase = true;
                    break;
                }
            }

            if (hasLowerCase == true)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    Char chr = data[i];

                    if (counter == 0)       // start with an upper case character always
                    {
                        output += chr.ToString().ToUpper();
                    }
                    else
                    {
                        if (char.IsLower(chr) == true)
                        {
                            output += chr.ToString();
                        }
                        else
                        {
                            if ((i + 1) < data.Length)
                            {
                                // only add a space and lower case the char if the next char isn't an upper case.
                                if (char.IsUpper(data[i + 1]) == false)
                                {
                                    output += " ";
                                    output += chr.ToString().ToUpper();
                                }
                                else
                                {
                                    output += chr.ToString();
                                }
                            }
                            else if ((i + 1) == data.Length)
                            {
                                output += chr.ToString().ToUpper();
                            }
                            else
                            {
                                output += " ";
                                output += chr.ToString().ToUpper();
                            }
                        }
                    }

                    counter++;
                }
            }
            else
            {
                // do nothing if there are no lower case characters in the data
                output = data;
            }

            return output;
        }


        public static Dictionary<string, string> StringToDictionary(string data)
        {
            //
            // Expects data in this format name1=value1,name2=value2
            //
            // Does not support commas or equals signs inside quoted strings.
            // 
            //
            if (data == null)
            {
                return null;
            }

            String[] nameAndValueList = data.Split(',');

            if (nameAndValueList != null && nameAndValueList.Length > 0)
            {
                Dictionary<String, String> output = new Dictionary<string, string>();

                for (int i = 0; i < nameAndValueList.Length; i++)
                {
                    string item = nameAndValueList[i];

                    int indexOfEquals = item.IndexOf("=");

                    if (indexOfEquals > -1 && indexOfEquals < (item.Length - 1))
                    {
                        string name = item.Substring(0, indexOfEquals);
                        string value = item.Substring(indexOfEquals + 1);

                        output.Add(name, value);
                    }
                }

                return output;
            }
            else
            {
                return null;
            }

        }

        //
        // Quick google got me this.  It's probably good enough for simple things.
        //
        public static void DataTableToCSV(DataTable dtDataTable, string fileName)
        {
            StreamWriter sw = new StreamWriter(fileName, false);

            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }


        public static DataTable ConvertCSVtoDataTable(byte[] csvData)
        {
            try
            {
                StreamReader sr = new StreamReader(new MemoryStream(csvData), System.Text.Encoding.UTF8);

                // this does a better job of processing the header line - it will handle an obscure case of where a header contains a comma.  Unlikely, but possible.
                string[] headers = ConvertCSVLineIntoStringArray(sr.ReadLine());

                // old way that just uses split
                //string[] headers = sr.ReadLine().Split(new char[] { ',' });

                DataTable dt = new DataTable();
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    // we can't just split the row arbitrarily on the comma character because the CSV spec allows for commas to be embedded within a field that 
                    // is wrapped in double quotes.
                    //string[] rowData = sr.ReadLine().Split(',');

                    // The convertCSVLineIntoStringArray function process the csv data according to our spec.
                    string[] rowData = ConvertCSVLineIntoStringArray(sr.ReadLine());

                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        if (i < rowData.Length)
                        {
                            string data = rowData[i];

                            data = data.Trim();

                            dr[i] = data;
                        }
                    }
                    dt.Rows.Add(dr);
                }
                return dt;

            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string[] ConvertCSVLineIntoStringArray(string csvData)
        {
            //
            // The purpose of this function is to parse a line of CSV data and gracefully handle lines that include commas nested within quotes.
            //
            // It will not take off the leading and trailing quotes for fields.
            //
            //
            // Example 1:     abc,def,ghi    
            // Exmaple 2:     "abc","def","ghi"
            //
            //
            // This should parse out as a 3 element array of strings, the 2nd element of which containing a comma.
            //
            //  "first","second,withembeddedcomma","third"
            //
            // We will also support double quotes inside of double quootes by using a C style escape of \"
            //
            // Example 3:  "This here \" is an escaped double quote", that will, "be handled by this function"
            //
            //
            // For further reading, see this spec for something similar:  http://tools.ietf.org/html/rfc4180
            //
            // Note that our format deviates from that in the following major ways:   1 - we don't support CRLF's inside quoted string data.  We could, but we'd need to add a lot to this function to send in the complete file instead of one single line.
            //                                                                        2 - we use a \" to escape double quotes inside quoted string data instead of "".  This is simply style.  We could do it the other way too.
            //
            //
            List<String> output = new List<String>();

            string dataBuffer = "";
            bool dataStartsWithQuote = false;

            for (int i = 0; i < csvData.Length; i++)
            {

                char dataCharacter = csvData.ElementAt(i);

                //
                // Are we starting the search for a new field of data and hit a double quote to start with?
                //
                // If so, put loop into the quote mode and restart.
                //
                if (dataStartsWithQuote == false &&
                    dataBuffer.Length == 0 &&
                    dataCharacter == '"')
                {
                    // set the mode flag, and add the double quote to the field buffer.
                    dataStartsWithQuote = true;

                    // NOTE THAT WE'RE NOT INCLUDING THE LEADING DOUBLE QUOTE IN THE OUTPUT . -> WE'RE TREATING IT AS A CONTROL CHAR.
                    continue;
                }


                // process differently depending upon whether or not we're expecting to be inside a double quoted field
                if (dataStartsWithQuote == true)
                {
                    //
                    // here we want to split the fields when we find an unescaped double quote.  An escaped double quote loooks like this:  \"
                    //
                    if (dataCharacter == '"')
                    {
                        //
                        // we found a terminating quote character.  We expect that this terminates the field.  Sanity check the next stuff though
                        // to see if it's either end of data or a comma that is the next non whitespace character.  If it's not, then we need to complain.
                        //
                        int nextCommaPos = -1;

                        for (int j = (i + 1); j < csvData.Length; j++)
                        {
                            if (csvData.ElementAt(j) == ' ')
                            {
                                continue;
                            }

                            if (csvData.ElementAt(j) != ',')
                            {
                                throw new Exception("Field starting with '" + dataBuffer + "' found unexpected data at position " + (j + 1) + " such that the termination of the current field or the staring of the next is ambiguous.");
                            }
                            else
                            {
                                nextCommaPos = j;
                                break;
                            }
                        }

                        // NOTE THAT WE'RE NOT INCLUDE THE TRAILING QUOTE. -> WE'RE TREATING IT AS A CONTROL CHAR.

                        // flush the data into the output list
                        output.Add(dataBuffer);

                        // reset the control variables.
                        dataBuffer = "";
                        dataStartsWithQuote = false;

                        // kick up the main index because the next field starts after the comma...
                        i = nextCommaPos;
                    }
                    else if (dataCharacter == '\\' &&
                             csvData.Length >= i &&
                             csvData.ElementAt(i + 1) == '"')
                    {
                        //
                        // we found an escape sequence that represents a double quote.  
                        //
                        // Put a double quote onto the buffer, kick up the character counter and restart the loop
                        //
                        dataBuffer += '"';
                        i++;
                        continue;
                    }
                    else
                    {
                        // we treat this as a data character, regardless of what it is.  It can be a comma, and that's OK.  we won't break 
                        dataBuffer += dataCharacter;
                    }
                }
                else
                {
                    // here we want to split the fields when we find a comma.

                    //
                    // Kick out leading white space when not escaped in double quotes.
                    //
                    if (dataCharacter == ' ' &&
                        dataBuffer.Length == 0)
                    {
                        continue;
                    }

                    //
                    // have we hit the end of the field?
                    //
                    if (dataCharacter == ',')
                    {
                        // flush the data into the output list
                        output.Add(dataBuffer);

                        // reset the processing variables
                        dataBuffer = "";
                        dataStartsWithQuote = false;
                    }
                    else
                    {
                        dataBuffer += dataCharacter;
                    }
                }
            }

            //
            // We could complain here if we get to the end of the data and haven't closed a data field that is in quote mode, but it's probably irreleveant
            // at the end of the day.  We have all the data we're going to get anyhow.
            // 
            if (dataBuffer.Length > 0 || csvData.EndsWith(","))
            {
                output.Add(dataBuffer);
            }

            return output.ToArray();
        }

        public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            //
            // This method allows case insensitive string replaces.
            //
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static string GuidStringToEscapedCharacterString(string guid)
        {
            //https://social.technet.microsoft.com/wiki/contents/articles/5392.active-directory-ldap-syntax-filters.aspx
            //
            // This is useful for LDAP query building
            //


            //Byte arrays, like the objectGUID attribute, can be represented as a series of escaped hexadecimal bytes. 
            //The GUID {b95f3990-b59a-4a1b-9e96-86c66cb18d99} is equivalent to the hex representation "90395fb99ab51b4a9e9686c66cb18d99". 

            //  b9 5f 39 90-b5 9a-4a 1b-9e 96-86 c6 6c b1 8d 99
            //  90 39 5f b9 9a b5 1b 4a 9e 96 86 c6 6c b1 8d 99
            //
            //Notice how the order of the first 8 bytes is reversed in groups. 
            // You specify the escaped hex bytes. You cannot specify the form in curly braces in a filter. 


            if (guid == null || guid.Length != 36)
            {
                return null;
            }

            StringBuilder output = new StringBuilder();

            //
            // Take away the hypens and go to upper case.
            //
            guid = guid.Replace("-", "").ToUpper();

            //
            // Note the out of sequence mapping into the new string for the first 8 sets of adds
            //
            output.Append("\\");
            output.Append(guid.Substring(6, 2));

            output.Append("\\");
            output.Append(guid.Substring(4, 2));

            output.Append("\\");
            output.Append(guid.Substring(2, 2));

            output.Append("\\");
            output.Append(guid.Substring(0, 2));

            output.Append("\\");
            output.Append(guid.Substring(10, 2));

            output.Append("\\");
            output.Append(guid.Substring(8, 2));

            output.Append("\\");
            output.Append(guid.Substring(14, 2));

            output.Append("\\");
            output.Append(guid.Substring(12, 2));

            output.Append("\\");
            output.Append(guid.Substring(16, 2));

            output.Append("\\");
            output.Append(guid.Substring(18, 2));

            output.Append("\\");
            output.Append(guid.Substring(20, 2));

            output.Append("\\");
            output.Append(guid.Substring(22, 2));

            output.Append("\\");
            output.Append(guid.Substring(24, 2));

            output.Append("\\");
            output.Append(guid.Substring(26, 2));

            output.Append("\\");
            output.Append(guid.Substring(28, 2));

            output.Append("\\");
            output.Append(guid.Substring(30, 2));

            return output.ToString();
        }


        /// <summary>
        /// Decrypts the specified encryption key.
        /// </summary>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <param name="cipherString">The cipher string.</param>
        /// <param name="useHashing">if set to <c>true</c> [use hashing].</param>
        /// <returns>
        ///  The decrypted string based on the key
        /// </returns>
        public static string Decrypt(string encryptionKey, string cipherString, bool useHashing)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(encryptionKey));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(encryptionKey);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;

            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;

            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);

            //Release resources held by TripleDes Encryptor
            tdes.Clear();

            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }



        /// <summary>
        /// Encrypts the specified to encrypt.
        /// </summary>
        /// <param name="toEncrypt">To encrypt.</param>
        /// <param name="useHashing">if set to <c>true</c> [use hashing].</param>
        /// <returns>
        /// The encrypted string to be stored in the Database
        /// </returns>
        public static string Encrypt(string encryptionKey, string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            //System.Configuration.AppSettingsReader settingsReader =
            //                                    new AppSettingsReader();


            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(encryptionKey));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(encryptionKey);
            }


            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;

            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();

            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);

            //Release resources held by TripleDes Encryptor
            tdes.Clear();

            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }

        public static bool IsFloat(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }

        public static bool IsInteger(this string s)
        {
            int output;
            return int.TryParse(s, out output);
        }

        /// <summary>
        /// 
        /// This makes a string plural, by either plunking an s on the end, or using a plural term for common words like person to people.
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Pluralize(string str)
        {
            if (str == "Person")        // I could use 'ends with' person here, but I don't think the EF Core power tools pluralizer does that, so I'm trying to follow that.
            {
                return "People";
            }

            //
            // Using the EF6 Classic pluralizer makes this a non-issue.  Ideally, this function will take in a parameter for a pluralizer selection.  EF6 or the new one.
            //
            else if (str.ToUpper() == "EQUIPMENT")    //  //else if (str.ToUpper().EndsWith("EQUIPMENT"))  Ideally, I'd be able to use endsWith on equipment, but the EF Core power tools only does its pluralization on the the full word equipment, so I have to follow that.
            {
                //
                // Equipment is singular and plural in English, but the pluralization in the DB Context creators between Framework and Core are different, so 
                // that needs to be dealt with.
                //
                string frameworkDescription = RuntimeInformation.FrameworkDescription;

                if (frameworkDescription.StartsWith(".NET Framework"))
                {
                    //
                    // Running on .NET Framework - the ADO.Net DB Context creator shoves an s onto the word equipment.
                    //
                    return str + "s";
                }
                else
                {
                    //
                    // Running on .Net 9, the EF Core Power tools context generators respects the singular and plural for the word equipment
                    //
                    return str;
                }

            }
            else if (str.EndsWith("tch"))       // batch for example
            {
                return str + "es";
            }
            else if (str.EndsWith("Status") || str.EndsWith("Campus"))
            {
                string frameworkDescription = RuntimeInformation.FrameworkDescription;

                if (frameworkDescription.Contains(".NET Framework"))
                {
                    // EF DotNet Framework's DB Reverse engineer drops the last 'S' off status for some stupid reason when naming it in plural
                    return str;
                }
                else
                {
                    // EF Core Power Tools does a proper naming of plurals for status and campus
                    return str + "es";
                }
            }
            else if (str.EndsWith("Datum"))     // New EF Core Power Tools pluralizer does this
            {
                // Change Datum to Data
                return str.Substring(0, str.Length - 5) + "Data";       // need to do this because the EF Context's DBSet will be suffixed with 'Data', so we can't just tack on an s here.
            }
            else if (str.EndsWith("y") == true && str.EndsWith("ay") == false && str.EndsWith("ey") == false)    // country to countries but not day to daies or key to keis
            {
                return str.Substring(0, str.Length - 1) + "ies";
            }
            else if (str.EndsWith("ss") == true)
            {
                return str + "es";      // Address -> Addresses for example
            }
            else if (str.EndsWith("sh") == true)
            {
                return str + "es";      // Finish -> Finishes for example
            }
            else
            {
                return str + "s";
            }
        }

        public static string ConvertToAngularComponentName(string camelCaseName)
        {
            if (string.IsNullOrWhiteSpace(camelCaseName))
                throw new ArgumentException("Input cannot be null or empty", nameof(camelCaseName));

            // Use a regex to find uppercase letters and prepend them with a hyphen


            string kebabCase = Regex.Replace(camelCaseName, "(?<!^)([A-Z])", "-$1").ToLower();

            return kebabCase;
        }

        /// <summary>
        /// This turns a string into a header string with spaces added before uppper case characters, suited for camel case input.
        /// </summary>
        /// <param name="camelCaseName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ConvertToHeader(string camelCaseName)
        {
            if (string.IsNullOrWhiteSpace(camelCaseName))
            {
                throw new ArgumentException("Input cannot be null or empty", nameof(camelCaseName));
            }

            // Use a regex to add spaces before uppercase letters and numbers
            string spacedWords = Regex.Replace(camelCaseName, "(?<!^)([A-Z0-9])", " $1");

            // Capitalize the first letter of the resulting string
            string formattedHeader = char.ToUpper(spacedWords[0]) + spacedWords.Substring(1);

            return formattedHeader.Trim();
        }

        /// <summary>
        /// 
        /// This function will sanitize a string and make sure that it is safe to use as a file name on both Windows and Linux.
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="linuxOnly"></param>
        /// <returns></returns>
        public static string SanitizeFileName(string fileName, bool linuxOnly = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "default.txt"; // Fallback for empty or null input
            }

            // Define invalid characters
            char[] invalidChars = linuxOnly
                ? new[] { '/', '\0' } // Linux-specific: only / and null are invalid
                : Path.GetInvalidFileNameChars(); // Cross-platform (Windows-centric)

            // Replace invalid characters with an underscore
            var sanitized = new StringBuilder(fileName.Length);

            foreach (char c in fileName)
            {
                sanitized.Append(invalidChars.Contains(c) ? '_' : c);
            }

            // Trim leading/trailing spaces or dots
            string result = sanitized.ToString().Trim().Trim('.');

            // Prevent hidden files (starting with dot) in Linux
            if (linuxOnly && result.StartsWith("."))
            {
                result = $"_{result}";
            }

            // Ensure the result is not empty
            if (string.IsNullOrWhiteSpace(result))
            {
                return "default.txt";
            }

            // Truncate to safe length (255 bytes for Linux, approximated as chars for simplicity)
            if (linuxOnly && Encoding.UTF8.GetByteCount(result) > 255)
            {
                // Truncate to approximate 255 bytes (simplified; for precision, iterate to find exact cut-off)
                result = result.Substring(0, Math.Min(result.Length, 255));
            }
            else if (result.Length > 255) // Cross-platform length limit
            {
                result = result.Substring(0, 255);
            }

            return result;
        }
    }
}
