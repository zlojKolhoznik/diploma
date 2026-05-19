import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TablerIconComponent } from '@tabler/icons-angular';
import { AnalyticsService } from '../../core/services/analytics.service';
import { RestaurantsService } from '../../core/services/restaurants.service';
import { GenerateReportRequest, ReportResponse, ReportAnalysisResponse, ReportMetricResponse, ReportSectionResponse } from '../../core/models/analytics.models';
import { RestaurantBrief } from '../../core/models/restaurant.models';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule, TablerIconComponent, SpinnerComponent],
  template: `
    <div class="page-container">
      <h2>Analytics</h2>

      <div class="filter-panel">
        <div class="form-group">
          <label for="rtype">Report Type *</label>
          <select id="rtype" [(ngModel)]="form.type" class="filter-select">
            <option value="profitability">Profitability</option>
            <option value="menu">Menu</option>
            <option value="waiter">Waiter Performance</option>
          </select>
        </div>
        <div class="form-group">
          <label for="rfmt">Format</label>
          <select id="rfmt" [(ngModel)]="form.format" class="filter-select">
            <option value="summary">Summary</option>
            <option value="xlsx">Excel (.xlsx)</option>
            <option value="pdf">PDF</option>
          </select>
        </div>
        <div class="form-group">
          <label for="rrest">Restaurant (optional)</label>
          <select id="rrest" [(ngModel)]="form.restaurantId" class="filter-select">
            <option value="">All</option>
            @for (r of restaurants(); track r.id) {
              <option [value]="r.id">{{ r.city }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="rfrom">From</label>
          <input id="rfrom" type="datetime-local" [(ngModel)]="form.fromUtc" />
        </div>
        <div class="form-group">
          <label for="rto">To</label>
          <input id="rto" type="datetime-local" [(ngModel)]="form.toUtc" />
        </div>
        <button class="btn-primary" (click)="generate()" [disabled]="reportLoading()">
          <tabler-icon [icon]="'chart-bar'" [size]="16"></tabler-icon>
          {{ reportLoading() ? 'Generating...' : 'Generate Report' }}
        </button>
      </div>

      @if (reportLoading()) { <app-spinner></app-spinner> }

      @if (report()) {
        <div class="report-section">
          <h3>{{ report()!.title }}</h3>
          <p class="report-meta">Generated: {{ report()!.generatedAtUtc | date:'medium' }}</p>

          <div class="report-sub-grid">
            @for (section of report()!.sections ?? []; track section.title; let i = $index) {
              @if (i === 0) {
                <section class="report-summary">
                  <p class="report-summary__eyebrow">Report Summary</p>
                  <h4>{{ section.title }}</h4>
                  <div class="report-sub-metrics">
                    @for (metric of visibleMetrics(section); track $index) {
                      <div class="metric-row">
                        <p class="metric-name">{{ metric.name }}</p>
                        <p class="metric-value">{{ metric.value }}</p>
                      </div>
                    }
                    @if (reviewCommentMetrics(section).length > 0) {
                      <details class="review-comments">
                        <summary>Review Comments ({{ reviewCommentMetrics(section).length }})</summary>
                        <div class="review-comments__list">
                          @for (comment of reviewCommentMetrics(section); track $index) {
                            <p class="review-comments__item">{{ comment.value }}</p>
                          }
                        </div>
                      </details>
                    }
                  </div>
                </section>
              } @else {
                <article class="report-sub-card">
                  <h4>{{ section.title }}</h4>
                  <div class="report-sub-metrics">
                    @for (metric of visibleMetrics(section); track $index) {
                      <div class="metric-row">
                        <p class="metric-name">{{ metric.name }}</p>
                        <p class="metric-value">{{ metric.value }}</p>
                      </div>
                    }
                    @if (reviewCommentMetrics(section).length > 0) {
                      <details class="review-comments">
                        <summary>Review Comments ({{ reviewCommentMetrics(section).length }})</summary>
                        <div class="review-comments__list">
                          @for (comment of reviewCommentMetrics(section); track $index) {
                            <p class="review-comments__item">{{ comment.value }}</p>
                          }
                        </div>
                      </details>
                    }
                  </div>
                </article>
              }
            }
          </div>

          <button class="btn-ai" (click)="analyseWithAi()" [disabled]="aiLoading()">
            <tabler-icon [icon]="'eyeglass'" [size]="16"></tabler-icon>
            {{ aiLoading() ? 'Analysing...' : 'Analyse with AI' }}
          </button>
        </div>
      }

      @if (aiResult()) {
        <div class="ai-panel">
          <h4>AI Analysis</h4>
          <div class="ai-html" [innerHTML]="aiAnalysisHtml()"></div>
          <small>Analysed at {{ aiResult()!.analyzedAtUtc | date:'medium' }}</small>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: var(--space-xl) var(--space-lg); max-width: 900px; }
    h2 { margin-bottom: var(--space-xl); }
    .filter-panel { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: var(--space-md); align-items: end; background: var(--surface-800); border: 1px solid var(--border-default); border-radius: var(--radius-lg); padding: var(--space-lg); margin-bottom: var(--space-xl); }
    .form-group { display: flex; flex-direction: column; gap: var(--space-xs); label { font-size: var(--text-sm); color: var(--text-secondary); font-weight: 500; } }
    .filter-select, input[type="datetime-local"] { padding: var(--space-sm) var(--space-md); border-radius: var(--radius-md); font-size: var(--text-sm); }
    .btn-primary { display: inline-flex; align-items: center; gap: var(--space-xs); background: var(--accent-primary); color: white; border: none; padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); font-weight: 600; cursor: pointer; align-self: flex-end; &:hover { background: var(--accent-primary-hover); } &:disabled { opacity: 0.5; } }
    .btn-ai { display: inline-flex; align-items: center; gap: var(--space-xs); background: var(--surface-700); border: 1px solid var(--accent-primary); color: var(--accent-primary); padding: var(--space-sm) var(--space-lg); border-radius: var(--radius-md); cursor: pointer; font-size: var(--text-sm); margin-top: var(--space-lg); &:hover { background: rgba(255,107,53,0.1); } &:disabled { opacity: 0.5; } }
    .report-section { margin-top: var(--space-lg); }
    .report-meta { font-size: var(--text-sm); color: var(--text-tertiary); margin-bottom: var(--space-lg); }
    .report-sub-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: var(--space-md);
    }
    .report-summary {
      grid-column: 1 / -1;
      background: linear-gradient(120deg, rgba(255, 107, 53, 0.16), rgba(255, 107, 53, 0.06));
      border: 1px solid rgba(255, 107, 53, 0.45);
      border-left: 4px solid var(--accent-primary);
      border-radius: var(--radius-lg);
      padding: var(--space-md) var(--space-lg);
      display: grid;
      gap: var(--space-sm);
    }
    .report-summary h4 { margin: 0; font-size: var(--text-lg); }
    .report-summary__eyebrow {
      margin: 0;
      color: var(--accent-primary);
      font-size: var(--text-xs);
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }
    .report-sub-card {
      border: 1px solid var(--border-default);
      border-radius: var(--radius-lg);
      background: var(--surface-800);
      padding: var(--space-md);
      display: grid;
      gap: var(--space-sm);
    }
    .report-sub-card h4 { margin: 0; }
    .report-sub-metrics {
      display: grid;
      gap: 2px;
    }
    .metric-row {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      gap: var(--space-md);
      padding: 6px 0;
      border-bottom: 1px solid var(--border-subtle);
    }
    .metric-row:last-child {
      border-bottom: none;
    }
    .metric-name { color: var(--text-secondary); font-size: var(--text-sm); margin: 0; }
    .metric-value { font-weight: 700; margin: 0; font-size: var(--text-base); text-align: right; }
    .review-comments {
      margin-top: var(--space-xs);
      border: 1px dashed var(--border-default);
      border-radius: var(--radius-md);
      background: rgba(255, 255, 255, 0.02);
      padding: var(--space-xs) var(--space-sm);
    }
    .review-comments summary {
      cursor: pointer;
      color: var(--text-secondary);
      font-size: var(--text-sm);
      font-weight: 600;
      user-select: none;
    }
    .review-comments__list {
      display: grid;
      gap: 6px;
      margin-top: var(--space-xs);
      padding-top: var(--space-xs);
      border-top: 1px solid var(--border-subtle);
    }
    .review-comments__item {
      margin: 0;
      color: var(--text-secondary);
      font-size: var(--text-sm);
      line-height: 1.45;
      padding-left: 0.9rem;
      position: relative;
    }
    .review-comments__item::before {
      content: '•';
      position: absolute;
      left: 0;
      color: var(--accent-primary);
    }
    .ai-panel { background: rgba(255,107,53,0.07); border: 1px solid var(--accent-primary); border-radius: var(--radius-lg); padding: var(--space-lg); margin-top: var(--space-xl); }
    .ai-panel h4 { color: var(--accent-primary); margin-bottom: var(--space-md); }
    .ai-html { color: var(--text-secondary); line-height: 1.6; }
    .ai-html :is(h1, h2, h3, h4, h5, h6) { color: var(--text-primary); margin: 0; }
    .ai-html p { margin: 0; }
    .ai-html .ai-report {
      background: linear-gradient(160deg, rgba(11, 16, 28, 0.96), rgba(24, 33, 51, 0.9));
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: var(--radius-lg);
      padding: var(--space-lg);
      box-shadow: 0 10px 32px rgba(0, 0, 0, 0.28);
      display: grid;
      gap: var(--space-lg);
    }
    .ai-html .ai-report__header { display: grid; gap: var(--space-sm); padding-bottom: var(--space-md); border-bottom: 1px solid rgba(255,255,255,0.1); }
    .ai-html .ai-report__title { font-size: clamp(1.1rem, 1.8vw, 1.35rem); font-weight: 700; letter-spacing: 0.01em; }
    .ai-html .ai-report__subtitle { color: var(--text-tertiary); font-size: var(--text-sm); }
    .ai-html .ai-report__summary {
      background: rgba(255, 107, 53, 0.12);
      border: 1px solid rgba(255, 107, 53, 0.45);
      border-radius: var(--radius-md);
      padding: var(--space-md);
      color: var(--text-primary);
      font-size: var(--text-sm);
    }
    .ai-html .ai-report__section { display: grid; gap: var(--space-sm); }
    .ai-html .ai-report__section-title { font-size: var(--text-base); font-weight: 650; color: var(--text-primary); }
    .ai-html .ai-report__list { margin: 0; padding-left: 1.1rem; display: grid; gap: 6px; }
    .ai-html .ai-report__list li::marker { color: var(--accent-primary); }
    .ai-html .ai-report__table-wrap {
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: var(--radius-md);
      overflow: auto;
      background: rgba(255, 255, 255, 0.02);
    }
    .ai-html .ai-report__table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); min-width: 380px; }
    .ai-html .ai-report__table th,
    .ai-html .ai-report__table td { text-align: left; padding: var(--space-sm) var(--space-md); border-bottom: 1px solid rgba(255,255,255,0.08); }
    .ai-html .ai-report__table th { color: var(--text-primary); font-weight: 600; background: rgba(255,255,255,0.03); }
    .ai-html .ai-report__table tr:last-child td { border-bottom: none; }
    .ai-html .ai-report__callout {
      border-radius: var(--radius-md);
      padding: var(--space-sm) var(--space-md);
      font-size: var(--text-sm);
      border: 1px solid transparent;
    }
    .ai-html .ai-report__callout--success { background: rgba(45, 212, 140, 0.13); border-color: rgba(45, 212, 140, 0.4); color: #d8fff0; }
    .ai-html .ai-report__callout--warning { background: rgba(250, 204, 21, 0.14); border-color: rgba(250, 204, 21, 0.45); color: #fff3cc; }
    .ai-html .ai-report__callout--danger { background: rgba(248, 113, 113, 0.14); border-color: rgba(248, 113, 113, 0.45); color: #ffd9d9; }
    .ai-html .ai-report__grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: var(--space-sm);
    }
    .ai-html .ai-report__card {
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: var(--radius-md);
      padding: var(--space-sm) var(--space-md);
      display: grid;
      gap: 4px;
    }
    .ai-html .ai-report__card-title { font-size: var(--text-sm); color: var(--text-tertiary); }
    .ai-html .ai-report__card-value { font-size: var(--text-lg); color: var(--text-primary); font-weight: 700; }
  `],
})
export class AdminAnalyticsComponent {
  private readonly analyticsService = inject(AnalyticsService);
  private readonly restaurantsService = inject(RestaurantsService);
  private readonly notifications = inject(NotificationService);

