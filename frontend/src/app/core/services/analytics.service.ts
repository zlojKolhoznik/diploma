import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenerateReportRequest, ReportResponse, ReportAnalysisRequest, ReportAnalysisResponse } from '../models/analytics.models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/analytics`;

  generateReport(req: GenerateReportRequest): Observable<ReportResponse | HttpResponse<Blob>> {
    const binaryFormats = new Set(['excel', 'xlsx', 'pdf']);
    if (binaryFormats.has(req.format)) {
      const acceptHeader = req.format === 'pdf'
        ? 'application/pdf, application/octet-stream'
        : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/octet-stream';

      return this.http.post(`${this.baseUrl}/reports`, req, {
        observe: 'response',
        responseType: 'blob',
        headers: new HttpHeaders({ Accept: acceptHeader }),
      });
    }

    return this.http.post<ReportResponse>(`${this.baseUrl}/reports`, req);
  }

  analyzeReport(req: ReportAnalysisRequest): Observable<ReportAnalysisResponse> {
    return this.http.post<ReportAnalysisResponse>(`${this.baseUrl}/reports/analyze`, req);
  }
}

