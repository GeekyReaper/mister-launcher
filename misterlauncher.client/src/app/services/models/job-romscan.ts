import { JobLog } from "./job-log";
import { RomUpdateResult } from "./rom-update-result";

export interface JobRomscan {
  jobName: string;
  jobType: string;
  logs: JobLog[];
  start: Date;
  delay: number;
  state: string;
  result: RomUpdateResult;
}
