using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class TenantProfileChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)tenantProfileId; }
			set { tenantProfileId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TenantProfileChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 tenantProfileId { get; set; }
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
		public class TenantProfileChangeHistoryOutputDTO : TenantProfileChangeHistoryDTO
		{
			public TenantProfile.TenantProfileDTO tenantProfile { get; set; }
		}


		/// <summary>
		///
		/// Converts a TenantProfileChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TenantProfileChangeHistoryDTO ToDTO()
		{
			return new TenantProfileChangeHistoryDTO
			{
				id = this.id,
				tenantProfileId = this.tenantProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a TenantProfileChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TenantProfileChangeHistoryDTO> ToDTOList(List<TenantProfileChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TenantProfileChangeHistoryDTO> output = new List<TenantProfileChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (TenantProfileChangeHistory tenantProfileChangeHistory in data)
			{
				output.Add(tenantProfileChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TenantProfileChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TenantProfileChangeHistoryEntity type directly.
		///
		/// </summary>
		public TenantProfileChangeHistoryOutputDTO ToOutputDTO()
		{
			return new TenantProfileChangeHistoryOutputDTO
			{
				id = this.id,
				tenantProfileId = this.tenantProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				tenantProfile = this.tenantProfile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TenantProfileChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of TenantProfileChangeHistory objects to avoid using the TenantProfileChangeHistory entity type directly.
		///
		/// </summary>
		public static List<TenantProfileChangeHistoryOutputDTO> ToOutputDTOList(List<TenantProfileChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TenantProfileChangeHistoryOutputDTO> output = new List<TenantProfileChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (TenantProfileChangeHistory tenantProfileChangeHistory in data)
			{
				output.Add(tenantProfileChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TenantProfileChangeHistory Object.
		///
		/// </summary>
		public static Database.TenantProfileChangeHistory FromDTO(TenantProfileChangeHistoryDTO dto)
		{
			return new Database.TenantProfileChangeHistory
			{
				id = dto.id,
				tenantProfileId = dto.tenantProfileId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TenantProfileChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(TenantProfileChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.tenantProfileId = dto.tenantProfileId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TenantProfileChangeHistory Object.
		///
		/// </summary>
		public TenantProfileChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TenantProfileChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				tenantProfileId = this.tenantProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TenantProfileChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TenantProfileChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TenantProfileChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TenantProfileChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TenantProfileChangeHistory tenantProfileChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (tenantProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = tenantProfileChangeHistory.id,
				tenantProfileId = tenantProfileChangeHistory.tenantProfileId,
				versionNumber = tenantProfileChangeHistory.versionNumber,
				timeStamp = tenantProfileChangeHistory.timeStamp,
				userId = tenantProfileChangeHistory.userId,
				data = tenantProfileChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TenantProfileChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TenantProfileChangeHistory tenantProfileChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (tenantProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = tenantProfileChangeHistory.id,
				tenantProfileId = tenantProfileChangeHistory.tenantProfileId,
				versionNumber = tenantProfileChangeHistory.versionNumber,
				timeStamp = tenantProfileChangeHistory.timeStamp,
				userId = tenantProfileChangeHistory.userId,
				data = tenantProfileChangeHistory.data,
				tenantProfile = TenantProfile.CreateMinimalAnonymous(tenantProfileChangeHistory.tenantProfile),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TenantProfileChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TenantProfileChangeHistory tenantProfileChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (tenantProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = tenantProfileChangeHistory.id,
				name = tenantProfileChangeHistory.id,
				description = tenantProfileChangeHistory.id
			 };
		}
	}
}
