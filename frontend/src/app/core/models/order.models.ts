export interface OrderItemResponse {
  id: string;
  dishId: string;
  dishName: string | null;
  quantity: number;
  unitPrice: number;
  notes: string | null;
  status: string | null;
  rejectionReason: string | null;
  lineTotal: number;
}

export interface OrderResponse {
  id: string;
  restaurantId: string;
  reservationId: string | null;
  waiterId: string | null;
  status: string | null;
  notes: string | null;
  createdAtUtc: string;
  closedAtUtc: string | null;
  items: OrderItemResponse[] | null;
  totalAmount: number;
}

export interface AddOrderItemRequest {
  dishId: string;
  quantity?: number;
  notes?: string | null;
}

export interface CreateOrderRequest {
  notes?: string | null;
  items?: AddOrderItemRequest[] | null;
}

export interface UpdateOrderStatusRequest {
  status: string;
}

export interface UpdateOrderItemRequest {
  quantity?: number;
  notes?: string | null;
}

export interface RejectOrderItemRequest {
  rejectionReason: string;
}

