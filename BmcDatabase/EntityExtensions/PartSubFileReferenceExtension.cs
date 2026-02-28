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
	public partial class PartSubFileReference : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PartSubFileReferenceDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 parentBrickPartId { get; set; }
			public Int32? referencedBrickPartId { get; set; }
			[Required]
			public String referencedFileName { get; set; }
			[Required]
			public String transformMatrix { get; set; }
			public Int32? colorCode { get; set; }
			public Int32? sequence { get; set; }
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
		public class PartSubFileReferenceOutputDTO : PartSubFileReferenceDTO
		{
			public BrickPart.BrickPartDTO parentBrickPart { get; set; }
			public BrickPart.BrickPartDTO referencedBrickPart { get; set; }
		}


		/// <summary>
		///
		/// Converts a PartSubFileReference to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PartSubFileReferenceDTO ToDTO()
		{
			return new PartSubFileReferenceDTO
			{
				id = this.id,
				parentBrickPartId = this.parentBrickPartId,
				referencedBrickPartId = this.referencedBrickPartId,
				referencedFileName = this.referencedFileName,
				transformMatrix = this.transformMatrix,
				colorCode = this.colorCode,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PartSubFileReference list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PartSubFileReferenceDTO> ToDTOList(List<PartSubFileReference> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PartSubFileReferenceDTO> output = new List<PartSubFileReferenceDTO>();

			output.Capacity = data.Count;

			foreach (PartSubFileReference partSubFileReference in data)
			{
				output.Add(partSubFileReference.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PartSubFileReference to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PartSubFileReferenceEntity type directly.
		///
		/// </summary>
		public PartSubFileReferenceOutputDTO ToOutputDTO()
		{
			return new PartSubFileReferenceOutputDTO
			{
				id = this.id,
				parentBrickPartId = this.parentBrickPartId,
				referencedBrickPartId = this.referencedBrickPartId,
				referencedFileName = this.referencedFileName,
				transformMatrix = this.transformMatrix,
				colorCode = this.colorCode,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				parentBrickPart = this.parentBrickPart?.ToDTO(),
				referencedBrickPart = this.referencedBrickPart?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PartSubFileReference list to list of Output Data Transfer Object intended to be used for serializing a list of PartSubFileReference objects to avoid using the PartSubFileReference entity type directly.
		///
		/// </summary>
		public static List<PartSubFileReferenceOutputDTO> ToOutputDTOList(List<PartSubFileReference> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PartSubFileReferenceOutputDTO> output = new List<PartSubFileReferenceOutputDTO>();

			output.Capacity = data.Count;

			foreach (PartSubFileReference partSubFileReference in data)
			{
				output.Add(partSubFileReference.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PartSubFileReference Object.
		///
		/// </summary>
		public static Database.PartSubFileReference FromDTO(PartSubFileReferenceDTO dto)
		{
			return new Database.PartSubFileReference
			{
				id = dto.id,
				parentBrickPartId = dto.parentBrickPartId,
				referencedBrickPartId = dto.referencedBrickPartId,
				referencedFileName = dto.referencedFileName,
				transformMatrix = dto.transformMatrix,
				colorCode = dto.colorCode,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PartSubFileReference Object.
		///
		/// </summary>
		public void ApplyDTO(PartSubFileReferenceDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.parentBrickPartId = dto.parentBrickPartId;
			this.referencedBrickPartId = dto.referencedBrickPartId;
			this.referencedFileName = dto.referencedFileName;
			this.transformMatrix = dto.transformMatrix;
			this.colorCode = dto.colorCode;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a PartSubFileReference Object.
		///
		/// </summary>
		public PartSubFileReference Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PartSubFileReference{
				id = this.id,
				parentBrickPartId = this.parentBrickPartId,
				referencedBrickPartId = this.referencedBrickPartId,
				referencedFileName = this.referencedFileName,
				transformMatrix = this.transformMatrix,
				colorCode = this.colorCode,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PartSubFileReference Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PartSubFileReference Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PartSubFileReference Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PartSubFileReference Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PartSubFileReference partSubFileReference)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (partSubFileReference == null)
			{
				return null;
			}

			return new {
				id = partSubFileReference.id,
				parentBrickPartId = partSubFileReference.parentBrickPartId,
				referencedBrickPartId = partSubFileReference.referencedBrickPartId,
				referencedFileName = partSubFileReference.referencedFileName,
				transformMatrix = partSubFileReference.transformMatrix,
				colorCode = partSubFileReference.colorCode,
				sequence = partSubFileReference.sequence,
				objectGuid = partSubFileReference.objectGuid,
				active = partSubFileReference.active,
				deleted = partSubFileReference.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PartSubFileReference Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PartSubFileReference partSubFileReference)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (partSubFileReference == null)
			{
				return null;
			}

			return new {
				id = partSubFileReference.id,
				parentBrickPartId = partSubFileReference.parentBrickPartId,
				referencedBrickPartId = partSubFileReference.referencedBrickPartId,
				referencedFileName = partSubFileReference.referencedFileName,
				transformMatrix = partSubFileReference.transformMatrix,
				colorCode = partSubFileReference.colorCode,
				sequence = partSubFileReference.sequence,
				objectGuid = partSubFileReference.objectGuid,
				active = partSubFileReference.active,
				deleted = partSubFileReference.deleted,
				parentBrickPart = BrickPart.CreateMinimalAnonymous(partSubFileReference.parentBrickPart),
				referencedBrickPart = BrickPart.CreateMinimalAnonymous(partSubFileReference.referencedBrickPart)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PartSubFileReference Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PartSubFileReference partSubFileReference)
		{
			//
			// Return a very minimal object.
			//
			if (partSubFileReference == null)
			{
				return null;
			}

			return new {
				id = partSubFileReference.id,
				name = partSubFileReference.referencedFileName,
				description = string.Join(", ", new[] { partSubFileReference.referencedFileName, partSubFileReference.transformMatrix}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
