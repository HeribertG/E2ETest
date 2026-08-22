using Klacks.E2ETest.Constants;
using Klacks.E2ETest.Helpers;
using Klacks.E2ETest.Wrappers;
using Microsoft.Playwright;

namespace Klacks.E2ETest.ClientAvailability;

/// <summary>
/// Verifies the two report triggers of the client-availability grid: the per-client
/// context-menu entry (right-click on the canvas row header) and the quick-print button
/// for the filtered client list. Both must open the jsPDF preview in a new tab (blob: URL).
/// </summary>
[TestFixture]
[Category("ClientAvailability")]
public class ClientAvailabilityReportPrintTests : PlaywrightSetup
{
    private Listener? _listener;
    private readonly List<string> _consoleErrors = new();
    private EventHandler<IConsoleMessage>? _consoleHandler;

    private static string ScreenshotFolder
    {
        get
        {
            var projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", ".."));
            var folder = Path.Combine(projectRoot, "Screenshots");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }

    [OneTimeSetUp]
    public async Task PrepareEnvironmentAsync()
    {
        await EnableExpertModeAndDismissOnboardingAsync();
    }

    [SetUp]
    public async Task SetupInternalAsync()
    {
        _listener = new Listener(Page);
        _listener.RecognizeApiErrors();

        _consoleErrors.Clear();
        _consoleHandler = (_, msg) =>
        {
            if (msg.Type == "error")
            {
                _consoleErrors.Add(msg.Text);
            }
        };
        Page.Console += _consoleHandler;

        await Actions.NavigateTo(BaseUrl + ClientAvailabilityIds.Route);
        await Actions.WaitForSpinnerToDisappear();
        await Actions.ElementIsVisibleByCssSelector(ClientAvailabilityIds.RowHeaderCanvasSelector);
        await Actions.Wait2000();
    }

    [TearDown]
    public async Task CleanupAfterTestAsync()
    {
        if (_listener != null)
        {
            await _listener.WaitForResponseHandlingAsync();
            if (_listener.HasApiErrors())
            {
                TestContext.WriteLine(_listener.GetLastErrorMessage());
            }

            _listener.ResetErrors();
        }

        _listener = null;

        if (_consoleHandler != null)
        {
            Page.Console -= _consoleHandler;
            _consoleHandler = null;
        }
    }

    [Test]
    [Order(1)]
    public async Task ContextMenuReport_OnRightClickFirstRow_OpensPdfPreviewInNewTab()
    {
        // Arrange
        await Actions.TakeScreenshotAsync(Path.Combine(ScreenshotFolder, "context-menu-report-01-grid-loaded.png"));

        // Act
        await Actions.RightClickByCssSelectorAtPosition(
            ClientAvailabilityIds.RowHeaderCanvasSelector,
            ClientAvailabilityIds.FirstRowRightClickX,
            ClientAvailabilityIds.FirstRowRightClickY);
        await Actions.ElementIsVisibleByCssSelector(ClientAvailabilityIds.ContextMenuReportItemSelector);
        await Actions.TakeScreenshotAsync(Path.Combine(ScreenshotFolder, "context-menu-report-02-menu-open.png"));

        var popupUrl = await Actions.RunAndWaitForPopupUrlAsync(
            () => Actions.ClickButtonById(ClientAvailabilityIds.ContextMenuReportItemId),
            ClientAvailabilityIds.BlobUrlPrefix,
            ClientAvailabilityIds.PdfPopupTimeoutMs);

        // Assert
        await AssertPdfPopupAsync(popupUrl, "context-menu-report");
    }

    [Test]
    [Order(2)]
    public async Task QuickPrintButton_OnClick_OpensPdfPreviewInNewTab()
    {
        // Arrange
        await Actions.TakeScreenshotAsync(Path.Combine(ScreenshotFolder, "quick-print-01-grid-loaded.png"));
        var button = await Actions.FindElementById(ClientAvailabilityIds.QuickPrintButtonId);
        Assert.That(button, Is.Not.Null,
            "Quick-print button not found - is the default template for 'client-availability' assigned?");
        Assert.That(await Actions.IsDisabled(button), Is.False, "Quick-print button is disabled");

        // Act
        var popupUrl = await Actions.RunAndWaitForPopupUrlAsync(
            () => Actions.ClickButtonById(ClientAvailabilityIds.QuickPrintButtonId),
            ClientAvailabilityIds.BlobUrlPrefix,
            ClientAvailabilityIds.PdfPopupTimeoutMs);

        // Assert
        await AssertPdfPopupAsync(popupUrl, "quick-print");
    }

    private async Task AssertPdfPopupAsync(string? popupUrl, string screenshotPrefix)
    {
        if (popupUrl == null)
        {
            await Actions.TakeScreenshotAsync(Path.Combine(ScreenshotFolder, $"{screenshotPrefix}-03-no-popup.png"));
            var toastText = await Actions.GetTextContentByCssSelector(ClientAvailabilityIds.ToastTextSelector);
            var consoleErrors = _consoleErrors.Count == 0
                ? "none"
                : string.Join(" | ", _consoleErrors);
            Assert.Fail(string.IsNullOrEmpty(toastText)
                ? $"No new tab opened and no toast appeared - the PDF preview was not triggered. Console errors: {consoleErrors}"
                : $"No new tab opened - toast says: '{toastText}'. Console errors: {consoleErrors}");
        }

        TestContext.Out.WriteLine($"Popup URL: {popupUrl}");
        await Actions.TakeScreenshotAsync(Path.Combine(ScreenshotFolder, $"{screenshotPrefix}-03-after-trigger.png"));

        Assert.That(popupUrl, Does.StartWith(ClientAvailabilityIds.BlobUrlPrefix),
            $"New tab did not open a blob: PDF preview - URL was '{popupUrl}'");
        Assert.That(_listener!.HasApiErrors(), Is.False,
            $"API error while generating the report: {_listener!.GetLastErrorMessage()}");
    }
}
