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
	public partial class TelemetryNetworkHealth : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryNetworkHealthDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetrySnapshotId { get; set; }
			[Required]
			public String interfaceName { get; set; }
			public String interfaceDescription { get; set; }
			public Double? linkSpeedMbps { get; set; }
			public Int64? bytesSentTotal { get; set; }
			public Int64? bytesReceivedTotal { get; set; }
			public Double? bytesSentPerSecond { get; set; }
			public Double? bytesReceivedPerSecond { get; set; }
			public Double? utilizationPercent { get; set; }
			public String status { get; set; }
			[Required]
			public Boolean isActive { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryNetworkHealthOutputDTO : TelemetryNetworkHealthDTO
		{
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetryNetworkHealth to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryNetworkHealthDTO ToDTO()
		{
			return new TelemetryNetworkHealthDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				interfaceName = this.interfaceName,
				interfaceDescription = this.interfaceDescription,
				linkSpeedMbps = this.linkSpeedMbps,
				bytesSentTotal = this.bytesSentTotal,
				bytesReceivedTotal = this.bytesReceivedTotal,
				bytesSentPerSecond = this.bytesSentPerSecond,
				bytesReceivedPerSecond = this.bytesReceivedPerSecond,
				utilizationPercent = this.utilizationPercent,
				status = this.status,
				isActive = this.isActive
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryNetworkHealth list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryNetworkHealthDTO> ToDTOList(List<TelemetryNetworkHealth> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryNetworkHealthDTO> output = new List<TelemetryNetworkHealthDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryNetworkHealth telemetryNetworkHealth in data)
			{
				output.Add(telemetryNetworkHealth.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryNetworkHealth to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryNetworkHealthEntity type directly.
		///
		/// </summary>
		public TelemetryNetworkHealthOutputDTO ToOutputDTO()
		{
			return new TelemetryNetworkHealthOutputDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				interfaceName = this.interfaceName,
				interfaceDescription = this.interfaceDescription,
				linkSpeedMbps = this.linkSpeedMbps,
				bytesSentTotal = this.bytesSentTotal,
				bytesReceivedTotal = this.bytesReceivedTotal,
				bytesSentPerSecond = this.bytesSentPerSecond,
				bytesReceivedPerSecond = this.bytesReceivedPerSecond,
				utilizationPercent = this.utilizationPercent,
				status = this.status,
				isActive = this.isActive,
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryNetworkHealth list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryNetworkHealth objects to avoid using the TelemetryNetworkHealth entity type directly.
		///
		/// </summary>
		public static List<TelemetryNetworkHealthOutputDTO> ToOutputDTOList(List<TelemetryNetworkHealth> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryNetworkHealthOutputDTO> output = new List<TelemetryNetworkHealthOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryNetworkHealth telemetryNetworkHealth in data)
			{
				output.Add(telemetryNetworkHealth.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryNetworkHealth Object.
		///
		/// </summary>
		public static Database.TelemetryNetworkHealth FromDTO(TelemetryNetworkHealthDTO dto)
		{
			return new Database.TelemetryNetworkHealth
			{
				id = dto.id,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				interfaceName = dto.interfaceName,
				interfaceDescription = dto.interfaceDescription,
				linkSpeedMbps = dto.linkSpeedMbps,
				bytesSentTotal = dto.bytesSentTotal,
				bytesReceivedTotal = dto.bytesReceivedTotal,
				bytesSentPerSecond = dto.bytesSentPerSecond,
				bytesReceivedPerSecond = dto.bytesReceivedPerSecond,
				utilizationPercent = dto.utilizationPercent,
				status = dto.status,
				isActive = dto.isActive
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryNetworkHealth Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryNetworkHealthDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.interfaceName = dto.interfaceName;
			this.interfaceDescription = dto.interfaceDescription;
			this.linkSpeedMbps = dto.linkSpeedMbps;
			this.bytesSentTotal = dto.bytesSentTotal;
			this.bytesReceivedTotal = dto.bytesReceivedTotal;
			this.bytesSentPerSecond = dto.bytesSentPerSecond;
			this.bytesReceivedPerSecond = dto.bytesReceivedPerSecond;
			this.utilizationPercent = dto.utilizationPercent;
			this.status = dto.status;
			this.isActive = dto.isActive;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryNetworkHealth Object.
		///
		/// </summary>
		public TelemetryNetworkHealth Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryNetworkHealth{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				interfaceName = this.interfaceName,
				interfaceDescription = this.interfaceDescription,
				linkSpeedMbps = this.linkSpeedMbps,
				bytesSentTotal = this.bytesSentTotal,
				bytesReceivedTotal = this.bytesReceivedTotal,
				bytesSentPerSecond = this.bytesSentPerSecond,
				bytesReceivedPerSecond = this.bytesReceivedPerSecond,
				utilizationPercent = this.utilizationPercent,
				status = this.status,
				isActive = this.isActive,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryNetworkHealth Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryNetworkHealth Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryNetworkHealth Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryNetworkHealth Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TelemetryNetworkHealth telemetryNetworkHealth)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryNetworkHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryNetworkHealth.id,
				telemetrySnapshotId = telemetryNetworkHealth.telemetrySnapshotId,
				interfaceName = telemetryNetworkHealth.interfaceName,
				interfaceDescription = telemetryNetworkHealth.interfaceDescription,
				linkSpeedMbps = telemetryNetworkHealth.linkSpeedMbps,
				bytesSentTotal = telemetryNetworkHealth.bytesSentTotal,
				bytesReceivedTotal = telemetryNetworkHealth.bytesReceivedTotal,
				bytesSentPerSecond = telemetryNetworkHealth.bytesSentPerSecond,
				bytesReceivedPerSecond = telemetryNetworkHealth.bytesReceivedPerSecond,
				utilizationPercent = telemetryNetworkHealth.utilizationPercent,
				status = telemetryNetworkHealth.status,
				isActive = telemetryNetworkHealth.isActive,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryNetworkHealth Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryNetworkHealth telemetryNetworkHealth)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryNetworkHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryNetworkHealth.id,
				telemetrySnapshotId = telemetryNetworkHealth.telemetrySnapshotId,
				interfaceName = telemetryNetworkHealth.interfaceName,
				interfaceDescription = telemetryNetworkHealth.interfaceDescription,
				linkSpeedMbps = telemetryNetworkHealth.linkSpeedMbps,
				bytesSentTotal = telemetryNetworkHealth.bytesSentTotal,
				bytesReceivedTotal = telemetryNetworkHealth.bytesReceivedTotal,
				bytesSentPerSecond = telemetryNetworkHealth.bytesSentPerSecond,
				bytesReceivedPerSecond = telemetryNetworkHealth.bytesReceivedPerSecond,
				utilizationPercent = telemetryNetworkHealth.utilizationPercent,
				status = telemetryNetworkHealth.status,
				isActive = telemetryNetworkHealth.isActive,
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetryNetworkHealth.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryNetworkHealth Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryNetworkHealth telemetryNetworkHealth)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryNetworkHealth == null)
			{
				return null;
			}

			return new {
				id = telemetryNetworkHealth.id,
				name = telemetryNetworkHealth.interfaceName,
				description = string.Join(", ", new[] { telemetryNetworkHealth.interfaceName, telemetryNetworkHealth.interfaceDescription, telemetryNetworkHealth.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
