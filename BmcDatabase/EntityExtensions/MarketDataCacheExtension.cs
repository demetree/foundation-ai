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
	public partial class MarketDataCache : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MarketDataCacheDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String source { get; set; }
			[Required]
			public String itemType { get; set; }
			[Required]
			public String itemNumber { get; set; }
			public String condition { get; set; }
			public String responseJson { get; set; }
			[Required]
			public DateTime fetchedDate { get; set; }
			[Required]
			public DateTime expiresDate { get; set; }
			[Required]
			public Int32 ttlMinutes { get; set; }
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
		public class MarketDataCacheOutputDTO : MarketDataCacheDTO
		{
		}


		/// <summary>
		///
		/// Converts a MarketDataCache to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MarketDataCacheDTO ToDTO()
		{
			return new MarketDataCacheDTO
			{
				id = this.id,
				source = this.source,
				itemType = this.itemType,
				itemNumber = this.itemNumber,
				condition = this.condition,
				responseJson = this.responseJson,
				fetchedDate = this.fetchedDate,
				expiresDate = this.expiresDate,
				ttlMinutes = this.ttlMinutes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MarketDataCache list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MarketDataCacheDTO> ToDTOList(List<MarketDataCache> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MarketDataCacheDTO> output = new List<MarketDataCacheDTO>();

			output.Capacity = data.Count;

			foreach (MarketDataCache marketDataCache in data)
			{
				output.Add(marketDataCache.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MarketDataCache to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MarketDataCacheEntity type directly.
		///
		/// </summary>
		public MarketDataCacheOutputDTO ToOutputDTO()
		{
			return new MarketDataCacheOutputDTO
			{
				id = this.id,
				source = this.source,
				itemType = this.itemType,
				itemNumber = this.itemNumber,
				condition = this.condition,
				responseJson = this.responseJson,
				fetchedDate = this.fetchedDate,
				expiresDate = this.expiresDate,
				ttlMinutes = this.ttlMinutes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MarketDataCache list to list of Output Data Transfer Object intended to be used for serializing a list of MarketDataCache objects to avoid using the MarketDataCache entity type directly.
		///
		/// </summary>
		public static List<MarketDataCacheOutputDTO> ToOutputDTOList(List<MarketDataCache> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MarketDataCacheOutputDTO> output = new List<MarketDataCacheOutputDTO>();

			output.Capacity = data.Count;

			foreach (MarketDataCache marketDataCache in data)
			{
				output.Add(marketDataCache.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MarketDataCache Object.
		///
		/// </summary>
		public static Database.MarketDataCache FromDTO(MarketDataCacheDTO dto)
		{
			return new Database.MarketDataCache
			{
				id = dto.id,
				source = dto.source,
				itemType = dto.itemType,
				itemNumber = dto.itemNumber,
				condition = dto.condition,
				responseJson = dto.responseJson,
				fetchedDate = dto.fetchedDate,
				expiresDate = dto.expiresDate,
				ttlMinutes = dto.ttlMinutes,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MarketDataCache Object.
		///
		/// </summary>
		public void ApplyDTO(MarketDataCacheDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.source = dto.source;
			this.itemType = dto.itemType;
			this.itemNumber = dto.itemNumber;
			this.condition = dto.condition;
			this.responseJson = dto.responseJson;
			this.fetchedDate = dto.fetchedDate;
			this.expiresDate = dto.expiresDate;
			this.ttlMinutes = dto.ttlMinutes;
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
		/// Creates a deep copy clone of a MarketDataCache Object.
		///
		/// </summary>
		public MarketDataCache Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MarketDataCache{
				id = this.id,
				source = this.source,
				itemType = this.itemType,
				itemNumber = this.itemNumber,
				condition = this.condition,
				responseJson = this.responseJson,
				fetchedDate = this.fetchedDate,
				expiresDate = this.expiresDate,
				ttlMinutes = this.ttlMinutes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MarketDataCache Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MarketDataCache Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MarketDataCache Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MarketDataCache Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MarketDataCache marketDataCache)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (marketDataCache == null)
			{
				return null;
			}

			return new {
				id = marketDataCache.id,
				source = marketDataCache.source,
				itemType = marketDataCache.itemType,
				itemNumber = marketDataCache.itemNumber,
				condition = marketDataCache.condition,
				responseJson = marketDataCache.responseJson,
				fetchedDate = marketDataCache.fetchedDate,
				expiresDate = marketDataCache.expiresDate,
				ttlMinutes = marketDataCache.ttlMinutes,
				objectGuid = marketDataCache.objectGuid,
				active = marketDataCache.active,
				deleted = marketDataCache.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MarketDataCache Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MarketDataCache marketDataCache)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (marketDataCache == null)
			{
				return null;
			}

			return new {
				id = marketDataCache.id,
				source = marketDataCache.source,
				itemType = marketDataCache.itemType,
				itemNumber = marketDataCache.itemNumber,
				condition = marketDataCache.condition,
				responseJson = marketDataCache.responseJson,
				fetchedDate = marketDataCache.fetchedDate,
				expiresDate = marketDataCache.expiresDate,
				ttlMinutes = marketDataCache.ttlMinutes,
				objectGuid = marketDataCache.objectGuid,
				active = marketDataCache.active,
				deleted = marketDataCache.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MarketDataCache Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MarketDataCache marketDataCache)
		{
			//
			// Return a very minimal object.
			//
			if (marketDataCache == null)
			{
				return null;
			}

			return new {
				id = marketDataCache.id,
				name = marketDataCache.source,
				description = string.Join(", ", new[] { marketDataCache.source, marketDataCache.itemType, marketDataCache.itemNumber}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
