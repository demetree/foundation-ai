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
	public partial class BuildManualPage : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildManualPageDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualId { get; set; }
			public Int32? pageNum { get; set; }
			public String title { get; set; }
			public String notes { get; set; }
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
		public class BuildManualPageOutputDTO : BuildManualPageDTO
		{
			public BuildManual.BuildManualDTO buildManual { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildManualPage to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildManualPageDTO ToDTO()
		{
			return new BuildManualPageDTO
			{
				id = this.id,
				buildManualId = this.buildManualId,
				pageNum = this.pageNum,
				title = this.title,
				notes = this.notes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualPage list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildManualPageDTO> ToDTOList(List<BuildManualPage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualPageDTO> output = new List<BuildManualPageDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualPage buildManualPage in data)
			{
				output.Add(buildManualPage.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildManualPage to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildManualPageEntity type directly.
		///
		/// </summary>
		public BuildManualPageOutputDTO ToOutputDTO()
		{
			return new BuildManualPageOutputDTO
			{
				id = this.id,
				buildManualId = this.buildManualId,
				pageNum = this.pageNum,
				title = this.title,
				notes = this.notes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				buildManual = this.buildManual?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualPage list to list of Output Data Transfer Object intended to be used for serializing a list of BuildManualPage objects to avoid using the BuildManualPage entity type directly.
		///
		/// </summary>
		public static List<BuildManualPageOutputDTO> ToOutputDTOList(List<BuildManualPage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualPageOutputDTO> output = new List<BuildManualPageOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualPage buildManualPage in data)
			{
				output.Add(buildManualPage.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildManualPage Object.
		///
		/// </summary>
		public static Database.BuildManualPage FromDTO(BuildManualPageDTO dto)
		{
			return new Database.BuildManualPage
			{
				id = dto.id,
				buildManualId = dto.buildManualId,
				pageNum = dto.pageNum,
				title = dto.title,
				notes = dto.notes,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildManualPage Object.
		///
		/// </summary>
		public void ApplyDTO(BuildManualPageDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualId = dto.buildManualId;
			this.pageNum = dto.pageNum;
			this.title = dto.title;
			this.notes = dto.notes;
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
		/// Creates a deep copy clone of a BuildManualPage Object.
		///
		/// </summary>
		public BuildManualPage Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildManualPage{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualId = this.buildManualId,
				pageNum = this.pageNum,
				title = this.title,
				notes = this.notes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualPage Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualPage Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildManualPage Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualPage Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildManualPage buildManualPage)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildManualPage == null)
			{
				return null;
			}

			return new {
				id = buildManualPage.id,
				buildManualId = buildManualPage.buildManualId,
				pageNum = buildManualPage.pageNum,
				title = buildManualPage.title,
				notes = buildManualPage.notes,
				objectGuid = buildManualPage.objectGuid,
				active = buildManualPage.active,
				deleted = buildManualPage.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualPage Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildManualPage buildManualPage)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildManualPage == null)
			{
				return null;
			}

			return new {
				id = buildManualPage.id,
				buildManualId = buildManualPage.buildManualId,
				pageNum = buildManualPage.pageNum,
				title = buildManualPage.title,
				notes = buildManualPage.notes,
				objectGuid = buildManualPage.objectGuid,
				active = buildManualPage.active,
				deleted = buildManualPage.deleted,
				buildManual = BuildManual.CreateMinimalAnonymous(buildManualPage.buildManual)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildManualPage Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildManualPage buildManualPage)
		{
			//
			// Return a very minimal object.
			//
			if (buildManualPage == null)
			{
				return null;
			}

			return new {
				id = buildManualPage.id,
				name = buildManualPage.title,
				description = string.Join(", ", new[] { buildManualPage.title}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
