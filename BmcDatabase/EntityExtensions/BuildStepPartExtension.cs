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
	public partial class BuildStepPart : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildStepPartDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualStepId { get; set; }
			[Required]
			public Int32 placedBrickId { get; set; }
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
		public class BuildStepPartOutputDTO : BuildStepPartDTO
		{
			public BuildManualStep.BuildManualStepDTO buildManualStep { get; set; }
			public PlacedBrick.PlacedBrickDTO placedBrick { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildStepPart to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildStepPartDTO ToDTO()
		{
			return new BuildStepPartDTO
			{
				id = this.id,
				buildManualStepId = this.buildManualStepId,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepPart list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildStepPartDTO> ToDTOList(List<BuildStepPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepPartDTO> output = new List<BuildStepPartDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepPart buildStepPart in data)
			{
				output.Add(buildStepPart.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildStepPart to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildStepPartEntity type directly.
		///
		/// </summary>
		public BuildStepPartOutputDTO ToOutputDTO()
		{
			return new BuildStepPartOutputDTO
			{
				id = this.id,
				buildManualStepId = this.buildManualStepId,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				buildManualStep = this.buildManualStep?.ToDTO(),
				placedBrick = this.placedBrick?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepPart list to list of Output Data Transfer Object intended to be used for serializing a list of BuildStepPart objects to avoid using the BuildStepPart entity type directly.
		///
		/// </summary>
		public static List<BuildStepPartOutputDTO> ToOutputDTOList(List<BuildStepPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepPartOutputDTO> output = new List<BuildStepPartOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepPart buildStepPart in data)
			{
				output.Add(buildStepPart.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildStepPart Object.
		///
		/// </summary>
		public static Database.BuildStepPart FromDTO(BuildStepPartDTO dto)
		{
			return new Database.BuildStepPart
			{
				id = dto.id,
				buildManualStepId = dto.buildManualStepId,
				placedBrickId = dto.placedBrickId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildStepPart Object.
		///
		/// </summary>
		public void ApplyDTO(BuildStepPartDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualStepId = dto.buildManualStepId;
			this.placedBrickId = dto.placedBrickId;
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
		/// Creates a deep copy clone of a BuildStepPart Object.
		///
		/// </summary>
		public BuildStepPart Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildStepPart{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualStepId = this.buildManualStepId,
				placedBrickId = this.placedBrickId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepPart Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepPart Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildStepPart Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepPart Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildStepPart buildStepPart)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildStepPart == null)
			{
				return null;
			}

			return new {
				id = buildStepPart.id,
				buildManualStepId = buildStepPart.buildManualStepId,
				placedBrickId = buildStepPart.placedBrickId,
				objectGuid = buildStepPart.objectGuid,
				active = buildStepPart.active,
				deleted = buildStepPart.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepPart Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildStepPart buildStepPart)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildStepPart == null)
			{
				return null;
			}

			return new {
				id = buildStepPart.id,
				buildManualStepId = buildStepPart.buildManualStepId,
				placedBrickId = buildStepPart.placedBrickId,
				objectGuid = buildStepPart.objectGuid,
				active = buildStepPart.active,
				deleted = buildStepPart.deleted,
				buildManualStep = BuildManualStep.CreateMinimalAnonymous(buildStepPart.buildManualStep),
				placedBrick = PlacedBrick.CreateMinimalAnonymous(buildStepPart.placedBrick)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildStepPart Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildStepPart buildStepPart)
		{
			//
			// Return a very minimal object.
			//
			if (buildStepPart == null)
			{
				return null;
			}

			return new {
				id = buildStepPart.id,
				name = buildStepPart.id,
				description = buildStepPart.id
			 };
		}
	}
}
