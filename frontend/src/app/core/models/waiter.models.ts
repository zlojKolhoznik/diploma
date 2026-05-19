export interface WaiterResponse {
  userId: string | null;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  restaurantId: string | null;
}

export interface AssignWaiterRoleRequest {
  restaurantId?: string | null;
}

