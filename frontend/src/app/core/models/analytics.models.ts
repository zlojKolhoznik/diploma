export type ReportType = 'profitability' | 'menu' | 'waiter';

export type ReportFormat = 'summary' | 'xlsx' | 'pdf';

export interface GenerateReportRequest {
  type: ReportType;
  format: ReportFormat;
  restaurantId?: string | null;
  fromUtc?: string | null;
  toUtc?: string | null;
}

export interface ReportMetricResponse {
  name: string | null;
  value: string | null;
}

export interface ReportSectionResponse {
  title: string | null;
  metrics: ReportMetricResponse[] | null;
}

export interface ReportResponse {
  type: string | null;
  format: string | null;
  restaurantId: string | null;
  generatedAtUtc: string;
  title: string | null;
  sections: ReportSectionResponse[] | null;
}

export interface ReportAnalysisRequest {
  reportContent?: string | null;
  analysisType?: ReportType | null;
  restaurantId?: string | null;
}

export interface ReportAnalysisResponse {
  analysis: string | null;
  analyzedAtUtc: string;
}

