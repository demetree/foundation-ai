import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomePageComponent } from './pages/home/home-page.component';
import { PageViewComponent } from './pages/page-view/page-view.component';
import { PostListComponent } from './pages/post-list/post-list.component';
import { PostDetailComponent } from './pages/post-detail/post-detail.component';
import { ContactComponent } from './pages/contact/contact.component';


const routes: Routes = [
  { path: '', component: HomePageComponent, title: 'Home' },
  { path: 'news', component: PostListComponent, title: 'News & Updates' },
  { path: 'news/:slug', component: PostDetailComponent },
  { path: 'contact', component: ContactComponent, title: 'Contact Us' },
  // Catch-all: CMS pages by slug (must be last)
  { path: ':slug', component: PageViewComponent }
];


@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'top'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
