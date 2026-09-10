import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LeafletMap } from './leaflet-map';

describe('Map', () => {
  let component: LeafletMap;
  let fixture: ComponentFixture<LeafletMap>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Map],
    }).compileComponents();

    fixture = TestBed.createComponent(LeafletMap);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
