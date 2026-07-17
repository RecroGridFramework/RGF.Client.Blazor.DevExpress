# RecroGrid Framework Blazor DevExpress UI

[![NuGet Version](https://img.shields.io/nuget/v/Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI.svg?label=RGF.Client.Blazor.DevExpressUI)](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI/)

[![NuGet Version](https://img.shields.io/nuget/v/Recrovit.RecroGridFramework.Client.Blazor.UI.svg?label=Recrovit.RecroGridFramework.Client.Blazor.UI)](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor.UI/)
[![NuGet Version](https://img.shields.io/nuget/v/Recrovit.RecroGridFramework.Core.svg?label=RGF.Core)](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Core/)
[![NuGet Version](https://img.shields.io/nuget/v/RecroGrid.svg?label=RecroGrid)](https://www.nuget.org/packages/RecroGrid/)

[`Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI/) provides a DevExpress-based UI implementation for the [RecroGrid Framework](https://recrogridframework.com/) Blazor client stack.

The package demonstrates how a third-party Blazor component library can be integrated with RecroGrid Framework without reimplementing the framework runtime, metadata processing, API communication, security, localization, or entity orchestration.

Official Website: [RecroGrid Framework](https://RecroGridFramework.com)

## Overview

RecroGrid Framework separates application functionality from the concrete UI component library.

```text
RecroGrid Framework metadata and runtime services
                |
                v
Recrovit.RecroGridFramework.Client.Blazor
                |
                v
DevExpress UI adapter
                |
                v
DevExpress Blazor components
```

The base [`Recrovit.RecroGridFramework.Client.Blazor`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor/) package provides the Blazor integration layer, component parameters, templates, events, runtime services, and component registration points.

This package maps those integration points to DevExpress Blazor components.

As a result, the application continues to use the same RecroGrid Framework entity definitions, metadata, views, permissions, handlers, and server APIs while the visual components are provided by DevExpress.

## Why UI Library Integration Is Simple

The RecroGrid Framework Blazor layer separates the following responsibilities:

* entity metadata and runtime behavior
* API communication and data operations
* security and authorization
* localization
* filtering, sorting, paging, and view configuration
* framework events and commands
* visual rendering through replaceable Blazor components

A UI adapter therefore does not need to recreate the complete RecroGrid Framework functionality.

It primarily needs to:

1. implement the required visual components
2. connect them to the existing RGF component parameters and events
3. register them with the RGF Blazor runtime
4. load the required styles and scripts

The DevExpress integration follows exactly this model.

## How the Adapter Works

### Entity composition

The DevExpress `EntityComponent` uses the standard `RgfEntityComponent` and supplies UI-specific templates for the toolbar, grid, filter, pager, form, and chart areas.

```razor
<RgfEntityComponent EntityParameters="EntityParameters"
                    ToolbarTemplate="ToolbarTemplate"
                    GridTemplate="GridTemplate"
                    FilterTemplate="FilterTemplate"
                    PagerTemplate="PagerTemplate"
                    FormTemplate="FormTemplate"
                    ChartTemplate="ChartTemplate">
</RgfEntityComponent>
```

The entity lifecycle and runtime behavior remain in RecroGrid Framework. The adapter only supplies the concrete UI components.

### Grid implementation

The DevExpress grid implementation uses the standard RGF grid wrapper and renders its data through `DxGrid`.

```razor
<RgfGridComponent EntityParameters="EntityParameters"
                  GridComponent="this">
    <GridTemplate Context="RgfGrid">
        <DxGrid Data="@RgfGrid.GridData">
            <!-- DevExpress column implementation -->
        </DxGrid>
    </GridTemplate>
</RgfGridComponent>
```

Column definitions, titles, visibility, ordering, sorting, width, formatting, and rendered cell content are driven by RecroGrid Framework metadata.

The DevExpress component is responsible for presenting that information and forwarding UI events to the corresponding RGF handlers.

### Component registration

UI implementations are registered through the component extension points exposed by the base RGF Blazor package.

```csharp
RgfBlazorConfiguration.RegisterComponent<MenuComponent>(
    RgfBlazorConfiguration.ComponentType.Menu);

RgfBlazorConfiguration.RegisterComponent<DialogComponent>(
    RgfBlazorConfiguration.ComponentType.Dialog);

RgfBlazorConfiguration.RegisterEntityComponent<EntityComponent>(
    string.Empty);
```

This registration model allows an application or another adapter package to replace individual UI components without modifying the RecroGrid Framework runtime.

## What This Package Provides

The package contains DevExpress-based implementations for the main RecroGrid Framework UI areas, including:

* entity composition
* data grid
* forms and form fields
* filtering
* paging
* toolbar actions
* menus
* dialogs
* loading indicators
* column settings
* theme integration
* JavaScript and stylesheet resource loading
* chart integration through the RGF ApexCharts package

## Reference UI Implementation

For a complete and extensively documented implementation of the RecroGrid Framework Blazor UI layer, see:

### Recrovit.RecroGridFramework.Client.Blazor.UI

[`Recrovit.RecroGridFramework.Client.Blazor.UI`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor.UI/) is the ready-to-use default RecroGrid Framework UI package.

It contains a complete Bootstrap-based implementation of the main RGF UI areas, including:

* menu and navigation
* dialogs and notifications
* entity views
* grids
* filters
* forms
* pagers
* trees
* toolbars
* charts
* dashboard pages
* dashboard runtime rendering
* dashboard designer
* reusable input and layout controls
* resource loading and theme handling

The package is also the most detailed reference implementation for developers who want to create another RecroGrid Framework UI adapter.

* [Default UI documentation](https://github.com/Recrovit/rgf-client/blob/main/src/RGF.Client.Blazor.UI/README.md)
* [Default UI source code](https://github.com/Recrovit/rgf-client/tree/main/src/RGF.Client.Blazor.UI)
* [Default UI NuGet package](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor.UI/)
* [Base RGF Blazor package](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor/)

The DevExpress package is an alternative UI implementation built directly on the base RGF Blazor integration layer. It is not dependent on the Bootstrap-based default UI package.

## Creating Another UI Adapter

The same integration model can be used with another Blazor component library or a custom design system.

A typical adapter implementation consists of the following steps.

### 1. Build on the base RGF Blazor integration layer

Use `Recrovit.RecroGridFramework.Client.Blazor` as the integration layer that provides the component extension points, parameters, templates, events, and runtime services required by the adapter.

### 2. Implement the required UI components

Create UI-specific implementations for the required areas, such as:

* entity
* grid
* form
* filter
* pager
* toolbar
* menu
* dialog

The components should use the parameters, templates, models, event dispatchers, and handlers provided by the RGF Blazor layer.

### 3. Map UI events to RGF handlers

Forward component library events such as:

* row selection
* double-click
* sorting
* column movement
* column resizing
* paging
* filtering
* toolbar commands
* form value changes

to the appropriate RecroGrid Framework handlers.

### 4. Register the UI implementations

Register the adapter components through the extension points provided by `RgfBlazorConfiguration`. This allows the UI layer to be replaced without modifying the RecroGrid Framework runtime.

### 5. Load library resources

Provide an initialization method that loads the required:

* stylesheets
* themes
* JavaScript modules
* icons
* component library services

The framework runtime does not need to be changed when a new visual adapter is introduced.

## RGF UI Implementations

The RecroGrid Framework Blazor integration model is implemented by the following UI packages:

* [Bootstrap](https://github.com/Recrovit/rgf-client/blob/main/src/RGF.Client.Blazor.UI) - primary and officially supported RGF Blazor UI implementation
* [DevExpress](https://github.com/RecroGridFramework/RGF.Client.Blazor.DevExpress)
* [Radzen](https://github.com/RecroGridFramework/RGF.Client.Blazor.Radzen)
* [Syncfusion](https://github.com/RecroGridFramework/RGF.Client.Blazor.Syncfusion)
* [Telerik](https://github.com/RecroGridFramework/RGF.Client.Blazor.Telerik)

These implementations demonstrate that RecroGrid Framework applications are not locked to a single UI component library, while the Bootstrap-based UI remains the primary, officially supported UI and reference implementation.

## Related Packages

* [`Recrovit.RecroGridFramework.Abstraction`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Abstraction/)
  Shared contracts, models, and abstractions used across the RecroGrid Framework client and server packages.

* [`Recrovit.RecroGridFramework.Client`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client/)
  Client-side API access, security, localization, runtime services, and entity orchestration.

* [`Recrovit.RecroGridFramework.Client.Blazor`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor/)
  Base Blazor integration layer, component extension points, templates, parameters, and runtime components.

* [`Recrovit.RecroGridFramework.Client.Blazor.UI`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Client.Blazor.UI/)
  Complete ready-to-use Bootstrap-based RecroGrid Framework UI implementation.

* [`Recrovit.RecroGridFramework.Blazor.RgfApexCharts`](https://www.nuget.org/packages/Recrovit.RecroGridFramework.Blazor.RgfApexCharts/)
  ApexCharts-based chart implementation for RecroGrid Framework Blazor applications.

## Requirements

The package is designed for Blazor applications using the RecroGrid Framework client stack and DevExpress Blazor components.

Refer to the project file and NuGet package metadata for the currently supported framework and dependency versions.

## Links

* [RecroGrid Framework](https://recrogridframework.com/)
* [RecroGrid Framework Quickstart](https://recrogridframework.com/quickstart)
* [RGF client packages and source code](https://github.com/Recrovit/rgf-client)
* [Default RGF Blazor UI documentation](https://github.com/Recrovit/rgf-client/blob/main/src/RGF.Client.Blazor.UI/README.md)
* [DevExpress Blazor documentation](https://docs.devexpress.com/Blazor)
