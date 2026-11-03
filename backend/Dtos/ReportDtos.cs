using Backend.Enums;

namespace Backend.Dtos;

public record CreateReportDto([System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.RegularExpression(@"(?s).*\S.*")] string Message, [System.ComponentModel.DataAnnotations.EnumDataType(typeof(ReportType))] ReportType ReportType, Guid UserId, Guid CollectionPointId);
public record UpdateReportDto(string Message, ReportType ReportType, Guid UserId, Guid CollectionPointId);
public record ReportResponseDto(Guid Id, string Message, ReportType ReportType, Guid UserId, Guid CollectionPointId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);