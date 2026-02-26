using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class IpAddressLocation : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IpAddressLocationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String ipAddress { get; set; }
			public String countryCode { get; set; }
			public String countryName { get; set; }
			public String city { get; set; }
			public Double? latitude { get; set; }
			public Double? longitude { get; set; }
			[Required]
			public DateTime lastLookupDate { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class IpAddressLocationOutputDTO : IpAddressLocationDTO
		{
		}


		/// <summary>
		///
		/// Converts a IpAddressLocation to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IpAddressLocationDTO ToDTO()
		{
			return new IpAddressLocationDTO
			{
				id = this.id,
				ipAddress = this.ipAddress,
				countryCode = this.countryCode,
				countryName = this.countryName,
				city = this.city,
				latitude = this.latitude,
				longitude = this.longitude,
				lastLookupDate = this.lastLookupDate,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a IpAddressLocation list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IpAddressLocationDTO> ToDTOList(List<IpAddressLocation> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IpAddressLocationDTO> output = new List<IpAddressLocationDTO>();

			output.Capacity = data.Count;

			foreach (IpAddressLocation ipAddressLocation in data)
			{
				output.Add(ipAddressLocation.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IpAddressLocation to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IpAddressLocationEntity type directly.
		///
		/// </summary>
		public IpAddressLocationOutputDTO ToOutputDTO()
		{
			return new IpAddressLocationOutputDTO
			{
				id = this.id,
				ipAddress = this.ipAddress,
				countryCode = this.countryCode,
				countryName = this.countryName,
				city = this.city,
				latitude = this.latitude,
				longitude = this.longitude,
				lastLookupDate = this.lastLookupDate,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a IpAddressLocation list to list of Output Data Transfer Object intended to be used for serializing a list of IpAddressLocation objects to avoid using the IpAddressLocation entity type directly.
		///
		/// </summary>
		public static List<IpAddressLocationOutputDTO> ToOutputDTOList(List<IpAddressLocation> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IpAddressLocationOutputDTO> output = new List<IpAddressLocationOutputDTO>();

			output.Capacity = data.Count;

			foreach (IpAddressLocation ipAddressLocation in data)
			{
				output.Add(ipAddressLocation.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IpAddressLocation Object.
		///
		/// </summary>
		public static Database.IpAddressLocation FromDTO(IpAddressLocationDTO dto)
		{
			return new Database.IpAddressLocation
			{
				id = dto.id,
				ipAddress = dto.ipAddress,
				countryCode = dto.countryCode,
				countryName = dto.countryName,
				city = dto.city,
				latitude = dto.latitude,
				longitude = dto.longitude,
				lastLookupDate = dto.lastLookupDate,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IpAddressLocation Object.
		///
		/// </summary>
		public void ApplyDTO(IpAddressLocationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.ipAddress = dto.ipAddress;
			this.countryCode = dto.countryCode;
			this.countryName = dto.countryName;
			this.city = dto.city;
			this.latitude = dto.latitude;
			this.longitude = dto.longitude;
			this.lastLookupDate = dto.lastLookupDate;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a IpAddressLocation Object.
		///
		/// </summary>
		public IpAddressLocation Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IpAddressLocation{
				id = this.id,
				ipAddress = this.ipAddress,
				countryCode = this.countryCode,
				countryName = this.countryName,
				city = this.city,
				latitude = this.latitude,
				longitude = this.longitude,
				lastLookupDate = this.lastLookupDate,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IpAddressLocation Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IpAddressLocation Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IpAddressLocation Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IpAddressLocation Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IpAddressLocation ipAddressLocation)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (ipAddressLocation == null)
			{
				return null;
			}

			return new {
				id = ipAddressLocation.id,
				ipAddress = ipAddressLocation.ipAddress,
				countryCode = ipAddressLocation.countryCode,
				countryName = ipAddressLocation.countryName,
				city = ipAddressLocation.city,
				latitude = ipAddressLocation.latitude,
				longitude = ipAddressLocation.longitude,
				lastLookupDate = ipAddressLocation.lastLookupDate,
				active = ipAddressLocation.active,
				deleted = ipAddressLocation.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IpAddressLocation Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IpAddressLocation ipAddressLocation)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (ipAddressLocation == null)
			{
				return null;
			}

			return new {
				id = ipAddressLocation.id,
				ipAddress = ipAddressLocation.ipAddress,
				countryCode = ipAddressLocation.countryCode,
				countryName = ipAddressLocation.countryName,
				city = ipAddressLocation.city,
				latitude = ipAddressLocation.latitude,
				longitude = ipAddressLocation.longitude,
				lastLookupDate = ipAddressLocation.lastLookupDate,
				active = ipAddressLocation.active,
				deleted = ipAddressLocation.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IpAddressLocation Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IpAddressLocation ipAddressLocation)
		{
			//
			// Return a very minimal object.
			//
			if (ipAddressLocation == null)
			{
				return null;
			}

			return new {
				id = ipAddressLocation.id,
				name = string.Join(", ", new[] { ipAddressLocation.ipAddress}.Where(s => !string.IsNullOrWhiteSpace(s))),
				description = string.Join(", ", new[] { ipAddressLocation.ipAddress, ipAddressLocation.countryCode, ipAddressLocation.countryName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
