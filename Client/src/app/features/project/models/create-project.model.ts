export interface CreateProjectRequest {
  projectNumber: number;
  name: string;
  customer: string;
  groupId: number;
  members: number[];
  status: string;
  startDate: Date;
  endDate: Date;
  version?: string;
}
