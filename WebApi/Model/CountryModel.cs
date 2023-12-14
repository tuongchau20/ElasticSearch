using System;
using System.Collections.Generic;

public class CountryModel
{
    public CountryModel()
    {
        Cca2 = "";
        Ccn3 = "";
        Cca3 = "";
        Languages = new Dictionary<string, string>();
    }

    public string Cca2 { get; set; }
    public string Ccn3 { get; set; }
    public string Cca3 { get; set; }

    public Dictionary<string, string> Languages { get; set; }

    public string CommonName { get; set; }
    public string OfficialName { get; set; }
    public string NativeCommonName { get; set; }
    public string NativeOfficialName { get; set; }
    public List<string> TopLevelDomain { get; set; }
    public bool Independent { get; set; }
    public string Status { get; set; }
    public bool UNMember { get; set; }
    public Dictionary<string, Currency> Currencies { get; set; }
    public IDD Ids { get; set; }
    public List<string> Capital { get; set; }
    public List<string> AltSpellings { get; set; }
    public string Region { get; set; }
    public string Subregion { get; set; }
    public Dictionary<string, string> Translations { get; set; }
    public List<double> LatLng { get; set; }
    public bool Landlocked { get; set; }
    public double Area { get; set; }
    public Demonyms Demonyms { get; set; }
    public string Flag { get; set; }
    public Maps Maps { get; set; }
    public int Population { get; set; }
    public Car Car { get; set; }
    public List<string> Timezones { get; set; }
    public List<string> Continents { get; set; }
    public Flags Flags { get; set; }
    public CoatOfArms CoatOfArms { get; set; }
    public string StartOfWeek { get; set; }
    public CapitalInfo CapitalInfo { get; set; }
    public PostalCode PostalCode { get; set; }
}

public class Currency
{
    public string Name { get; set; }
    public string Symbol { get; set; }
}

public class IDD
{
    public string Root { get; set; }
    public List<string> Suffixes { get; set; }
}

public class Demonyms
{
    public Dictionary<string, string> Eng { get; set; }
}

public class Maps
{
    public string GoogleMaps { get; set; }
    public string OpenStreetMaps { get; set; }
}

public class Car
{
    public List<string> Signs { get; set; }
    public string Side { get; set; }
}

public class Flags
{
    public string Png { get; set; }
    public string Svg { get; set; }
}

public class CoatOfArms
{
    public string Png { get; set; }
    public string Svg { get; set; }
}

public class CapitalInfo
{
    public List<double> LatLng { get; set; }
}

public class PostalCode
{
    public string Format { get; set; }
    public string Regex { get; set; }
}
