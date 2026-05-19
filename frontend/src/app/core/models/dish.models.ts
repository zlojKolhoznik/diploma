export interface DishBrief {
  id: string;
  name: string | null;
  price: number;
  imageUrl: string | null;
}

export interface DishDetail {
  id: string;
  name: string | null;
  description: string | null;
  price: number;
  imageUrl: string | null;
}

export interface CreateDishRequest {
  name: string;
  description: string;
  price: number;
  imageUrl?: string | null;
}

export interface UpdateDishRequest {
  name?: string | null;
  description?: string | null;
  price?: number | null;
  imageUrl?: string | null;
}

export interface DishAvailabilityRequest {
  isAvailable: boolean;
}

