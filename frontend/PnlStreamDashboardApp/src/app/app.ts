import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SignalRService } from './core/services/signalr.service';
import { ReportService } from './core/services/report.service';
import { ValidPnlDataComponent } from './features/valid-pnl-data/valid-pnl-data.component';
import { ExcludedDataComponent } from './features/excluded-data/excluded-data.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    ValidPnlDataComponent,
    ExcludedDataComponent,
  ],
  template: `
    <mat-toolbar color="primary" class="app-toolbar">
      <span class="app-title">
        <mat-icon class="title-icon">dashboard</mat-icon>
        PnL Stream Dashboard
      </span>
      <span class="spacer"></span>
      <button 
        mat-icon-button 
        [matMenuTriggerFor]="testMenu"
        class="test-button"
        matTooltip="Test Data (Dev Only)"
      >
        <mat-icon>bug_report</mat-icon>
      </button>
      <mat-menu #testMenu="matMenu">
        <button mat-menu-item (click)="addMockValidRecords(3)">
          <mat-icon>add</mat-icon>
          <span>Add 3 Valid Records</span>
        </button>
        <button mat-menu-item (click)="addMockInvalidRecords(3)">
          <mat-icon>add</mat-icon>
          <span>Add 3 Invalid Records</span>
        </button>
        <button mat-menu-item (click)="clearAllRecords()">
          <mat-icon>delete</mat-icon>
          <span>Clear All Records</span>
        </button>
      </mat-menu>
      <span class="connection-status" [class.connected]="isConnected">
        <span class="status-indicator"></span>
        {{ isConnected ? 'Connected' : 'Disconnected' }}
      </span>
    </mat-toolbar>

    <mat-progress-bar 
      *ngIf="isLoading" 
      mode="indeterminate" 
      class="loading-bar"
    ></mat-progress-bar>

    <mat-tab-group class="tab-group" animationDuration="300">
      <mat-tab label="Valid PnL Data">
        <ng-template mat-tab-label>
          <mat-icon class="tab-icon">check_circle</mat-icon>
          Valid PnL Data
        </ng-template>
        <app-valid-pnl-data></app-valid-pnl-data>
      </mat-tab>

      <mat-tab label="Excluded Data">
        <ng-template mat-tab-label>
          <mat-icon class="tab-icon">cancel</mat-icon>
          Excluded Data
        </ng-template>
        <app-excluded-data></app-excluded-data>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .app-toolbar {
      display: flex;
      align-items: center;
      padding: 0 16px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      z-index: 10;
    }

    .loading-bar {
      height: 4px;
      position: sticky;
      top: 0;
      z-index: 9;
    }

    .app-title {
      display: flex;
      align-items: center;
      font-size: 20px;
      font-weight: 500;
    }

    .title-icon {
      margin-right: 12px;
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .spacer {
      flex: 1 1 auto;
    }

    .test-button {
      margin-right: 8px;
      opacity: 0.6;
      
      &:hover {
        opacity: 1;
      }
    }

    .connection-status {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 4px 12px;
      border-radius: 20px;
      background-color: rgba(255, 255, 255, 0.2);
      font-size: 12px;
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .connection-status.connected {
      background-color: rgba(76, 175, 80, 0.3);
      color: #fff;
    }

    .status-indicator {
      display: inline-block;
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background-color: #f44336;
      animation: pulse 2s infinite;
    }

    .connection-status.connected .status-indicator {
      background-color: #4caf50;
      animation: none;
    }

    @keyframes pulse {
      0%, 100% {
        opacity: 1;
      }
      50% {
        opacity: 0.5;
      }
    }

    .tab-group {
      flex: 1;
      overflow-y: auto;
    }

    .tab-icon {
      margin-right: 8px;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    @media (max-width: 600px) {
      .app-toolbar {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
        padding: 8px 12px;
      }

      .app-title {
        font-size: 16px;
      }

      .title-icon {
        width: 20px;
        height: 20px;
        font-size: 20px;
      }

      .connection-status {
        align-self: flex-end;
        font-size: 11px;
      }
    }
  `],
})
export class App implements OnInit, OnDestroy {
  isConnected = false;
  isLoading = true;
  private destroy$ = new Subject<void>();

  constructor(
    private signalRService: SignalRService,
    private reportService: ReportService
  ) {}

  ngOnInit(): void {
    // Load initial data from API/database
    this.loadInitialData();

    // Initialize SignalR connection
    this.signalRService
      .connect()
      .then(() => {
        console.log('SignalR connected');
      })
      .catch((error) => {
        console.error('Failed to connect to SignalR:', error);
      });

    // Monitor connection status
    this.signalRService.connectionStatus$
      .pipe(takeUntil(this.destroy$))
      .subscribe((isConnected) => {
        this.isConnected = isConnected;
      });
  }

  /**
   * Load initial data from the API
   */
  private loadInitialData(): void {
    console.log('Loading initial data from API...');
    this.isLoading = true;

    forkJoin([
      this.reportService.getInitialValidRecords(),
      this.reportService.getInitialInvalidRecords(),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        ([validRecords, invalidRecords]) => {
          console.log('Initial data loaded:', {
            validRecords: validRecords.length,
            invalidRecords: invalidRecords.length,
          });

          // Load data into the SignalR service
          this.signalRService.loadInitialValidRecords(validRecords);
          this.signalRService.loadInitialInvalidRecords(invalidRecords);

          this.isLoading = false;
        },
        (error) => {
          console.error('Error loading initial data:', error);
          this.isLoading = false;
        }
      );
  }

  /**
   * Add mock valid records for testing
   */
  addMockValidRecords(count: number): void {
    for (let i = 0; i < count; i++) {
      this.signalRService.sendMockValidRecord({
        sourceSystem: `SYSTEM_${String.fromCharCode(65 + i)}`,
        portfolioNumber: 10000 + Math.floor(Math.random() * 90000),
        pnlAmount: Math.floor(Math.random() * 100000),
        dataSource: 'TEST',
      });
    }
    console.log(`Added ${count} mock valid records`);
  }

  /**
   * Add mock invalid records for testing
   */
  addMockInvalidRecords(count: number): void {
    for (let i = 0; i < count; i++) {
      this.signalRService.sendMockInvalidRecord({
        sourceSystem: `SYSTEM_${String.fromCharCode(65 + i)}`,
        portfolioNumber: 10000 + Math.floor(Math.random() * 90000),
        pnlAmount: Math.floor(Math.random() * 100000),
        validationReasons: 'Failed validation check',
        dataSource: 'TEST',
      });
    }
    console.log(`Added ${count} mock invalid records`);
  }

  /**
   * Clear all records (for testing)
   */
  clearAllRecords(): void {
    // Reset the subjects to empty arrays
    this.signalRService['validRecordsSubject'].next([]);
    this.signalRService['invalidRecordsSubject'].next([]);
    console.log('Cleared all records');
  }

  ngOnDestroy(): void {
    this.signalRService
      .disconnect()
      .then(() => {
        console.log('SignalR disconnected');
      })
      .catch((error) => {
        console.error('Error disconnecting SignalR:', error);
      });

    this.destroy$.next();
    this.destroy$.complete();
  }
}
