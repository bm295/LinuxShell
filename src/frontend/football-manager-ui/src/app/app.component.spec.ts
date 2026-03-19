import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('opens and closes the game menu on hover', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const gameMenu = fixture.nativeElement.querySelector('.nav-menu') as HTMLElement;

    gameMenu.dispatchEvent(new MouseEvent('mouseenter'));
    fixture.detectChanges();

    expect(fixture.componentInstance.isGameMenuOpen()).toBeTrue();
    expect(fixture.nativeElement.querySelector('.game-menu-panel')).not.toBeNull();

    gameMenu.dispatchEvent(new MouseEvent('mouseleave'));
    fixture.detectChanges();

    expect(fixture.componentInstance.isGameMenuOpen()).toBeFalse();
    expect(fixture.nativeElement.querySelector('.game-menu-panel')).toBeNull();
  });
});
