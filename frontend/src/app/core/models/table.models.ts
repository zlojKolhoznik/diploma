export interface TableBrief {
  tableNumber: number;
  seats: number;
}

export interface AddTableRequest {
  tableNumber: number;
  seats: number;
}

export interface UpdateTableSeatsRequest {
  seats: number;
}