  readonly reportLoading = signal(false);
  readonly aiLoading = signal(false);
  readonly report = signal<ReportResponse | null>(null);
  readonly aiResult = signal<ReportAnalysisResponse | null>(null);
  readonly restaurants = signal<RestaurantBrief[]>([]);
  readonly aiAnalysisHtml = computed(() => {
    return this.aiResult()?.analysis ?? '';
  });

  form: GenerateReportRequest = { type: 'profitability', format: 'summary', restaurantId: '', fromUtc: null, toUtc: null };

  constructor() {
    this.restaurantsService.getAll().subscribe({ next: res => this.restaurants.set(res) });
  }

  generate(): void {
    const req = { ...this.form, restaurantId: this.form.restaurantId || null, fromUtc: this.form.fromUtc || null, toUtc: this.form.toUtc || null };
    this.reportLoading.set(true);
    this.analyticsService.generateReport(req).subscribe({
      next: res => {
        if (res instanceof HttpResponse) {
          this.downloadBinaryReport(res);
          this.report.set(null);
          this.aiResult.set(null);
        } else {
          this.report.set(res);
          this.aiResult.set(null);
        }
        this.reportLoading.set(false);
      },
      error: () => { this.notifications.error('Failed to generate report.'); this.reportLoading.set(false); },
    });
  }

