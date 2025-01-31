using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Recrovit.RecroGridFramework.Blazor.RgfApexCharts;
using Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI.Components;
using System.Reflection;

namespace Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI;

public static class RGFClientBlazorDevExpressConfiguration
{
    public static async Task InitializeRgfDevExpressUIAsync(this IServiceProvider serviceProvider, string themeName = "blazing-berry.bs5", bool loadResources = true)
    {
        RgfBlazorConfiguration.RegisterComponent<MenuComponent>(RgfBlazorConfiguration.ComponentType.Menu);
        RgfBlazorConfiguration.RegisterComponent<DialogComponent>(RgfBlazorConfiguration.ComponentType.Dialog);
        RgfBlazorConfiguration.RegisterEntityComponent<EntityComponent>(string.Empty);

        if (loadResources)
        {
            var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
            await LoadResourcesAsync(jsRuntime, themeName);
        }

        await serviceProvider.InitializeRGFBlazorApexChartsAsync(loadResources);
    }

    public static async Task LoadResourcesAsync(IJSRuntime jsRuntime, string themeName)
    {
        var libName = Assembly.GetExecutingAssembly().GetName().Name;
        await jsRuntime.InvokeVoidAsync("Recrovit.LPUtils.AddStyleSheetLink", $"{RgfClientConfiguration.AppRootPath}/_content/DevExpress.Blazor.Themes/{themeName}.min.css", false, "devexpress-theme", $"{Assembly.GetEntryAssembly()?.GetName().Name}.styles.css");
        await jsRuntime.InvokeVoidAsync("Recrovit.LPUtils.AddStyleSheetLink", DevExtreme, true, "devextreme", "DevExpress.Blazor.Themes");
        await jsRuntime.InvokeVoidAsync("import", $"{RgfClientConfiguration.AppRootPath}/_content/{libName}/scripts/recrovit-rgf-blazor-devexpress.js");
    }

    public static async Task UnloadResourcesAsync(IJSRuntime jsRuntime)
    {
        await jsRuntime.InvokeVoidAsync("eval", "document.getElementById('devexpress-theme')?.remove();");
        await jsRuntime.InvokeVoidAsync("eval", "document.getElementById('devextreme')?.remove();");

        await RgfApexChartsConfiguration.UnloadResourcesAsync(jsRuntime);
    }

    public static readonly string JsBlazorDevExpressNamespace = "Recrovit.RGF.Blazor.DevExpress";
    public static readonly string DevExtreme = "https://cdnjs.cloudflare.com/ajax/libs/devextreme/23.1.4/css/dx.light.css";
}