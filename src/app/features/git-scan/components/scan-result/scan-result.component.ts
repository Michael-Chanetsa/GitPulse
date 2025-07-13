import { Component, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, takeUntil } from 'rxjs';

interface SonarMetricsDTO {
  projectKey: string;
  bugs: number;
  codeSmells: number;
  vulnerabilities: number;
  coverage: number | null;
  duplicatedLinesDensity: number | null;
  summary: string;
  qualityGate?: string;
  topIssues?: Array<{
    message: string;
    severity: string;
    rule: string;
  }>;
}

interface AiAdviceResponse {
  prompt: string;
  advice: string;
}

@Component({
  selector: 'app-scan-result',
  templateUrl: './scan-result.component.html',
  styleUrls: ['./scan-result.component.scss'],
})
export class ScanResultComponent implements OnInit, OnDestroy {
  repoUrl = '';
  scanName = '';
  isPrivateRepo = false;

  metrics: SonarMetricsDTO | null = null;
  error: string | null = null;
  loading = false;
  analysisStage: 'idle' | 'cloning' | 'fetching' | 'analyzing' | 'done' | 'error' = 'idle';

  selectedIssue: { message: string; severity: string; rule: string } | null = null;
  aiAdvice: string | null = null;
  gettingAdvice = false;

  private destroy$ = new Subject<void>();
  private apiBase = 'https://localhost:7250';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token');

    if (token) {
      localStorage.setItem('github_token', token);
      alert('✅ GitHub login successful!');
      window.history.replaceState({}, document.title, window.location.pathname);
    }

    if (!localStorage.getItem('github_token')) {
      console.warn('⚠️ GitHub token not found in localStorage.');
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loginWithGitHub(): void {
    window.location.href = `${this.apiBase}/api/github/login`;
  }

  logoutFromGitHub(): void {
    localStorage.removeItem('github_token');
    alert('🚪 Logged out of GitHub!');
    window.location.href = '/scan';
  }

  analyzeRepo(event: Event): void {
    event.preventDefault();
    this.error = null;
    this.loading = true;
    this.analysisStage = 'cloning';

    const cleanedUrl = this.repoUrl.trim().replace(/\/+$/, '');
    const token = this.isPrivateRepo ? localStorage.getItem('github_token') || '' : '';

    this.http
      .post<any>(`${this.apiBase}/api/github/analyze`, {
        repoUrl: cleanedUrl,
        name: this.scanName,
        token: token,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.analysisStage = 'fetching';
          this.getMetrics(res.sonarKey);
        },
        error: (err) => this.handleError('Scan failed: ', err),
      });
  }

  private getMetrics(projectKey: string): void {
    setTimeout(() => {
      this.analysisStage = 'analyzing';

      this.http
        .get<SonarMetricsDTO>(`${this.apiBase}/api/sonar/summary/${projectKey}`)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (data) => {
            this.metrics = {
              projectKey: data.projectKey,
              summary: data.summary,
              qualityGate: data.qualityGate || 'N/A',
              bugs: Number(data.bugs) || 0,
              vulnerabilities: Number(data.vulnerabilities) || 0,
              codeSmells: Number(data.codeSmells) || 0,
              coverage: data.coverage != null ? Number(data.coverage) : null,
              duplicatedLinesDensity:
                data.duplicatedLinesDensity != null ? Number(data.duplicatedLinesDensity) : null,
              topIssues: Array.isArray(data.topIssues) ? data.topIssues : [],
            };
            this.loading = false;
            this.analysisStage = 'done';
          },
          error: (err) => this.handleError('❌ Failed to fetch metrics: ', err),
        });
    }, 10000);
  }

  /** Only CRITICAL or MAJOR severities */
  get importantIssues() {
    return (this.metrics?.topIssues || []).filter(
      (issue) => issue.severity === 'CRITICAL' || issue.severity === 'MAJOR'
    );
  }

  createJiraTask(issue: { message: string; severity: string; rule: string }): void {
    this.http.post<any>(`${this.apiBase}/api/jira/create-task-from-issue`, issue).subscribe({
      next: () => alert('✅ Jira task created!'),
      error: (err) => {
        console.error('Failed to create Jira task', err);
        alert('❌ Failed to create Jira task.');
      },
    });
  }

  getAdviceForIssue(issue: { message: string; severity: string; rule: string }): void {
    this.gettingAdvice = true;
    this.selectedIssue = issue;
    this.aiAdvice = null;

    this.http.post<AiAdviceResponse>(`${this.apiBase}/api/github/ai-advice`, [issue]).subscribe({
      next: (res) => {
        this.aiAdvice = res.advice;
        this.gettingAdvice = false;
      },
      error: (err) => {
        console.error('❌ Failed to fetch AI advice', err);
        this.aiAdvice = 'AI failed to respond.';
        this.gettingAdvice = false;
      }
    });
  }

  getQualityGateClass(): string {
    if (!this.metrics?.qualityGate) return '';
    return this.metrics.qualityGate === 'OK' ? 'passed' : 'failed';
  }

  private handleError(context: string, err: any): void {
    console.error(err);
    this.error = context + (err.error?.message || err.message || 'Unknown error');
    this.loading = false;
    this.analysisStage = 'error';
  }
}
