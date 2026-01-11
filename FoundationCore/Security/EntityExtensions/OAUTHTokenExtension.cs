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
	public partial class OAUTHToken : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class OAUTHTokenDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String token { get; set; }
			[Required]
			public DateTime expiryDateTime { get; set; }
			public String userData { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class OAUTHTokenOutputDTO : OAUTHTokenDTO
		{
		}


		/// <summary>
		///
		/// Converts a OAUTHToken to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public OAUTHTokenDTO ToDTO()
		{
			return new OAUTHTokenDTO
			{
				id = this.id,
				token = this.token,
				expiryDateTime = this.expiryDateTime,
				userData = this.userData,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a OAUTHToken list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<OAUTHTokenDTO> ToDTOList(List<OAUTHToken> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OAUTHTokenDTO> output = new List<OAUTHTokenDTO>();

			output.Capacity = data.Count;

			foreach (OAUTHToken oAUTHToken in data)
			{
				output.Add(oAUTHToken.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a OAUTHToken to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the OAUTHTokenEntity type directly.
		///
		/// </summary>
		public OAUTHTokenOutputDTO ToOutputDTO()
		{
			return new OAUTHTokenOutputDTO
			{
				id = this.id,
				token = this.token,
				expiryDateTime = this.expiryDateTime,
				userData = this.userData,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a OAUTHToken list to list of Output Data Transfer Object intended to be used for serializing a list of OAUTHToken objects to avoid using the OAUTHToken entity type directly.
		///
		/// </summary>
		public static List<OAUTHTokenOutputDTO> ToOutputDTOList(List<OAUTHToken> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OAUTHTokenOutputDTO> output = new List<OAUTHTokenOutputDTO>();

			output.Capacity = data.Count;

			foreach (OAUTHToken oAUTHToken in data)
			{
				output.Add(oAUTHToken.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a OAUTHToken Object.
		///
		/// </summary>
		public static Database.OAUTHToken FromDTO(OAUTHTokenDTO dto)
		{
			return new Database.OAUTHToken
			{
				id = dto.id,
				token = dto.token,
				expiryDateTime = dto.expiryDateTime,
				userData = dto.userData,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a OAUTHToken Object.
		///
		/// </summary>
		public void ApplyDTO(OAUTHTokenDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.token = dto.token;
			this.expiryDateTime = dto.expiryDateTime;
			this.userData = dto.userData;
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
		/// Creates a deep copy clone of a OAUTHToken Object.
		///
		/// </summary>
		public OAUTHToken Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new OAUTHToken{
				id = this.id,
				token = this.token,
				expiryDateTime = this.expiryDateTime,
				userData = this.userData,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OAUTHToken Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OAUTHToken Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a OAUTHToken Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a OAUTHToken Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.OAUTHToken oAUTHToken)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (oAUTHToken == null)
			{
				return null;
			}

			return new {
				id = oAUTHToken.id,
				token = oAUTHToken.token,
				expiryDateTime = oAUTHToken.expiryDateTime,
				userData = oAUTHToken.userData,
				active = oAUTHToken.active,
				deleted = oAUTHToken.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a OAUTHToken Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(OAUTHToken oAUTHToken)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (oAUTHToken == null)
			{
				return null;
			}

			return new {
				id = oAUTHToken.id,
				token = oAUTHToken.token,
				expiryDateTime = oAUTHToken.expiryDateTime,
				userData = oAUTHToken.userData,
				active = oAUTHToken.active,
				deleted = oAUTHToken.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a OAUTHToken Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(OAUTHToken oAUTHToken)
		{
			//
			// Return a very minimal object.
			//
			if (oAUTHToken == null)
			{
				return null;
			}

			return new {
				id = oAUTHToken.id,
				name = oAUTHToken.token,
				description = string.Join(", ", new[] { oAUTHToken.token, oAUTHToken.userData}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
