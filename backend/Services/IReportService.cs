using Backend.Dtos;

namespace Backend.Services;

public interface IReportService : ICrudService<CreateReportDto, UpdateReportDto, ReportResponseDto>
{
}