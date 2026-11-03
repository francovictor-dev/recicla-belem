import { TestBed } from '@angular/core/testing';
import { Loading } from './loading';
describe('Loading', () => {
  it('exibe estado acessível', async () => {
    const fixture = TestBed.createComponent(Loading);
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('Aguarde');
  });
});
