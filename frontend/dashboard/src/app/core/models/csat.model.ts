export interface CsatResponse {
  id: number;
  conversationId: number;
  contactId: number;
  assigneeId?: number;
  rating?: number;
  feedbackText?: string;
  createdAt: string;
}

export interface CsatMetrics {
  totalResponses: number;
  averageRating: number;
  satisfactionScore: number;
  ratingDistribution: Record<number, number>;
}

export interface CsatReportFilter {
  since?: string;
  until?: string;
  page?: number;
  pageSize?: number;
}
