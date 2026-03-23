using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class TenantQuotaChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)tenantQuotaId; }
			set { tenantQuotaId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TenantQuotaChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 tenantQuotaId { get; set; }
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
		public class TenantQuotaChangeHistoryOutputDTO : TenantQuotaChangeHistoryDTO
		{
			public TenantQuota.TenantQuotaDTO tenantQuota { get; set; }
		}


		/// <summary>
		///
		/// Converts a TenantQuotaChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TenantQuotaChangeHistoryDTO ToDTO()
		{
			return new TenantQuotaChangeHistoryDTO
			{
				id = this.id,
				tenantQuotaId = this.tenantQuotaId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a TenantQuotaChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TenantQuotaChangeHistoryDTO> ToDTOList(List<TenantQuotaChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TenantQuotaChangeHistoryDTO> output = new List<TenantQuotaChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (TenantQuotaChangeHistory tenantQuotaChangeHistory in data)
			{
				output.Add(tenantQuotaChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TenantQuotaChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TenantQuotaChangeHistory Entity type directly.
		///
		/// </summary>
		public TenantQuotaChangeHistoryOutputDTO ToOutputDTO()
		{
			return new TenantQuotaChangeHistoryOutputDTO
			{
				id = this.id,
				tenantQuotaId = this.tenantQuotaId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				tenantQuota = this.tenantQuota?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TenantQuotaChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of TenantQuotaChangeHistory objects to avoid using the TenantQuotaChangeHistory entity type directly.
		///
		/// </summary>
		public static List<TenantQuotaChangeHistoryOutputDTO> ToOutputDTOList(List<TenantQuotaChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TenantQuotaChangeHistoryOutputDTO> output = new List<TenantQuotaChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (TenantQuotaChangeHistory tenantQuotaChangeHistory in data)
			{
				output.Add(tenantQuotaChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TenantQuotaChangeHistory Object.
		///
		/// </summary>
		public static Database.TenantQuotaChangeHistory FromDTO(TenantQuotaChangeHistoryDTO dto)
		{
			return new Database.TenantQuotaChangeHistory
			{
				id = dto.id,
				tenantQuotaId = dto.tenantQuotaId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TenantQuotaChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(TenantQuotaChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.tenantQuotaId = dto.tenantQuotaId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TenantQuotaChangeHistory Object.
		///
		/// </summary>
		public TenantQuotaChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TenantQuotaChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				tenantQuotaId = this.tenantQuotaId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TenantQuotaChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TenantQuotaChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TenantQuotaChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TenantQuotaChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TenantQuotaChangeHistory tenantQuotaChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (tenantQuotaChangeHistory == null)
			{
				return null;
			}

			return new {
				id = tenantQuotaChangeHistory.id,
				tenantQuotaId = tenantQuotaChangeHistory.tenantQuotaId,
				versionNumber = tenantQuotaChangeHistory.versionNumber,
				timeStamp = tenantQuotaChangeHistory.timeStamp,
				userId = tenantQuotaChangeHistory.userId,
				data = tenantQuotaChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TenantQuotaChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TenantQuotaChangeHistory tenantQuotaChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (tenantQuotaChangeHistory == null)
			{
				return null;
			}

			return new {
				id = tenantQuotaChangeHistory.id,
				tenantQuotaId = tenantQuotaChangeHistory.tenantQuotaId,
				versionNumber = tenantQuotaChangeHistory.versionNumber,
				timeStamp = tenantQuotaChangeHistory.timeStamp,
				userId = tenantQuotaChangeHistory.userId,
				data = tenantQuotaChangeHistory.data,
				tenantQuota = TenantQuota.CreateMinimalAnonymous(tenantQuotaChangeHistory.tenantQuota)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TenantQuotaChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TenantQuotaChangeHistory tenantQuotaChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (tenantQuotaChangeHistory == null)
			{
				return null;
			}

			return new {
				id = tenantQuotaChangeHistory.id,
				name = tenantQuotaChangeHistory.id,
				description = tenantQuotaChangeHistory.id
			 };
		}
	}
}
