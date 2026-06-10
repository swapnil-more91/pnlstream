import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { PnlNotificationDto } from '../models/pnl-notification.model';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private validRecordsSubject = new BehaviorSubject<PnlNotificationDto[]>([]);
  private invalidRecordsSubject = new BehaviorSubject<PnlNotificationDto[]>([]);
  private connectionStatusSubject = new BehaviorSubject<boolean>(false);

  public validRecords$ = this.validRecordsSubject.asObservable();
  public invalidRecords$ = this.invalidRecordsSubject.asObservable();
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  constructor() {}

  /**
   * Establishes connection to the SignalR hub
   */
  public connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
        resolve();
        return;
      }

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7162/pnlHub', {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
          // For development, you might need to disable SSL verification
          accessTokenFactory: () => Promise.resolve(''),
        })
        .withAutomaticReconnect([0, 0, 0, 5000, 5000, 10000])
        .withServerTimeout(30000)
        .build();

      // Subscribe to connection state changes
      this.hubConnection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
        this.connectionStatusSubject.next(false);
      });

      this.hubConnection.onreconnected(() => {
        console.log('SignalR reconnected!');
        this.connectionStatusSubject.next(true);
      });

      this.hubConnection.onclose(() => {
        console.log('SignalR disconnected');
        this.connectionStatusSubject.next(false);
      });

      // Start the connection
      this.hubConnection
        .start()
        .then(() => {
          console.log('SignalR connected successfully');
          this.connectionStatusSubject.next(true);
          this.setupEventListeners();
          resolve();
        })
        .catch((err) => {
          console.error('SignalR connection error:', err);
          this.connectionStatusSubject.next(false);
          reject(err);
        });
    });
  }

  /**
   * Disconnects from the SignalR hub
   */
  public disconnect(): Promise<void> {
    return new Promise((resolve) => {
      if (this.hubConnection) {
        this.hubConnection
          .stop()
          .then(() => {
            console.log('SignalR disconnected');
            this.connectionStatusSubject.next(false);
            resolve();
          })
          .catch((err) => {
            console.error('Error disconnecting SignalR:', err);
            resolve();
          });
      } else {
        resolve();
      }
    });
  }

  /**
   * Sets up event listeners for SignalR hub events
   */
  private setupEventListeners(): void {
    if (!this.hubConnection) return;

    // Listen for ValidRecordReceived events
    this.hubConnection.on('ValidRecordReceived', (notification: PnlNotificationDto) => {
      console.log('Received valid record:', notification);
      const currentRecords = this.validRecordsSubject.getValue();
      this.validRecordsSubject.next([notification, ...currentRecords]);
    });

    // Listen for InvalidRecordReceived events
    this.hubConnection.on('InvalidRecordReceived', (notification: PnlNotificationDto) => {
      console.log('Received invalid record:', notification);
      const currentRecords = this.invalidRecordsSubject.getValue();
      this.invalidRecordsSubject.next([notification, ...currentRecords]);
    });
  }

  /**
   * Load initial valid records (from database/API)
   */
  public loadInitialValidRecords(records: PnlNotificationDto[]): void {
    console.log('Loading initial valid records:', records.length, records);
    this.validRecordsSubject.next(records);
  }

  /**
   * Load initial invalid records (from database/API)
   */
  public loadInitialInvalidRecords(records: PnlNotificationDto[]): void {
    console.log('Loading initial invalid records:', records.length, records);
    this.invalidRecordsSubject.next(records);
  }

  /**
   * Add a single valid record to the stream
   */
  public addValidRecord(notification: PnlNotificationDto): void {
    const currentRecords = this.validRecordsSubject.getValue();
    this.validRecordsSubject.next([notification, ...currentRecords]);
  }

  /**
   * Add a single invalid record to the stream
   */
  public addInvalidRecord(notification: PnlNotificationDto): void {
    const currentRecords = this.invalidRecordsSubject.getValue();
    this.invalidRecordsSubject.next([notification, ...currentRecords]);
  }

  /**
   * Get the current connection instance (for testing or advanced scenarios)
   */
  public getConnection(): signalR.HubConnection | null {
    return this.hubConnection;
  }

  /**
   * Check if currently connected
   */
  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Send a mock valid record (for testing purposes)
   */
  public sendMockValidRecord(notification?: Partial<PnlNotificationDto>): void {
    const now = new Date().toISOString();
    const mockRecord: PnlNotificationDto = {
      sourceSystem: notification?.sourceSystem || 'TEST_SYSTEM',
      portfolioNumber: notification?.portfolioNumber || Math.floor(Math.random() * 100000),
      pnlAmount: notification?.pnlAmount || Math.floor(Math.random() * 50000),
      isValid: true,
      validationReasons: notification?.validationReasons || '',
      dataSource: notification?.dataSource || 'MOCK',
      createdAt: notification?.createdAt || now,
      lastUpdatedAt: notification?.lastUpdatedAt || now,
    };
    console.log('[MOCK] Sending valid record:', mockRecord);
    this.addValidRecord(mockRecord);
  }

  /**
   * Send a mock invalid record (for testing purposes)
   */
  public sendMockInvalidRecord(notification?: Partial<PnlNotificationDto>): void {
    const now = new Date().toISOString();
    const mockRecord: PnlNotificationDto = {
      sourceSystem: notification?.sourceSystem || 'TEST_SYSTEM',
      portfolioNumber: notification?.portfolioNumber || Math.floor(Math.random() * 100000),
      pnlAmount: notification?.pnlAmount || Math.floor(Math.random() * 50000),
      isValid: false,
      validationReasons: notification?.validationReasons || 'Failed validation',
      dataSource: notification?.dataSource || 'MOCK',
      createdAt: notification?.createdAt || now,
      lastUpdatedAt: notification?.lastUpdatedAt || now,
    };
    console.log('[MOCK] Sending invalid record:', mockRecord);
    this.addInvalidRecord(mockRecord);
  }
}
