using Nest;
using System;
using System.Collections.Generic;
using static CountryModel;
public class CountryName : OfficalCommon
{
    public Dictionary<string, OfficalCommon> nativeName { get; set; }
}

public class Currency
{
    public string Name { get; set; }
    public string Symbol { get; set; }
}
public class OfficalCommon
{
    public string common { get; set; }
    public string official { get; set; }
}
public class IDD
{
    public string root { get; set; }
    public List<string> suffixes { get; set; }
}


public class DemonymInfo
{
    public string f { get; set; }
    public string m { get; set; }
}
public class Map
{
    public string googleMaps { get; set; }
    public string openStreetMaps { get; set; }
}
public class Cars
{
    public List<string> Signs { get; set; }
    public string Side { get; set; }
}


public class CapitalInfos
{
    public List<double> LatLng { get; set; }
}
public class CountryModel
{
  

    public CountryModel()
    {
        Name = new CountryName();
        Cca2 = "";
        Ccn3 = "";
        Cca3 = "";
        Languages = new Dictionary<string, string>();
        Independent = false;
        Status = "";
        UNMember = false;
        Currencies = new Dictionary<string, Currency>();
        Capital = new List<string>();
        AltSpellings = new List<string>();
        LatLng = new List<double>();
        Timezones = new List<string>();
        Continents = new List<string>();
        Region = "";
        Subregion = "";
        Landlocked = false;
        Area = 0.0;
        Population = 0;
        StartOfWeek = "";
        Idd = new IDD();
        Translations = new Dictionary<string, OfficalCommon>();
        Demonyms = new Dictionary<string, DemonymInfo>();
        Fifa = "";
        Car = new Cars();
        Maps = new Map();
        Flags = new Dictionary<string, string>();
        CapitalInfo = new CapitalInfos();
        CoatOfArms = new Dictionary<string, string>();
    }



    public CountryName Name { get; set; }
    public string flag { get; set; }
    public string Cca2 { get; set; }
    public string Ccn3 { get; set; }
    public string Cca3 { get; set; }
    public Dictionary<string, string> Languages { get; set; }
    public bool Independent { get; set; }
    public string Status { get; set; }
    public bool UNMember { get; set; }
    public Dictionary<string, Currency> Currencies { get; set; }
    public IDD Idd { get; set; }
    public List<string> Capital { get; set; }
    public List<string> AltSpellings { get; set; }
    public string Region { get; set; }
    public string Subregion { get; set; }
    public Dictionary<string, OfficalCommon> Translations { get; set; }
    public List<double> LatLng { get; set; }
    public bool Landlocked { get; set; }
    public double Area { get; set; }
    public Dictionary<string,DemonymInfo> Demonyms { get; set; }
    public Map Maps { get; set; }
    public int Population { get; set; }
    public string Fifa { get; set; }
    public Cars Car { get; set; }
    public List<string> Timezones { get; set; }
    public List<string> Continents { get; set; }
    public Dictionary<string,string> Flags { get; set; }
    public Dictionary<string, string> CoatOfArms { get; set; }
    public string StartOfWeek { get; set; }
    public CapitalInfos CapitalInfo { get; set; }


}
