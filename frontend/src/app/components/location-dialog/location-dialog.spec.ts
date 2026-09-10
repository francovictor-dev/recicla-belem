import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LocationDialog } from './location-dialog';

describe('LocationDialog', () => {
  let component: LocationDialog;
  let fixture: ComponentFixture<LocationDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LocationDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(LocationDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
