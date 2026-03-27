import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    localStorage.clear();

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

    const gameMenu = fixture.nativeElement.querySelector('.game-menu') as HTMLElement;

    gameMenu.dispatchEvent(new MouseEvent('mouseenter'));
    fixture.detectChanges();

    expect(fixture.componentInstance.isMenuOpen('game')).toBeTrue();
    expect(fixture.nativeElement.querySelector('.nav-menu-panel')).not.toBeNull();

    gameMenu.dispatchEvent(new MouseEvent('mouseleave'));
    fixture.detectChanges();

    expect(fixture.componentInstance.isMenuOpen('game')).toBeFalse();
    expect(fixture.nativeElement.querySelector('.nav-menu-panel')).toBeNull();
  });

  it('renders the club and league menus when an active game exists', () => {
    localStorage.setItem('football-manager.active-game', JSON.stringify({
      gameId: 'game-1',
      selectedClub: 'Arsenal',
      seasonId: 'season-1',
      seasonName: 'Season 1',
      saveName: 'Arsenal - Season 1'
    }));

    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const clubMenu = fixture.nativeElement.querySelector('.club-menu') as HTMLElement | null;
    const clubMenuButton = fixture.nativeElement.querySelector('.club-menu .nav-button') as HTMLButtonElement | null;
    const leagueMenu = fixture.nativeElement.querySelector('.league-menu') as HTMLElement | null;
    const leagueMenuButton = fixture.nativeElement.querySelector('.league-menu .nav-button') as HTMLButtonElement | null;

    expect(clubMenu).not.toBeNull();
    expect(clubMenuButton).not.toBeNull();
    expect(clubMenuButton?.textContent).toContain('Club Menu');
    expect(leagueMenu).not.toBeNull();
    expect(leagueMenuButton).not.toBeNull();
    expect(leagueMenuButton?.textContent).toContain('League Menu');

    clubMenu?.dispatchEvent(new MouseEvent('mouseenter'));
    fixture.detectChanges();

    const clubMenuItems = fixture.nativeElement.querySelectorAll('.club-menu .nav-menu-panel button');
    const clubMenuLinks = fixture.nativeElement.querySelectorAll('.club-menu .nav-menu-panel a');

    expect(clubMenuItems.length).toBe(4);
    expect(clubMenuLinks.length).toBe(0);

    leagueMenu?.dispatchEvent(new MouseEvent('mouseenter'));
    fixture.detectChanges();

    const leagueMenuItems = fixture.nativeElement.querySelectorAll('.league-menu .nav-menu-panel button');
    const leagueMenuLabels = [...leagueMenuItems].map((item) => item.textContent?.trim());

    expect(leagueMenuItems.length).toBe(4);
    expect(leagueMenuLabels).toEqual(['Match Center', 'League Table', 'Fixtures', 'Top Players']);
  });
});
