import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { appPaths, resolveGameId } from '../../core/routing/app-paths';
import { ActiveGameService } from '../../core/services/active-game.service';
import { BootstrapApiService } from '../../core/services/bootstrap-api.service';
import { LineupEditor } from '../../models/lineup';
import { SquadPlayer } from '../../models/squad';

type PositionKey = 'Goalkeeper' | 'Defender' | 'Midfielder' | 'Forward';

const positionOrder: PositionKey[] = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];

@Component({
  selector: 'app-lineup-editor',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './lineup-editor.component.html',
  styleUrl: './lineup-editor.component.scss'
})
export class LineupEditorComponent implements OnInit {
  private readonly api = inject(BootstrapApiService);
  private readonly activeGameService = inject(ActiveGameService);
  private readonly route = inject(ActivatedRoute);

  readonly positions = positionOrder;
  readonly gameId = signal<string | null>(null);
  readonly editorData = signal<LineupEditor | null>(null);
  readonly selectedFormationId = signal<string | null>(null);
  readonly selectedPlayerIds = signal<string[]>([]);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly feedbackMessage = signal<string | null>(null);
  readonly feedbackTone = signal<'idle' | 'success' | 'error'>('idle');
  readonly squadLink = computed(() => this.gameId() ? appPaths.squad : '/');
  readonly dashboardLink = computed(() => this.gameId() ? appPaths.dashboard : '/');
  readonly matchCenterLink = computed(() => this.gameId() ? appPaths.matchCenter : '/');
  readonly activeFormation = computed(() => {
    const data = this.editorData();
    const formationId = this.selectedFormationId();
    return data?.formations.find((formation) => formation.id === formationId) ?? data?.formations[0] ?? null;
  });
  readonly requirements = computed<Record<PositionKey, number>>(() => {
    const formation = this.activeFormation();

    return {
      Goalkeeper: 1,
      Defender: formation?.defenders ?? 0,
      Midfielder: formation?.midfielders ?? 0,
      Forward: formation?.forwards ?? 0
    };
  });
  readonly playerBuckets = computed<Record<PositionKey, SquadPlayer[]>>(() => {
    const selectedIds = new Set(this.selectedPlayerIds());
    const players = [...(this.editorData()?.players ?? [])].sort((left, right) =>
      this.comparePlayers(left, right, selectedIds));

    return {
      Goalkeeper: players.filter((player) => this.toPositionKey(player.position) === 'Goalkeeper'),
      Defender: players.filter((player) => this.toPositionKey(player.position) === 'Defender'),
      Midfielder: players.filter((player) => this.toPositionKey(player.position) === 'Midfielder'),
      Forward: players.filter((player) => this.toPositionKey(player.position) === 'Forward')
    };
  });
  readonly selectionCounts = computed<Record<PositionKey, number>>(() => {
    const counts: Record<PositionKey, number> = {
      Goalkeeper: 0,
      Defender: 0,
      Midfielder: 0,
      Forward: 0
    };

    const selectedIds = new Set(this.selectedPlayerIds());
    for (const player of this.editorData()?.players ?? []) {
      if (selectedIds.has(player.id)) {
        counts[this.toPositionKey(player.position)] += 1;
      }
    }

    return counts;
  });
  readonly selectedPlayers = computed(() => {
    const selectedIds = new Set(this.selectedPlayerIds());
    return (this.editorData()?.players ?? [])
      .filter((player) => selectedIds.has(player.id))
      .sort((left, right) => this.comparePlayers(left, right, selectedIds));
  });
  readonly summary = computed(() => {
    const players = this.selectedPlayers();
    return {
      total: players.length,
      averageRating: this.calculateAverage(players.map((player) => player.overallRating)),
      averageFitness: this.calculateAverage(players.map((player) => player.fitness)),
      averageMorale: this.calculateAverage(players.map((player) => player.morale))
    };
  });
  readonly canSave = computed(() => {
    if (!this.activeFormation() || this.isSaving()) {
      return false;
    }

    const counts = this.selectionCounts();
    const requirements = this.requirements();
    return this.positions.every((position) => counts[position] === requirements[position]);
  });

  ngOnInit(): void {
    const gameId = resolveGameId(this.activeGameService, this.route);

    if (!gameId) {
      this.errorMessage.set('Missing game identifier. Start a new save before setting a lineup.');
      this.isLoading.set(false);
      return;
    }

    this.gameId.set(gameId);
    this.loadEditor(gameId);
  }

  onFormationChange(event: Event): void {
    const nextFormationId = (event.target as HTMLSelectElement).value;

    if (!nextFormationId || nextFormationId === this.selectedFormationId()) {
      return;
    }

    this.selectedFormationId.set(nextFormationId);
    this.feedbackMessage.set(null);
    this.feedbackTone.set('idle');
    this.rebalanceSelection();
  }

  togglePlayer(player: SquadPlayer): void {
    if (player.isInjured) {
      this.feedbackMessage.set(`${player.name} is unavailable for ${player.injuryMatchesRemaining} more matchday(s).`);
      this.feedbackTone.set('error');
      return;
    }

    const nextIds = new Set(this.selectedPlayerIds());

    if (nextIds.has(player.id)) {
      nextIds.delete(player.id);
      this.applySelection(nextIds);
      this.feedbackMessage.set(null);
      this.feedbackTone.set('idle');
      return;
    }

    const position = this.toPositionKey(player.position);
    if (this.selectionCounts()[position] >= this.requirements()[position]) {
      const replacedPlayer = this.findReplaceablePlayer(position, nextIds);

      if (!replacedPlayer) {
        this.feedbackMessage.set(`No ${position.toLowerCase()} slot can be changed right now.`);
        this.feedbackTone.set('error');
        return;
      }

      nextIds.delete(replacedPlayer.id);
      nextIds.add(player.id);
      this.applySelection(nextIds);
      this.feedbackMessage.set(`${player.name} replaces ${replacedPlayer.name} in the ${position.toLowerCase()} unit.`);
      this.feedbackTone.set('success');
      return;
    }

    nextIds.add(player.id);
    this.applySelection(nextIds);
    this.feedbackMessage.set(null);
    this.feedbackTone.set('idle');
  }

