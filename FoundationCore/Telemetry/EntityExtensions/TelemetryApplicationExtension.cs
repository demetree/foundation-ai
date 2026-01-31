using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Telemetry.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class TelemetryApplication : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryApplicationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String url { get; set; }
			[Required]
			public Boolean isSelf { get; set; }
			[Required]
			public DateTime firstSeen { get; set; }
			public DateTime? lastSeen { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryApplicationOutputDTO : TelemetryApplicationDTO
		{
		}


		/// <summary>
		///
		/// Converts a TelemetryApplication to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryApplicationDTO ToDTO()
		{
			return new TelemetryApplicationDTO
			{
				id = this.id,
				name = this.name,
				url = this.url,
				isSelf = this.isSelf,
				firstSeen = this.firstSeen,
				lastSeen = this.lastSeen
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryApplication list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryApplicationDTO> ToDTOList(List<TelemetryApplication> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryApplicationDTO> output = new List<TelemetryApplicationDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryApplication telemetryApplication in data)
			{
				output.Add(telemetryApplication.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryApplication to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryApplicationEntity type directly.
		///
		/// </summary>
		public TelemetryApplicationOutputDTO ToOutputDTO()
		{
			return new TelemetryApplicationOutputDTO
			{
				id = this.id,
				name = this.name,
				url = this.url,
				isSelf = this.isSelf,
				firstSeen = this.firstSeen,
				lastSeen = this.lastSeen
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryApplication list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryApplication objects to avoid using the TelemetryApplication entity type directly.
		///
		/// </summary>
		public static List<TelemetryApplicationOutputDTO> ToOutputDTOList(List<TelemetryApplication> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryApplicationOutputDTO> output = new List<TelemetryApplicationOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryApplication telemetryApplication in data)
			{
				output.Add(telemetryApplication.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryApplication Object.
		///
		/// </summary>
		public static Database.TelemetryApplication FromDTO(TelemetryApplicationDTO dto)
		{
			return new Database.TelemetryApplication
			{
				id = dto.id,
				name = dto.name,
				url = dto.url,
				isSelf = dto.isSelf,
				firstSeen = dto.firstSeen,
				lastSeen = dto.lastSeen
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryApplication Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryApplicationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.url = dto.url;
			this.isSelf = dto.isSelf;
			this.firstSeen = dto.firstSeen;
			this.lastSeen = dto.lastSeen;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryApplication Object.
		///
		/// </summary>
		public TelemetryApplication Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryApplication{
				id = this.id,
				name = this.name,
				url = this.url,
				isSelf = this.isSelf,
				firstSeen = this.firstSeen,
				lastSeen = this.lastSeen,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryApplication Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryApplication Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryApplication Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryApplication Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TelemetryApplication telemetryApplication)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryApplication == null)
			{
				return null;
			}

			return new {
				id = telemetryApplication.id,
				name = telemetryApplication.name,
				url = telemetryApplication.url,
				isSelf = telemetryApplication.isSelf,
				firstSeen = telemetryApplication.firstSeen,
				lastSeen = telemetryApplication.lastSeen,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryApplication Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryApplication telemetryApplication)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryApplication == null)
			{
				return null;
			}

			return new {
				id = telemetryApplication.id,
				name = telemetryApplication.name,
				url = telemetryApplication.url,
				isSelf = telemetryApplication.isSelf,
				firstSeen = telemetryApplication.firstSeen,
				lastSeen = telemetryApplication.lastSeen,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryApplication Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryApplication telemetryApplication)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryApplication == null)
			{
				return null;
			}

			return new {
				id = telemetryApplication.id,
				name = telemetryApplication.name,
				description = string.Join(", ", new[] { telemetryApplication.name, telemetryApplication.url}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
