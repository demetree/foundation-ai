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
        public void SetupVersionInquiry(CommunityContext context)
        {
            _contextForVersionInquiry = context;
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
            if (_contextForVersionInquiry == null)
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
                // Note that this system is has neither multi tenancy or data visibility enabled, so it gets its change history users from the security module, and gets all users.
                version.user = await Foundation.Security.ChangeHistory.GetChangeHistoryUserAsync(versionAudit.userId.Value, cancellationToken).ConfigureAwait(false);
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
            if (_contextForVersionInquiry == null)
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
                // Note that this system is has neither multi tenancy or data visibility enabled, so it gets its change history users from the security module, and gets all users.
                version.user = await Foundation.Security.ChangeHistory.GetChangeHistoryUserAsync(versionAudit.userId.Value, cancellationToken).ConfigureAwait(false);
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
			public Int32 Id { get; set; }
			public String Title { get; set; }
			public String Slug { get; set; }
			public String Body { get; set; }
			public String Excerpt { get; set; }
			public String AuthorName { get; set; }
			public Int32? PostCategoryId { get; set; }
			public String FeaturedImageUrl { get; set; }
			public String MetaDescription { get; set; }
			public Boolean IsPublished { get; set; }
			public DateTime? PublishedDate { get; set; }
			public Boolean IsFeatured { get; set; }
			public Int32 VersionNumber { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class PostOutputDTO : PostDTO
		{
			public PostCategory.PostCategoryDTO PostCategory { get; set; }
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
				Id = this.Id,
				Title = this.Title,
				Slug = this.Slug,
				Body = this.Body,
				Excerpt = this.Excerpt,
				AuthorName = this.AuthorName,
				PostCategoryId = this.PostCategoryId,
				FeaturedImageUrl = this.FeaturedImageUrl,
				MetaDescription = this.MetaDescription,
				IsPublished = this.IsPublished,
				PublishedDate = this.PublishedDate,
				IsFeatured = this.IsFeatured,
				VersionNumber = this.VersionNumber,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				Title = this.Title,
				Slug = this.Slug,
				Body = this.Body,
				Excerpt = this.Excerpt,
				AuthorName = this.AuthorName,
				PostCategoryId = this.PostCategoryId,
				FeaturedImageUrl = this.FeaturedImageUrl,
				MetaDescription = this.MetaDescription,
				IsPublished = this.IsPublished,
				PublishedDate = this.PublishedDate,
				IsFeatured = this.IsFeatured,
				VersionNumber = this.VersionNumber,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
				PostCategory = this.PostCategory?.ToDTO()
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
				Id = dto.Id,
				Title = dto.Title,
				Slug = dto.Slug,
				Body = dto.Body,
				Excerpt = dto.Excerpt,
				AuthorName = dto.AuthorName,
				PostCategoryId = dto.PostCategoryId,
				FeaturedImageUrl = dto.FeaturedImageUrl,
				MetaDescription = dto.MetaDescription,
				IsPublished = dto.IsPublished,
				PublishedDate = dto.PublishedDate,
				IsFeatured = dto.IsFeatured,
				VersionNumber = dto.VersionNumber,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.Title = dto.Title;
			this.Slug = dto.Slug;
			this.Body = dto.Body;
			this.Excerpt = dto.Excerpt;
			this.AuthorName = dto.AuthorName;
			this.PostCategoryId = dto.PostCategoryId;
			this.FeaturedImageUrl = dto.FeaturedImageUrl;
			this.MetaDescription = dto.MetaDescription;
			this.IsPublished = dto.IsPublished;
			this.PublishedDate = dto.PublishedDate;
			this.IsFeatured = dto.IsFeatured;
			this.VersionNumber = dto.VersionNumber;
			this.ObjectGuid = dto.ObjectGuid;
			if (dto.Active.HasValue == true)
			{
				this.Active = dto.Active.Value;
			}
			if (dto.Deleted.HasValue == true)
			{
				this.Deleted = dto.Deleted.Value;
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
				Id = this.Id,
				Title = this.Title,
				Slug = this.Slug,
				Body = this.Body,
				Excerpt = this.Excerpt,
				AuthorName = this.AuthorName,
				PostCategoryId = this.PostCategoryId,
				FeaturedImageUrl = this.FeaturedImageUrl,
				MetaDescription = this.MetaDescription,
				IsPublished = this.IsPublished,
				PublishedDate = this.PublishedDate,
				IsFeatured = this.IsFeatured,
				VersionNumber = this.VersionNumber,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted,
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
				Id = post.Id,
				Title = post.Title,
				Slug = post.Slug,
				Body = post.Body,
				Excerpt = post.Excerpt,
				AuthorName = post.AuthorName,
				PostCategoryId = post.PostCategoryId,
				FeaturedImageUrl = post.FeaturedImageUrl,
				MetaDescription = post.MetaDescription,
				IsPublished = post.IsPublished,
				PublishedDate = post.PublishedDate,
				IsFeatured = post.IsFeatured,
				VersionNumber = post.VersionNumber,
				ObjectGuid = post.ObjectGuid,
				Active = post.Active,
				Deleted = post.Deleted,
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
				Id = post.Id,
				Title = post.Title,
				Slug = post.Slug,
				Body = post.Body,
				Excerpt = post.Excerpt,
				AuthorName = post.AuthorName,
				PostCategoryId = post.PostCategoryId,
				FeaturedImageUrl = post.FeaturedImageUrl,
				MetaDescription = post.MetaDescription,
				IsPublished = post.IsPublished,
				PublishedDate = post.PublishedDate,
				IsFeatured = post.IsFeatured,
				VersionNumber = post.VersionNumber,
				ObjectGuid = post.ObjectGuid,
				Active = post.Active,
				Deleted = post.Deleted,
				PostCategory = PostCategory.CreateMinimalAnonymous(post.PostCategory),
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
				name = post.title,
				description = string.Join(", ", new[] { post.title, post.slug, post.excerpt}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
