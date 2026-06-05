namespace Dfe.PlanTech.Core.Constants;

public static class SessionConstants
{
    public const string SelectedEstablishmentsKey = "selected_establishments";
    public const string FocusedEstablishmentKey = "focused_establishment";
    public static Dictionary<string, Type> SessionKeyTypes = new Dictionary<string, Type>
    {
        {
            SelectedEstablishmentsKey, typeof(IEnumerable<int>)
        },
        {
            FocusedEstablishmentKey, typeof(int)
        }
    };
}
