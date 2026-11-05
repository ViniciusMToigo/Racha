import { HttpErrorResponse } from '@angular/common/http';
import { errorMessage, fieldErrors } from './problem-details';

describe('problem-details', () => {
  const httpError = (status: number, error: unknown) => new HttpErrorResponse({ status, error });

  it('uses the ProblemDetails detail when there are no field errors', () => {
    const error = httpError(409, {
      title: 'Conflict',
      detail: 'An account with this email already exists.',
    });

    expect(errorMessage(error)).toBe('An account with this email already exists.');
  });

  it('joins validation messages when the API returns field errors', () => {
    const error = httpError(400, {
      title: 'One or more validation errors occurred.',
      errors: { name: ['Name is required.'], amount: ['Amount must be greater than 0.'] },
    });

    expect(errorMessage(error)).toBe('Name is required. Amount must be greater than 0.');
    expect(fieldErrors(error)).toEqual({
      name: ['Name is required.'],
      amount: ['Amount must be greater than 0.'],
    });
  });

  it('explains when the API cannot be reached', () => {
    expect(errorMessage(httpError(0, null))).toContain('Cannot reach the server');
  });

  it('falls back to a generic message for unknown errors', () => {
    expect(errorMessage(new Error('boom'))).toBe('Something went wrong. Please try again.');
    expect(errorMessage(httpError(500, 'not json'))).toBe(
      'Something went wrong. Please try again.',
    );
    expect(fieldErrors(new Error('boom'))).toEqual({});
  });
});
