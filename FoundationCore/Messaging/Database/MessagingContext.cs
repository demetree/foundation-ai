using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Messaging.Database
{
    /// <summary>
    /// 
    /// Foundation Messaging Context - lightweight EF context that maps only the messaging-related tables.
    /// 
    /// This context is designed to work against any Foundation module's database, since the messaging
    /// tables are created by SecurityDatabaseGenerator.AddConversationTablesToDatabase() and 
    /// SecurityDatabaseGenerator.AddMessagingTablesToDatabase() — which produce identical schema 
    /// structures across all modules.
    /// 
    /// The schema name is configurable via the SchemaName property so each module can use its own 
    /// schema (e.g., "Catalyst", "Basecamp", "Scheduler").
    /// 
    /// </summary>
    public partial class MessagingContext : DbContext
    {
        public MessagingContext(DbContextOptions<MessagingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Conversation> Conversations { get; set; }
        public virtual DbSet<ConversationChannel> ConversationChannels { get; set; }
        public virtual DbSet<ConversationChannelChangeHistory> ConversationChannelChangeHistories { get; set; }
        public virtual DbSet<ConversationMessage> ConversationMessages { get; set; }
        public virtual DbSet<ConversationMessageAttachment> ConversationMessageAttachments { get; set; }
        public virtual DbSet<ConversationMessageLinkPreview> ConversationMessageLinkPreviews { get; set; }
        public virtual DbSet<ConversationMessageLinkPreviewChangeHistory> ConversationMessageLinkPreviewChangeHistories { get; set; }
        public virtual DbSet<ConversationMessageAttachmentChangeHistory> ConversationMessageAttachmentChangeHistories { get; set; }
        public virtual DbSet<ConversationMessageChangeHistory> ConversationMessageChangeHistories { get; set; }
        public virtual DbSet<ConversationMessageReaction> ConversationMessageReactions { get; set; }
        public virtual DbSet<ConversationMessageUser> ConversationMessageUsers { get; set; }
        public virtual DbSet<ConversationPin> ConversationPins { get; set; }
        public virtual DbSet<ConversationType> ConversationTypes { get; set; }
        public virtual DbSet<ConversationUser> ConversationUsers { get; set; }
        public virtual DbSet<UserPresence> UserPresences { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotificationType> NotificationTypes { get; set; }
        public virtual DbSet<NotificationDistribution> NotificationDistributions { get; set; }
        public virtual DbSet<NotificationAttachment> NotificationAttachments { get; set; }
        public virtual DbSet<PushDeliveryLog> PushDeliveryLogs { get; set; }
        public virtual DbSet<PushProviderConfiguration> PushProviderConfigurations { get; set; }
        public virtual DbSet<MessageFlag> MessageFlags { get; set; }
        public virtual DbSet<MessagingAuditLog> MessagingAuditLogs { get; set; }
        public virtual DbSet<CallType> CallTypes { get; set; }
        public virtual DbSet<CallStatus> CallStatuses { get; set; }
        public virtual DbSet<Call> Calls { get; set; }
        public virtual DbSet<CallParticipant> CallParticipants { get; set; }
        public virtual DbSet<CallEventLog> CallEventLogs { get; set; }
        public virtual DbSet<ConversationThreadUser> ConversationThreadUsers { get; set; }
        public virtual DbSet<MessageBookmark> MessageBookmarks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string schema = SchemaName ?? "dbo";


            // ---------------------------------------------------------------
            // Conversation
            // ---------------------------------------------------------------
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("Conversation", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationTypeId });
                entity.HasIndex(e => new { e.tenantGuid, e.createdByUserId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.entity).HasMaxLength(250);
                entity.Property(e => e.externalURL).HasMaxLength(1000);
                entity.Property(e => e.name).HasMaxLength(200);
                entity.Property(e => e.description).HasMaxLength(1000);
                entity.Property(e => e.isPublic).HasDefaultValue(false);
                entity.Property(e => e.priority).HasDefaultValue(100);

                entity.HasOne(d => d.conversationType).WithMany(p => p.Conversations).HasForeignKey(d => d.conversationTypeId);
            });


            // ---------------------------------------------------------------
            // ConversationType
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationType>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationType", schema);

                entity.HasIndex(e => e.active);
                entity.HasIndex(e => e.deleted);
                entity.HasIndex(e => e.name);

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.description).HasMaxLength(500);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
            });


            // ---------------------------------------------------------------
            // ConversationChannel
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationChannel>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationChannel", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.name).IsRequired().HasMaxLength(250);
                entity.Property(e => e.topic).HasMaxLength(1000);
                entity.Property(e => e.versionNumber).HasDefaultValue(1);

                entity.HasOne(d => d.conversation).WithMany(p => p.ConversationChannels)
                    .HasForeignKey(d => d.conversationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationChannelChangeHistory
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationChannelChangeHistory>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationChannelChangeHistory", schema);

                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.conversationChannelId });
                entity.HasIndex(e => new { e.tenantGuid, e.timeStamp });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.tenantGuid, e.versionNumber });

                entity.Property(e => e.data).IsRequired();

                entity.HasOne(d => d.conversationChannel).WithMany(p => p.ConversationChannelChangeHistories)
                    .HasForeignKey(d => d.conversationChannelId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessage
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessage>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessage", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.parentConversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.HasIndex(e => new { e.tenantGuid, e.conversationChannelId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.entity).HasMaxLength(250);
                entity.Property(e => e.externalURL).HasMaxLength(1000);
                entity.Property(e => e.message).IsRequired();
                entity.Property(e => e.versionNumber).HasDefaultValue(1);

                entity.HasOne(d => d.conversation).WithMany(p => p.ConversationMessages)
                    .HasForeignKey(d => d.conversationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.parentConversationMessage).WithMany(p => p.InverseparentConversationMessage)
                    .HasForeignKey(d => d.parentConversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.conversationChannel).WithMany(p => p.ConversationMessages)
                    .HasForeignKey(d => d.conversationChannelId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageAttachment
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageAttachment>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageAttachment", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.contentData).IsRequired();
                entity.Property(e => e.contentFileName).IsRequired().HasMaxLength(250);
                entity.Property(e => e.contentMimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.versionNumber).HasDefaultValue(1);

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.ConversationMessageAttachments)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageAttachmentChangeHistory
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageAttachmentChangeHistory>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageAttachmentChangeHistory", schema);

                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageAttachmentId });
                entity.HasIndex(e => new { e.tenantGuid, e.timeStamp });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.tenantGuid, e.versionNumber });

                entity.Property(e => e.data).IsRequired();

                entity.HasOne(d => d.conversationMessageAttachment).WithMany(p => p.ConversationMessageAttachmentChangeHistories)
                    .HasForeignKey(d => d.conversationMessageAttachmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageChangeHistory
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageChangeHistory>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageChangeHistory", schema);

                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.timeStamp });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.tenantGuid, e.versionNumber });

                entity.Property(e => e.data).IsRequired();

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.ConversationMessageChangeHistories)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageLinkPreview
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageLinkPreview>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageLinkPreview", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.url).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.title).HasMaxLength(500);
                entity.Property(e => e.description).HasMaxLength(2000);
                entity.Property(e => e.imageUrl).HasMaxLength(2000);
                entity.Property(e => e.siteName).HasMaxLength(250);
                entity.Property(e => e.versionNumber).HasDefaultValue(1);

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.ConversationMessageLinkPreviews)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageLinkPreviewChangeHistory
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageLinkPreviewChangeHistory>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageLinkPreviewChangeHistory", schema);

                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageLinkPreviewId });
                entity.HasIndex(e => new { e.tenantGuid, e.timeStamp });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.tenantGuid, e.versionNumber });

                entity.Property(e => e.data).IsRequired();

                entity.HasOne(d => d.conversationMessageLinkPreview).WithMany(p => p.ConversationMessageLinkPreviewChangeHistories)
                    .HasForeignKey(d => d.conversationMessageLinkPreviewId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageReaction
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageReaction>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageReaction", schema);

                entity.HasIndex(e => new { e.conversationMessageId, e.active, e.deleted });
                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.reaction).IsRequired().HasMaxLength(50);

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.ConversationMessageReactions)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationMessageUser
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationMessageUser>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationMessageUser", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.Property(e => e.active).HasDefaultValue(true);

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.ConversationMessageUsers)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationPin
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationPin>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationPin", schema);

                entity.HasIndex(e => new { e.conversationId, e.active, e.deleted });
                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationId });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.pinnedByUserId });

                entity.Property(e => e.active).HasDefaultValue(true);

                entity.HasOne(d => d.conversation).WithMany(p => p.ConversationPins)
                    .HasForeignKey(d => d.conversationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.ConversationPins)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationUser
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationUser>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationUser", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationId });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.role).HasMaxLength(50).HasDefaultValue("Member");

                entity.HasOne(d => d.conversation).WithMany(p => p.ConversationUsers)
                    .HasForeignKey(d => d.conversationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // UserPresence
            // ---------------------------------------------------------------
            modelBuilder.Entity<UserPresence>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("UserPresence", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.userId, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.customStatusMessage).HasMaxLength(500);
                entity.Property(e => e.status).IsRequired().HasMaxLength(50);
            });


            // ---------------------------------------------------------------
            // Notification
            // ---------------------------------------------------------------
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("Notification", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.createdByUserId });
                entity.HasIndex(e => new { e.tenantGuid, e.notificationTypeId });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.entity).HasMaxLength(250);
                entity.Property(e => e.externalURL).HasMaxLength(1000);
                entity.Property(e => e.message).IsRequired();
                entity.Property(e => e.priority).HasDefaultValue(10);
                entity.Property(e => e.versionNumber).HasDefaultValue(1);

                entity.HasOne(d => d.notificationType).WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.notificationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // NotificationType
            // ---------------------------------------------------------------
            modelBuilder.Entity<NotificationType>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("NotificationType", schema);

                entity.HasIndex(e => e.active);
                entity.HasIndex(e => e.deleted);
                entity.HasIndex(e => e.name);

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.description).HasMaxLength(500);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
            });


            // ---------------------------------------------------------------
            // NotificationDistribution
            // ---------------------------------------------------------------
            modelBuilder.Entity<NotificationDistribution>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("NotificationDistribution", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.notificationId });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.userId, e.acknowledged, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);

                entity.HasOne(d => d.notification).WithMany(p => p.NotificationDistributions)
                    .HasForeignKey(d => d.notificationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // NotificationAttachment
            // ---------------------------------------------------------------
            modelBuilder.Entity<NotificationAttachment>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("NotificationAttachment", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.notificationId });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.contentFileName).IsRequired().HasMaxLength(250);
                entity.Property(e => e.contentMimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.versionNumber).HasDefaultValue(1);

                entity.HasOne(d => d.notification).WithMany(p => p.NotificationAttachments)
                    .HasForeignKey(d => d.notificationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // PushDeliveryLog
            // ---------------------------------------------------------------
            modelBuilder.Entity<PushDeliveryLog>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("PushDeliveryLog", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.tenantGuid, e.providerId });
                entity.HasIndex(e => new { e.tenantGuid, e.dateTimeCreated });
                entity.HasIndex(e => new { e.tenantGuid, e.success });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.providerId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.destination).HasMaxLength(250);
                entity.Property(e => e.sourceType).HasMaxLength(50);
                entity.Property(e => e.externalId).HasMaxLength(250);
                entity.Property(e => e.errorMessage).HasMaxLength(2000);
            });


            // ---------------------------------------------------------------
            // PushProviderConfiguration
            // ---------------------------------------------------------------
            modelBuilder.Entity<PushProviderConfiguration>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("PushProviderConfiguration", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.providerId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.providerId).IsRequired().HasMaxLength(50);
            });


            // ---------------------------------------------------------------
            // MessageFlag
            // ---------------------------------------------------------------
            modelBuilder.Entity<MessageFlag>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("MessageFlag", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.status });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.tenantGuid, e.flaggedByUserId });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.reason).IsRequired().HasMaxLength(100);
                entity.Property(e => e.details).HasMaxLength(2000);
                entity.Property(e => e.status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.resolutionNotes).HasMaxLength(2000);
            });


            // ---------------------------------------------------------------
            // MessagingAuditLog
            // ---------------------------------------------------------------
            modelBuilder.Entity<MessagingAuditLog>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("MessagingAuditLog", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.performedByUserId });
                entity.HasIndex(e => new { e.tenantGuid, e.action });
                entity.HasIndex(e => new { e.tenantGuid, e.dateTimeCreated });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.entityType).HasMaxLength(100);
                entity.Property(e => e.details).HasMaxLength(4000);
                entity.Property(e => e.ipAddress).HasMaxLength(50);
            });


            // ---------------------------------------------------------------
            // CallType
            // ---------------------------------------------------------------
            modelBuilder.Entity<CallType>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("CallType", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.description).HasMaxLength(500);
            });


            // ---------------------------------------------------------------
            // CallStatus
            // ---------------------------------------------------------------
            modelBuilder.Entity<CallStatus>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("CallStatus", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.description).HasMaxLength(500);
            });


            // ---------------------------------------------------------------
            // Call
            // ---------------------------------------------------------------
            modelBuilder.Entity<Call>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("Call", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.conversationId, e.active, e.deleted });
                entity.HasIndex(e => new { e.initiatorUserId, e.active, e.deleted });
                entity.HasIndex(e => new { e.callStatusId, e.active, e.deleted });
                entity.HasIndex(e => new { e.providerId, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.providerId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.providerCallId).HasMaxLength(250);

                entity.HasOne(d => d.callType).WithMany(p => p.Calls)
                    .HasForeignKey(d => d.callTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.callStatus).WithMany(p => p.Calls)
                    .HasForeignKey(d => d.callStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.conversation).WithMany()
                    .HasForeignKey(d => d.conversationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // CallParticipant
            // ---------------------------------------------------------------
            modelBuilder.Entity<CallParticipant>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("CallParticipant", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.callId, e.active, e.deleted });
                entity.HasIndex(e => new { e.userId, e.active, e.deleted });
                entity.HasIndex(e => new { e.status, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.status).IsRequired().HasMaxLength(50);

                entity.HasOne(d => d.call).WithMany(p => p.CallParticipants)
                    .HasForeignKey(d => d.callId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // CallEventLog
            // ---------------------------------------------------------------
            modelBuilder.Entity<CallEventLog>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("CallEventLog", schema);

                entity.HasIndex(e => new { e.id, e.active, e.deleted });
                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.callId, e.active, e.deleted });
                entity.HasIndex(e => new { e.eventType, e.active, e.deleted });
                entity.HasIndex(e => new { e.userId, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.eventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.providerId).HasMaxLength(50);

                entity.HasOne(d => d.call).WithMany(p => p.CallEventLogs)
                    .HasForeignKey(d => d.callId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ---------------------------------------------------------------
            // ConversationThreadUser
            // ---------------------------------------------------------------
            modelBuilder.Entity<ConversationThreadUser>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("ConversationThreadUser", schema);

                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationId });
                entity.HasIndex(e => new { e.userId, e.parentConversationMessageId });

                entity.Property(e => e.active).HasDefaultValue(true);
            });


            // ---------------------------------------------------------------
            // MessageBookmark
            // ---------------------------------------------------------------
            modelBuilder.Entity<MessageBookmark>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.ToTable("MessageBookmark", schema);

                entity.HasIndex(e => e.tenantGuid);
                entity.HasIndex(e => new { e.tenantGuid, e.active });
                entity.HasIndex(e => new { e.tenantGuid, e.deleted });
                entity.HasIndex(e => new { e.tenantGuid, e.userId });
                entity.HasIndex(e => new { e.tenantGuid, e.conversationMessageId });
                entity.HasIndex(e => new { e.userId, e.active, e.deleted });

                entity.Property(e => e.active).HasDefaultValue(true);
                entity.Property(e => e.note).HasMaxLength(500);

                entity.HasOne(d => d.conversationMessage).WithMany(p => p.MessageBookmarks)
                    .HasForeignKey(d => d.conversationMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });


            OnModelCreatingPartial(modelBuilder);
        }


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
