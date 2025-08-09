import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitScanRoutingModule } from './git-scan-routing.module';
import { ScanResultComponent } from './components/scan-result/scan-result.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ScanHistoryComponent } from './components/scan-history/scan-history.component';
import { NavbarComponent } from './components/shared/navbar/navbar.component';
import { JiraSyncComponent } from './components/jira-sync/jira-sync.component';

@NgModule({
  declarations: [
    ScanResultComponent,
    ScanHistoryComponent,
    NavbarComponent,
    JiraSyncComponent
  ],
  imports: [
    CommonModule,
    GitScanRoutingModule,
    FormsModule,
    HttpClientModule
  ]
})
export class GitScanModule {}
