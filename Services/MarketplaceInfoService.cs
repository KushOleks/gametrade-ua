namespace OrderSystem.Services;

public class MarketplaceInfoService : IMarketplaceInfoService
{
    public string GetProjectName()
    {
        return "GameTrade UA";
    }

    public string GetDescription()
    {
        return "Український маркетплейс для купівлі та продажу ігрових цінностей.";
    }
}