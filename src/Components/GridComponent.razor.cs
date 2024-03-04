using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Recrovit.RecroGridFramework.Abstraction.Contracts.Services;
using Recrovit.RecroGridFramework.Abstraction.Models;
using Recrovit.RecroGridFramework.Client.Blazor.Components;
using Recrovit.RecroGridFramework.Client.Events;
using Recrovit.RecroGridFramework.Client.Handlers;

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

    private List<IDisposable> _disposables { get; set; } = new();

    private bool _initialized { get; set; }

    private Dictionary<string, int> _sort { get; set; } = new();

    private RgfEntity EntityDesc => Manager.EntityDesc;

    public IRgListHandler ListHandler => Manager.ListHandler;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GridParameters.EventDispatcher.Subscribe(RgfListEventKind.CreateRowData, OnCreateAttributes);
        _initialized = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _disposables.Add(Manager.NotificationManager.Subscribe<RgfToolbarEventArgs>(this, OnToolbarCommand));
        }
        if (_initialized && _selfRef == null)
        {
            _selfRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync(RGFClientBlazorDevExpressConfiguration.JsBlazorDevExpressNamespace + ".Grid.initializeTable", _selfRef, Manager.EntityDomId);
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
        if (_disposables != null)
        {
            _disposables.ForEach(disposable => disposable.Dispose());
            _disposables = null!;
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

    protected virtual Task OnCreateAttributes(IRgfEventArgs<RgfListEventArgs> arg)
    {
        _logger.LogDebug("CreateAttributes");
        var rowData = arg.Args.Data ?? throw new ArgumentException();
        foreach (var prop in EntityDesc.SortedVisibleColumns)
        {
            string? propClass = null;
            if (prop.FormType == PropertyFormType.CheckBox)
            {
                propClass = "rg-center";
            }
            else
            {
                switch (prop.ListType)
                {
                    case PropertyListType.Numeric:
                        propClass = "rg-numeric";
                        break;

                    case PropertyListType.String:
                        propClass = "rg-string";
                        break;

                    case PropertyListType.Image:
                        propClass = "rg-html";
                        break;
                }
            }
            if (propClass != null)
            {
                var attributes = rowData.GetOrNew<RgfDynamicDictionary>("__attributes");
                var propAttributes = attributes.GetOrNew<RgfDynamicDictionary>(prop.Alias);
                propAttributes.Set<string>("class", (old) => string.IsNullOrEmpty(old) ? propClass : $"{old.Trim()} {propClass}");
            }
        }
        return Task.CompletedTask;
    }

    protected virtual void OnCustomizeElement(GridCustomizeElementEventArgs args)
    {
        var rowData = args.Grid.GetDataItem(args.VisibleIndex) as RgfDynamicDictionary;
        if (rowData != null)
        {
            var attributes = rowData.Get<RgfDynamicDictionary>("__attributes");
            if (attributes != null)
            {
                if (args.ElementType == GridElementType.DataRow)
                {
                    var val = attributes.Get<string>("class");
                    if (val != null)
                    {
                        args.CssClass = val;
                    }
                    val = attributes.Get<string>("style");
                    if (val != null)
                    {
                        args.Style = val;
                    }
                }
                else if (args.ElementType == GridElementType.DataCell)
                {
                    var prop = EntityDesc.Properties.SingleOrDefault(e => e.ClientName == args.Column.Name);
                    if (prop?.ColPos > 0)
                    {
                        var propAttributes = attributes.Get<RgfDynamicDictionary>(prop.Alias);
                        if (propAttributes != null)
                        {
                            var val = propAttributes.Get<string>("class");
                            if (val != null)
                            {
                                args.CssClass = val;
                            }
                            val = propAttributes.Get<string>($"style");
                            if (val != null)
                            {
                                args.Style = val;
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnCustomSort(GridCustomSortEventArgs args)
    {
        //TODO: Somehow the sorting should be disabled because the server already sends a sorted list.
        var v1 = (args.DataItem1 as RgfDynamicDictionary)?.GetMember(args.FieldName);
        var v2 = (args.DataItem2 as RgfDynamicDictionary)?.GetMember(args.FieldName);
        //_logger.LogDebug("OnCustomSort: {v1},{v2}", v1, v2);
        //Console.WriteLine($"{args.FieldName}: {v1},{v2}");
        if (v1 == null && v2 == null)
        {
            args.Result = 0;
        }
        else if (v1 == null && v2 != null)
        {
            args.Result = -1;
        }
        else if (v1 != null && v2 == null)
        {
            args.Result = 1;
        }
        else
        {
            try
            {
                if (v1!.GetType() == typeof(string))
                {
                    args.Result = string.Compare(v1.ToString(), v2!.ToString());
                }
                else
                {
                    args.Result = v1!.Equals(v2) ? 0 : (dynamic)v1 > (dynamic)v2! ? 1 : -1;
                }
            }
            catch { }
        }
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

    private void OnToolbarCommand(IRgfEventArgs<RgfToolbarEventArgs> arg)
    {
        switch (arg.Args.Command)
        {
            case ToolbarAction.Edit:
            case ToolbarAction.Read:
                var data = _rgfGridRef.SelectedItems.Single();
                int rowIndex = Manager.ListHandler.GetRelativeRowIndex(data);
                if (rowIndex != -1)
                {
                    _dxGridRef.ClearSelection();
                    _dxGridRef.SelectRow(rowIndex);
                }
                break;
        }
    }
}