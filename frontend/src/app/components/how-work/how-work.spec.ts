import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HowWork } from './how-work';

describe('HowWork', () => {
  let component: HowWork;
  let fixture: ComponentFixture<HowWork>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HowWork],
    }).compileComponents();

    fixture = TestBed.createComponent(HowWork);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
