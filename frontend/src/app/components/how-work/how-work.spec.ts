import { MatDialog } from '@angular/material/dialog';
import { RegisterDialog } from '../register-dialog/register-dialog';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HowWork } from './how-work';

describe('HowWork', () => {
  const dialog = { open: vi.fn() };
  let component: HowWork;
  let fixture: ComponentFixture<HowWork>;

  beforeEach(async () => {
    dialog.open.mockReset();
    await TestBed.configureTestingModule({
      imports: [HowWork],
    }).overrideProvider(MatDialog, { useValue: dialog }).compileComponents();

    fixture = TestBed.createComponent(HowWork);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('abre diálogo existente ao clicar em Criar conta', () => {
    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find(button => button.textContent?.includes('Criar conta'));
    expect(button).toBeTruthy();
    button!.click();
    expect(dialog.open).toHaveBeenCalledWith(RegisterDialog, expect.objectContaining({ width: '460px', maxWidth: 'calc(100vw - 32px)' }));
  });

  it('should create' , () => {
    expect(component).toBeTruthy();
  });
});
