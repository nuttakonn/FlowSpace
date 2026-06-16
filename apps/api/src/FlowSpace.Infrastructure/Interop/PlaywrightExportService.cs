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

    public async Task<byte[]> ExportToPngAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        // Route must render the canvas without UI bars
        await page.GotoAsync($"{_baseUrl}/boards/{boardId}?export=true", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Wait for React Flow to be ready (custom event or timeout)
        await page.WaitForSelectorAsync(".react-flow__renderer");

        return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true, Type = ScreenshotType.Png });
    }

    public async Task<byte[]> ExportToPdfAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{_baseUrl}/boards/{boardId}?export=true", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");

        return await page.PdfAsync(new PagePdfOptions { PrintBackground = true });
    }

    public async Task<string> ExportToSvgAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{_baseUrl}/boards/{boardId}?export=true", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync(".react-flow__renderer");

        // Extract SVG content from the DOM
        var svgHtml = await page.InnerHTMLAsync(".react-flow__renderer svg");
        return svgHtml;
    }
}
