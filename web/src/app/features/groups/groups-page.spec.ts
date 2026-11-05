import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { TEST_CONFIG, TRIP } from '../../../testing/test-config';
import { APP_CONFIG } from '../../core/config/app-config';
import { GroupsPage } from './groups-page';

describe('GroupsPage', () => {
  let httpTesting: HttpTestingController;
  const groupsUrl = `${TEST_CONFIG.apiUrl}/groups`;

  async function render() {
    TestBed.configureTestingModule({
      imports: [GroupsPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(GroupsPage);
    TestBed.tick();
    httpTesting.expectOne({ method: 'GET', url: groupsUrl }).flush([]);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  function typeName(element: HTMLElement, value: string): void {
    const input = element.querySelector<HTMLInputElement>('.create-form input')!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  afterEach(() => httpTesting.verify());

  it('shows the empty state when the user has no groups', async () => {
    const { element } = await render();

    expect(element.textContent).toContain("You're not in any group yet");
  });

  // Regression: the form used to have no [formGroup], so (ngSubmit) never fired
  // and the browser reloaded the page instead of creating the group.
  it('creates a group when the button is clicked and opens it', async () => {
    const { fixture, element } = await render();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    typeName(element, '  Beach house  ');
    element.querySelector<HTMLButtonElement>('.create-form button[type=submit]')!.click();

    const request = httpTesting.expectOne({ method: 'POST', url: groupsUrl });
    expect(request.request.body).toEqual({ name: 'Beach house' });
    request.flush({ ...TRIP, id: 'new-id', name: 'Beach house' });
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/groups', 'new-id']);
  });

  it('does not call the API when the name is blank', async () => {
    const { fixture, element } = await render();

    typeName(element, '   ');
    element.querySelector<HTMLButtonElement>('.create-form button[type=submit]')!.click();
    await fixture.whenStable();

    httpTesting.expectNone({ method: 'POST', url: groupsUrl });
    expect(element.textContent).toContain('Give the group a name.');
  });
});
