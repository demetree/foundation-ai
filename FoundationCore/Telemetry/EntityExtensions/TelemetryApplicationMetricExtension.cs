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
	public partial class TelemetryApplicationMetric : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryApplicationMetricDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetrySnapshotId { get; set; }
			[Required]
			public String metricName { get; set; }
			public String metricValue { get; set; }
			public Int32? state { get; set; }
			public Int32? dataType { get; set; }
			public Double? numericValue { get; set; }
			public String category { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryApplicationMetricOutputDTO : TelemetryApplicationMetricDTO
		{
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetryApplicationMetric to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryApplicationMetricDTO ToDTO()
		{
			return new TelemetryApplicationMetricDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				metricName = this.metricName,
				metricValue = this.metricValue,
				state = this.state,
				dataType = this.dataType,
				numericValue = this.numericValue,
				category = this.category
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryApplicationMetric list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryApplicationMetricDTO> ToDTOList(List<TelemetryApplicationMetric> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryApplicationMetricDTO> output = new List<TelemetryApplicationMetricDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryApplicationMetric telemetryApplicationMetric in data)
			{
				output.Add(telemetryApplicationMetric.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryApplicationMetric to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryApplicationMetricEntity type directly.
		///
		/// </summary>
		public TelemetryApplicationMetricOutputDTO ToOutputDTO()
		{
			return new TelemetryApplicationMetricOutputDTO
			{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				metricName = this.metricName,
				metricValue = this.metricValue,
				state = this.state,
				dataType = this.dataType,
				numericValue = this.numericValue,
				category = this.category,
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryApplicationMetric list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryApplicationMetric objects to avoid using the TelemetryApplicationMetric entity type directly.
		///
		/// </summary>
		public static List<TelemetryApplicationMetricOutputDTO> ToOutputDTOList(List<TelemetryApplicationMetric> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryApplicationMetricOutputDTO> output = new List<TelemetryApplicationMetricOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryApplicationMetric telemetryApplicationMetric in data)
			{
				output.Add(telemetryApplicationMetric.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryApplicationMetric Object.
		///
		/// </summary>
		public static Telemetry.Database.TelemetryApplicationMetric FromDTO(TelemetryApplicationMetricDTO dto)
		{
			return new Telemetry.Database.TelemetryApplicationMetric
			{
				id = dto.id,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				metricName = dto.metricName,
				metricValue = dto.metricValue,
				state = dto.state,
				dataType = dto.dataType,
				numericValue = dto.numericValue,
				category = dto.category
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryApplicationMetric Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryApplicationMetricDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.metricName = dto.metricName;
			this.metricValue = dto.metricValue;
			this.state = dto.state;
			this.dataType = dto.dataType;
			this.numericValue = dto.numericValue;
			this.category = dto.category;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryApplicationMetric Object.
		///
		/// </summary>
		public TelemetryApplicationMetric Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryApplicationMetric{
				id = this.id,
				telemetrySnapshotId = this.telemetrySnapshotId,
				metricName = this.metricName,
				metricValue = this.metricValue,
				state = this.state,
				dataType = this.dataType,
				numericValue = this.numericValue,
				category = this.category,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryApplicationMetric Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryApplicationMetric Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryApplicationMetric Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryApplicationMetric Object.
		///
		/// </summary>
		public static object CreateAnonymous(Telemetry.Database.TelemetryApplicationMetric telemetryApplicationMetric)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryApplicationMetric == null)
			{
				return null;
			}

			return new {
				id = telemetryApplicationMetric.id,
				telemetrySnapshotId = telemetryApplicationMetric.telemetrySnapshotId,
				metricName = telemetryApplicationMetric.metricName,
				metricValue = telemetryApplicationMetric.metricValue,
				state = telemetryApplicationMetric.state,
				dataType = telemetryApplicationMetric.dataType,
				numericValue = telemetryApplicationMetric.numericValue,
				category = telemetryApplicationMetric.category,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryApplicationMetric Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryApplicationMetric telemetryApplicationMetric)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryApplicationMetric == null)
			{
				return null;
			}

			return new {
				id = telemetryApplicationMetric.id,
				telemetrySnapshotId = telemetryApplicationMetric.telemetrySnapshotId,
				metricName = telemetryApplicationMetric.metricName,
				metricValue = telemetryApplicationMetric.metricValue,
				state = telemetryApplicationMetric.state,
				dataType = telemetryApplicationMetric.dataType,
				numericValue = telemetryApplicationMetric.numericValue,
				category = telemetryApplicationMetric.category,
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetryApplicationMetric.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryApplicationMetric Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryApplicationMetric telemetryApplicationMetric)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryApplicationMetric == null)
			{
				return null;
			}

			return new {
				id = telemetryApplicationMetric.id,
				name = telemetryApplicationMetric.metricName,
				description = string.Join(", ", new[] { telemetryApplicationMetric.metricName, telemetryApplicationMetric.metricValue, telemetryApplicationMetric.category}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
