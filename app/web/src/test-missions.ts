// The example missions in docs/scenarios/missions, parsed from the YAML itself, for tests.
import { parse } from 'yaml'
import type { Mission } from './mission'
import shipRaw from '../../../docs/scenarios/missions/mission-ship-42.yaml?raw'
import landRaw from '../../../docs/scenarios/missions/mission-land-11.yaml?raw'

export const shipExample = parse(shipRaw) as Mission
export const landExample = parse(landRaw) as Mission
