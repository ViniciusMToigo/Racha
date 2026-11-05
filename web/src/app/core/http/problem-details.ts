import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/api.models';

const FALLBACK_MESSAGE = 'Something went wrong. Please try again.';

/** Turns any error from the APIs into one message that can be shown to the user. */
export function errorMessage(error: unknown, fallback = FALLBACK_MESSAGE): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 0) {
    return 'Cannot reach the server. Check that the APIs are running.';
  }

  const problem = asProblem(error.error);
  const validationMessages = Object.values(problem?.errors ?? {}).flat();

  if (validationMessages.length > 0) {
    return validationMessages.join(' ');
  }

  return problem?.detail ?? problem?.title ?? fallback;
}

/** Per-field validation errors (keys are the camelCase request property names). */
export function fieldErrors(error: unknown): Record<string, string[]> {
  return error instanceof HttpErrorResponse ? (asProblem(error.error)?.errors ?? {}) : {};
}

function asProblem(body: unknown): ProblemDetails | null {
  return body !== null && typeof body === 'object' ? (body as ProblemDetails) : null;
}
