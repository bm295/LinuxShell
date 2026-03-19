import { Injectable, signal } from '@angular/core';

export type GameMenuAction = 'save' | 'load';

@Injectable({ providedIn: 'root' })
export class GameMenuService {
  readonly pendingAction = signal<GameMenuAction | null>(null);

  requestAction(action: GameMenuAction): void {
    this.pendingAction.set(action);
  }

  clearAction(expectedAction?: GameMenuAction): void {
    if (expectedAction && this.pendingAction() !== expectedAction) {
      return;
    }

    this.pendingAction.set(null);
  }
}
