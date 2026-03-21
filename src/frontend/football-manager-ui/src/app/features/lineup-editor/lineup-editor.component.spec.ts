import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { LineupEditor } from '../../models/lineup';
import { SquadPlayer } from '../../models/squad';
import { LineupEditorComponent } from './lineup-editor.component';

describe('LineupEditorComponent', () => {
  let api: jasmine.SpyObj<BootstrapApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<BootstrapApiService>('BootstrapApiService', ['getLineup', 'saveLineup']);

    const editor = buildEditor();
    api.getLineup.and.returnValue(of(editor));
    api.saveLineup.and.callFake((_gameId, request) => of({
      ...editor.lineup,
      formationId: request.formationId,
      starterPlayerIds: request.playerIds,
      starterCount: request.playerIds.length
    }));

    await TestBed.configureTestingModule({
      imports: [LineupEditorComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ gameId: 'game-1' })
            }
          }
        },
        { provide: BootstrapApiService, useValue: api }
      ]
    }).compileComponents();
  });

  it('replaces an existing starter when selecting another player in a full position bucket', () => {
    const fixture = TestBed.createComponent(LineupEditorComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    const replacement = component.playerBuckets().Defender.find((player) => player.id === 'd5');

    expect(replacement).toBeTruthy();

    component.togglePlayer(replacement as SquadPlayer);
    fixture.detectChanges();

    expect(component.selectedPlayerIds()).toContain('d5');
    expect(component.selectedPlayerIds()).not.toContain('d4');
    expect(component.selectionCounts().Defender).toBe(4);
    expect(component.canSave()).toBeTrue();
  });

  it('shows the saved formation when the editor reloads with a non-default shape', () => {
    const fixture = TestBed.createComponent(LineupEditorComponent);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    const selectedOption = select.selectedOptions.item(0);

    expect(select.value).toBe('formation-442');
    expect(selectedOption?.textContent?.trim()).toBe('4-4-2');
  });

  it('reconciles a reclassified starter into a valid lineup when the page loads', () => {
    const editor = buildEditor();
    const reclassifiedStarter = editor.players.find((player) => player.id === 'f2');

    expect(reclassifiedStarter).toBeTruthy();

    (reclassifiedStarter as SquadPlayer).position = 'Midfielder';
    api.getLineup.and.returnValue(of(editor));

    const fixture = TestBed.createComponent(LineupEditorComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.selectionCounts()).toEqual({
      Goalkeeper: 1,
      Defender: 4,
      Midfielder: 4,
      Forward: 2
    });
    expect(fixture.componentInstance.selectedPlayerIds()).toContain('f2');
    expect(fixture.componentInstance.selectedPlayerIds()).toContain('f3');
    expect(fixture.componentInstance.selectedPlayerIds()).not.toContain('m4');
    expect(fixture.componentInstance.canSave()).toBeTrue();
  });

  it('auto-balances the starters when switching formations so the lineup stays saveable', () => {
    const fixture = TestBed.createComponent(LineupEditorComponent);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    const saveButton = fixture.nativeElement.querySelector('.save-button') as HTMLButtonElement;

    select.value = 'formation-433';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(saveButton.disabled).toBeFalse();
    expect(select.value).toBe('formation-433');
    expect(fixture.componentInstance.selectionCounts()).toEqual({
      Goalkeeper: 1,
      Defender: 4,
      Midfielder: 3,
      Forward: 3
    });
    expect(fixture.componentInstance.selectedPlayerIds()).toContain('f3');
    expect(fixture.componentInstance.selectedPlayerIds()).not.toContain('m4');

    select.value = 'formation-352';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(saveButton.disabled).toBeFalse();
    expect(select.value).toBe('formation-352');
    expect(fixture.componentInstance.selectionCounts()).toEqual({
      Goalkeeper: 1,
      Defender: 3,
      Midfielder: 5,
      Forward: 2
    });
    expect(fixture.componentInstance.selectedPlayerIds()).toContain('m5');
    expect(fixture.componentInstance.selectedPlayerIds()).not.toContain('d4');
  });

  it('sends the swapped lineup when saving after a same-position replacement', () => {
    const fixture = TestBed.createComponent(LineupEditorComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    const replacement = component.playerBuckets().Defender.find((player) => player.id === 'd5');

    component.togglePlayer(replacement as SquadPlayer);
    component.saveLineup();

    expect(api.saveLineup).toHaveBeenCalled();
    const request = api.saveLineup.calls.mostRecent().args[1];

    expect(request.formationId).toBe('formation-442');
    expect(request.playerIds).toContain('d5');
    expect(request.playerIds).not.toContain('d4');
  });
});

function buildEditor(): LineupEditor {
  return {
    clubName: 'Arsenal',
    lineup: {
      formationId: 'formation-442',
      formationName: '4-4-2',
      starterCount: 11,
      requiredStarters: 11,
      averageRating: 75,
      averageFitness: 81,
      averageMorale: 79,
      readiness: 'Ready for kickoff',
      starterPlayerIds: ['g1', 'd1', 'd2', 'd3', 'd4', 'm1', 'm2', 'm3', 'm4', 'f1', 'f2']
    },
    formations: [
      {
        id: 'formation-352',
        name: '3-5-2',
        defenders: 3,
        midfielders: 5,
        forwards: 2
      },
      {
        id: 'formation-442',
        name: '4-4-2',
        defenders: 4,
        midfielders: 4,
        forwards: 2
      },
      {
        id: 'formation-433',
        name: '4-3-3',
        defenders: 4,
        midfielders: 3,
        forwards: 3
      }
    ],
    players: [
      buildPlayer('g1', 'Goalkeeper One', 'Goalkeeper', 1, 72, true),
      buildPlayer('g2', 'Goalkeeper Two', 'Goalkeeper', 12, 67, false),
      buildPlayer('d1', 'Defender One', 'Defender', 2, 78, true),
      buildPlayer('d2', 'Defender Two', 'Defender', 3, 76, true),
      buildPlayer('d3', 'Defender Three', 'Defender', 4, 74, true),
      buildPlayer('d4', 'Defender Four', 'Defender', 5, 68, true),
      buildPlayer('d5', 'Defender Five', 'Defender', 13, 75, false),
      buildPlayer('m1', 'Midfielder One', 'Midfielder', 6, 80, true),
      buildPlayer('m2', 'Midfielder Two', 'Midfielder', 7, 79, true),
      buildPlayer('m3', 'Midfielder Three', 'Midfielder', 8, 77, true),
      buildPlayer('m4', 'Midfielder Four', 'Midfielder', 14, 73, true),
      buildPlayer('m5', 'Midfielder Five', 'Midfielder', 15, 71, false),
      buildPlayer('f1', 'Forward One', 'Forward', 9, 82, true),
      buildPlayer('f2', 'Forward Two', 'Forward', 10, 81, true),
      buildPlayer('f3', 'Forward Three', 'Forward', 11, 74, false)
    ]
  };
}

function buildPlayer(
  id: string,
  name: string,
  position: string,
  squadNumber: number,
  overallRating: number,
  isStarter: boolean
): SquadPlayer {
  return {
    id,
    name,
    position,
    age: 24,
    squadNumber,
    attack: overallRating,
    defense: overallRating,
    passing: overallRating,
    fitness: 80,
    morale: 78,
    overallRating,
    isCaptain: false,
    isStarter,
    isInjured: false,
    injuryMatchesRemaining: 0
  };
}
