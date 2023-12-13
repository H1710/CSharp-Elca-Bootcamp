import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  providers: [TitleCasePipe],
})
export class HomeComponent {
  title: string = 'project';
  action: string = 'list';
  constructor(private router: Router) {
    const logged = localStorage.getItem('pimtool-logged') == 'logged';
    console.log(localStorage.getItem('pimtool-logged'));
    if (!logged) {
      this.router.navigateByUrl('/');
    }
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        const part = this.router.url.split('/');
        this.action = part[3];
        this.title = part[2];
      });
  }
}
