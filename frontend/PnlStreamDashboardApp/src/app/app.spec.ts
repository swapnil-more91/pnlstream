import { TestBed } from '@angular/core/testing';
import { BehaviorSubject, of } from 'rxjs';
import { App } from './app';
import { PnlNotificationDto } from './core/models/pnl-notification.model';
import { ReportService } from './core/services/report.service';
import { SignalRService } from './core/services/signalr.service';

describe('App', () => {
  const validRecordsSubject = new BehaviorSubject<PnlNotificationDto[]>([]);
  const invalidRecordsSubject = new BehaviorSubject<PnlNotificationDto[]>([]);
  const connectionStatusSubject = new BehaviorSubject<boolean>(true);

  const mockReportService = {
    getInitialValidRecords: () => of([]),
    getInitialInvalidRecords: () => of([]),
    getValidRecordsPaginated: () =>
      of({
        data: [],
        totalRecords: 0,
        totalPages: 0,
        currentPage: 0,
        pageSize: 10,
        hasNext: false,
        hasPrevious: false,
      }),
    getInvalidRecordsPaginated: () =>
      of({
        data: [],
        totalRecords: 0,
        totalPages: 0,
        currentPage: 0,
        pageSize: 10,
        hasNext: false,
        hasPrevious: false,
      }),
  };

  const mockSignalRService = {
    validRecords$: validRecordsSubject.asObservable(),
    invalidRecords$: invalidRecordsSubject.asObservable(),
    connectionStatus$: connectionStatusSubject.asObservable(),
    connect: () => Promise.resolve(),
    disconnect: () => Promise.resolve(),
    loadInitialValidRecords: (records: PnlNotificationDto[]) => validRecordsSubject.next(records),
    loadInitialInvalidRecords: (records: PnlNotificationDto[]) => invalidRecordsSubject.next(records),
    sendMockValidRecord: () => undefined,
    sendMockInvalidRecord: () => undefined,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: ReportService, useValue: mockReportService },
        { provide: SignalRService, useValue: mockSignalRService },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.app-title')?.textContent).toContain('PnL Stream Dashboard');
  });
});
