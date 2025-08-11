export interface ProblemDetailsDto {
  type?: string; // optional URI reference
  title: string;
  status: number;
  detail: string;
  instance?: string; // optional URI reference to the specific occurrence
  [key: string]: any; // allow additional fields like "errors" or "traceId"
}
