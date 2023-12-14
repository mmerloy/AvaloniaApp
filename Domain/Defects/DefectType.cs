using System.ComponentModel;

namespace Domain.Defects;

public enum DefectType : byte
{
    [Description("Не определено")]
    None = 0,

    ///<summary>механика ободка</summary> 
    [Description("Механика ободка")]
    rim_mechanics,

    ///<summary>стекло</summary> 
    [Description("Стекло")]
    glass,

    ///<summary>механика дна наклепы</summary> 
    [Description("Механика дна наклепы")]
    bottom_mechanics_rivets,

    ///<summary>механика дна вмятины</summary> 
    [Description("Механика дна вмятины")]
    bottom_mechanics_dents,

    ///<summary>нависание</summary> 
    [Description("Нависание")]
    overhang,

    ///<summary>залив контакта</summary> 
    [Description("Залив контакта")]
    contact_bay,

    ///<summary>залив монтажной площадки</summary> 
    [Description("Залив монтажной площадки")]
    installation_site_bay,

    ///<summary>гнутые выводы</summary> 
    [Description("Гнутые выводы")]
    bent_conclusions,

    ///<summary>раковина</summary> 
    [Description("Раковина")]
    sink,

    ///<summary>непараллельность выводов</summary> 
    [Description("Непараллельность выводов")]
    non_parallelism_of_conclusions,

    ///<summary>трещина</summary> 
    [Description("Трещина")]
    crack
}
