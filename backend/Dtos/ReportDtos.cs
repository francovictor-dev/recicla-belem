using Backend.Enums;

namespace Backend.Dtos;

public record CreateReportDto(string Message, ReportType ReportType, Guid UserId, Guid CollectionPointId);
public record UpdateReportDto(string Message, ReportType ReportType, Guid UserId, Guid CollectionPointId);
public record ReportResponseDto(Guid Id, string Message, ReportType ReportType, Guid UserId, Guid CollectionPointId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);