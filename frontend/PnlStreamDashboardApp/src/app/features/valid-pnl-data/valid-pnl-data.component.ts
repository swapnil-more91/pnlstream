import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SignalRService } from '../../core/services/signalr.service';
import { ReportService } from '../../core/services/report.service';
import { PnlNotificationDto } from '../../core/models/pnl-notification.model';
import { PaginationRequest } from '../../core/models/pagination.model';
import { ReportDownloadComponent } from '../report-download/report-download.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-valid-pnl-data',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
  ],
  template: `
    <div class="valid-records-container">
      <mat-card class="data-card">
        <mat-card-header>
          <mat-card-title>Valid PnL Data</mat-card-title>
          <button mat-raised-button color="primary" (click)="openDownloadDialog()">
            <mat-icon>download</mat-icon>
            Download Report
          </button>
        </mat-card-header>
        <mat-card-content>
          <div class="table-container">
            <table mat-table [dataSource]="dataSource" class="valid-records-table">
              <!-- SourceSystem Column -->
              <ng-container matColumnDef="sourceSystem">
                <th mat-header-cell *matHeaderCellDef>Source System</th>
                <td mat-cell *matCellDef="let element">{{ element.sourceSystem }}</td>
              </ng-container>

              <!-- PortfolioNumber Column -->
              <ng-container matColumnDef="portfolioNumber">
                <th mat-header-cell *matHeaderCellDef>Portfolio Number</th>
                <td mat-cell *matCellDef="let element">{{ element.portfolioNumber }}</td>
              </ng-container>

              <!-- PnlAmount Column -->
              <ng-container matColumnDef="pnlAmount">
                <th mat-header-cell *matHeaderCellDef>PnL Amount</th>
                <td mat-cell *matCellDef="let element">{{ element.pnlAmount | currency: 'INR' }}</td>
              </ng-container>

              <!-- CreatedAt Column -->
              <ng-container matColumnDef="createdAt">
                <th mat-header-cell *matHeaderCellDef>Created Date Time</th>
                <td mat-cell *matCellDef="let element">{{ element.createdAt | date: 'medium' }}</td>
              </ng-container>

              <!-- LastUpdatedAt Column -->
              <ng-container matColumnDef="lastUpdatedAt">
                <th mat-header-cell *matHeaderCellDef>Last Updated Date Time</th>
                <td mat-cell *matCellDef="let element">{{ element.lastUpdatedAt | date: 'medium' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;" class="data-row"></tr>
            </table>
          </div>
          <mat-paginator
            #paginator
            [length]="totalRecords"
            [pageSize]="pageSize"
            [pageSizeOptions]="[5, 10, 25, 50, 100]"
            (page)="onPageChange($event)"
            showFirstLastButtons
            aria-label="Select page">
          </mat-paginator>
          <div *ngIf="dataSource.data.length === 0" class="no-data">
            <p>No valid records received yet</p>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .valid-records-container {
      padding: 16px;
    }

    .data-card {
      margin-bottom: 24px;
    }

    mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    mat-card-header button {
      margin-left: 16px;
    }

    .table-container {
      overflow-x: auto;
    }

    table {
      width: 100%;
      border-collapse: collapse;
    }

    th {
      background-color: #c8e6c9;
      color: #2e7d32;
      font-weight: 600;
      padding: 12px;
      text-align: left;
      border-bottom: 2px solid #81c784;
    }

    td {
      padding: 12px;
      border-bottom: 1px solid #e0e0e0;
    }

    tr.data-row:nth-child(even) {
      background-color: #f1f8f6;
    }

    tr.data-row:hover {
      background-color: #e8f5e9;
    }

    .no-data {
      text-align: center;
      padding: 32px;
      color: #666;
    }

    ::ng-deep .mat-mdc-paginator {
      background-color: #f5f5f5;
      border-top: 1px solid #e0e0e0;
    }

    @media (max-width: 768px) {
      .valid-records-container {
        padding: 8px;
      }

      mat-card-header {
        flex-direction: column;
        align-items: flex-start;
      }

      mat-card-header button {
        margin-left: 0;
        margin-top: 12px;
        width: 100%;
      }

      td, th {
        padding: 8px;
        font-size: 12px;
      }
    }
  `],
})
export class ValidPnlDataComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns: string[] = ['sourceSystem', 'portfolioNumber', 'pnlAmount', 'createdAt', 'lastUpdatedAt'];
  dataSource = new MatTableDataSource<PnlNotificationDto>();

  // Pagination properties
  totalRecords = 0;
  pageSize = 10;
  currentPageIndex = 0;
  isLoadingPage = false;

  private destroy$ = new Subject<void>();

  constructor(
    private signalRService: SignalRService,
    private reportService: ReportService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Load first page of data
    this.loadPage(0, this.pageSize);

    // Subscribe to real-time updates
    this.signalRService.validRecords$
      .pipe(takeUntil(this.destroy$))
      .subscribe((records) => {
        console.log('ValidPnlDataComponent received records:', records.length);
        // Update current page data, but keep pagination state
        if (this.currentPageIndex === 0) {
          // Only update if on first page
          this.dataSource.data = records;
          this.cdr.detectChanges();
        }
      });
  }

  /**
   * Load a specific page of records
   * @param pageIndex 0-based page index
   * @param pageSize Records per page
   */
  private loadPage(pageIndex: number, pageSize: number): void {
    this.isLoadingPage = true;

    const paginationRequest: PaginationRequest = {
      page: pageIndex,
      pageSize: pageSize,
      sortBy: 'sourceSystem',
      sortOrder: 'ASC',
    };

    this.reportService
      .getValidRecordsPaginated(paginationRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        (response) => {
          this.dataSource.data = response.data;
          this.totalRecords = response.totalRecords;
          this.currentPageIndex = response.currentPage;
          this.pageSize = response.pageSize;
          this.isLoadingPage = false;
          this.cdr.detectChanges();
        },
        (error) => {
          console.error('Error loading page:', error);
          this.isLoadingPage = false;
        }
      );
  }

  /**
   * Handle page change event from mat-paginator
   * @param event PageEvent from mat-paginator
   */
  onPageChange(event: PageEvent): void {
    this.loadPage(event.pageIndex, event.pageSize);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openDownloadDialog(): void {
    const dialogRef = this.dialog.open(ReportDownloadComponent, {
      width: '400px',
      data: { reportType: 'valid' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && result.startDate && result.endDate) {
        this.downloadReport(result.startDate, result.endDate);
      }
    });
  }

  private downloadReport(startDate: Date, endDate: Date): void {
    this.reportService.downloadValidReport(startDate, endDate).subscribe(
      (blob) => {
        const filename = `valid-pnl-report-${new Date().getTime()}.csv`;
        this.reportService.downloadBlob(blob, filename);
      },
      (error) => {
        console.error('Error downloading report:', error);
      }
    );
  }
}
