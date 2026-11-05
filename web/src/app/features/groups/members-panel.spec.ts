import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ALICE, BOB, TEST_CONFIG, TRIP } from '../../../testing/test-config';
import { APP_CONFIG } from '../../core/config/app-config';
import { Group } from '../../core/models/api.models';
import { MembersPanel } from './members-panel';

describe('MembersPanel', () => {
  let httpTesting: HttpTestingController;
  const soloGroup: Group = { ...TRIP, members: [ALICE] };

  async function render() {
    TestBed.configureTestingModule({
      imports: [MembersPanel],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(MembersPanel);
    fixture.componentRef.setInput('group', soloGroup);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  function submitEmail(element: HTMLElement, email: string): void {
    const input = element.querySelector<HTMLInputElement>('input[type=email]')!;
    input.value = email;
    input.dispatchEvent(new Event('input'));
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
  }

  afterEach(() => httpTesting.verify());

  it('lists members and marks the creator', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('Alice');
    expect(element.querySelector('.badge')?.textContent).toContain('creator');
  });

  it('adds a member by email and emits the updated group', async () => {
    const { fixture, element } = await render();
    const emitted: Group[] = [];
    fixture.componentInstance.groupChange.subscribe((group) => emitted.push(group));

    submitEmail(element, BOB.email);
    const request = httpTesting.expectOne(`${TEST_CONFIG.apiUrl}/groups/${TRIP.id}/members`);
    expect(request.request.body).toEqual({ email: BOB.email });
    request.flush(TRIP);
    await fixture.whenStable();

    expect(emitted).toEqual([TRIP]);
  });

  it('shows the API error when the user does not exist', async () => {
    const { fixture, element } = await render();

    submitEmail(element, 'ghost@racha.dev');
    httpTesting
      .expectOne(`${TEST_CONFIG.apiUrl}/groups/${TRIP.id}/members`)
      .flush(
        { detail: "User 'ghost@racha.dev' was not found." },
        { status: 404, statusText: 'Not Found' },
      );
    await fixture.whenStable();

    expect(element.querySelector('.hint-error')?.textContent).toContain('was not found');
  });
});
