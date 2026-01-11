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
	public partial class SecurityUserPasswordResetToken : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SecurityUserPasswordResetTokenDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 securityUserId { get; set; }
			[Required]
			public String token { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public DateTime expiry { get; set; }
			[Required]
			public Boolean systemInitiated { get; set; }
			[Required]
			public Boolean completed { get; set; }
			public String comments { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SecurityUserPasswordResetTokenOutputDTO : SecurityUserPasswordResetTokenDTO
		{
			public SecurityUser.SecurityUserDTO securityUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a SecurityUserPasswordResetToken to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SecurityUserPasswordResetTokenDTO ToDTO()
		{
			return new SecurityUserPasswordResetTokenDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				token = this.token,
				timeStamp = this.timeStamp,
				expiry = this.expiry,
				systemInitiated = this.systemInitiated,
				completed = this.completed,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserPasswordResetToken list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SecurityUserPasswordResetTokenDTO> ToDTOList(List<SecurityUserPasswordResetToken> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserPasswordResetTokenDTO> output = new List<SecurityUserPasswordResetTokenDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserPasswordResetToken securityUserPasswordResetToken in data)
			{
				output.Add(securityUserPasswordResetToken.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SecurityUserPasswordResetToken to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SecurityUserPasswordResetTokenEntity type directly.
		///
		/// </summary>
		public SecurityUserPasswordResetTokenOutputDTO ToOutputDTO()
		{
			return new SecurityUserPasswordResetTokenOutputDTO
			{
				id = this.id,
				securityUserId = this.securityUserId,
				token = this.token,
				timeStamp = this.timeStamp,
				expiry = this.expiry,
				systemInitiated = this.systemInitiated,
				completed = this.completed,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
				securityUser = this.securityUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SecurityUserPasswordResetToken list to list of Output Data Transfer Object intended to be used for serializing a list of SecurityUserPasswordResetToken objects to avoid using the SecurityUserPasswordResetToken entity type directly.
		///
		/// </summary>
		public static List<SecurityUserPasswordResetTokenOutputDTO> ToOutputDTOList(List<SecurityUserPasswordResetToken> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SecurityUserPasswordResetTokenOutputDTO> output = new List<SecurityUserPasswordResetTokenOutputDTO>();

			output.Capacity = data.Count;

			foreach (SecurityUserPasswordResetToken securityUserPasswordResetToken in data)
			{
				output.Add(securityUserPasswordResetToken.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SecurityUserPasswordResetToken Object.
		///
		/// </summary>
		public static Database.SecurityUserPasswordResetToken FromDTO(SecurityUserPasswordResetTokenDTO dto)
		{
			return new Database.SecurityUserPasswordResetToken
			{
				id = dto.id,
				securityUserId = dto.securityUserId,
				token = dto.token,
				timeStamp = dto.timeStamp,
				expiry = dto.expiry,
				systemInitiated = dto.systemInitiated,
				completed = dto.completed,
				comments = dto.comments,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SecurityUserPasswordResetToken Object.
		///
		/// </summary>
		public void ApplyDTO(SecurityUserPasswordResetTokenDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.securityUserId = dto.securityUserId;
			this.token = dto.token;
			this.timeStamp = dto.timeStamp;
			this.expiry = dto.expiry;
			this.systemInitiated = dto.systemInitiated;
			this.completed = dto.completed;
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
		/// Creates a deep copy clone of a SecurityUserPasswordResetToken Object.
		///
		/// </summary>
		public SecurityUserPasswordResetToken Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SecurityUserPasswordResetToken{
				id = this.id,
				securityUserId = this.securityUserId,
				token = this.token,
				timeStamp = this.timeStamp,
				expiry = this.expiry,
				systemInitiated = this.systemInitiated,
				completed = this.completed,
				comments = this.comments,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserPasswordResetToken Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SecurityUserPasswordResetToken Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SecurityUserPasswordResetToken Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserPasswordResetToken Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SecurityUserPasswordResetToken securityUserPasswordResetToken)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (securityUserPasswordResetToken == null)
			{
				return null;
			}

			return new {
				id = securityUserPasswordResetToken.id,
				securityUserId = securityUserPasswordResetToken.securityUserId,
				token = securityUserPasswordResetToken.token,
				timeStamp = securityUserPasswordResetToken.timeStamp,
				expiry = securityUserPasswordResetToken.expiry,
				systemInitiated = securityUserPasswordResetToken.systemInitiated,
				completed = securityUserPasswordResetToken.completed,
				comments = securityUserPasswordResetToken.comments,
				active = securityUserPasswordResetToken.active,
				deleted = securityUserPasswordResetToken.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SecurityUserPasswordResetToken Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SecurityUserPasswordResetToken securityUserPasswordResetToken)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (securityUserPasswordResetToken == null)
			{
				return null;
			}

			return new {
				id = securityUserPasswordResetToken.id,
				securityUserId = securityUserPasswordResetToken.securityUserId,
				token = securityUserPasswordResetToken.token,
				timeStamp = securityUserPasswordResetToken.timeStamp,
				expiry = securityUserPasswordResetToken.expiry,
				systemInitiated = securityUserPasswordResetToken.systemInitiated,
				completed = securityUserPasswordResetToken.completed,
				comments = securityUserPasswordResetToken.comments,
				active = securityUserPasswordResetToken.active,
				deleted = securityUserPasswordResetToken.deleted,
				securityUser = SecurityUser.CreateMinimalAnonymous(securityUserPasswordResetToken.securityUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SecurityUserPasswordResetToken Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SecurityUserPasswordResetToken securityUserPasswordResetToken)
		{
			//
			// Return a very minimal object.
			//
			if (securityUserPasswordResetToken == null)
			{
				return null;
			}

			return new {
				id = securityUserPasswordResetToken.id,
				name = securityUserPasswordResetToken.token,
				description = string.Join(", ", new[] { securityUserPasswordResetToken.token, securityUserPasswordResetToken.comments}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
