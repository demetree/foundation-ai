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
	public partial class TelemetryDiskHealth : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryDiskHealthDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetrySnapshotId { get; set; }
			[Required]
			public String driveName { get; set; }
			public String driveLabel { get; set; }
			public Double? totalGB { get; set; }
			public Double? freeGB { get; set; }
			public Double? freePercent { get; set; }
			public Double? usedPercent { get; set; }
			public String status { get; set; }
			[Required]
			public Boolean isApplicationDrive { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryDiskHealthOutputDTO : TelemetryDiskHealthDTO
		{
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetryDiskHealth to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryDiskHealthDTO ToDTO()
		{
			return new TelemetryDiskHealthDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				driveName = this.driveName,
				driveLabel = this.driveLabel,
				totalGB = this.totalGB,
				freeGB = this.freeGB,
				freePercent = this.freePercent,
				usedPercent = this.usedPercent,
				status = this.status,
				isApplicationDrive = this.isApplicationDrive
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryDiskHealth list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryDiskHealthDTO> ToDTOList(List<TelemetryDiskHealth> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryDiskHealthDTO> output = new List<TelemetryDiskHealthDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryDiskHealth telemetryDiskHealth in data)
			{
				output.Add(telemetryDiskHealth.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryDiskHealth to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryDiskHealthEntity type directly.
		///
		/// </summary>
		public TelemetryDiskHealthOutputDTO ToOutputDTO()
		{
			return new TelemetryDiskHealthOutputDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				driveName = this.driveName,
				driveLabel = this.driveLabel,
				totalGB = this.totalGB,
				freeGB = this.freeGB,
				freePercent = this.freePercent,
				usedPercent = this.usedPercent,
				status = this.status,
				isApplicationDrive = this.isApplicationDrive,
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryDiskHealth list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryDiskHealth objects to avoid using the TelemetryDiskHealth entity type directly.
		///
		/// </summary>
		public static List<TelemetryDiskHealthOutputDTO> ToOutputDTOList(List<TelemetryDiskHealth> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryDiskHealthOutputDTO> output = new List<TelemetryDiskHealthOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryDiskHealth telemetryDiskHealth in data)
			{
				output.Add(telemetryDiskHealth.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryDiskHealth Object.
		///
		/// </summary>
		public static Database.TelemetryDiskHealth FromDTO(TelemetryDiskHealthDTO dto)
		{
			return new Database.TelemetryDiskHealth
			{
				id = dto.id,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				driveName = dto.driveName,
				driveLabel = dto.driveLabel,
				totalGB = dto.totalGB,
				freeGB = dto.freeGB,
				freePercent = dto.freePercent,
				usedPercent = dto.usedPercent,
				status = dto.status,
				isApplicationDrive = dto.isApplicationDrive
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryDiskHealth Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryDiskHealthDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.driveName = dto.driveName;
			this.driveLabel = dto.driveLabel;
			this.totalGB = dto.totalGB;
			this.freeGB = dto.freeGB;
			this.freePercent = dto.freePercent;
			this.usedPercent = dto.usedPercent;
			this.status = dto.status;
			this.isApplicationDrive = dto.isApplicationDrive;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryDiskHealth Object.
		///
		/// </summary>
		public TelemetryDiskHealth Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryDiskHealth{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				driveName = this.driveName,
				driveLabel = this.driveLabel,
				totalGB = this.totalGB,
				freeGB = this.freeGB,
				freePercent = this.freePercent,
				usedPercent = this.usedPercent,
				status = this.status,
				isApplicationDrive = this.isApplicationDrive,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryDiskHealth Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryDiskHealth Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryDiskHealth Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryDiskHealth Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TelemetryDiskHealth telemetryDiskHealth)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryDiskHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryDiskHealth.id,
				telemetrySnapshotId = telemetryDiskHealth.telemetrySnapshotId,
				driveName = telemetryDiskHealth.driveName,
				driveLabel = telemetryDiskHealth.driveLabel,
				totalGB = telemetryDiskHealth.totalGB,
				freeGB = telemetryDiskHealth.freeGB,
				freePercent = telemetryDiskHealth.freePercent,
				usedPercent = telemetryDiskHealth.usedPercent,
				status = telemetryDiskHealth.status,
				isApplicationDrive = telemetryDiskHealth.isApplicationDrive,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryDiskHealth Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryDiskHealth telemetryDiskHealth)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryDiskHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryDiskHealth.id,
				telemetrySnapshotId = telemetryDiskHealth.telemetrySnapshotId,
				driveName = telemetryDiskHealth.driveName,
				driveLabel = telemetryDiskHealth.driveLabel,
				totalGB = telemetryDiskHealth.totalGB,
				freeGB = telemetryDiskHealth.freeGB,
				freePercent = telemetryDiskHealth.freePercent,
				usedPercent = telemetryDiskHealth.usedPercent,
				status = telemetryDiskHealth.status,
				isApplicationDrive = telemetryDiskHealth.isApplicationDrive,
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetryDiskHealth.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryDiskHealth Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryDiskHealth telemetryDiskHealth)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryDiskHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryDiskHealth.id,
				name = telemetryDiskHealth.driveName,
				description = string.Join(", ", new[] { telemetryDiskHealth.driveName, telemetryDiskHealth.driveLabel, telemetryDiskHealth.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
