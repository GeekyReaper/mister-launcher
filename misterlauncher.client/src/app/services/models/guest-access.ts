export interface GuestAccess {
  created: Date;
  expired: Date;
  state: string;
  code: string;
  clientkey?: string;
}
