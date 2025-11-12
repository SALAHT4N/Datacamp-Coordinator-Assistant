using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Repositories;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

Console.WriteLine("DataCamp AI Coordinator");

string? cookieValue = null ??
                      "dc_consent={\"version\":\"2.0.0\",\"essential\":1,\"functional\":1,\"performance\":1,\"advertisement\":1}; _gcl_au=1.1.441699415.1758745331; _ga=GA1.1.1258694992.1758745331; IR_gbd=datacamp.com; _mkto_trk=id:307-OAT-968&token:_mch-datacamp.com-11309d0e8fe8acb51613d9ca0520eaf; FPID=FPID2.2.ZG4Ja%2FkXcQJM2IN%2F%2BdH%2F3zX88AVVx2WiGccRxSaMmwY%3D.1758745331; FPAU=1.1.441699415.1758745331; _fbp=fb.1.1758745331539.1633593819; _ttp=kEDQ0tJexuVClkJ41P2SHjupLEY; smc_ls_session=1758745331379; _dct=eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJneUZvOVJGU21lTUIzcGZRNk1oMkhFa182dksySDYxR05tbF9aSENZZzdnIn0.eyJleHAiOjE3NjY1MjEzNDMsImlhdCI6MTc1ODc0NTM0MywianRpIjoiMGI0MTY3NzQtOTRkNi00ZGQxLWExYTgtNWY0NWQ5YTZkMWUxIiwiaXNzIjoiaHR0cDovL2tleWNsb2FrLWh0dHAuaWRlbnRpdHktbWFuYWdlbWVudC9yZWFsbXMvZGF0YWNhbXAtdXNlcnMiLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiOTczYmZjMjItNGE4NC00ZGNkLTg4YTMtOGMxZTljOTZiMjAxIiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiQjgxMTAyRkUtQzdEQS00MjIyLUE2MEMtRUExRkVCRkQ3ODc4Iiwic2lkIjoiMTc5NDI0NzMtYTE1OC00NWJjLTljNGUtZDcwZWNjNDQ2YjAyIiwiYWNyIjoiMSIsInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1kYXRhY2FtcC11c2VycyJdfSwicmVzb3VyY2VfYWNjZXNzIjp7ImFjY291bnQiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJtYW5hZ2UtYWNjb3VudC1saW5rcyIsInZpZXctcHJvZmlsZSJdfX0sInNjb3BlIjoiZW1haWwgZGMtdXNlci1pZCBwcm9maWxlIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsInVzZXJfaWQiOjE5NjAzMTg1LCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJ0YW5ib3Vyc2FsYWhAZ21haWwuY29tIiwiZW1haWwiOiJ0YW5ib3Vyc2FsYWhAZ21haWwuY29tIn0.LZYYhGx3rpiW8sSPPiPIrQzZmCmKWThdnEgAjevjjMEriwB0O5FowodPGlD2bT9HBF1EsW32oakcuSY12mNCR2qEEzvskq-BmvtgjbO7X7hpci5c2zze7OeFX5gwNnhwQ4hR9_uHIyVD7GqPUQaxy6eareC1BkdiZ_2BHkZUXwIKamlciSS-Q7BPt7Ga3R4zzqrs3iDt_Da1vC3J46H1Df3ZeguwU55c7WK1E_ltrfKkrlVy7aq72LJgUGEQE4FUi6-t2-nJmFG_2D1raTLCFKBp3JscbvyFaa1rc7ElCvQ41TuoFJQPH3VzOa802X3rhDKw2gEhQ56FdHOikrNshQ; _gtmeec=eyJlbSI6IjA1NGFlYzZjMWI1OGEyYjRjMThlNGJkYmEyYzZmYzM5NjNmNDY3Nzc0YmM5NzA2NThjMjI2MDhlZTBkOWRiZGMiLCJleHRlcm5hbF9pZCI6IjBlZTk1NTZlNTI0YmYyYWRkNWViZTg2MzU5NTY5ZDk5ODA3MDM4ZTgyZGUxNDAxMTU3OGE1NGZjYjBiOTk3OTAifQ%3D%3D; _cioid=19603185; _hjSessionUser_2445625=eyJpZCI6IjgyYmUyNzk3LTllMmUtNTkwMS1iYzc3LWNlNjMyZDdkNzhhNyIsImNyZWF0ZWQiOjE3NTg3NDUzNDU2NTMsImV4aXN0aW5nIjp0cnVlfQ==; dc_anonid=e5472399-2667-4dae-89d4-be97706eb20c; __cf_bm=LYBOr.9cSrAl0ITLcVKI45Kzd2E4OBKt0wlo4Pc3e2A-1761385945-1.0.1.1-dxv3XAZJHOd5CmhCIwnTLj1aTb0NaUdm6uCssmF7WVjxaAZZz2N_r3XrJjgMMU9HprLPhjo2exe1DOWOOfdHpBkoNt2FnNqTqoGRkMhQbug; _cfuvid=4a_pI5htgfofjadD7.jzLJZLc4WnOgQ31swc3iHHhUQ-1761385945241-0.0.1.1-604800000; cf_clearance=jXhg189W1kuLYxveRt9vLvBXCLY5vp_qZIYDA0o6WFc-1761385945-1.2.1.1-qPXqvKGj.x0HXulr._jw7MTGRGvMwH65K6B3iujXI.mgwjjzVVE1crFLvr2eaQ5Ah6By46ECWKKGUvbjIS4.c38fnLmeyCgiW7mxZ_h0T3YMGT3ZmmMIlkyFeyKbL.S9XlNC3JFc0NHCgKVPmq09jkDl.SlKROJ.zMpcXe4buOykVrnWmxIi8TCMmh07xkA_HuKXr64sme7Z65SMgX3H67LeZ1QXvlyUJKbqOqx4Zg8; FPLC=E3I2gOSrqvtfv1JNwOwzZTNV%2BvjHzht%2FxQzSZvFnJQrwX0JWfrDHJspMtNWUW17pt%2BrHls%2FrktgdjIF3Cb1%2BKDx9KXU2O5cnbbtlKpNht78KYNS25Hzgj4ybKXvIqw%3D%3D; FPGSID=1.1761385947.1761385947.G-0VFSSYW4KG.r4Xrfizr-MMYKny4P2X5QA; _hjSession_2445625=eyJpZCI6IjU4YWI2MDlmLWFjOGMtNDdlMC05MDhlLTIzMmQxMGNhYTVjMSIsImMiOjE3NjEzODU5NDgyNjIsInMiOjAsInIiOjAsInNiIjowLCJzciI6MCwic2UiOjAsImZzIjowfQ==; _hp2_ses_props.4292810930=%7B%22ts%22%3A1761385948011%2C%22d%22%3A%22app.datacamp.com%22%2C%22h%22%3A%22%2Flearn%2Fleaderboard%2Fgaza-sky-geeks-25-26%22%2C%22q%22%3A%22%3Fpage%3D3%22%7D; unsafe_logged_in=1; unsafe_user_id=19603185; _hp2_props.4292810930=%7B%22Browser%20Language%22%3A%22en-US%22%2C%22Browser%20Languages%22%3A%22en-US%2Cen%22%2C%22Signed%20In%22%3A%22true%22%2C%22Subscription%20Active%22%3A%22true%22%2C%22Active%20Products%22%3A%22learn.classroom%2Cworkspace.classroom%22%2C%22User%20Language%22%3A%22en-US%22%7D; IR_13294=1761385996656%7C2778423%7C1761385996656%7C%7C; _ga_0VFSSYW4KG=GS2.1.s1761385947$o10$g1$t1761385996$j11$l1$h2117699123; _hp2_id.4292810930=%7B%22userId%22%3A%226188276596859792%22%2C%22pageviewId%22%3A%223355342662319753%22%2C%22sessionId%22%3A%22498777807616631%22%2C%22identity%22%3A%2219603185%22%2C%22trackerVersion%22%3A%224.0%22%2C%22identityField%22%3Anull%2C%22isIdentified%22%3A1%7D; _uetsid=50ce3130b18811f088caa155871ba42c; _uetvid=268a3030998411f0ae327f389923b7c6; IR_PI=26db89e1-9984-11f0-9efd-c541aefe68d1%7C1761385996656";

