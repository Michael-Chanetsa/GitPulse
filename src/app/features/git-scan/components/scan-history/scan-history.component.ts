import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-scan-history',
  templateUrl: './scan-history.component.html',
  styleUrls: ['./scan-history.component.scss']
})
export class ScanHistoryComponent implements OnInit {
  scans: any[] = [];
  loading = false;
  error = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loading = true;
    this.http.get<any[]>('https://localhost:7250/api/sonar/scans').subscribe({
      next: (data) => {
        this.scans = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load scans.';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
