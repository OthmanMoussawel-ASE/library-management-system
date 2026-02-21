export interface CheckoutRecord {
  id: string;
  bookId: string;
  bookTitle: string;
  patronId: string;
  patronName: string;
  patronEmail: string;
  checkedOutAt: string;
  dueDate: string;
  returnedAt: string | null;
  status: string;
  isOverdue: boolean;
  notes: string | null;
}

export interface CheckoutRequest {
  bookId: string;
  dueDays: number;
  notes?: string;
}

export interface ReturnRequest {
  checkoutId: string;
  notes?: string;
}

export interface DashboardStats {
  totalBooks: number;
  availableBooks: number;
  totalAuthors: number;
  activeCheckouts: number;
  overdueCheckouts: number;
  totalPatrons?: number;
}
