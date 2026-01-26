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
	public partial class TelemetryDatabaseHealth : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryDatabaseHealthDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetrySnapshotId { get; set; }
			[Required]
			public String databaseName { get; set; }
			[Required]
			public Boolean isConnected { get; set; }
			public String status { get; set; }
			public String server { get; set; }
			public String provider { get; set; }
			public String errorMessage { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryDatabaseHealthOutputDTO : TelemetryDatabaseHealthDTO
		{
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetryDatabaseHealth to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryDatabaseHealthDTO ToDTO()
		{
			return new TelemetryDatabaseHealthDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				databaseName = this.databaseName,
				isConnected = this.isConnected,
				status = this.status,
				server = this.server,
				provider = this.provider,
				errorMessage = this.errorMessage
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryDatabaseHealth list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryDatabaseHealthDTO> ToDTOList(List<TelemetryDatabaseHealth> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryDatabaseHealthDTO> output = new List<TelemetryDatabaseHealthDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryDatabaseHealth telemetryDatabaseHealth in data)
			{
				output.Add(telemetryDatabaseHealth.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryDatabaseHealth to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryDatabaseHealthEntity type directly.
		///
		/// </summary>
		public TelemetryDatabaseHealthOutputDTO ToOutputDTO()
		{
			return new TelemetryDatabaseHealthOutputDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				databaseName = this.databaseName,
				isConnected = this.isConnected,
				status = this.status,
				server = this.server,
				provider = this.provider,
				errorMessage = this.errorMessage,
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryDatabaseHealth list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryDatabaseHealth objects to avoid using the TelemetryDatabaseHealth entity type directly.
		///
		/// </summary>
		public static List<TelemetryDatabaseHealthOutputDTO> ToOutputDTOList(List<TelemetryDatabaseHealth> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryDatabaseHealthOutputDTO> output = new List<TelemetryDatabaseHealthOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryDatabaseHealth telemetryDatabaseHealth in data)
			{
				output.Add(telemetryDatabaseHealth.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryDatabaseHealth Object.
		///
		/// </summary>
		public static Telemetry.Database.TelemetryDatabaseHealth FromDTO(TelemetryDatabaseHealthDTO dto)
		{
			return new Telemetry.Database.TelemetryDatabaseHealth
			{
				id = dto.id,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				databaseName = dto.databaseName,
				isConnected = dto.isConnected,
				status = dto.status,
				server = dto.server,
				provider = dto.provider,
				errorMessage = dto.errorMessage
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryDatabaseHealth Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryDatabaseHealthDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.databaseName = dto.databaseName;
			this.isConnected = dto.isConnected;
			this.status = dto.status;
			this.server = dto.server;
			this.provider = dto.provider;
			this.errorMessage = dto.errorMessage;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryDatabaseHealth Object.
		///
		/// </summary>
		public TelemetryDatabaseHealth Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryDatabaseHealth{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				databaseName = this.databaseName,
				isConnected = this.isConnected,
				status = this.status,
				server = this.server,
				provider = this.provider,
				errorMessage = this.errorMessage,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryDatabaseHealth Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryDatabaseHealth Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryDatabaseHealth Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryDatabaseHealth Object.
		///
		/// </summary>
		public static object CreateAnonymous(Telemetry.Database.TelemetryDatabaseHealth telemetryDatabaseHealth)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryDatabaseHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryDatabaseHealth.id,
				telemetrySnapshotId = telemetryDatabaseHealth.telemetrySnapshotId,
				databaseName = telemetryDatabaseHealth.databaseName,
				isConnected = telemetryDatabaseHealth.isConnected,
				status = telemetryDatabaseHealth.status,
				server = telemetryDatabaseHealth.server,
				provider = telemetryDatabaseHealth.provider,
				errorMessage = telemetryDatabaseHealth.errorMessage,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryDatabaseHealth Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryDatabaseHealth telemetryDatabaseHealth)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryDatabaseHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryDatabaseHealth.id,
				telemetrySnapshotId = telemetryDatabaseHealth.telemetrySnapshotId,
				databaseName = telemetryDatabaseHealth.databaseName,
				isConnected = telemetryDatabaseHealth.isConnected,
				status = telemetryDatabaseHealth.status,
				server = telemetryDatabaseHealth.server,
				provider = telemetryDatabaseHealth.provider,
				errorMessage = telemetryDatabaseHealth.errorMessage,
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetryDatabaseHealth.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryDatabaseHealth Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryDatabaseHealth telemetryDatabaseHealth)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryDatabaseHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryDatabaseHealth.id,
				name = telemetryDatabaseHealth.databaseName,
				description = string.Join(", ", new[] { telemetryDatabaseHealth.databaseName, telemetryDatabaseHealth.status, telemetryDatabaseHealth.server}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
