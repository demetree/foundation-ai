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
	public partial class IncidentNoteChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)incidentNoteId; }
			set { incidentNoteId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IncidentNoteChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 incidentNoteId { get; set; }
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
		public class IncidentNoteChangeHistoryOutputDTO : IncidentNoteChangeHistoryDTO
		{
			public IncidentNote.IncidentNoteDTO incidentNote { get; set; }
		}


		/// <summary>
		///
		/// Converts a IncidentNoteChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentNoteChangeHistoryDTO ToDTO()
		{
			return new IncidentNoteChangeHistoryDTO
			{
				id = this.id,
				incidentNoteId = this.incidentNoteId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a IncidentNoteChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentNoteChangeHistoryDTO> ToDTOList(List<IncidentNoteChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentNoteChangeHistoryDTO> output = new List<IncidentNoteChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (IncidentNoteChangeHistory incidentNoteChangeHistory in data)
			{
				output.Add(incidentNoteChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IncidentNoteChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentNoteChangeHistoryEntity type directly.
		///
		/// </summary>
		public IncidentNoteChangeHistoryOutputDTO ToOutputDTO()
		{
			return new IncidentNoteChangeHistoryOutputDTO
			{
				id = this.id,
				incidentNoteId = this.incidentNoteId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				incidentNote = this.incidentNote?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a IncidentNoteChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of IncidentNoteChangeHistory objects to avoid using the IncidentNoteChangeHistory entity type directly.
		///
		/// </summary>
		public static List<IncidentNoteChangeHistoryOutputDTO> ToOutputDTOList(List<IncidentNoteChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentNoteChangeHistoryOutputDTO> output = new List<IncidentNoteChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (IncidentNoteChangeHistory incidentNoteChangeHistory in data)
			{
				output.Add(incidentNoteChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IncidentNoteChangeHistory Object.
		///
		/// </summary>
		public static Database.IncidentNoteChangeHistory FromDTO(IncidentNoteChangeHistoryDTO dto)
		{
			return new Database.IncidentNoteChangeHistory
			{
				id = dto.id,
				incidentNoteId = dto.incidentNoteId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IncidentNoteChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentNoteChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentNoteId = dto.incidentNoteId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a IncidentNoteChangeHistory Object.
		///
		/// </summary>
		public IncidentNoteChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IncidentNoteChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentNoteId = this.incidentNoteId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentNoteChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentNoteChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IncidentNoteChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentNoteChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IncidentNoteChangeHistory incidentNoteChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incidentNoteChangeHistory == null)
			{
				return null;
			}

			return new {
				id = incidentNoteChangeHistory.id,
				incidentNoteId = incidentNoteChangeHistory.incidentNoteId,
				versionNumber = incidentNoteChangeHistory.versionNumber,
				timeStamp = incidentNoteChangeHistory.timeStamp,
				userId = incidentNoteChangeHistory.userId,
				data = incidentNoteChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentNoteChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IncidentNoteChangeHistory incidentNoteChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incidentNoteChangeHistory == null)
			{
				return null;
			}

			return new {
				id = incidentNoteChangeHistory.id,
				incidentNoteId = incidentNoteChangeHistory.incidentNoteId,
				versionNumber = incidentNoteChangeHistory.versionNumber,
				timeStamp = incidentNoteChangeHistory.timeStamp,
				userId = incidentNoteChangeHistory.userId,
				data = incidentNoteChangeHistory.data,
				incidentNote = IncidentNote.CreateMinimalAnonymous(incidentNoteChangeHistory.incidentNote)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IncidentNoteChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IncidentNoteChangeHistory incidentNoteChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (incidentNoteChangeHistory == null)
			{
				return null;
			}

			return new {
				id = incidentNoteChangeHistory.id,
				name = incidentNoteChangeHistory.id,
				description = incidentNoteChangeHistory.id
			 };
		}
	}
}
