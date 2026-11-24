namespace NieFarm.Web.Components.Admin.Shared;

/// <summary>
/// Collects the <c>DateInput</c> boxes of one form that currently hold text which is not a date,
/// so the form's Save can refuse instead of silently keeping the previous value.
/// </summary>
public sealed class DateInputGroup
{
    private readonly HashSet<object> _invalid = [];

    public bool HasInvalid => _invalid.Count > 0;

    internal void Report(object owner, bool invalid)
    {
        if (invalid) _invalid.Add(owner);
        else _invalid.Remove(owner);
    }

    public void Reset() => _invalid.Clear();
}
