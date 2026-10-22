// A land table from GET /land/{environment}?size=…&seed=N (Imperator.Core/Land), in the scenario format.
import type { Board, BoardCover } from './board'

export interface LandTable extends Board {
  id: string
  title: string
  rules_version: string
  content_version: string
  kind: 'land'
  environment: string
  size: 'small' | 'large'
  seed: number
  sectors: { name: string; text: string }[]
  cover: (BoardCover & { id: string; terrain: string; piece: string; sector: string })[]
  /** The conditions id (content terrain.conditions). */
  conditions: string
  /** The roll log, in order. */
  rolls: string[]
}
