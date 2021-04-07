# CacheAnt
.NET auto-refreshing cache

Very useful for situations where you want to offload cache preloading to a background service and make data instantly available.
This is a kinda never-expiring, background-refreshing cache. And it's super simple to use :-)

## Install
Install [CacheAnt with NuGet](https://www.nuget.org/packages/CacheAnt):

    Install-Package CacheAnt

Or via the .NET Core CLI:

    dotnet add package CacheAnt

## Example
Create a caching definition:
```csharp
public class CashedCurrencies : AutoCached<IEnumerable<Currency>>
{
  private readonly IDataContext _dataContext;

  public CashedCurrencies(IDataContext dataContext)
  {
    _dataContext = dataContext;
  }

  public override TimeSpan AutoRefreshInterval => TimeSpan.FromSeconds(15);

  public override IEnumerable<Currency> Compute()
  {
    return _dataContext.Currency.AsNoTracking().ToList();
  }
}
```
Here we are caching an IEnumerable\<Currency\>. It could be usefull to cache whole dictionaries also.

Add CacheAnt and pass assemblies containg your AutoCached definitions:
```csharp
services.AddCacheAnt(Assembly.GetExecutingAssembly());
```
Use it:
```csharp
public class SomeService
{
  private readonly CashedCurrencies _cashedCurrencies;

  public SomeService(CashedCurrencies cashedCurrencies)
  {
    _cashedCurrencies = cashedCurrencies;
  }

  public IEnumerable<Currency> GetCurrencies() => _cashedCurrencies.GetCached() ?? Enumerable.Empty<Currency>();
}
```

Background IHostedService CacheAntService manages cache and executes refresh at specified intervals. Initial cache loading is done on application start.