using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class MocCollaborator : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocCollaboratorDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
			[Required]
			public Guid collaboratorTenantGuid { get; set; }
			[Required]
			public String accessLevel { get; set; }
			[Required]
			public DateTime invitedDate { get; set; }
			public DateTime? acceptedDate { get; set; }
			[Required]
			public Boolean isAccepted { get; set; }
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
		public class MocCollaboratorOutputDTO : MocCollaboratorDTO
		{
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocCollaborator to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocCollaboratorDTO ToDTO()
		{
			return new MocCollaboratorDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				collaboratorTenantGuid = this.collaboratorTenantGuid,
				accessLevel = this.accessLevel,
				invitedDate = this.invitedDate,
				acceptedDate = this.acceptedDate,
				isAccepted = this.isAccepted,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MocCollaborator list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocCollaboratorDTO> ToDTOList(List<MocCollaborator> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocCollaboratorDTO> output = new List<MocCollaboratorDTO>();

			output.Capacity = data.Count;

			foreach (MocCollaborator mocCollaborator in data)
			{
				output.Add(mocCollaborator.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocCollaborator to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocCollaboratorEntity type directly.
		///
		/// </summary>
		public MocCollaboratorOutputDTO ToOutputDTO()
		{
			return new MocCollaboratorOutputDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				collaboratorTenantGuid = this.collaboratorTenantGuid,
				accessLevel = this.accessLevel,
				invitedDate = this.invitedDate,
				acceptedDate = this.acceptedDate,
				isAccepted = this.isAccepted,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocCollaborator list to list of Output Data Transfer Object intended to be used for serializing a list of MocCollaborator objects to avoid using the MocCollaborator entity type directly.
		///
		/// </summary>
		public static List<MocCollaboratorOutputDTO> ToOutputDTOList(List<MocCollaborator> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocCollaboratorOutputDTO> output = new List<MocCollaboratorOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocCollaborator mocCollaborator in data)
			{
				output.Add(mocCollaborator.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocCollaborator Object.
		///
		/// </summary>
		public static Database.MocCollaborator FromDTO(MocCollaboratorDTO dto)
		{
			return new Database.MocCollaborator
			{
				id = dto.id,
				publishedMocId = dto.publishedMocId,
				collaboratorTenantGuid = dto.collaboratorTenantGuid,
				accessLevel = dto.accessLevel,
				invitedDate = dto.invitedDate,
				acceptedDate = dto.acceptedDate,
				isAccepted = dto.isAccepted,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocCollaborator Object.
		///
		/// </summary>
		public void ApplyDTO(MocCollaboratorDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.publishedMocId = dto.publishedMocId;
			this.collaboratorTenantGuid = dto.collaboratorTenantGuid;
			this.accessLevel = dto.accessLevel;
			this.invitedDate = dto.invitedDate;
			this.acceptedDate = dto.acceptedDate;
			this.isAccepted = dto.isAccepted;
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
		/// Creates a deep copy clone of a MocCollaborator Object.
		///
		/// </summary>
		public MocCollaborator Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocCollaborator{
				id = this.id,
				tenantGuid = this.tenantGuid,
				publishedMocId = this.publishedMocId,
				collaboratorTenantGuid = this.collaboratorTenantGuid,
				accessLevel = this.accessLevel,
				invitedDate = this.invitedDate,
				acceptedDate = this.acceptedDate,
				isAccepted = this.isAccepted,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocCollaborator Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocCollaborator Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocCollaborator Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocCollaborator Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocCollaborator mocCollaborator)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocCollaborator == null)
			{
				return null;
			}

			return new {
				id = mocCollaborator.id,
				publishedMocId = mocCollaborator.publishedMocId,
				collaboratorTenantGuid = mocCollaborator.collaboratorTenantGuid,
				accessLevel = mocCollaborator.accessLevel,
				invitedDate = mocCollaborator.invitedDate,
				acceptedDate = mocCollaborator.acceptedDate,
				isAccepted = mocCollaborator.isAccepted,
				objectGuid = mocCollaborator.objectGuid,
				active = mocCollaborator.active,
				deleted = mocCollaborator.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocCollaborator Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocCollaborator mocCollaborator)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocCollaborator == null)
			{
				return null;
			}

			return new {
				id = mocCollaborator.id,
				publishedMocId = mocCollaborator.publishedMocId,
				collaboratorTenantGuid = mocCollaborator.collaboratorTenantGuid,
				accessLevel = mocCollaborator.accessLevel,
				invitedDate = mocCollaborator.invitedDate,
				acceptedDate = mocCollaborator.acceptedDate,
				isAccepted = mocCollaborator.isAccepted,
				objectGuid = mocCollaborator.objectGuid,
				active = mocCollaborator.active,
				deleted = mocCollaborator.deleted,
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(mocCollaborator.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocCollaborator Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocCollaborator mocCollaborator)
		{
			//
			// Return a very minimal object.
			//
			if (mocCollaborator == null)
			{
				return null;
			}

			return new {
				id = mocCollaborator.id,
				name = mocCollaborator.accessLevel,
				description = string.Join(", ", new[] { mocCollaborator.accessLevel}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
