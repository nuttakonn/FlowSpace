using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using FlowSpace.Application.Common.Abstractions.Interop;

namespace FlowSpace.Infrastructure.Interop;

public class PlaywrightExportService : IExportService
{
    private readonly string _baseUrl;

    public PlaywrightExportService(IConfiguration configuration)
    {
        _baseUrl = configuration["FrontendUrl"] 
            ?? configuration["FRONTEND_URL"] 
            ?? "http://localhost:3000";
    }

    private async Task<IBrowser> LaunchBrowserWithAutoInstallAsync(IPlaywright playwright)
    {
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
        };

        try
        {
            return await playwright.Chromium.LaunchAsync(launchOptions);
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist") || ex.Message.Contains("run the following command to download"))
        {
            // Programmatically install chromium using Playwright's own CLI entrypoint
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
            
            // Retry launching the browser
            return await playwright.Chromium.LaunchAsync(launchOptions);
        }
    }

    private async Task SetupAuthenticationAsync(IPage page, string jwtToken)
    {
        if (!string.IsNullOrEmpty(jwtToken))
        {
            await page.AddInitScriptAsync(@$"
                localStorage.setItem('flowspace-auth-storage', JSON.stringify({{
                    state: {{
                        user: {{ id: 'system', displayName: 'System', email: 'system@flowspace.internal' }},
                        accessToken: '{jwtToken}',
                        refreshToken: null,
                        isAuthenticated: true
                    }},
                    version: 0
                }}));
            ");
        }
    }

    public async Task<byte[]> ExportToPngAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserWithAutoInstallAsync(playwright);
        
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true
        });
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
 
        // Wait for React Flow to be ready (custom event or timeout)
        await page.WaitForSelectorAsync(".react-flow__renderer");
        
        // Wait for React Flow viewport, hub connection, and nodes to settle rendering
        await page.WaitForTimeoutAsync(2000);
 
        return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true, Type = ScreenshotType.Png });
    }
 
    public async Task<byte[]> ExportToJpgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserWithAutoInstallAsync(playwright);
        
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true
        });
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");
        
        // Wait for React Flow viewport, hub connection, and nodes to settle rendering
        await page.WaitForTimeoutAsync(2000);
 
        return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true, Type = ScreenshotType.Jpeg });
    }
 
    public async Task<byte[]> ExportToPdfAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserWithAutoInstallAsync(playwright);
        
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true
        });
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");
        
        // Wait for React Flow viewport, hub connection, and nodes to settle rendering
        await page.WaitForTimeoutAsync(2000);
 
        return await page.PdfAsync(new PagePdfOptions { PrintBackground = true });
    }
 
    public async Task<string> ExportToSvgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserWithAutoInstallAsync(playwright);
        
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true
        });
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");
        
        // Wait for React Flow viewport, hub connection, and nodes to settle rendering
        await page.WaitForTimeoutAsync(2000);
 
        // Extract SVG content from the DOM - outerHTML returns the <svg> wrapper itself
        var svgHtml = await page.EvalOnSelectorAsync<string>(".react-flow__renderer svg", "el => el.outerHTML");
        return svgHtml;
    }
}
