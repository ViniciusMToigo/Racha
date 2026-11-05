// TypeScript mirrors of the API's DTOs (camelCase JSON). Ids are GUID strings, dates are ISO strings.

export interface User {
  id: string;
  name: string;
  email: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  expiresAt: string;
}

export interface GroupSummary {
  id: string;
  name: string;
  memberCount: number;
}

export interface Group {
  id: string;
  name: string;
  createdByUserId: string;
  members: User[];
}

export interface ExpenseShare {
  userId: string;
  amount: number;
}

export interface Expense {
  id: string;
  groupId: string;
  description: string;
  amount: number;
  /** yyyy-MM-dd */
  date: string;
  paidByUserId: string;
  shares: ExpenseShare[];
}

export interface ExpenseRequest {
  description: string;
  amount: number;
  /** yyyy-MM-dd */
  date: string;
  paidByUserId: string;
}

/** Positive: the member is owed money. Negative: the member owes money. */
export interface MemberBalance {
  userId: string;
  name: string;
  amount: number;
}

export interface Transfer {
  fromUserId: string;
  fromName: string;
  toUserId: string;
  toName: string;
  amount: number;
}

export interface GroupBalances {
  groupId: string;
  balances: MemberBalance[];
  suggestedTransfers: Transfer[];
}

/** RFC 9457 error body returned by both APIs. */
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
