# PnLStream

## Overview

PnLStream is a real-time Profit & Loss (PnL) capture, validation, persistence, and reporting platform built using .NET, Kafka, SQL Server, SignalR, and Angular.

The system supports:

* Start-of-Day (SOD) file ingestion
* Real-time message ingestion
* Business rule validation
* Persistence of valid and excluded records
* Real-time dashboard updates
* Reporting of valid and excluded PnL records

## Solution Structure

```text
PnLStream

├── backend
│
│   ├── PnLStream.Common
│   ├── PnLStream.ValidationEngine
│   ├── PnLStream.Persistence
│   ├── PnLStream.FileWatcher
│   ├── PnLStream.FeedProcessor
│   ├── PnLStream.Reporting.Api
│   └── PnLStream.Dashboard.Api
│
├── frontend
│   └── pnlstream-ui
│
├── samples
│   └── PnLFile_500Records.csv
│
└── README.md
```

## Running the Solution

### Prerequisites

* .NET 10 SDK
* SQL Server
* Kafka
* Angular CLI

### Step 1 – Start SQL Server

Start SQL Server, create Database with name PnLStream.

Run query from pnlRecords.sql to create required table.

Update connectionstring in appsettings.json.

### Step 2 – Create Kafka Topics

```text
pnl.file.feed
pnl.realtime.feed
```

### Step 3 – Start FileIngestor

```bash
dotnet run --project PnLStream.FileIngester
```

### Step 4 – Start Feed Processor (File Feed)

```bash
dotnet run --ProcessorType=File
```

### Step 4 – Start Feed Processor (Realtime Feed)

```bash
dotnet run --ProcessorType=Realtime
```

### Step 5 – Start Reporting API

```bash
dotnet run --project PnLStream.Reporting.Api
```

### Step 6 – Start Angular Application(PnlStreamDashboardApp)

```bash
ng serve --port 4200
```

## Sample SOD FIle

A sample file containing 500 records is available under:

```text
TestData/pnl_sod_file.csv
```
