using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class IncidentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)incidentId; }
			set { incidentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IncidentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 incidentId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class IncidentChangeHistoryOutputDTO : IncidentChangeHistoryDTO
		{
			public Incident.IncidentDTO incident { get; set; }
		}


		/// <summary>
		///
		/// Converts a IncidentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentChangeHistoryDTO ToDTO()
		{
			return new IncidentChangeHistoryDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a IncidentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentChangeHistoryDTO> ToDTOList(List<IncidentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentChangeHistoryDTO> output = new List<IncidentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (IncidentChangeHistory incidentChangeHistory in data)
			{
				output.Add(incidentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IncidentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentChangeHistoryEntity type directly.
		///
		/// </summary>
		public IncidentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new IncidentChangeHistoryOutputDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				incident = this.incident?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a IncidentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of IncidentChangeHistory objects to avoid using the IncidentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<IncidentChangeHistoryOutputDTO> ToOutputDTOList(List<IncidentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentChangeHistoryOutputDTO> output = new List<IncidentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (IncidentChangeHistory incidentChangeHistory in data)
			{
				output.Add(incidentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IncidentChangeHistory Object.
		///
		/// </summary>
		public static Database.IncidentChangeHistory FromDTO(IncidentChangeHistoryDTO dto)
		{
			return new Database.IncidentChangeHistory
			{
				id = dto.id,
				incidentId = dto.incidentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IncidentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentId = dto.incidentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a IncidentChangeHistory Object.
		///
		/// </summary>
		public IncidentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IncidentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentId = this.incidentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IncidentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IncidentChangeHistory incidentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incidentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = incidentChangeHistory.id,
				incidentId = incidentChangeHistory.incidentId,
				versionNumber = incidentChangeHistory.versionNumber,
				timeStamp = incidentChangeHistory.timeStamp,
				userId = incidentChangeHistory.userId,
				data = incidentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IncidentChangeHistory incidentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incidentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = incidentChangeHistory.id,
				incidentId = incidentChangeHistory.incidentId,
				versionNumber = incidentChangeHistory.versionNumber,
				timeStamp = incidentChangeHistory.timeStamp,
				userId = incidentChangeHistory.userId,
				data = incidentChangeHistory.data,
				incident = Incident.CreateMinimalAnonymous(incidentChangeHistory.incident)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IncidentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IncidentChangeHistory incidentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (incidentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = incidentChangeHistory.id,
				name = incidentChangeHistory.id,
				description = incidentChangeHistory.id
			 };
		}
	}
}
