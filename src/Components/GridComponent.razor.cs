using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Recrovit.RecroGridFramework.Abstraction.Infrastructure.Events;
using Recrovit.RecroGridFramework.Abstraction.Models;
using Recrovit.RecroGridFramework.Client.Blazor.Components;
using Recrovit.RecroGridFramework.Client.Handlers;
using System;
using System.Linq;

namespace Recrovit.RecroGridFramework.Client.Blazor.DevExpressUI.Components;

public partial class GridComponent : ComponentBase, IDisposable
{
    [Inject]
    private ILogger<GridComponent> _logger { get; set; } = default!;

    [Inject]
    private IJSRuntime _jsRuntime { get; set; } = default!;

    private RgfGridComponent _rgfGridRef { get; set; } = default!;

    private DxGrid _dxGridRef { get; set; } = default!;

    private DotNetObjectReference<GridComponent>? _selfRef;

    private bool _initialized { get; set; }

    private Dictionary<string, int> _sort { get; set; } = new();

    private RgfEntity EntityDesc => Manager.EntityDesc;

    public IRgListHandler ListHandler => Manager.ListHandler;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GridParameters.Events.CreateAttributes.Subscribe(OnCreateAttributes);
        _initialized = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
        }
        if (_initialized && _selfRef == null)
        {
            _selfRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync(Configuration.JsBlazorDevExpressNamespace + ".Grid.initializeTable", _selfRef, Manager.EntityDomId);
        }
    }

    private void Recreate()
    {
        if (_initialized)
        {
            foreach (var item in _dxGridRef.GetSortedColumns())
            {
                _sort.Add(item.FieldName, item.SortOrder == GridColumnSortOrder.Ascending ? item.SortIndex + 1 : -(item.SortIndex + 1));
            }

            _initialized = false;
            Dispose();
            StateHasChanged();
            _ = Task.Run(() =>
            {
                _initialized = true;
                if (_sort.Any())
                {
                    ListHandler.SetSortAsync(_sort);
                    _sort.Clear();
                }
                StateHasChanged();
            });
        }
    }

    public void Dispose()
    {
        if (_selfRef != null)
        {
            _selfRef.Dispose();
            _selfRef = null;
        }
    }

    //private void UnboundColumnData(GridUnboundColumnDataEventArgs e) { e.Value = ((RgfDynamicDictionary)e.DataItem).GetMember(e.FieldName); }

    private async Task OnVisibleIndexChanged(RgfProperty property, int newIdx)
    {
        if (property.ColPos != newIdx + 1)
        {
            await ListHandler.MoveColumnAsync(property.ColPos, newIdx + 1, false);
            Recreate();
        }
    }

    private void OnWidthChanged(RgfProperty property, string width)
    {
        if (_initialized)
        {
            ListHandler.ReplaceColumnWidth(property.Alias, Convert.ToInt32(width.Replace("px", "")));
            Recreate();
        }
    }

    protected virtual Task OnCreateAttributes(DataEventArgs<RgfDynamicDictionary> rowData)
    {
        foreach (var prop in EntityDesc.SortedVisibleColumns)
        {
            var attr = rowData.Value["__attributes"] as RgfDynamicDictionary;
            if (attr != null)
            {
                string? propAttr = null;
                if (prop.FormType == PropertyFormType.CheckBox)
                {
                    propAttr = " rg-center";
                }
                else
                {
                    switch (prop.ListType)
                    {
                        case PropertyListType.Numeric:
                            propAttr = " rg-numeric";
                            break;

                        case PropertyListType.String:
                            propAttr = " rg-string";
                            break;

                        case PropertyListType.Image:
                            propAttr = " rg-html";
                            break;
                    }
                }
                if (propAttr != null)
                {
                    attr[$"class-{prop.Alias}"] += propAttr;
                }
            }
        }
        return Task.CompletedTask;
    }

    protected virtual void OnCustomizeElement(GridCustomizeElementEventArgs args)
    {
        var rowData = (RgfDynamicDictionary)args.Grid.GetDataItem(args.VisibleIndex);
        if (rowData != null)
        {
            var attributes = (RgfDynamicDictionary)rowData["__attributes"];
            if (args.ElementType == GridElementType.DataRow)
            {
                var attr = attributes.GetItemData("class").StringValue;
                if (attr != null)
                {
                    args.CssClass = attr;
                }
                attr = attributes.GetItemData("style").StringValue;
                if (attr != null)
                {
                    args.Style = attr;
                }
            }
            else if (args.ElementType == GridElementType.DataCell)
            {
                var prop = EntityDesc.Properties.SingleOrDefault(e => e.ClientName == args.Column.Name);
                if (prop?.ColPos > 0)
                {
                    var attr = attributes.GetItemData($"class-{prop.Alias}").StringValue;
                    if (attr != null)
                    {
                        args.CssClass = attr;
                    }
                    attr = attributes.GetItemData($"style-{prop.Alias}").StringValue;
                    if (attr != null)
                    {
                        args.Style = attr;
                    }
                }
            }
        }
    }

    private void OnCustomSort(GridCustomSortEventArgs args)
    {
        args.Result = 1;
        args.Handled = true;
    }

    private async Task OnSort(MouseEventArgs args, RgfProperty property)
    {
        var dict = new Dictionary<string, int>();
        if (args.ShiftKey)
        {
            int idx = 1;
            bool add = true;
            foreach (var item in EntityDesc.SortColumns)
            {
                int sort = item.Sort;
                if (property.Id == item.Id)
                {
                    add = false;
                    if (!args.CtrlKey)
                    {
                        //remove
                        continue;
                    }
                    dict.Add(item.Alias, item.Sort > 0 ? -idx : idx);//reverse
                }
                else
                {
                    dict.Add(item.Alias, item.Sort > 0 ? idx : -idx);
                }
                idx++;
            }
            if (add)
            {
                dict.Add(property.Alias, args.CtrlKey ? -idx : idx);
            }
        }
        else
        {
            if (EntityDesc.SortColumns.Count() == 1 && EntityDesc.SortColumns.Single().Id == property.Id)
            {
                dict.Add(property.Alias, -EntityDesc.SortColumns.Single().Sort);
            }
            else
            {
                dict.Add(property.Alias, args.CtrlKey ? -1 : 1);
            }
        }
        await ListHandler.SetSortAsync(dict);
    }

    protected virtual async Task OnRowClick(GridRowClickEventArgs args)
    {
        var rowData = (RgfDynamicDictionary)args.Grid.GetDataItem(args.VisibleIndex);
        args.Grid.ClearSelection();
        if (_rgfGridRef.SelectedItems.Any())
        {
            bool deselect = _rgfGridRef.SelectedItems[0] == rowData;
            await _rgfGridRef.RowDeselectHandlerAsync(rowData);
            if (deselect)
            {
                return;
            }
        }
        args.Grid.SelectRow(args.VisibleIndex);
        await _rgfGridRef.RowSelectHandlerAsync(rowData);
    }

    protected virtual async Task OnRowDoubleClick(GridRowClickEventArgs args)
    {
        args.Grid.ClearSelection();
        args.Grid.SelectRow(args.VisibleIndex);
        var rowData = (RgfDynamicDictionary)args.Grid.GetDataItem(args.VisibleIndex);
        await _rgfGridRef.OnRecordDoubleClickAsync(rowData);
    }
}
