/*

   GENERATED SERVICE FOR THE COMMUNITY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Community table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import {Injectable} from '@angular/core';
import {AnnouncementService} from  './announcement.service';
import {AnnouncementChangeHistoryService} from  './announcement-change-history.service';
import {ContactSubmissionService} from  './contact-submission.service';
import {DocumentDownloadService} from  './document-download.service';
import {GalleryAlbumService} from  './gallery-album.service';
import {GalleryImageService} from  './gallery-image.service';
import {MediaAssetService} from  './media-asset.service';
import {MediaContentService} from  './media-content.service';
import {MenuService} from  './menu.service';
import {MenuItemService} from  './menu-item.service';
import {PageService} from  './page.service';
import {PageChangeHistoryService} from  './page-change-history.service';
import {PostService} from  './post.service';
import {PostCategoryService} from  './post-category.service';
import {PostChangeHistoryService} from  './post-change-history.service';
import {PostTagService} from  './post-tag.service';
import {PostTagAssignmentService} from  './post-tag-assignment.service';
import {SiteSettingService} from  './site-setting.service';


@Injectable({
  providedIn: 'root'
})
export class CommunityDataServiceManagerService  {

    constructor(public announcementService: AnnouncementService
              , public announcementChangeHistoryService: AnnouncementChangeHistoryService
              , public contactSubmissionService: ContactSubmissionService
              , public documentDownloadService: DocumentDownloadService
              , public galleryAlbumService: GalleryAlbumService
              , public galleryImageService: GalleryImageService
              , public mediaAssetService: MediaAssetService
              , public mediaContentService: MediaContentService
              , public menuService: MenuService
              , public menuItemService: MenuItemService
              , public pageService: PageService
              , public pageChangeHistoryService: PageChangeHistoryService
              , public postService: PostService
              , public postCategoryService: PostCategoryService
              , public postChangeHistoryService: PostChangeHistoryService
              , public postTagService: PostTagService
              , public postTagAssignmentService: PostTagAssignmentService
              , public siteSettingService: SiteSettingService
) { }  


    public ClearAllCaches() {

        this.announcementService.ClearAllCaches();
        this.announcementChangeHistoryService.ClearAllCaches();
        this.contactSubmissionService.ClearAllCaches();
        this.documentDownloadService.ClearAllCaches();
        this.galleryAlbumService.ClearAllCaches();
        this.galleryImageService.ClearAllCaches();
        this.mediaAssetService.ClearAllCaches();
        this.mediaContentService.ClearAllCaches();
        this.menuService.ClearAllCaches();
        this.menuItemService.ClearAllCaches();
        this.pageService.ClearAllCaches();
        this.pageChangeHistoryService.ClearAllCaches();
        this.postService.ClearAllCaches();
        this.postCategoryService.ClearAllCaches();
        this.postChangeHistoryService.ClearAllCaches();
        this.postTagService.ClearAllCaches();
        this.postTagAssignmentService.ClearAllCaches();
        this.siteSettingService.ClearAllCaches();
    }
}