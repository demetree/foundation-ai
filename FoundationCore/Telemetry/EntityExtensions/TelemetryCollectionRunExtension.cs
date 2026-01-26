using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Telemetry.Telemetry.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class TelemetryCollectionRun : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryCollectionRunDTO
		{
			public Int32 id { get; set; }
			[Required]
			public DateTime startTime { get; set; }
			public DateTime? endTime { get; set; }
			public Int32? applicationsPolled { get; set; }
			public Int32? applicationsSucceeded { get; set; }
			public String errorMessage { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryCollectionRunOutputDTO : TelemetryCollectionRunDTO
		{
		}


		/// <summary>
		///
		/// Converts a TelemetryCollectionRun to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryCollectionRunDTO ToDTO()
		{
			return new TelemetryCollectionRunDTO
			{
				id = this.id,
				startTime = this.startTime,
				endTime = this.endTime,
				applicationsPolled = this.applicationsPolled,
				applicationsSucceeded = this.applicationsSucceeded,
				errorMessage = this.errorMessage
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryCollectionRun list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryCollectionRunDTO> ToDTOList(List<TelemetryCollectionRun> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryCollectionRunDTO> output = new List<TelemetryCollectionRunDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryCollectionRun telemetryCollectionRun in data)
			{
				output.Add(telemetryCollectionRun.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryCollectionRun to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryCollectionRunEntity type directly.
		///
		/// </summary>
		public TelemetryCollectionRunOutputDTO ToOutputDTO()
		{
			return new TelemetryCollectionRunOutputDTO
			{
				id = this.id,
				startTime = this.startTime,
				endTime = this.endTime,
				applicationsPolled = this.applicationsPolled,
				applicationsSucceeded = this.applicationsSucceeded,
				errorMessage = this.errorMessage
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryCollectionRun list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryCollectionRun objects to avoid using the TelemetryCollectionRun entity type directly.
		///
		/// </summary>
		public static List<TelemetryCollectionRunOutputDTO> ToOutputDTOList(List<TelemetryCollectionRun> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryCollectionRunOutputDTO> output = new List<TelemetryCollectionRunOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryCollectionRun telemetryCollectionRun in data)
			{
				output.Add(telemetryCollectionRun.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryCollectionRun Object.
		///
		/// </summary>
		public static Telemetry.Database.TelemetryCollectionRun FromDTO(TelemetryCollectionRunDTO dto)
		{
			return new Telemetry.Database.TelemetryCollectionRun
			{
				id = dto.id,
				startTime = dto.startTime,
				endTime = dto.endTime,
				applicationsPolled = dto.applicationsPolled,
				applicationsSucceeded = dto.applicationsSucceeded,
				errorMessage = dto.errorMessage
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryCollectionRun Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryCollectionRunDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.startTime = dto.startTime;
			this.endTime = dto.endTime;
			this.applicationsPolled = dto.applicationsPolled;
			this.applicationsSucceeded = dto.applicationsSucceeded;
			this.errorMessage = dto.errorMessage;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryCollectionRun Object.
		///
		/// </summary>
		public TelemetryCollectionRun Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryCollectionRun{
				id = this.id,
				startTime = this.startTime,
				endTime = this.endTime,
				applicationsPolled = this.applicationsPolled,
				applicationsSucceeded = this.applicationsSucceeded,
				errorMessage = this.errorMessage,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryCollectionRun Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryCollectionRun Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryCollectionRun Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryCollectionRun Object.
		///
		/// </summary>
		public static object CreateAnonymous(Telemetry.Database.TelemetryCollectionRun telemetryCollectionRun)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryCollectionRun == null)
			{
				return null;
			}

			return new {
				id = telemetryCollectionRun.id,
				startTime = telemetryCollectionRun.startTime,
				endTime = telemetryCollectionRun.endTime,
				applicationsPolled = telemetryCollectionRun.applicationsPolled,
				applicationsSucceeded = telemetryCollectionRun.applicationsSucceeded,
				errorMessage = telemetryCollectionRun.errorMessage,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryCollectionRun Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryCollectionRun telemetryCollectionRun)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryCollectionRun == null)
			{
				return null;
			}

			return new {
				id = telemetryCollectionRun.id,
				startTime = telemetryCollectionRun.startTime,
				endTime = telemetryCollectionRun.endTime,
				applicationsPolled = telemetryCollectionRun.applicationsPolled,
				applicationsSucceeded = telemetryCollectionRun.applicationsSucceeded,
				errorMessage = telemetryCollectionRun.errorMessage,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryCollectionRun Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryCollectionRun telemetryCollectionRun)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryCollectionRun == null)
			{
				return null;
			}

			return new {
				id = telemetryCollectionRun.id,
				name = telemetryCollectionRun.id,
				description = telemetryCollectionRun.id
			 };
		}
	}
}
