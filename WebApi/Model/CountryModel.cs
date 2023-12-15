using System;
using System.Collections.Generic;

public class CountryModel
{
    public class CountryName:OfficalCommon
    {
        public Dictionary<string, OfficalCommon> nativeName { get; set; }
    }
   

    public class OfficalCommon
    {
        public string common { get; set; }
        public string official { get; set; }
    }
    public CountryModel()
    {
        Cca2 = "";
        Ccn3 = "";
        Cca3 = "";
        Languages = new Dictionary<string, string>();
        TopLevelDomain = new List<string>();
        Capital = new List<string>();
        AltSpellings = new List<string>();
        LatLng = new List<double>();
        Timezones = new List<string>();
        Continents = new List<string>();
        Name = new CountryName();
    }
    

    
    public CountryName Name { get; set; }
   
    public string Cca2 { get; set; }
    public string Ccn3 { get; set; }
    public string Cca3 { get; set; }
    public Dictionary<string, string> Languages { get; set; }
    public List<string> TopLevelDomain { get; set; }
    public bool Independent { get; set; }
    public string Status { get; set; }
    public bool UNMember { get; set; }
    // public Dictionary<string, Currency> Currencies { get; set; }
    // public IDD Ids { get; set; }
    public List<string> Capital { get; set; }
    public List<string> AltSpellings { get; set; }
    public string Region { get; set; }
    public string Subregion { get; set; }
    // public Dictionary<string, string> Translations { get; set; }
    public List<double> LatLng { get; set; }
    public bool Landlocked { get; set; }
    public double Area { get; set; }
    // public Demonyms Demonyms { get; set; }
    public string Flag { get; set; }
    // public Maps Maps { get; set; }
    public int Population { get; set; }
    // public Car Car { get; set; }
    public List<string> Timezones { get; set; }
    public List<string> Continents { get; set; }
    // public Flags Flags { get; set; }
    // public CoatOfArms CoatOfArms { get; set; }
    public string StartOfWeek { get; set; }
  
   


}
