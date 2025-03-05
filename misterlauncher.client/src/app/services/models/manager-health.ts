import { ModuleHealthcheck } from "./module-healthcheck"

export interface ManagerHealth {
  moduleHealthchecks: ModuleHealthcheck[]
  misterState: string
  messages: string[]
}