  isSelected(playerId: string): boolean {
    return this.selectedPlayerIds().includes(playerId);
  }

  isLocked(player: SquadPlayer): boolean {
    if (player.isInjured) {
      return true;
    }

    if (this.isSelected(player.id)) {
      return false;
    }

    const position = this.toPositionKey(player.position);
    return this.selectionCounts()[position] >= this.requirements()[position] &&
      this.findReplaceablePlayer(position) === null;
  }

  saveLineup(): void {
    const gameId = this.gameId();
    const formationId = this.selectedFormationId();

    if (!gameId || !formationId || !this.canSave()) {
      return;
    }

    this.isSaving.set(true);
    this.feedbackMessage.set(null);
    this.feedbackTone.set('idle');

    this.api.saveLineup(gameId, {
      formationId,
      playerIds: this.selectedPlayerIds()
    }).subscribe({
      next: (lineup) => {
        const current = this.editorData();

        if (!current) {
          this.isSaving.set(false);
          return;
        }

        const starterIds = new Set(lineup.starterPlayerIds);
        this.editorData.set({
          ...current,
          lineup,
          players: current.players.map((player) => ({
            ...player,
            isStarter: starterIds.has(player.id)
          }))
        });
        this.selectedPlayerIds.set(this.sortStarterIds([...lineup.starterPlayerIds]));
        this.selectedFormationId.set(lineup.formationId);
        this.feedbackMessage.set(`${lineup.formationName} saved. ${lineup.readiness}.`);
        this.feedbackTone.set('success');
        this.isSaving.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.feedbackMessage.set(error.error?.message ?? 'The lineup could not be saved right now.');
        this.feedbackTone.set('error');
        this.isSaving.set(false);
      }
    });
  }

  private loadEditor(gameId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api.getLineup(gameId).subscribe({
      next: (editor) => {
        this.editorData.set(editor);
        this.selectedFormationId.set(editor.lineup.formationId);
        this.applySelection(editor.lineup.starterPlayerIds);
        this.rebalanceSelection();
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The tactics room is unavailable. Reopen the save and try again.');
        this.isLoading.set(false);
      }
    });
  }

  private rebalanceSelection(): void {
    const currentSelectedIds = new Set(this.selectedPlayerIds());
    const nextIds: string[] = [];

    for (const position of this.positions) {
      const starters = (this.editorData()?.players ?? [])
        .filter((player) => !player.isInjured && this.toPositionKey(player.position) === position)
        .sort((left, right) => this.comparePlayers(left, right, currentSelectedIds))
        .slice(0, this.requirements()[position]);

      nextIds.push(...starters.map((player) => player.id));
    }

    this.applySelection(nextIds);
  }

  private sortStarterIds(playerIds: string[]): string[] {
    const playersById = new Map((this.editorData()?.players ?? []).map((player) => [player.id, player]));
    const selectedIds = new Set(playerIds);

    return [...playerIds].sort((leftId, rightId) => {
      const left = playersById.get(leftId);
      const right = playersById.get(rightId);

      if (!left || !right) {
        return 0;
      }

      return positionOrder.indexOf(this.toPositionKey(left.position)) - positionOrder.indexOf(this.toPositionKey(right.position)) ||
        this.comparePlayers(left, right, selectedIds);
    });
  }

  private findReplaceablePlayer(position: PositionKey, selectedIds = new Set(this.selectedPlayerIds())): SquadPlayer | null {
    const selectedPlayers = (this.editorData()?.players ?? [])
      .filter((player) => selectedIds.has(player.id) && this.toPositionKey(player.position) === position)
      .sort((left, right) => this.comparePlayers(left, right, selectedIds));

    return selectedPlayers.at(-1) ?? null;
  }

  private applySelection(playerIds: Iterable<string>): void {
    const nextIds = this.sortStarterIds([...new Set(playerIds)]);
    const starterIds = new Set(nextIds);

    this.selectedPlayerIds.set(nextIds);

    const current = this.editorData();
    if (!current) {
      return;
    }

    this.editorData.set({
      ...current,
      players: current.players.map((player) => ({
        ...player,
        isStarter: starterIds.has(player.id)
      }))
    });
  }

  private comparePlayers(left: SquadPlayer, right: SquadPlayer, selectedIds = new Set(this.selectedPlayerIds())): number {
    return Number(selectedIds.has(right.id)) - Number(selectedIds.has(left.id)) ||
    Number(left.isInjured) - Number(right.isInjured) ||
    right.overallRating - left.overallRating ||
    right.fitness - left.fitness ||
    left.squadNumber - right.squadNumber;
  }

  private toPositionKey(position: string): PositionKey {
    return positionOrder.includes(position as PositionKey)
      ? position as PositionKey
      : 'Midfielder';
  }

  private calculateAverage(values: number[]): number {
    return values.length === 0
      ? 0
      : Math.round(values.reduce((total, value) => total + value, 0) / values.length);
  }
}
