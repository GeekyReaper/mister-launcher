import { SortField } from "./sort-field"

export interface SystemSearchRequest {
  name?: string
  regex?: string
  year?: number
  yearMax?: number
  yearMin?: number
  AllowNoVideoGame?: boolean
  allowUnknowYear?: boolean
  category?: string
  family?: string
  generation?: number
  company?: string
  limit?: number
  sortFields?: SortField[]
}
