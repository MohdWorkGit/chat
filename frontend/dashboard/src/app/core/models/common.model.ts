export interface PaginatedResult<T> {
  data: T[];
  meta: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
}

export interface ApiError {
  status: number;
  message: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

export interface SearchResult {
  conversations: {
    id: number;
    displayId: number;
    contactName: string;
    lastMessage: string;
  }[];
  contacts: {
    id: number;
    name: string;
    email?: string;
    phone?: string;
  }[];
  messages: {
    id: number;
    conversationId: number;
    content: string;
  }[];
}
