import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PromptDialog, PromptDialogData } from './prompt-dialog';

describe('PromptDialog', () => {
  async function render(data: PromptDialogData) {
    const close = vi.fn();
    TestBed.configureTestingModule({
      imports: [PromptDialog],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: { close } },
      ],
    });
    const fixture = TestBed.createComponent(PromptDialog);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement, close };
  }

  it('closes with the trimmed text when saved', async () => {
    const { fixture, element, close } = await render({
      title: 'Rename',
      label: 'Name',
      value: 'Trip',
    });
    const input = element.querySelector<HTMLInputElement>('input')!;

    input.value = '  Beach trip ';
    input.dispatchEvent(new Event('input'));
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();

    expect(close).toHaveBeenCalledWith('Beach trip');
  });

  it('does not close while the text is empty', async () => {
    const { fixture, element, close } = await render({ title: 'Rename', label: 'Name', value: '' });

    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();

    expect(close).not.toHaveBeenCalled();
    expect(element.textContent).toContain('This field is required.');
  });
});
