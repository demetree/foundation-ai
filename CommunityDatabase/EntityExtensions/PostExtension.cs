using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class Post : IVersionTrackedEntity<Post>, IAnonymousConvertible
	{
        /// <summary>
        /// This is for setting the context for change history inquiries.
        /// </summary>
        private CommunityContext _contextForVersionInquiry = null;
        private Guid _tenantGuidForVersionInquiry = Guid.Empty;



        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <param name="securityUser">The security user that the changes will be made on behalf of.</param>
        /// <param name="insideTransaction">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ChangeHistoryToolset<Post, PostChangeHistory> GetChangeHistoryToolsetForWriting(CommunityContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (securityUser == null)
            {
                throw new ArgumentNullException(nameof(securityUser));
            }

            //
            // This table does not have data visibility enabled, therefore the user ID is to be taken directly from the security user object.
            // 
            return new ChangeHistoryToolset<Post, PostChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
        }

        /// <summary>
        /// 
        /// Gets the a Change History toolset for read only purposes.
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ChangeHistoryToolset<Post, PostChangeHistory> GetChangeHistoryToolsetForReading(CommunityContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Post, PostChangeHistory>(context, cancellationToken);
        }


        /// <summary>
        /// 
        /// This needs to be called before running any version inquiry method from the IVersionTrackedEntity interface.
        ///
        /// It sets up the context and the tenant guid to use.  Provide the context used for the work, and the tenant guid of the user executing the logic.
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tenantGuid"></param>
        public void SetupVersionInquiry(CommunityContext context, Guid tenantGuid)
        {
            _contextForVersionInquiry = context;
            _tenantGuidForVersionInquiry = tenantGuid;
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the entity's version history using the version of the entity as the basis for the query.
        /// 
        /// Use this to get the update user/time metadata for this version.  IncludingData here is optional and default to false, as it is probably redundant in most cases 
        /// unless the entity you're working with might have unsaved changes.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Post>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(this.versionNumber, includeData, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the first version of the entity.  Equivalent to GetVersionAsync(1, includeData), but name is a bit more concise.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Post>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(1, includeData, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the version of the entity at the provided point in time.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Post>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.  Please call SetupVersionInquiry() before calling this function.");
            }


            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            // Get the version for the point in time provided
            AuditEntry versionAudit = await chts.GetAuditForTime(this, pointInTime).ConfigureAwait(false);

            if (versionAudit == null)
            {
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Post entity.");
            }

            VersionInformation<Post> version = new VersionInformation<Post>();

            version.versionNumber = versionAudit.versionNumber;

            version.timeStamp = versionAudit.timeStamp;

            if (versionAudit.userId.HasValue == true)
            {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continency to return a change history user configured to indicate that we don't know the user.
                version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
            }

            if (includeData == true)
            {
                version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
            }

            return version;
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about a specific version of the entity.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<Post>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.  Please call SetupVersionInquiry() before accessing the GetVersion function.");
            }

            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            // Get the requested version
            AuditEntry versionAudit = await chts.GetAuditForVersion(this, versionNumber).ConfigureAwait(false);

            if (versionAudit == null)
            {
                throw new Exception($"No change history found for version {versionNumber} of this Post entity.");
            }

            VersionInformation<Post> version = new VersionInformation<Post>();

            version.versionNumber = versionAudit.versionNumber;
            version.timeStamp = versionAudit.timeStamp;

            if (versionAudit.userId.HasValue == true)
            {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continency to return a change history user configured to indicate that we don't know the user.
                version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
            }

            if (includeData == true)
            {
                version.data = await chts.GetVersionAsync(this, versionNumber).ConfigureAwait(false);
            }

            return version;
        }


        /// <summary>
        /// 
        /// This gets all the available meta data version information for this entity, and optionally the entity states too
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<VersionInformation<Post>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.Please call SetupVersionInquiry() before accessing the GetAllVersions function.");
            }

            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            List<AuditEntry> versionAudits = await chts.GetAuditTrailAsync(this).ConfigureAwait(false);

            if (versionAudits == null)
            {
                throw new Exception($"No change history audits found for this entity.");
            }

            List <VersionInformation<Post>> versions = new List<VersionInformation<Post>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Post> version = new VersionInformation<Post>();

                version.versionNumber = versionAudit.versionNumber;
                version.timeStamp = versionAudit.timeStamp;

                if (versionAudit.userId.HasValue == true)
                {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Continency to return a change history user configured to indicate that we don't know the user.
                    version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
                }

                if (includeData == true)
                {
                    version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
                }

                versions.Add(version);
            }

            return versions;
        }


		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PostDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String title { get; set; }
			[Required]
			public String slug { get; set; }
			public String body { get; set; }
			public String excerpt { get; set; }
			public String authorName { get; set; }
			public Int32? postCategoryId { get; set; }
			public String featuredImageUrl { get; set; }
			public String metaDescription { get; set; }
			[Required]
			public Boolean isPublished { get; set; }
			public DateTime? publishedDate { get; set; }
			[Required]
			public Boolean isFeatured { get; set; }
			public Int32 versionNumber { get; set; }
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
		public class PostOutputDTO : PostDTO
		{
			public PostCategory.PostCategoryDTO postCategory { get; set; }
		}


		/// <summary>
		///
		/// Converts a Post to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PostDTO ToDTO()
		{
			return new PostDTO
			{
				id = this.id,
				title = this.title,
				slug = this.slug,
				body = this.body,
				excerpt = this.excerpt,
				authorName = this.authorName,
				postCategoryId = this.postCategoryId,
				featuredImageUrl = this.featuredImageUrl,
				metaDescription = this.metaDescription,
				isPublished = this.isPublished,
				publishedDate = this.publishedDate,
				isFeatured = this.isFeatured,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Post list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PostDTO> ToDTOList(List<Post> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostDTO> output = new List<PostDTO>();

			output.Capacity = data.Count;

			foreach (Post post in data)
			{
				output.Add(post.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Post to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PostEntity type directly.
		///
		/// </summary>
		public PostOutputDTO ToOutputDTO()
		{
			return new PostOutputDTO
			{
				id = this.id,
				title = this.title,
				slug = this.slug,
				body = this.body,
				excerpt = this.excerpt,
				authorName = this.authorName,
				postCategoryId = this.postCategoryId,
				featuredImageUrl = this.featuredImageUrl,
				metaDescription = this.metaDescription,
				isPublished = this.isPublished,
				publishedDate = this.publishedDate,
				isFeatured = this.isFeatured,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				postCategory = this.postCategory?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Post list to list of Output Data Transfer Object intended to be used for serializing a list of Post objects to avoid using the Post entity type directly.
		///
		/// </summary>
		public static List<PostOutputDTO> ToOutputDTOList(List<Post> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PostOutputDTO> output = new List<PostOutputDTO>();

			output.Capacity = data.Count;

			foreach (Post post in data)
			{
				output.Add(post.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Post Object.
		///
		/// </summary>
		public static Database.Post FromDTO(PostDTO dto)
		{
			return new Database.Post
			{
				id = dto.id,
				title = dto.title,
				slug = dto.slug,
				body = dto.body,
				excerpt = dto.excerpt,
				authorName = dto.authorName,
				postCategoryId = dto.postCategoryId,
				featuredImageUrl = dto.featuredImageUrl,
				metaDescription = dto.metaDescription,
				isPublished = dto.isPublished,
				publishedDate = dto.publishedDate,
				isFeatured = dto.isFeatured,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Post Object.
		///
		/// </summary>
		public void ApplyDTO(PostDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.title = dto.title;
			this.slug = dto.slug;
			this.body = dto.body;
			this.excerpt = dto.excerpt;
			this.authorName = dto.authorName;
			this.postCategoryId = dto.postCategoryId;
			this.featuredImageUrl = dto.featuredImageUrl;
			this.metaDescription = dto.metaDescription;
			this.isPublished = dto.isPublished;
			this.publishedDate = dto.publishedDate;
			this.isFeatured = dto.isFeatured;
			this.versionNumber = dto.versionNumber;
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
		/// Creates a deep copy clone of a Post Object.
		///
		/// </summary>
		public Post Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Post{
				id = this.id,
				tenantGuid = this.tenantGuid,
				title = this.title,
				slug = this.slug,
				body = this.body,
				excerpt = this.excerpt,
				authorName = this.authorName,
				postCategoryId = this.postCategoryId,
				featuredImageUrl = this.featuredImageUrl,
				metaDescription = this.metaDescription,
				isPublished = this.isPublished,
				publishedDate = this.publishedDate,
				isFeatured = this.isFeatured,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Post Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Post Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Post Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Post Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Post post)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (post == null)
			{
				return null;
			}

			return new {
				id = post.id,
				title = post.title,
				slug = post.slug,
				body = post.body,
				excerpt = post.excerpt,
				authorName = post.authorName,
				postCategoryId = post.postCategoryId,
				featuredImageUrl = post.featuredImageUrl,
				metaDescription = post.metaDescription,
				isPublished = post.isPublished,
				publishedDate = post.publishedDate,
				isFeatured = post.isFeatured,
				versionNumber = post.versionNumber,
				objectGuid = post.objectGuid,
				active = post.active,
				deleted = post.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Post Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Post post)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (post == null)
			{
				return null;
			}

			return new {
				id = post.id,
				title = post.title,
				slug = post.slug,
				body = post.body,
				excerpt = post.excerpt,
				authorName = post.authorName,
				postCategoryId = post.postCategoryId,
				featuredImageUrl = post.featuredImageUrl,
				metaDescription = post.metaDescription,
				isPublished = post.isPublished,
				publishedDate = post.publishedDate,
				isFeatured = post.isFeatured,
				versionNumber = post.versionNumber,
				objectGuid = post.objectGuid,
				active = post.active,
				deleted = post.deleted,
				postCategory = PostCategory.CreateMinimalAnonymous(post.postCategory)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Post Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Post post)
		{
			//
			// Return a very minimal object.
			//
			if (post == null)
			{
				return null;
			}

			return new {
				id = post.id,
				name = post.title,
				description = string.Join(", ", new[] { post.title, post.slug, post.excerpt}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