if (string.IsNullOrEmpty(cookieValue))
{
    Console.WriteLine("Error: DATACAMP_COOKIE environment variable is not set.");
    Console.WriteLine("Please set the DATACAMP_COOKIE environment variable with your DataCamp session cookie.");
    Environment.Exit(1);
}

// Initialize dependencies
var context = new DatacampDbContext();

// Create repositories
IStudentRepository studentRepository = new StudentRepository(context);
IStudentDailyStatusRepository studentDailyStatusRepository = new StudentDailyStatusRepository(context);
IStudentProgressRepository studentProgressRepository = new StudentProgressRepository(context);
IProcessRepository processRepository = new ProcessRepository(context);

// Create services
IDataCampService dataCampService = new DataCampService(new HttpClient());
IStudentSyncService studentSyncService = new StudentSyncService(studentRepository);
IStatusRecordService statusRecordService = new StatusRecordService(studentDailyStatusRepository, processRepository);
IProgressCalculationService progressCalculationService = new ProgressCalculationService(studentDailyStatusRepository, studentProgressRepository);
IReportService reportService = new ReportService(context);

// Create coordinator service
IDataCampCoordinatorService coordinatorService = new DataCampCoordinatorService(
    dataCampService,
    studentSyncService,
    statusRecordService,
    progressCalculationService,
    processRepository,
    reportService);

// Execute the full sync workflow
var progressCount = await coordinatorService.ExecuteFullSyncAsync(cookieValue);

Console.WriteLine($"\n=== Sync Complete ===");
Console.WriteLine($"Progress records created: {progressCount}");