  analyseWithAi(): void {
    const r = this.report();
    if (!r) return;
    this.aiLoading.set(true);
    this.analyticsService.analyzeReport({ reportContent: JSON.stringify(r), analysisType: this.form.type, restaurantId: this.form.restaurantId || null }).subscribe({
      next: res => { this.aiResult.set(res); this.aiLoading.set(false); },
      error: () => { this.notifications.error('AI analysis failed.'); this.aiLoading.set(false); },
    });
  }

  visibleMetrics(section: ReportSectionResponse): ReportMetricResponse[] {
    const metrics = section.metrics ?? [];
    if (!this.isWaiterReport()) return metrics;
    return metrics.filter(metric => !this.isReviewCommentMetric(metric));
  }

  reviewCommentMetrics(section: ReportSectionResponse): ReportMetricResponse[] {
    const metrics = section.metrics ?? [];
    if (!this.isWaiterReport()) return [];
    return metrics.filter(metric => this.isReviewCommentMetric(metric));
  }

  private isWaiterReport(): boolean {
    const reportType = (this.report()?.type ?? this.form.type ?? '').toLowerCase();
    return reportType === 'waiter';
  }

  private isReviewCommentMetric(metric: ReportMetricResponse): boolean {
    return (metric.name ?? '').trim().toLowerCase() === 'review comment';
  }

