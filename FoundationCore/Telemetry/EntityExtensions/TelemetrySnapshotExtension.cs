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
	public partial class TelemetrySnapshot : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetrySnapshotDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetryApplicationId { get; set; }
			[Required]
			public Int32 telemetryCollectionRunId { get; set; }
			[Required]
			public DateTime collectedAt { get; set; }
			[Required]
			public Boolean isOnline { get; set; }
			public Int64? uptimeSeconds { get; set; }
			public Double? memoryWorkingSetMB { get; set; }
			public Double? memoryGcHeapMB { get; set; }
			public Double? memoryPercent { get; set; }
			public Double? cpuPercent { get; set; }
			public Int32? threadPoolWorkerThreads { get; set; }
			public Int32? threadPoolCompletionPortThreads { get; set; }
			public Int32? threadPoolPendingWorkItems { get; set; }
			public String machineName { get; set; }
			public String dotNetVersion { get; set; }
			public String statusJson { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetrySnapshotOutputDTO : TelemetrySnapshotDTO
		{
			public TelemetryApplication.TelemetryApplicationDTO telemetryApplication { get; set; }
			public TelemetryCollectionRun.TelemetryCollectionRunDTO telemetryCollectionRun { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetrySnapshot to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetrySnapshotDTO ToDTO()
		{
			return new TelemetrySnapshotDTO
			{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetryCollectionRunId = this.telemetryCollectionRunId,
				collectedAt = this.collectedAt,
				isOnline = this.isOnline,
				uptimeSeconds = this.uptimeSeconds,
				memoryWorkingSetMB = this.memoryWorkingSetMB,
				memoryGcHeapMB = this.memoryGcHeapMB,
				memoryPercent = this.memoryPercent,
				cpuPercent = this.cpuPercent,
				threadPoolWorkerThreads = this.threadPoolWorkerThreads,
				threadPoolCompletionPortThreads = this.threadPoolCompletionPortThreads,
				threadPoolPendingWorkItems = this.threadPoolPendingWorkItems,
				machineName = this.machineName,
				dotNetVersion = this.dotNetVersion,
				statusJson = this.statusJson
			};
		}


		/// <summary>
		///
		/// Converts a TelemetrySnapshot list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetrySnapshotDTO> ToDTOList(List<TelemetrySnapshot> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetrySnapshotDTO> output = new List<TelemetrySnapshotDTO>();

			output.Capacity = data.Count;

			foreach (TelemetrySnapshot telemetrySnapshot in data)
			{
				output.Add(telemetrySnapshot.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetrySnapshot to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetrySnapshotEntity type directly.
		///
		/// </summary>
		public TelemetrySnapshotOutputDTO ToOutputDTO()
		{
			return new TelemetrySnapshotOutputDTO
			{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetryCollectionRunId = this.telemetryCollectionRunId,
				collectedAt = this.collectedAt,
				isOnline = this.isOnline,
				uptimeSeconds = this.uptimeSeconds,
				memoryWorkingSetMB = this.memoryWorkingSetMB,
				memoryGcHeapMB = this.memoryGcHeapMB,
				memoryPercent = this.memoryPercent,
				cpuPercent = this.cpuPercent,
				threadPoolWorkerThreads = this.threadPoolWorkerThreads,
				threadPoolCompletionPortThreads = this.threadPoolCompletionPortThreads,
				threadPoolPendingWorkItems = this.threadPoolPendingWorkItems,
				machineName = this.machineName,
				dotNetVersion = this.dotNetVersion,
				statusJson = this.statusJson,
				telemetryApplication = this.telemetryApplication?.ToDTO(),
				telemetryCollectionRun = this.telemetryCollectionRun?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetrySnapshot list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetrySnapshot objects to avoid using the TelemetrySnapshot entity type directly.
		///
		/// </summary>
		public static List<TelemetrySnapshotOutputDTO> ToOutputDTOList(List<TelemetrySnapshot> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetrySnapshotOutputDTO> output = new List<TelemetrySnapshotOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetrySnapshot telemetrySnapshot in data)
			{
				output.Add(telemetrySnapshot.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetrySnapshot Object.
		///
		/// </summary>
		public static Database.TelemetrySnapshot FromDTO(TelemetrySnapshotDTO dto)
		{
			return new Database.TelemetrySnapshot
			{
				id = dto.id,
				telemetryApplicationId = dto.telemetryApplicationId,
				telemetryCollectionRunId = dto.telemetryCollectionRunId,
				collectedAt = dto.collectedAt,
				isOnline = dto.isOnline,
				uptimeSeconds = dto.uptimeSeconds,
				memoryWorkingSetMB = dto.memoryWorkingSetMB,
				memoryGcHeapMB = dto.memoryGcHeapMB,
				memoryPercent = dto.memoryPercent,
				cpuPercent = dto.cpuPercent,
				threadPoolWorkerThreads = dto.threadPoolWorkerThreads,
				threadPoolCompletionPortThreads = dto.threadPoolCompletionPortThreads,
				threadPoolPendingWorkItems = dto.threadPoolPendingWorkItems,
				machineName = dto.machineName,
				dotNetVersion = dto.dotNetVersion,
				statusJson = dto.statusJson
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetrySnapshot Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetrySnapshotDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetryApplicationId = dto.telemetryApplicationId;
			this.telemetryCollectionRunId = dto.telemetryCollectionRunId;
			this.collectedAt = dto.collectedAt;
			this.isOnline = dto.isOnline;
			this.uptimeSeconds = dto.uptimeSeconds;
			this.memoryWorkingSetMB = dto.memoryWorkingSetMB;
			this.memoryGcHeapMB = dto.memoryGcHeapMB;
			this.memoryPercent = dto.memoryPercent;
			this.cpuPercent = dto.cpuPercent;
			this.threadPoolWorkerThreads = dto.threadPoolWorkerThreads;
			this.threadPoolCompletionPortThreads = dto.threadPoolCompletionPortThreads;
			this.threadPoolPendingWorkItems = dto.threadPoolPendingWorkItems;
			this.machineName = dto.machineName;
			this.dotNetVersion = dto.dotNetVersion;
			this.statusJson = dto.statusJson;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetrySnapshot Object.
		///
		/// </summary>
		public TelemetrySnapshot Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetrySnapshot{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetryCollectionRunId = this.telemetryCollectionRunId,
				collectedAt = this.collectedAt,
				isOnline = this.isOnline,
				uptimeSeconds = this.uptimeSeconds,
				memoryWorkingSetMB = this.memoryWorkingSetMB,
				memoryGcHeapMB = this.memoryGcHeapMB,
				memoryPercent = this.memoryPercent,
				cpuPercent = this.cpuPercent,
				threadPoolWorkerThreads = this.threadPoolWorkerThreads,
				threadPoolCompletionPortThreads = this.threadPoolCompletionPortThreads,
				threadPoolPendingWorkItems = this.threadPoolPendingWorkItems,
				machineName = this.machineName,
				dotNetVersion = this.dotNetVersion,
				statusJson = this.statusJson,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetrySnapshot Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetrySnapshot Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetrySnapshot Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetrySnapshot Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TelemetrySnapshot telemetrySnapshot)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetrySnapshot == null)
			{
				return null;
			}

			return new {
				id = telemetrySnapshot.id,
				telemetryApplicationId = telemetrySnapshot.telemetryApplicationId,
				telemetryCollectionRunId = telemetrySnapshot.telemetryCollectionRunId,
				collectedAt = telemetrySnapshot.collectedAt,
				isOnline = telemetrySnapshot.isOnline,
				uptimeSeconds = telemetrySnapshot.uptimeSeconds,
				memoryWorkingSetMB = telemetrySnapshot.memoryWorkingSetMB,
				memoryGcHeapMB = telemetrySnapshot.memoryGcHeapMB,
				memoryPercent = telemetrySnapshot.memoryPercent,
				cpuPercent = telemetrySnapshot.cpuPercent,
				threadPoolWorkerThreads = telemetrySnapshot.threadPoolWorkerThreads,
				threadPoolCompletionPortThreads = telemetrySnapshot.threadPoolCompletionPortThreads,
				threadPoolPendingWorkItems = telemetrySnapshot.threadPoolPendingWorkItems,
				machineName = telemetrySnapshot.machineName,
				dotNetVersion = telemetrySnapshot.dotNetVersion,
				statusJson = telemetrySnapshot.statusJson,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetrySnapshot Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetrySnapshot telemetrySnapshot)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetrySnapshot == null)
			{
				return null;
			}

			return new {
				id = telemetrySnapshot.id,
				telemetryApplicationId = telemetrySnapshot.telemetryApplicationId,
				telemetryCollectionRunId = telemetrySnapshot.telemetryCollectionRunId,
				collectedAt = telemetrySnapshot.collectedAt,
				isOnline = telemetrySnapshot.isOnline,
				uptimeSeconds = telemetrySnapshot.uptimeSeconds,
				memoryWorkingSetMB = telemetrySnapshot.memoryWorkingSetMB,
				memoryGcHeapMB = telemetrySnapshot.memoryGcHeapMB,
				memoryPercent = telemetrySnapshot.memoryPercent,
				cpuPercent = telemetrySnapshot.cpuPercent,
				threadPoolWorkerThreads = telemetrySnapshot.threadPoolWorkerThreads,
				threadPoolCompletionPortThreads = telemetrySnapshot.threadPoolCompletionPortThreads,
				threadPoolPendingWorkItems = telemetrySnapshot.threadPoolPendingWorkItems,
				machineName = telemetrySnapshot.machineName,
				dotNetVersion = telemetrySnapshot.dotNetVersion,
				statusJson = telemetrySnapshot.statusJson,
				telemetryApplication = TelemetryApplication.CreateMinimalAnonymous(telemetrySnapshot.telemetryApplication),
				telemetryCollectionRun = TelemetryCollectionRun.CreateMinimalAnonymous(telemetrySnapshot.telemetryCollectionRun)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetrySnapshot Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetrySnapshot telemetrySnapshot)
		{
			//
			// Return a very minimal object.
			//
			if (telemetrySnapshot == null)
			{
				return null;
			}

			return new {
				id = telemetrySnapshot.id,
				name = telemetrySnapshot.machineName,
				description = string.Join(", ", new[] { telemetrySnapshot.machineName, telemetrySnapshot.dotNetVersion}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
