import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { Subject, of } from 'rxjs';

import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { PlayerDetail, SquadPlayer } from '../../models/squad';
import { PlayerDetailComponent } from './player-detail.component';

describe('PlayerDetailComponent', () => {
  let api: jasmine.SpyObj<BootstrapApiService>;
  let updateResponse$: Subject<PlayerDetail>;

  beforeEach(async () => {
    updateResponse$ = new Subject<PlayerDetail>();
    api = jasmine.createSpyObj<BootstrapApiService>('BootstrapApiService', ['getPlayer', 'updatePlayerPosition', 'getSquad']);
    api.getSquad.and.returnValue(of([buildSquadPlayer()]));
    api.getPlayer.and.returnValue(of(buildPlayerDetail()));
    api.updatePlayerPosition.and.returnValue(updateResponse$);

    await TestBed.configureTestingModule({
      imports: [PlayerDetailComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActiveGameService,
          useValue: {
            activeGame: signal({
              gameId: 'game-1',
              selectedClub: 'Arsenal',
              seasonId: null,
              seasonName: 'Season 1',
              saveName: null
            })
          }
        },
        {
          provide: BootstrapApiService,
          useValue: api
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ playerKey: '11-theo-reed' })
            }
          }
        }
      ]
    }).compileComponents();
  });

  it('loads the player from the readable route key', () => {
    const fixture = TestBed.createComponent(PlayerDetailComponent);
    fixture.detectChanges();

    expect(api.getSquad).toHaveBeenCalledWith('game-1');
    expect(api.getPlayer).toHaveBeenCalledWith('game-1', 'player-1');
    expect(fixture.componentInstance.player()?.name).toBe('Theo Reed');
  });

  it('blocks leaving the page while a position update is still in progress', () => {
    const fixture = TestBed.createComponent(PlayerDetailComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.onPositionChange({ target: { value: 'Midfielder' } } as unknown as Event);
    component.updatePosition();
    fixture.detectChanges();

    expect(component.canDeactivate()).toBeFalse();

    const preventDefault = jasmine.createSpy('preventDefault');
    const stopPropagation = jasmine.createSpy('stopPropagation');
    component.preventNavigationWhileUpdating({
      preventDefault,
      stopPropagation
    } as unknown as Event);

    expect(preventDefault).toHaveBeenCalled();
    expect(stopPropagation).toHaveBeenCalled();
    expect(component.positionFeedbackMessage()).toBe('Wait for the role change to finish before leaving this page.');

    updateResponse$.next({
      ...buildPlayerDetail(),
      position: 'Midfielder'
    });
    updateResponse$.complete();
    fixture.detectChanges();

    expect(component.canDeactivate()).toBeTrue();
  });
});

function buildPlayerDetail(): PlayerDetail {
  return {
    id: 'player-1',
    name: 'Theo Reed',
    position: 'Forward',
    age: 22,
    squadNumber: 11,
    attack: 78,
    defense: 55,
    passing: 73,
    fitness: 82,
    morale: 79,
    overallRating: 76,
    isCaptain: false,
    isStarter: false,
    isInjured: false,
    injuryMatchesRemaining: 0,
    roleStatus: 'Squad Depth',
    managerNote: 'Direct and lively in the final third.'
  };
}

function buildSquadPlayer(): SquadPlayer {
  return {
    id: 'player-1',
    name: 'Theo Reed',
    position: 'Forward',
    age: 22,
    squadNumber: 11,
    attack: 78,
    defense: 55,
    passing: 73,
    fitness: 82,
    morale: 79,
    overallRating: 76,
    isCaptain: false,
    isStarter: false,
    isInjured: false,
    injuryMatchesRemaining: 0
  };
}
