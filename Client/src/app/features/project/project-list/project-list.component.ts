import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Project } from '../models/project.model';
import { ProjectService } from '../services/project.service';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { AppRoutingModule } from 'src/app/app-routing.module';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    DropdownModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    RouterLinkActive,
    DialogModule,
    ButtonModule,
  ],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss',
})
export class ProjectListComponent implements OnInit {
  projectList!: Project[];
  searchForm!: FormGroup;
  isOpenDialog: boolean = false;

  constructor(private projectService: ProjectService, private fb: FormBuilder) {
    this.searchForm = this.fb.group({
      searchValue: '',
      status: '',
    });
  }
  ngOnInit(): void {
    this.projectService.GetAllProject().subscribe((data) => {
      this.projectList = data;
    });
  }

  statusConvert: any = {
    NEW: 'New',
    PLA: 'Planned',
    INP: 'In Progress',
    FIN: 'Finished',
  };

  statusList: any[] = [
    { name: '-- None --', value: '' },
    { name: 'New', value: 'NEW' },
    { name: 'Planned', value: 'PLA' },
    { name: 'In progress', value: 'INP' },
    { name: 'Finished', value: 'FIN' },
  ];

  extractDateComponents(date: Date): string {
    const dateTime = new Date(date);
    const day = dateTime.getDate();
    const month = dateTime.getMonth() + 1;
    const year = dateTime.getFullYear();
    return `${day}.${month}.${year}`;
  }

  onFormSubmit() {
    this.projectService
      .SearchProject(
        this.searchForm.value.searchValue,
        this.searchForm.value.status
      )
      .subscribe((data) => {
        this.projectList = data;
      });
  }

  resetSearch() {
    this.projectService.GetAllProject().subscribe((data) => {
      this.projectList = data;
    });
  }

  navigateUpdateProject(id: number) {
    console.log(id);
  }

  showDialog() {
    this.isOpenDialog = true;
  }
}
