using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

internal class EditorVisibilityHelper
{
    private readonly bool _alwaysVisible = true;
    private readonly string? _filter;
    private readonly string? _filterGroup;
    private readonly IList<string>? _notVisibleIfElementInheritsType;
    private readonly EditorDefinition _parent;
    private readonly string? _relatedAttribute;
    private readonly bool _relatedAttributeNegate;
    private readonly Expression<bool>? _visibilityExpression;
    private readonly IList<string>? _visibleIfElementInheritsType;
    private readonly string? _visibleIfRelatedAttributeIsType;
    private readonly WorldModel _worldModel;
    private List<Element>? _notVisibleIfElementInheritsTypeElement;
    private List<Element>? _visibleIfElementInheritsTypeElement;

    public EditorVisibilityHelper(EditorDefinition parent, WorldModel worldModel, Element source)
    {
        _parent = parent;
        _worldModel = worldModel;
        _relatedAttribute = source.Fields.GetString("relatedattribute");
        if (_relatedAttribute != null)
        {
            _alwaysVisible = false;
        }

        _relatedAttributeNegate = source.Fields.GetAsType<bool>("relatedattributenegate");
        _visibleIfRelatedAttributeIsType = source.Fields.GetString("relatedattributedisplaytype");
        _visibleIfElementInheritsType = source.Fields.GetAsType<QuestList<string>>("mustinherit");
        _notVisibleIfElementInheritsType = source.Fields.GetAsType<QuestList<string>>("mustnotinherit");
        if (_visibleIfElementInheritsType != null || _notVisibleIfElementInheritsType != null)
        {
            _alwaysVisible = false;
        }

        _filterGroup = source.Fields.GetString("filtergroup");
        _filter = source.Fields.GetString("filter");
        if (_filter != null)
        {
            _alwaysVisible = false;
        }

        var expression = source.Fields.GetString("onlydisplayif");
        if (expression != null)
        {
            _visibilityExpression = new Expression<bool>(Engine.Utility.EncodeIdentifierSpaces(expression),
                new ScriptContext(worldModel, true));
            _alwaysVisible = false;
        }
    }

    public async Task<bool> IsVisible(IEditorData data)
    {
        if (_alwaysVisible)
        {
            return true;
        }

        if (_visibilityExpression != null)
        {
            // evaluate <onlydisplayif> expression, with "this" as the current element
            var context = new Context();
            // Visibility is only evaluated for element editors, whose data is always named
            context.Parameters = new Parameters("this", _worldModel.Elements.Get(data.Name!));
            var result = false;
            try
            {
                result = await _visibilityExpression.ExecuteAsync(context);
            }
            catch
            {
                // ignore any exceptions which may occur, for example if the element is being deleted
            }

            if (!result)
            {
                return false;
            }
        }

        return IsVisibleSyncCore(data);
    }

    // Synchronous subset of IsVisible, used by callers (e.g. script-command control building) that
    // can't await an <onlydisplayif> expression evaluation. A control that declares <onlydisplayif>
    // is treated as always visible here - this mirrors those callers' previous behaviour of applying
    // no visibility filtering at all, rather than silently hiding controls whose gate can't be
    // evaluated synchronously.
    public bool IsVisibleIgnoringExpression(IEditorData data)
    {
        if (_alwaysVisible)
        {
            return true;
        }

        return IsVisibleSyncCore(data);
    }

    private bool IsVisibleSyncCore(IEditorData data)
    {
        if (_notVisibleIfElementInheritsType != null)
        {
            if (_notVisibleIfElementInheritsTypeElement == null)
            {
                // convert "mustnotinherit" type names list into a list of type elements
                _notVisibleIfElementInheritsTypeElement = [.. _notVisibleIfElementInheritsType.Select(t => _worldModel.Elements.Get(ElementType.ObjectType, t))];
            }

            // if the element does inherit any of the "forbidden" types, then this control is not visible

            var element = _worldModel.Elements.Get(data.Name!);

            foreach (var forbiddenType in _notVisibleIfElementInheritsTypeElement)
            {
                if (element.Fields.InheritsTypeRecursive(forbiddenType))
                {
                    return false;
                }
            }
        }

        if (_relatedAttribute != null)
        {
            var relatedAttributeValue = data.GetAttribute(_relatedAttribute);
            if (relatedAttributeValue is IDataWrapper)
            {
                relatedAttributeValue = ((IDataWrapper) relatedAttributeValue).GetUnderlyingValue();
            }

            // An empty string (e.g. an unset script-parameter expression, which saves as "" rather
            // than null once touched - see DoScript/InvokeScript) counts as "no value" here, the same
            // as null, so <relatedattributedisplaytype> can mean "has a real value" rather than "has
            // any value including empty".
            if (relatedAttributeValue is string stringValue && string.IsNullOrEmpty(stringValue))
            {
                relatedAttributeValue = null;
            }

            var relatedAttributeType = relatedAttributeValue == null
                ? "null"
                : WorldModel.ConvertTypeToTypeName(relatedAttributeValue.GetType());
            return (relatedAttributeType == _visibleIfRelatedAttributeIsType) != _relatedAttributeNegate;
        }

        if (_visibleIfElementInheritsType != null)
        {
            if (_visibleIfElementInheritsTypeElement == null)
            {
                // convert "mustinherit" type names list into a list of type elements
                _visibleIfElementInheritsTypeElement = [.. _visibleIfElementInheritsType.Select(t => _worldModel.Elements.Get(ElementType.ObjectType, t))];
            }

            // if the element does inherit any of the types, then this control is visible

            var element = _worldModel.Elements.Get(data.Name!);

            foreach (var type in _visibleIfElementInheritsTypeElement)
            {
                if (element.Fields.InheritsTypeRecursive(type))
                {
                    return true;
                }
            }

            return false;
        }

        if (_filterGroup != null)
        {
            // This control is visible if the named filtergroup's current filter selection is this control's filter.
            var selectedFilter = data.GetSelectedFilter(_filterGroup);

            // Or, if the named filtergroup's current filter selection is not set, infer the current filter value
            // based on which attribute is populated for this data.
            if (selectedFilter == null)
            {
                selectedFilter = _parent.GetDefaultFilterName(_filterGroup, data);
            }

            return selectedFilter == _filter;
        }

        return true;
    }
}