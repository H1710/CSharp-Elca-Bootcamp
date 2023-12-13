import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { DynamicDialogRef } from 'primeng/dynamicdialog';

@Component({
  selector: 'app-logout-dialog',
  standalone: true,
  imports: [CommonModule, ButtonModule, TranslateModule],
  templateUrl: './logout-dialog.component.html',
  styleUrl: './logout-dialog.component.scss',
})
export class LogoutDialogComponent {
  constructor(public ref: DynamicDialogRef) {}

  cancel(): void {
    this.ref.close(false);
  }

  confirmDelete(): void {
    this.ref.close(true);
  }
}
