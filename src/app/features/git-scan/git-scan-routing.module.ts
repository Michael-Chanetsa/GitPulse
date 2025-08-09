import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ScanResultComponent } from './components/scan-result/scan-result.component';
import { NavbarComponent } from './components/shared/navbar/navbar.component';
import { ScanHistoryComponent } from './components/scan-history/scan-history.component';
import { JiraSyncComponent } from './components/jira-sync/jira-sync.component';

const routes: Routes = [
  { path: '', component: ScanResultComponent },
  { path: 'history', component: ScanHistoryComponent },
  { path: 'jira-sync', component: JiraSyncComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GitScanRoutingModule {}
