using System.Runtime.Serialization;

namespace Backend.Enums;

public enum ReportType
{
  [EnumMember(Value = "not_exist")]
  NotExist,
  [EnumMember(Value = "wrong_location")]
  WrongLocation,
  [EnumMember(Value = "inconsistent_photo")]
  InconsistentPhoto,
}
