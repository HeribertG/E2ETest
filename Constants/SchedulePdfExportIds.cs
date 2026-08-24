// Copyright (c) Heribert Gasparoli Private. All rights reserved.

namespace Klacks.E2ETest.Constants;

public static class SchedulePdfExportIds
{
    public const string Route = "workplace/schedule";
    public const string PdfExportButtonId = "schedule-pdf-export-btn";
    public const string PdfExportButtonSelector = "#schedule-pdf-export-btn";
    public const string RowHeaderRowSelector = "app-schedule-schedule-row-header .drag-row";
    public const string ViewModeStorageKey = "klacks.schedule.viewMode";
    public const string TableViewModeValue = "table";
    public const string ToastTextSelector = "span.toast-text";
    public const string BlobUrlPrefix = "blob:";
    public const int PdfPopupTimeoutMs = 120000;
}
