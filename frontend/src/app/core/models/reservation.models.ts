export interface ReservationResponse {
  id: string;
  restaurantId: string;
  tableNumber: number | null;
  guestId: string | null;
  guestName: string | null;
  startTime: string;
  approximateDurationMinutes: number;
  numberOfGuests: number;
  status: string | null;
  assignedWaiterId: string | null;
}

export interface CreateReservationRequest {
  restaurantId: string;
  guestId?: string | null;
  guestName?: string | null;
  startTime: string;
  approximateDurationMinutes?: number;
  numberOfGuests?: number;
}

export interface UpdateReservationStatusRequest {
  status: string;
}

export interface UpdateReservationTimeRequest {
  startTime: string;
  approximateDurationMinutes?: number;
}

export interface UpdateReservationTableRequest {
  tableNumber?: number;
}

export interface UpdateReservationAssignedWaiterRequest {
  assignedWaiterId: string;
}

