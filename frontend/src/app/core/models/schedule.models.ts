export interface TimeOnly {
  hour: number;
  minute: number;
}

export interface DateOnly {
  year: number;
  month: number;
  day: number;
}

export interface WaiterScheduleResponse {
  id: string;
  waiterId: string | null;
  date: DateOnly;
  shiftStart: TimeOnly;
  shiftEnd: TimeOnly;
}

export interface CreateWaiterScheduleRequest {
  waiterId: string;
  date: DateOnly;
  shiftStart: TimeOnly;
  shiftEnd: TimeOnly;
}

export interface UpdateWaiterScheduleRequest {
  shiftStart: TimeOnly;
  shiftEnd: TimeOnly;
}

