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
	public partial class TelemetrySessionSnapshot : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetrySessionSnapshotDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetrySnapshotId { get; set; }
			[Required]
			public Int32 activeSessionCount { get; set; }
			public Int32? expiredSessionCount { get; set; }
			public DateTime? oldestSessionStart { get; set; }
			public DateTime? newestSessionStart { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetrySessionSnapshotOutputDTO : TelemetrySessionSnapshotDTO
		{
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetrySessionSnapshot to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetrySessionSnapshotDTO ToDTO()
		{
			return new TelemetrySessionSnapshotDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				activeSessionCount = this.activeSessionCount,
				expiredSessionCount = this.expiredSessionCount,
				oldestSessionStart = this.oldestSessionStart,
				newestSessionStart = this.newestSessionStart
			};
		}


		/// <summary>
		///
		/// Converts a TelemetrySessionSnapshot list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetrySessionSnapshotDTO> ToDTOList(List<TelemetrySessionSnapshot> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetrySessionSnapshotDTO> output = new List<TelemetrySessionSnapshotDTO>();

			output.Capacity = data.Count;

			foreach (TelemetrySessionSnapshot telemetrySessionSnapshot in data)
			{
				output.Add(telemetrySessionSnapshot.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetrySessionSnapshot to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetrySessionSnapshotEntity type directly.
		///
		/// </summary>
		public TelemetrySessionSnapshotOutputDTO ToOutputDTO()
		{
			return new TelemetrySessionSnapshotOutputDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				activeSessionCount = this.activeSessionCount,
				expiredSessionCount = this.expiredSessionCount,
				oldestSessionStart = this.oldestSessionStart,
				newestSessionStart = this.newestSessionStart,
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetrySessionSnapshot list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetrySessionSnapshot objects to avoid using the TelemetrySessionSnapshot entity type directly.
		///
		/// </summary>
		public static List<TelemetrySessionSnapshotOutputDTO> ToOutputDTOList(List<TelemetrySessionSnapshot> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetrySessionSnapshotOutputDTO> output = new List<TelemetrySessionSnapshotOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetrySessionSnapshot telemetrySessionSnapshot in data)
			{
				output.Add(telemetrySessionSnapshot.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetrySessionSnapshot Object.
		///
		/// </summary>
		public static Telemetry.Database.TelemetrySessionSnapshot FromDTO(TelemetrySessionSnapshotDTO dto)
		{
			return new Telemetry.Database.TelemetrySessionSnapshot
			{
				id = dto.id,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				activeSessionCount = dto.activeSessionCount,
				expiredSessionCount = dto.expiredSessionCount,
				oldestSessionStart = dto.oldestSessionStart,
				newestSessionStart = dto.newestSessionStart
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetrySessionSnapshot Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetrySessionSnapshotDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.activeSessionCount = dto.activeSessionCount;
			this.expiredSessionCount = dto.expiredSessionCount;
			this.oldestSessionStart = dto.oldestSessionStart;
			this.newestSessionStart = dto.newestSessionStart;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetrySessionSnapshot Object.
		///
		/// </summary>
		public TelemetrySessionSnapshot Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetrySessionSnapshot{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				activeSessionCount = this.activeSessionCount,
				expiredSessionCount = this.expiredSessionCount,
				oldestSessionStart = this.oldestSessionStart,
				newestSessionStart = this.newestSessionStart,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetrySessionSnapshot Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetrySessionSnapshot Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetrySessionSnapshot Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetrySessionSnapshot Object.
		///
		/// </summary>
		public static object CreateAnonymous(Telemetry.Database.TelemetrySessionSnapshot telemetrySessionSnapshot)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetrySessionSnapshot == null)
			{
				return null;
			}

			return new {
				id = telemetrySessionSnapshot.id,
				telemetrySnapshotId = telemetrySessionSnapshot.telemetrySnapshotId,
				activeSessionCount = telemetrySessionSnapshot.activeSessionCount,
				expiredSessionCount = telemetrySessionSnapshot.expiredSessionCount,
				oldestSessionStart = telemetrySessionSnapshot.oldestSessionStart,
				newestSessionStart = telemetrySessionSnapshot.newestSessionStart,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetrySessionSnapshot Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetrySessionSnapshot telemetrySessionSnapshot)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetrySessionSnapshot == null)
			{
				return null;
			}

			return new {
				id = telemetrySessionSnapshot.id,
				telemetrySnapshotId = telemetrySessionSnapshot.telemetrySnapshotId,
				activeSessionCount = telemetrySessionSnapshot.activeSessionCount,
				expiredSessionCount = telemetrySessionSnapshot.expiredSessionCount,
				oldestSessionStart = telemetrySessionSnapshot.oldestSessionStart,
				newestSessionStart = telemetrySessionSnapshot.newestSessionStart,
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetrySessionSnapshot.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetrySessionSnapshot Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetrySessionSnapshot telemetrySessionSnapshot)
		{
			//
			// Return a very minimal object.
			//
			if (telemetrySessionSnapshot == null)
			{
				return null;
			}

			return new {
				id = telemetrySessionSnapshot.id,
				name = telemetrySessionSnapshot.id,
				description = telemetrySessionSnapshot.id
			 };
		}
	}
}
