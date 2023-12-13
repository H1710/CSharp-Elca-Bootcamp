import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { LogoutDialogComponent } from '../logout-dialog/logout-dialog.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  providers: [DialogService],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  userLang: string = 'en';
  isOpenDialog: boolean = false;
  dialogRef: DynamicDialogRef | undefined;

  constructor(
    private translateService: TranslateService,
    public dialogService: DialogService,
    private router: Router
  ) {
    this.userLang = localStorage.getItem('pimtool-language') || 'en';
    this.translateService.use(this.userLang);
  }

  setLang(language: string) {
    this.userLang = language;
    localStorage.setItem('pimtool-language', language);
    this.translateService.use(language);
  }

  showDialog() {
    this.dialogRef = this.dialogService.open(LogoutDialogComponent, {
      width: '40%',
      contentStyle: { overflow: 'auto' },
      baseZIndex: 10000,
    });
    this.dialogRef.onClose.subscribe((result) => {
      if (result === true) {
        localStorage.removeItem('pimtool-logged');
        this.router.navigateByUrl('/');
      }
    });
  }
}
