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
import { PaginatorModule } from 'primeng/paginator';
import {
  ActivatedRoute,
  Params,
  Router,
  RouterLink,
  RouterLinkActive,
} from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { DeleteDialogComponent } from 'src/app/components/delete-dialog/delete-dialog.component';
import { switchMap } from 'rxjs';
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
    PaginatorModule,
  ],
  providers: [DialogService],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss',
})
export class ProjectListComponent implements OnInit {
  projectList!: Project[];
  searchForm!: FormGroup;
  isOpenDialog: boolean = false;
  dialogRef: DynamicDialogRef | undefined;
  page: number = 1;
  totalProject: number = 0;
  pageSize: number = 5;
  checkedDeleteProject: number[] = [];

  constructor(
    private projectService: ProjectService,
    private fb: FormBuilder,
    public dialogService: DialogService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.searchForm = this.fb.group({
      searchValue: '',
      status: '',
    });
  }
  ngOnInit(): void {
    this.projectService
      .GetAllProject(this.page, this.pageSize)
      .subscribe((data) => {
        this.projectList = data.items;
        this.totalProject = data.totalCount;
        this.pageSize = data.pageSize;
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
    this.projectService
      .GetAllProject(this.page, this.pageSize)
      .subscribe((data) => {
        this.projectList = data.items;
      });
  }

  navigateUpdateProject(id: number) {
    console.log(id);
  }

  showDialog(projectNumber: number[]) {
    this.dialogRef = this.dialogService.open(DeleteDialogComponent, {
      header: 'Confirm delete project',
      width: '40%',
      contentStyle: { overflow: 'auto' },
      baseZIndex: 10000,
    });

    this.dialogRef.onClose.subscribe((result) => {
      if (result === true) {
        this.projectService
          .DeleteProject(projectNumber)
          .pipe(
            switchMap(() =>
              this.projectService.GetAllProject(this.page, this.pageSize)
            )
          )
          .subscribe((data) => {
            this.projectList = data.items;
            this.totalProject = data.totalCount;
            this.pageSize = data.pageSize;
          });
        this.checkedDeleteProject = [];
      }
    });
  }
  onPageChange(event: any) {
    this.page = event.page + 1;
    this.projectService
      .GetAllProject(this.page, this.pageSize)
      .subscribe((data) => {
        this.projectList = data.items;
        this.totalProject = data.totalCount;
        this.pageSize = data.pageSize;
      });
  }

  handleCheckedDelete() {
    const deleteProjects = this.projectList.filter(
      (project) => project.checked
    );
    this.checkedDeleteProject = deleteProjects.map(
      (project) => project.projectNumber
    );
  }
}
