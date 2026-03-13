using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class MocCollaboratorChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)mocCollaboratorId; }
			set { mocCollaboratorId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocCollaboratorChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 mocCollaboratorId { get; set; }
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
		public class MocCollaboratorChangeHistoryOutputDTO : MocCollaboratorChangeHistoryDTO
		{
			public MocCollaborator.MocCollaboratorDTO mocCollaborator { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocCollaboratorChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocCollaboratorChangeHistoryDTO ToDTO()
		{
			return new MocCollaboratorChangeHistoryDTO
			{
				id = this.id,
				mocCollaboratorId = this.mocCollaboratorId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a MocCollaboratorChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocCollaboratorChangeHistoryDTO> ToDTOList(List<MocCollaboratorChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocCollaboratorChangeHistoryDTO> output = new List<MocCollaboratorChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (MocCollaboratorChangeHistory mocCollaboratorChangeHistory in data)
			{
				output.Add(mocCollaboratorChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocCollaboratorChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocCollaboratorChangeHistoryEntity type directly.
		///
		/// </summary>
		public MocCollaboratorChangeHistoryOutputDTO ToOutputDTO()
		{
			return new MocCollaboratorChangeHistoryOutputDTO
			{
				id = this.id,
				mocCollaboratorId = this.mocCollaboratorId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				mocCollaborator = this.mocCollaborator?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocCollaboratorChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of MocCollaboratorChangeHistory objects to avoid using the MocCollaboratorChangeHistory entity type directly.
		///
		/// </summary>
		public static List<MocCollaboratorChangeHistoryOutputDTO> ToOutputDTOList(List<MocCollaboratorChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocCollaboratorChangeHistoryOutputDTO> output = new List<MocCollaboratorChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocCollaboratorChangeHistory mocCollaboratorChangeHistory in data)
			{
				output.Add(mocCollaboratorChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocCollaboratorChangeHistory Object.
		///
		/// </summary>
		public static Database.MocCollaboratorChangeHistory FromDTO(MocCollaboratorChangeHistoryDTO dto)
		{
			return new Database.MocCollaboratorChangeHistory
			{
				id = dto.id,
				mocCollaboratorId = dto.mocCollaboratorId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocCollaboratorChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(MocCollaboratorChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.mocCollaboratorId = dto.mocCollaboratorId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a MocCollaboratorChangeHistory Object.
		///
		/// </summary>
		public MocCollaboratorChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocCollaboratorChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				mocCollaboratorId = this.mocCollaboratorId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocCollaboratorChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocCollaboratorChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocCollaboratorChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocCollaboratorChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocCollaboratorChangeHistory mocCollaboratorChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocCollaboratorChangeHistory == null)
			{
				return null;
			}

			return new {
				id = mocCollaboratorChangeHistory.id,
				mocCollaboratorId = mocCollaboratorChangeHistory.mocCollaboratorId,
				versionNumber = mocCollaboratorChangeHistory.versionNumber,
				timeStamp = mocCollaboratorChangeHistory.timeStamp,
				userId = mocCollaboratorChangeHistory.userId,
				data = mocCollaboratorChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocCollaboratorChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocCollaboratorChangeHistory mocCollaboratorChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocCollaboratorChangeHistory == null)
			{
				return null;
			}

			return new {
				id = mocCollaboratorChangeHistory.id,
				mocCollaboratorId = mocCollaboratorChangeHistory.mocCollaboratorId,
				versionNumber = mocCollaboratorChangeHistory.versionNumber,
				timeStamp = mocCollaboratorChangeHistory.timeStamp,
				userId = mocCollaboratorChangeHistory.userId,
				data = mocCollaboratorChangeHistory.data,
				mocCollaborator = MocCollaborator.CreateMinimalAnonymous(mocCollaboratorChangeHistory.mocCollaborator)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocCollaboratorChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocCollaboratorChangeHistory mocCollaboratorChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (mocCollaboratorChangeHistory == null)
			{
				return null;
			}

			return new {
				id = mocCollaboratorChangeHistory.id,
				name = mocCollaboratorChangeHistory.id,
				description = mocCollaboratorChangeHistory.id
			 };
		}
	}
}
