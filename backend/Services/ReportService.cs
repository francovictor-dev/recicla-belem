using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ReportService(AppDbContext context) : IReportService
{
  private readonly AppDbContext _context = context;

  public async Task<ReportResponseDto> CreateAsync(CreateReportDto dto)
  {
    await ValidateReferencesAsync(dto.UserId, dto.CollectionPointId);

    var now = DateTimeOffset.UtcNow;
    var report = new Report
    {
      Id = Guid.NewGuid(),
      Message = dto.Message,
      ReportType = dto.ReportType,
      UserId = dto.UserId,
      CollectionPointId = dto.CollectionPointId,
      CreatedAt = now,
      UpdatedAt = now
    };

    _context.Reports.Add(report);
    await _context.SaveChangesAsync();
    return MapToDto(report);
  }

  public async Task<ReportResponseDto?> GetByIdAsync(Guid id)
  {
    var report = await _context.Reports.FindAsync(id);
    return report is null ? null : MapToDto(report);
  }

  public async Task<IEnumerable<ReportResponseDto>> GetAllAsync()
  {
    var reports = await _context.Reports.ToListAsync();
    return reports.Select(MapToDto);
  }

  public async Task<ReportResponseDto> UpdateAsync(Guid id, UpdateReportDto dto)
  {
    var report = await _context.Reports.FindAsync(id)
        ?? throw new KeyNotFoundException("Denúncia não encontrada.");

    await ValidateReferencesAsync(dto.UserId, dto.CollectionPointId);

    report.Message = dto.Message;
    report.ReportType = dto.ReportType;
    report.UserId = dto.UserId;
    report.CollectionPointId = dto.CollectionPointId;
    report.UpdatedAt = DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();
    return MapToDto(report);
  }

  public async Task DeleteAsync(Guid id)
  {
    var report = await _context.Reports.FindAsync(id)
        ?? throw new KeyNotFoundException("Denúncia não encontrada.");

    _context.Reports.Remove(report);
    await _context.SaveChangesAsync();
  }

  private async Task ValidateReferencesAsync(Guid userId, Guid collectionPointId)
  {
    var userExists = await _context.Users.AnyAsync(user => user.Id == userId);
    if (!userExists)
      throw new KeyNotFoundException("Usuário não encontrado.");

    var collectionPointExists = await _context.CollectionPoints.AnyAsync(collectionPoint => collectionPoint.Id == collectionPointId);
    if (!collectionPointExists)
      throw new KeyNotFoundException("Ponto de coleta não encontrado.");
  }

  private static ReportResponseDto MapToDto(Report report) =>
      new(report.Id, report.Message, report.ReportType, report.UserId, report.CollectionPointId, report.CreatedAt, report.UpdatedAt);
}