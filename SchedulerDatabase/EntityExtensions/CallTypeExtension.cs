using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class CallType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CallTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class CallTypeOutputDTO : CallTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a CallType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CallTypeDTO ToDTO()
		{
			return new CallTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a CallType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CallTypeDTO> ToDTOList(List<CallType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallTypeDTO> output = new List<CallTypeDTO>();

			output.Capacity = data.Count;

			foreach (CallType callType in data)
			{
				output.Add(callType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CallType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CallType Entity type directly.
		///
		/// </summary>
		public CallTypeOutputDTO ToOutputDTO()
		{
			return new CallTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a CallType list to list of Output Data Transfer Object intended to be used for serializing a list of CallType objects to avoid using the CallType entity type directly.
		///
		/// </summary>
		public static List<CallTypeOutputDTO> ToOutputDTOList(List<CallType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallTypeOutputDTO> output = new List<CallTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (CallType callType in data)
			{
				output.Add(callType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CallType Object.
		///
		/// </summary>
		public static Database.CallType FromDTO(CallTypeDTO dto)
		{
			return new Database.CallType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CallType Object.
		///
		/// </summary>
		public void ApplyDTO(CallTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a CallType Object.
		///
		/// </summary>
		public CallType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CallType{
				id = this.id,
				name = this.name,
				description = this.description,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CallType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CallType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CallType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CallType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CallType callType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (callType == null)
			{
				return null;
			}

			return new {
				id = callType.id,
				name = callType.name,
				description = callType.description,
				objectGuid = callType.objectGuid,
				active = callType.active,
				deleted = callType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CallType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CallType callType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (callType == null)
			{
				return null;
			}

			return new {
				id = callType.id,
				name = callType.name,
				description = callType.description,
				objectGuid = callType.objectGuid,
				active = callType.active,
				deleted = callType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CallType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CallType callType)
		{
			//
			// Return a very minimal object.
			//
			if (callType == null)
			{
				return null;
			}

			return new {
				id = callType.id,
				name = callType.name,
				description = callType.description,
			 };
		}
	}
}
