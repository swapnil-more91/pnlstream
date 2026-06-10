import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

interface DialogData {
  reportType: 'valid' | 'invalid';
}

interface DialogResult {
  startDate: Date;
  endDate: Date;
}

@Component({
  selector: 'app-report-download',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <div class="dialog-container">
      <h2 mat-dialog-title>Download {{ reportType | titlecase }} Report</h2>

      <mat-dialog-content>
        <div class="date-range-picker">
          <mat-form-field appearance="outline" class="date-field">
            <mat-label>Start Date</mat-label>
            <input
              matInput
              [matDatepicker]="startPicker"
              [(ngModel)]="startDate"
              required
            />
            <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline" class="date-field">
            <mat-label>End Date</mat-label>
            <input
              matInput
              [matDatepicker]="endPicker"
              [(ngModel)]="endDate"
              required
            />
            <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
            <mat-datepicker #endPicker></mat-datepicker>
          </mat-form-field>
        </div>

        <div class="validation-message" *ngIf="showValidationError">
          <p class="error">End date must be after start date</p>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button mat-dialog-close>Cancel</button>
        <button
          mat-raised-button
          color="primary"
          (click)="onDownload()"
          [disabled]="!isDateRangeValid()"
        >
          Download
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      padding: 16px;
    }

    .date-range-picker {
      display: flex;
      flex-direction: column;
      gap: 16px;
      margin: 24px 0;
    }

    .date-field {
      width: 100%;
    }

    .validation-message {
      margin-top: 12px;
    }

    .error {
      color: #d32f2f;
      font-size: 12px;
      margin: 0;
    }

    mat-dialog-actions {
      margin-top: 24px;
    }

    @media (max-width: 600px) {
      .dialog-container {
        padding: 12px;
      }

      .date-range-picker {
        gap: 12px;
      }
    }
  `],
})
export class ReportDownloadComponent {
  reportType: string = 'valid';
  startDate: Date | null = null;
  endDate: Date | null = null;
  showValidationError = false;

  constructor(
    public dialogRef: MatDialogRef<ReportDownloadComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {
    this.reportType = data.reportType;
    // Set default date range (last 7 days)
    this.endDate = new Date();
    this.startDate = new Date();
    this.startDate.setDate(this.startDate.getDate() - 7);
  }

  isDateRangeValid(): boolean {
    if (!this.startDate || !this.endDate) {
      return false;
    }
    return this.startDate < this.endDate;
  }

  onDownload(): void {
    this.showValidationError = false;

    if (!this.isDateRangeValid()) {
      this.showValidationError = true;
      return;
    }

    if (this.startDate && this.endDate) {
      const result: DialogResult = {
        startDate: this.startDate,
        endDate: this.endDate,
      };
      this.dialogRef.close(result);
    }
  }
}
