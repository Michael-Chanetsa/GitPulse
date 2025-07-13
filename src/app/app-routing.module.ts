import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'scan', pathMatch: 'full' },
  { path: 'scan', loadChildren: () => import('./features/git-scan/git-scan.module').then(m => m.GitScanModule) }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
