import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JiraSyncComponent } from './jira-sync.component';

describe('JiraSyncComponent', () => {
  let component: JiraSyncComponent;
  let fixture: ComponentFixture<JiraSyncComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [JiraSyncComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JiraSyncComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
