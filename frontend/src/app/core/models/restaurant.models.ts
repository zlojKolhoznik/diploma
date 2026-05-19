export interface RestaurantBrief {
  id: string;
  city: string | null;
  address: string | null;
  imageUrl: string | null;
  hasAvailablePlaces: boolean | null;
}

export interface DishBrief {
  id: string;
  name: string | null;
  price: number;
  imageUrl: string | null;
}

export interface RestaurantDetail extends RestaurantBrief {
  availableDishes: DishBrief[] | null;
}

export interface CreateRestaurantRequest {
  city: string;
  address: string;
}

export interface UpdateRestaurantRequest {
  city?: string | null;
  address?: string | null;
}