  private downloadBinaryReport(response: HttpResponse<Blob>): void {
    const blob = response.body;
    if (!blob || blob.size === 0) {
      this.notifications.error('Downloaded report is empty.');
      return;
    }

    const contentType = (response.headers.get('content-type') ?? blob.type ?? '').toLowerCase();
    if (contentType.includes('application/json')) {
      this.notifications.error('Report download failed. The server returned JSON instead of a file.');
      return;
    }

    const extension = this.form.format === 'pdf' ? 'pdf' : 'xlsx';
    const filenameFromHeader = this.extractFilename(response.headers.get('content-disposition'));
    const filename = filenameFromHeader || `${this.form.type}-report.${extension}`;

    const fileBlob = blob.type ? blob : new Blob([blob], { type: this.form.format === 'pdf' ? 'application/pdf' : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(fileBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.rel = 'noopener';
    document.body.appendChild(link);
    link.click();
    link.remove();

    // Delay revocation to avoid cancelling downloads in some browsers.
    setTimeout(() => URL.revokeObjectURL(url), 1500);
    this.notifications.success('Report downloaded.');
  }

  private extractFilename(contentDisposition: string | null): string | null {
    if (!contentDisposition) return null;

    const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);
    if (utf8Match?.[1]) {
      return decodeURIComponent(utf8Match[1]);
    }

    const quotedMatch = /filename="?([^";]+)"?/i.exec(contentDisposition);
    return quotedMatch?.[1] ?? null;
  }
}


