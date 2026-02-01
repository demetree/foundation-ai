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
	public partial class TelemetryLogError : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryLogErrorDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetryApplicationId { get; set; }
			public Int32? telemetrySnapshotId { get; set; }
			[Required]
			public DateTime capturedAt { get; set; }
			public String logFileName { get; set; }
			public DateTime? logTimestamp { get; set; }
			public String level { get; set; }
			public String message { get; set; }
			public String exception { get; set; }
			[Required]
			public Int32 occurrenceCount { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryLogErrorOutputDTO : TelemetryLogErrorDTO
		{
			public TelemetryApplication.TelemetryApplicationDTO telemetryApplication { get; set; }
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetryLogError to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryLogErrorDTO ToDTO()
		{
			return new TelemetryLogErrorDTO
			{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetrySnapshotId = this.telemetrySnapshotId,
				capturedAt = this.capturedAt,
				logFileName = this.logFileName,
				logTimestamp = this.logTimestamp,
				level = this.level,
				message = this.message,
				exception = this.exception,
				occurrenceCount = this.occurrenceCount
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryLogError list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryLogErrorDTO> ToDTOList(List<TelemetryLogError> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryLogErrorDTO> output = new List<TelemetryLogErrorDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryLogError telemetryLogError in data)
			{
				output.Add(telemetryLogError.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryLogError to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryLogErrorEntity type directly.
		///
		/// </summary>
		public TelemetryLogErrorOutputDTO ToOutputDTO()
		{
			return new TelemetryLogErrorOutputDTO
			{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetrySnapshotId = this.telemetrySnapshotId,
				capturedAt = this.capturedAt,
				logFileName = this.logFileName,
				logTimestamp = this.logTimestamp,
				level = this.level,
				message = this.message,
				exception = this.exception,
				occurrenceCount = this.occurrenceCount,
				telemetryApplication = this.telemetryApplication?.ToDTO(),
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryLogError list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryLogError objects to avoid using the TelemetryLogError entity type directly.
		///
		/// </summary>
		public static List<TelemetryLogErrorOutputDTO> ToOutputDTOList(List<TelemetryLogError> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryLogErrorOutputDTO> output = new List<TelemetryLogErrorOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryLogError telemetryLogError in data)
			{
				output.Add(telemetryLogError.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryLogError Object.
		///
		/// </summary>
		public static Database.TelemetryLogError FromDTO(TelemetryLogErrorDTO dto)
		{
			return new Database.TelemetryLogError
			{
				id = dto.id,
				telemetryApplicationId = dto.telemetryApplicationId,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				capturedAt = dto.capturedAt,
				logFileName = dto.logFileName,
				logTimestamp = dto.logTimestamp,
				level = dto.level,
				message = dto.message,
				exception = dto.exception,
				occurrenceCount = dto.occurrenceCount
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryLogError Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryLogErrorDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetryApplicationId = dto.telemetryApplicationId;
			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.capturedAt = dto.capturedAt;
			this.logFileName = dto.logFileName;
			this.logTimestamp = dto.logTimestamp;
			this.level = dto.level;
			this.message = dto.message;
			this.exception = dto.exception;
			this.occurrenceCount = dto.occurrenceCount;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryLogError Object.
		///
		/// </summary>
		public TelemetryLogError Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryLogError{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetrySnapshotId = this.telemetrySnapshotId,
				capturedAt = this.capturedAt,
				logFileName = this.logFileName,
				logTimestamp = this.logTimestamp,
				level = this.level,
				message = this.message,
				exception = this.exception,
				occurrenceCount = this.occurrenceCount,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryLogError Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryLogError Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryLogError Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryLogError Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TelemetryLogError telemetryLogError)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryLogError == null)
			{
				return null;
			}

			return new {
				id = telemetryLogError.id,
				telemetryApplicationId = telemetryLogError.telemetryApplicationId,
				telemetrySnapshotId = telemetryLogError.telemetrySnapshotId,
				capturedAt = telemetryLogError.capturedAt,
				logFileName = telemetryLogError.logFileName,
				logTimestamp = telemetryLogError.logTimestamp,
				level = telemetryLogError.level,
				message = telemetryLogError.message,
				exception = telemetryLogError.exception,
				occurrenceCount = telemetryLogError.occurrenceCount,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryLogError Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryLogError telemetryLogError)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryLogError == null)
			{
				return null;
			}

			return new {
				id = telemetryLogError.id,
				telemetryApplicationId = telemetryLogError.telemetryApplicationId,
				telemetrySnapshotId = telemetryLogError.telemetrySnapshotId,
				capturedAt = telemetryLogError.capturedAt,
				logFileName = telemetryLogError.logFileName,
				logTimestamp = telemetryLogError.logTimestamp,
				level = telemetryLogError.level,
				message = telemetryLogError.message,
				exception = telemetryLogError.exception,
				occurrenceCount = telemetryLogError.occurrenceCount,
				telemetryApplication = TelemetryApplication.CreateMinimalAnonymous(telemetryLogError.telemetryApplication),
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetryLogError.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryLogError Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryLogError telemetryLogError)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryLogError == null)
			{
				return null;
			}

			return new {
				id = telemetryLogError.id,
				name = telemetryLogError.logFileName,
				description = string.Join(", ", new[] { telemetryLogError.logFileName, telemetryLogError.level}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
