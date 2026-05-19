export interface ReviewResponse {
  id: string;
  reservationId: string;
  cuisineRating: number;
  cuisineComment: string | null;
  serviceRating: number;
  serviceComment: string | null;
  createdAtUtc: string;
}

export interface CreateReviewRequest {
  cuisineRating: number;
  cuisineComment?: string | null;
  serviceRating: number;
  serviceComment?: string | null;
}

export interface ReviewModerationRequest {
  cuisineRating: number;
  cuisineComment?: string | null;
  serviceRating: number;
  serviceComment?: string | null;
}

export interface ReviewModerationResult {
  approved: boolean;
  reason: string | null;
  suggestedRephrasing: string | null;
}

