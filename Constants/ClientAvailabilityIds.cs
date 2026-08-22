namespace Klacks.E2ETest.Constants;

public static class ClientAvailabilityIds
{
    public const string Route = "workplace/client-availability";
    public const string RowHeaderCanvasSelector = "#availability-row-header-canvas";
    public const string ContextMenuReportItemId = "clientAvailabilityReport";
    public const string ContextMenuReportItemSelector = "#clientAvailabilityReport";
    public const string QuickPrintButtonId = "client-availability-quick-print-button";
    public const string ToastTextSelector = "span.toast-text";
    public const string BlobUrlPrefix = "blob:";
    public const float FirstRowRightClickX = 50;
    public const float FirstRowRightClickY = 71;
    public const int PdfPopupTimeoutMs = 120000;
}
