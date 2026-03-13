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
	public partial class MocVersionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)mocVersionId; }
			set { mocVersionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocVersionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 mocVersionId { get; set; }
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
		public class MocVersionChangeHistoryOutputDTO : MocVersionChangeHistoryDTO
		{
			public MocVersion.MocVersionDTO mocVersion { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocVersionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocVersionChangeHistoryDTO ToDTO()
		{
			return new MocVersionChangeHistoryDTO
			{
				id = this.id,
				mocVersionId = this.mocVersionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a MocVersionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocVersionChangeHistoryDTO> ToDTOList(List<MocVersionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocVersionChangeHistoryDTO> output = new List<MocVersionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (MocVersionChangeHistory mocVersionChangeHistory in data)
			{
				output.Add(mocVersionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocVersionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocVersionChangeHistoryEntity type directly.
		///
		/// </summary>
		public MocVersionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new MocVersionChangeHistoryOutputDTO
			{
				id = this.id,
				mocVersionId = this.mocVersionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				mocVersion = this.mocVersion?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocVersionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of MocVersionChangeHistory objects to avoid using the MocVersionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<MocVersionChangeHistoryOutputDTO> ToOutputDTOList(List<MocVersionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocVersionChangeHistoryOutputDTO> output = new List<MocVersionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocVersionChangeHistory mocVersionChangeHistory in data)
			{
				output.Add(mocVersionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocVersionChangeHistory Object.
		///
		/// </summary>
		public static Database.MocVersionChangeHistory FromDTO(MocVersionChangeHistoryDTO dto)
		{
			return new Database.MocVersionChangeHistory
			{
				id = dto.id,
				mocVersionId = dto.mocVersionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocVersionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(MocVersionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.mocVersionId = dto.mocVersionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a MocVersionChangeHistory Object.
		///
		/// </summary>
		public MocVersionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocVersionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				mocVersionId = this.mocVersionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocVersionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocVersionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocVersionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocVersionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocVersionChangeHistory mocVersionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocVersionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = mocVersionChangeHistory.id,
				mocVersionId = mocVersionChangeHistory.mocVersionId,
				versionNumber = mocVersionChangeHistory.versionNumber,
				timeStamp = mocVersionChangeHistory.timeStamp,
				userId = mocVersionChangeHistory.userId,
				data = mocVersionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocVersionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocVersionChangeHistory mocVersionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocVersionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = mocVersionChangeHistory.id,
				mocVersionId = mocVersionChangeHistory.mocVersionId,
				versionNumber = mocVersionChangeHistory.versionNumber,
				timeStamp = mocVersionChangeHistory.timeStamp,
				userId = mocVersionChangeHistory.userId,
				data = mocVersionChangeHistory.data,
				mocVersion = MocVersion.CreateMinimalAnonymous(mocVersionChangeHistory.mocVersion),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocVersionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocVersionChangeHistory mocVersionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (mocVersionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = mocVersionChangeHistory.id,
				name = mocVersionChangeHistory.id,
				description = mocVersionChangeHistory.id
			 };
		}
	}
}
