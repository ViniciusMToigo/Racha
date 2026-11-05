import { AppConfig } from '../app/core/config/app-config';
import { AuthResponse, Group, User } from '../app/core/models/api.models';

/** Fake API URLs, so tests never depend on the real environment file. */
export const TEST_CONFIG: AppConfig = {
  authApiUrl: 'http://auth.test/api',
  apiUrl: 'http://api.test/api',
};

export const ALICE: User = { id: 'alice-id', name: 'Alice', email: 'alice@racha.dev' };
export const BOB: User = { id: 'bob-id', name: 'Bob', email: 'bob@racha.dev' };

export function sessionFor(user: User, expiresInMinutes = 60): AuthResponse {
  return {
    user,
    accessToken: `token-for-${user.id}`,
    expiresAt: new Date(Date.now() + expiresInMinutes * 60_000).toISOString(),
  };
}

export const TRIP: Group = {
  id: 'trip-id',
  name: 'Trip to Floripa',
  createdByUserId: ALICE.id,
  members: [ALICE, BOB],
};
