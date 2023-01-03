namespace NineChronicles.CLI.Type;

public class ActionTxQueryResponseType
{
    public ActionTxQueryType ActionTxQuery { get; set; }
}

public class ActionTxQueryType
{
    public string ActivateAccount { get; set; }
    public string CreateAvatar { get; set; }
    public string RuneEnhancement { get; set; }
    public string FaucetCurrency { get; set; }
    public string FaucetRune { get; set; }
}
