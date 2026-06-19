using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using FlowSpace.Application.Common.Abstractions.Interop;

namespace FlowSpace.Infrastructure.Interop;

public class PlaywrightExportService : IExportService
{
    private readonly string _baseUrl;

    public PlaywrightExportService(IConfiguration configuration)
    {
        _baseUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
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
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
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
 
        return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true, Type = ScreenshotType.Png });
    }
 
    public async Task<byte[]> ExportToJpgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");
 
        return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true, Type = ScreenshotType.Jpeg });
    }
 
    public async Task<byte[]> ExportToPdfAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");
 
        return await page.PdfAsync(new PagePdfOptions { PrintBackground = true });
    }
 
    public async Task<string> ExportToSvgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", string shareToken = "", CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        await SetupAuthenticationAsync(page, jwtToken);
        
        string baseUrl = !string.IsNullOrEmpty(frontendBaseUrl) ? frontendBaseUrl : _baseUrl;
        string targetUrl = $"{baseUrl}/boards/{boardId}?export=true";
        if (!string.IsNullOrEmpty(shareToken))
        {
            targetUrl += $"&token={shareToken}";
        }
        await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");
 
        // Extract SVG content from the DOM - outerHTML returns the <svg> wrapper itself
        var svgHtml = await page.EvalOnSelectorAsync<string>(".react-flow__renderer svg", "el => el.outerHTML");
        return svgHtml;
    }
}
