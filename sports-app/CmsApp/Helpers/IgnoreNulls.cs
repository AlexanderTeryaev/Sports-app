using Omu.ValueInjecter;

public class IgnoreNulls : ConventionInjection
{
    protected override bool Match(ConventionInfo c)
    {
        return c.SourceProp.Name == c.TargetProp.Name
              && !c.SourceProp.Type.FullName.Contains(nameof(System.Collections.Generic))
              && c.SourceProp.Value != null;
    }
}