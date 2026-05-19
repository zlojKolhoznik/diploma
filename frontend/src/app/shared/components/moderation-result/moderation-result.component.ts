import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewModerationResult } from '../../../core/models/review.models';

@Component({
  selector: 'app-moderation-result',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (result) {
      <div class="moderation" [class]="result.approved ? 'moderation--approved' : 'moderation--rejected'">
        <span class="moderation-badge">{{ result.approved ? '✓ Approved' : '✗ Needs Revision' }}</span>
        @if (result.reason) {
          <p class="moderation-reason">{{ result.reason }}</p>
        }
        @if (result.suggestedRephrasing) {
          <p class="moderation-suggestion"><strong>Suggested:</strong> {{ result.suggestedRephrasing }}</p>
        }
      </div>
    }
  `,
  styles: [`
    .moderation {
      padding: var(--space-md);
      border-radius: var(--radius-md);
      border: 1px solid;
      margin-top: var(--space-md);
    }
    .moderation--approved {
      border-color: var(--state-success);
      background: rgba(40,167,69,0.08);
    }
    .moderation--rejected {
      border-color: var(--state-warning);
      background: rgba(255,193,7,0.08);
    }
    .moderation-badge {
      font-weight: 700;
      font-size: var(--text-sm);
    }
    .moderation--approved .moderation-badge { color: var(--state-success); }
    .moderation--rejected .moderation-badge { color: var(--state-warning); }
    .moderation-reason, .moderation-suggestion {
      font-size: var(--text-sm);
      color: var(--text-secondary);
      margin-top: var(--space-sm);
    }
  `],
})
export class ModerationResultComponent {
  @Input() result: ReviewModerationResult | null = null;
}

