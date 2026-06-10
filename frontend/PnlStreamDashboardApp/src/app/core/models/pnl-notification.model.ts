export interface PnlNotificationDto {
  id?: number;
  sourceSystem: string;
  portfolioNumber: number;
  pnlAmount: number;
  isValid: boolean;
  validationReasons?: string;
  dataSource: number | string;
  createdAt: string;
  lastUpdatedAt: string;
}
