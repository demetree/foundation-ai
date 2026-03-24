import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-messaging-page',
  templateUrl: './messaging-page.component.html',
  styles: [`
    :host {
      display: flex;
      flex: 1;
      width: 100%;
      margin: -5px;
      overflow: hidden;
    }
  `]
})
export class MessagingPageComponent implements OnInit {
  mode: 'fullscreen' | 'popout' = 'fullscreen';

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    const popout = this.route.snapshot.queryParamMap.get('popout');
    this.mode = popout === 'true' ? 'popout' : 'fullscreen';
  }
}
