export interface LandingDto {
  departments: DepartmentEmpCountDto[];
  departmentsCount: number;
  employeeCount: number;
}

export interface DepartmentEmpCountDto {
  department: string;
  employeeCount: number;
}
