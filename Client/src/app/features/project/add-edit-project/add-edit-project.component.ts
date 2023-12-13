import { ProjectService } from '../services/project.service';
import { Group } from '../../group/models/group.model';
import {
  Component,
  OnInit,
  ɵSSR_CONTENT_INTEGRITY_MARKER,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupService } from '../../group/services/group.service';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelectModule } from 'primeng/multiselect';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { CreateProjectRequest } from '../models/create-project.model';
import { Employee } from '../../employee/models/employee.model';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CalendarModule } from 'primeng/calendar';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
@Component({
  selector: 'app-create-project',
  standalone: true,
  imports: [
    CommonModule,
    DropdownModule,
    FormsModule,
    ReactiveFormsModule,
    MultiSelectModule,
    NgbModule,
    CalendarModule,
    NgxSpinnerModule,
    TranslateModule,
  ],
  templateUrl: './add-edit-project.component.html',
  styleUrl: './add-edit-project.component.scss',
})
export class CreateProjectComponent implements OnInit {
  constructor(
    private groupService: GroupService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private projectService: ProjectService,
    private route: ActivatedRoute,
    private router: Router,
    private spinner: NgxSpinnerService,
    private translateService: TranslateService
  ) {
    this.formCreateProject = this.fb.group({
      projectNumber: ['', Validators.required],
      name: ['', Validators.required],
      customer: ['', Validators.required],
      groupId: ['', Validators.required],
      members: [[]],
      status: ['NEW', Validators.required],
      startDate: ['', Validators.required],
      endDate: [''],
      version: [''],
    });
    this.statusList = this.statusList.map((status) => {
      return {
        name: this.translateService.instant(`${status.name}`),
        value: status.value,
      };
    });
  }
  formCreateProject!: FormGroup;
  groupList!: Group[];
  statusList: any[] = [
    { name: 'New', value: 'NEW' },
    { name: 'Planned', value: 'PLA' },
    { name: 'In Progress', value: 'INP' },
    { name: 'Finished', value: 'FIN' },
  ];
  employeeList!: any[];
  isAddMode!: boolean;
  id!: number;
  isFillForm: boolean = true;
  isDateRangeValid: boolean = true;
  selectedCustomers!: string[];
  startDateInit: string = '';
  endDateInit: string = '';

  get projectNumber() {
    return this.formCreateProject.controls['projectNumber'];
  }
  get name() {
    return this.formCreateProject.controls['name'];
  }
  get customer() {
    return this.formCreateProject.controls['customer'];
  }
  get groupId() {
    return this.formCreateProject.controls['groupId'];
  }
  get members() {
    return this.formCreateProject.controls['members'];
  }
  get startDate() {
    return this.formCreateProject.controls['startDate'];
  }

  async ngOnInit() {
    this.id = parseInt(this.route.snapshot.params['id']);
    this.isAddMode = !this.id;

    await this.groupService.getAllGroup().subscribe((data) => {
      this.groupList = data;
      if (!this.isAddMode) {
        this.projectService.GetByProjectNumber(this.id).subscribe((project) => {
          const selectedGroup = this.groupList.find(
            (group) => group.id === project.groupId
          );
          if (selectedGroup) {
            const newList = selectedGroup.employees.map((employee) => ({
              id: employee.id,
              name: `${employee.visa}:${
                employee.firstName + ' ' + employee.lastName + ' '
              } ${
                employee.id === selectedGroup.groupLeaderId ? '(Leader)' : ''
              }`,
            }));
            this.employeeList = newList;
          }
          this.formCreateProject.controls['projectNumber'].setValue(
            project.projectNumber
          );
          this.formCreateProject.controls['name'].setValue(project.name);
          this.formCreateProject.controls['customer'].setValue(
            project.customer
          );
          this.formCreateProject.controls['status'].setValue(project.status);
          this.formCreateProject.controls['startDate'].setValue(
            new Date(project.startDate)
          );
          this.formCreateProject.controls['endDate'].setValue(
            new Date(project?.endDate)
          );
          this.formCreateProject.controls['groupId'].setValue(project.groupId);
          this.formCreateProject.controls['members'].setValue(project.members);
          this.formCreateProject.controls['version'].setValue(project.version);
        });
      }
    });
  }

  changeGroup(selectedGroupId: number) {
    const selectedGroup = this.groupList.find(
      (group) => group.id === selectedGroupId
    );

    if (selectedGroup) {
      this.formCreateProject.patchValue({
        members: [],
      });
      const newList = selectedGroup.employees.map((employee) => ({
        id: employee.id,
        name: `${employee.visa}:${
          employee.firstName + ' ' + employee.lastName + ' '
        } ${employee.id === selectedGroup.groupLeaderId ? '(Leader)' : ''}`,
      }));
      this.employeeList = newList;
    }
  }

  validatorsDateRange(startDate: Date, endDate: Date): boolean {
    if (!endDate) {
      return true;
    }
    return startDate <= endDate;
  }

  cancel() {
    this.router.navigateByUrl('/home');
  }

  timezoneDate(date: Date) {
    date.setDate(date.getDate() + 1);
  }

  onFormSubmit() {
    if (!this.formCreateProject.valid) {
      this.isFillForm = false;
      return;
    }
    const postData = { ...this.formCreateProject.value };
    if (!this.validatorsDateRange(postData.startDate, postData.endDate)) {
      this.isDateRangeValid = false;
      return;
    }
    this.timezoneDate(postData.startDate);
    if (this.isAddMode) {
      this.projectService
        .CreateProject(postData as CreateProjectRequest)
        .subscribe({
          next: (response: any) => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Create project successfully',
            });
            this.router.navigateByUrl('/home');
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error,
            });
          },
        });
    } else {
      this.projectService
        .UpdateProject(
          this.formCreateProject.value.projectNumber,
          postData as CreateProjectRequest
        )
        .subscribe({
          next: (response: any) => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Update project successfully',
            });
            this.router.navigateByUrl('/home');
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error,
            });
          },
        });
    }
  }
}
