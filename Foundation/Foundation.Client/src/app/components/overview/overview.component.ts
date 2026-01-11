import { Component, OnInit, AfterViewInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NavigationService } from '../../utility-services/navigation.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss'],
})
export class OverviewComponent implements OnInit, AfterViewInit {
  constructor(
    private navigationService: NavigationService,
    private http: HttpClient,
    private router: Router
  ) { }

  ngOnInit(): void { }

  ngAfterViewInit() {
    
  }


  public goBack(): void {
    this.navigationService.goBack();
  }

 
}
