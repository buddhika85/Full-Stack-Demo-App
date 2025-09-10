import { BaseDto } from './base.dto';

export interface CreateEmployeeDto {
  firstName: string;
  lastName: string;
  email: string;
  departmentId: number;
}

export interface UpdateEmployeeDto extends BaseDto {
  firstName: string;
  lastName: string;
  email: string;
  departmentId: number;
}

export interface EmployeeDto extends BaseDto {
  firstName: string;
  lastName: string;
  email: string;
  departmentId: number;
  departmentName: string;
}
