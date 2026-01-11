using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Security.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class EntityDataToken : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EntityDataTokenDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public Int32 moduleId { get; set; }
			[Required]
			public String entity { get; set; }
			[Required]
			public String sessionId { get; set; }
			[Required]
			public String authenticationToken { get; set; }
			[Required]
			public String token { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class EntityDataTokenOutputDTO : EntityDataTokenDTO
		{
			public Module.ModuleDTO module { get; set; }
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a EntityDataToken to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EntityDataTokenDTO ToDTO()
		{
			return new EntityDataTokenDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				moduleId = this.moduleId,
				entity = this.entity,
				sessionId = this.sessionId,
				authenticationToken = this.authenticationToken,
				token = this.token,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a EntityDataToken list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EntityDataTokenDTO> ToDTOList(List<EntityDataToken> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EntityDataTokenDTO> output = new List<EntityDataTokenDTO>();

			output.Capacity = data.Count;

			foreach (EntityDataToken entityDataToken in data)
			{
				output.Add(entityDataToken.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EntityDataToken to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EntityDataTokenEntity type directly.
		///
		/// </summary>
		public EntityDataTokenOutputDTO ToOutputDTO()
		{
			return new EntityDataTokenOutputDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				moduleId = this.moduleId,
				entity = this.entity,
				sessionId = this.sessionId,
				authenticationToken = this.authenticationToken,
				token = this.token,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				module = this.module?.ToDTO(),
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EntityDataToken list to list of Output Data Transfer Object intended to be used for serializing a list of EntityDataToken objects to avoid using the EntityDataToken entity type directly.
		///
		/// </summary>
		public static List<EntityDataTokenOutputDTO> ToOutputDTOList(List<EntityDataToken> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EntityDataTokenOutputDTO> output = new List<EntityDataTokenOutputDTO>();

			output.Capacity = data.Count;

			foreach (EntityDataToken entityDataToken in data)
			{
				output.Add(entityDataToken.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EntityDataToken Object.
		///
		/// </summary>
		public static Database.EntityDataToken FromDTO(EntityDataTokenDTO dto)
		{
			return new Database.EntityDataToken
			{
				id = dto.id,
				securityUserId = dto.securityUserId,
				moduleId = dto.moduleId,
				entity = dto.entity,
				sessionId = dto.sessionId,
				authenticationToken = dto.authenticationToken,
				token = dto.token,
				timeStamp = dto.timeStamp,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EntityDataToken Object.
		///
		/// </summary>
		public void ApplyDTO(EntityDataTokenDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityUserId = dto.securityUserId;
			this.moduleId = dto.moduleId;
			this.entity = dto.entity;
			this.sessionId = dto.sessionId;
			this.authenticationToken = dto.authenticationToken;
			this.token = dto.token;
			this.timeStamp = dto.timeStamp;
			this.comments = dto.comments;
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
		/// Creates a deep copy clone of a EntityDataToken Object.
		///
		/// </summary>
		public EntityDataToken Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EntityDataToken{
				id = this.id,
				securityUserId = this.securityUserId,
				moduleId = this.moduleId,
				entity = this.entity,
				sessionId = this.sessionId,
				authenticationToken = this.authenticationToken,
				token = this.token,
				timeStamp = this.timeStamp,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EntityDataToken Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EntityDataToken Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EntityDataToken Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EntityDataToken Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EntityDataToken entityDataToken)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (entityDataToken == null)
			{
				return null;
			}

			return new {
				id = entityDataToken.id,
				securityUserId = entityDataToken.securityUserId,
				moduleId = entityDataToken.moduleId,
				entity = entityDataToken.entity,
				sessionId = entityDataToken.sessionId,
				authenticationToken = entityDataToken.authenticationToken,
				token = entityDataToken.token,
				timeStamp = entityDataToken.timeStamp,
				comments = entityDataToken.comments,
				active = entityDataToken.active,
				deleted = entityDataToken.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EntityDataToken Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EntityDataToken entityDataToken)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (entityDataToken == null)
			{
				return null;
			}

			return new {
				id = entityDataToken.id,
				securityUserId = entityDataToken.securityUserId,
				moduleId = entityDataToken.moduleId,
				entity = entityDataToken.entity,
				sessionId = entityDataToken.sessionId,
				authenticationToken = entityDataToken.authenticationToken,
				token = entityDataToken.token,
				timeStamp = entityDataToken.timeStamp,
				comments = entityDataToken.comments,
				active = entityDataToken.active,
				deleted = entityDataToken.deleted,
				module = Module.CreateMinimalAnonymous(entityDataToken.module),
				securityUser = SecurityUser.CreateMinimalAnonymous(entityDataToken.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EntityDataToken Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EntityDataToken entityDataToken)
		{
			//
			// Return a very minimal object.
			//
			if (entityDataToken == null)
			{
				return null;
			}

			return new {
				id = entityDataToken.id,
				name = string.Join(", ", new[] { entityDataToken.token}.Where(s => !string.IsNullOrWhiteSpace(s))),
				description = string.Join(", ", new[] { entityDataToken.entity, entityDataToken.sessionId, entityDataToken.authenticationToken}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
