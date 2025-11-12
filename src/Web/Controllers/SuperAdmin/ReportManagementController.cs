using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Core.ReportManagement.Commands;
using HotelManagement.Application.Core.ReportManagement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin report management controller
/// </summary>
[ApiController]
[Route("cp/reports")]
[Authorize(Roles = "SuperAdmin")]
public class ReportManagementController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<ReportManagementController> _logger;

    public ReportManagementController(
        ISender mediator,
        ILogger<ReportManagementController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get list of reports with filtering and pagination
    /// </summary>
    /// <param name="request">Report filter parameters</param>
    /// <returns>Paginated list of reports</returns>
    [HttpGet]
    public async Task<ActionResult> GetReports([FromQuery] GetReportsQuery request)
    {
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Get specific report by ID
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <returns>Report details</returns>
    [HttpGet("{reportId}")]
    public async Task<ActionResult> GetReport(Guid reportId)
    {
        var request = new GetReportByIdQuery { ReportId = reportId };
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Generate a new report
    /// </summary>
    /// <param name="command">Report generation request</param>
    /// <returns>Generated report details</returns>
    [HttpPost]
    public async Task<ActionResult> GenerateReport([FromBody] GenerateReportCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Delete a report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("{reportId}")]
    public async Task<ActionResult> DeleteReport(Guid reportId)
    {
        var command = new DeleteReportCommand { ReportId = reportId };
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Download a generated report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <returns>Report file</returns>
    [HttpGet("{reportId}/download")]
    public async Task<ActionResult> DownloadReport(Guid reportId)
    {
        try
        {
            // Get report from database
            var reportQuery = new GetReportByIdQuery { ReportId = reportId };
            var reportResult = await _mediator.Send(reportQuery);

            if (!reportResult.Succeeded || reportResult.Data == null)
            {
                return NotFound(reportResult);
            }

            var report = reportResult.Data;

            // Check if report is ready
            if (report.Status != Domain.Enums.ReportStatus.Completed)
            {
                return BadRequest(new { message = "Report is not ready for download" });
            }

            // Check expiration
            if (report.ExpiresAt.HasValue && report.ExpiresAt.Value < DateTime.UtcNow)
            {
                return StatusCode(410, new { message = "Report download link has expired" });
            }

            // Get the report entity directly to access FilePath
            var dbContext = HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
            var reportEntity = await dbContext.Reports.FindAsync(new object[] { reportId });

            if (reportEntity == null || string.IsNullOrEmpty(reportEntity.FilePath))
            {
                return NotFound(new { message = "Report file not found" });
            }

            // Check if file exists on disk
            if (!System.IO.File.Exists(reportEntity.FilePath))
            {
                _logger.LogWarning("Report file not found on disk: {FilePath}", reportEntity.FilePath);
                return NotFound(new { message = "Report file not found" });
            }

            // Read file and return
            var fileBytes = await System.IO.File.ReadAllBytesAsync(reportEntity.FilePath);
            var fileName = Path.GetFileName(reportEntity.FilePath);
            var mimeType = GetMimeType(report.Format);

            return File(fileBytes, mimeType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading report: {ReportId}", reportId);
            return StatusCode(500, new { message = "Error downloading report" });
        }
    }

    /// <summary>
    /// Get available report types
    /// </summary>
    /// <returns>List of report types with metadata</returns>
    [HttpGet("types")]
    public async Task<ActionResult> GetReportTypes()
    {
        var request = new GetReportTypesQuery();
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Get report generation history
    /// </summary>
    /// <param name="request">History filter parameters</param>
    /// <returns>Paginated list of report executions</returns>
    [HttpGet("history")]
    public async Task<ActionResult> GetReportHistory([FromQuery] GetReportHistoryQuery request)
    {
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Get report analytics
    /// </summary>
    /// <returns>Report system analytics</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult> GetReportAnalytics()
    {
        var request = new GetReportAnalyticsQuery();
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    #region Report Schedules

    /// <summary>
    /// Get list of report schedules with filtering and pagination
    /// </summary>
    /// <param name="request">Schedule filter parameters</param>
    /// <returns>Paginated list of schedules</returns>
    [HttpGet("schedules")]
    public async Task<ActionResult> GetReportSchedules([FromQuery] GetReportSchedulesQuery request)
    {
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Get specific schedule by ID
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>Schedule details</returns>
    [HttpGet("schedules/{scheduleId}")]
    public async Task<ActionResult> GetReportSchedule(Guid scheduleId)
    {
        var request = new GetReportScheduleByIdQuery { ScheduleId = scheduleId };
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Create a new report schedule
    /// </summary>
    /// <param name="command">Schedule creation request</param>
    /// <returns>Created schedule details</returns>
    [HttpPost("schedules")]
    public async Task<ActionResult> CreateReportSchedule([FromBody] CreateReportScheduleCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Update an existing report schedule
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <param name="command">Schedule update request</param>
    /// <returns>Updated schedule details</returns>
    [HttpPut("schedules/{scheduleId}")]
    public async Task<ActionResult> UpdateReportSchedule(Guid scheduleId, [FromBody] UpdateReportScheduleCommand command)
    {
        if (scheduleId != command.ScheduleId)
        {
            return BadRequest(new { message = "Schedule ID mismatch" });
        }

        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Delete a report schedule
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("schedules/{scheduleId}")]
    public async Task<ActionResult> DeleteReportSchedule(Guid scheduleId)
    {
        var command = new DeleteReportScheduleCommand { ScheduleId = scheduleId };
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Pause a report schedule
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>Result</returns>
    [HttpPost("schedules/{scheduleId}/pause")]
    public async Task<ActionResult> PauseReportSchedule(Guid scheduleId)
    {
        try
        {
            var schedulerService = HttpContext.RequestServices.GetRequiredService<IReportSchedulerService>();
            var response = await schedulerService.PauseScheduleAsync(scheduleId);
            return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing schedule: {ScheduleId}", scheduleId);
            return StatusCode(500, new { message = "Error pausing schedule" });
        }
    }

    /// <summary>
    /// Resume a paused report schedule
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>Result</returns>
    [HttpPost("schedules/{scheduleId}/resume")]
    public async Task<ActionResult> ResumeReportSchedule(Guid scheduleId)
    {
        try
        {
            var schedulerService = HttpContext.RequestServices.GetRequiredService<IReportSchedulerService>();
            var response = await schedulerService.ResumeScheduleAsync(scheduleId);
            return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming schedule: {ScheduleId}", scheduleId);
            return StatusCode(500, new { message = "Error resuming schedule" });
        }
    }

    /// <summary>
    /// Execute a report schedule immediately
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>Result</returns>
    [HttpPost("schedules/{scheduleId}/execute")]
    public async Task<ActionResult> ExecuteReportSchedule(Guid scheduleId)
    {
        try
        {
            var schedulerService = HttpContext.RequestServices.GetRequiredService<IReportSchedulerService>();
            var response = await schedulerService.ExecuteScheduleNowAsync(scheduleId);
            return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing schedule: {ScheduleId}", scheduleId);
            return StatusCode(500, new { message = "Error executing schedule" });
        }
    }

    #endregion

    #region Report Subscriptions

    /// <summary>
    /// Get list of report subscriptions
    /// </summary>
    /// <param name="request">Subscription filter parameters</param>
    /// <returns>List of subscriptions</returns>
    [HttpGet("subscriptions")]
    public async Task<ActionResult> GetReportSubscriptions([FromQuery] GetReportSubscriptionsQuery request)
    {
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Create a new report subscription
    /// </summary>
    /// <param name="command">Subscription creation request</param>
    /// <returns>Created subscription details</returns>
    [HttpPost("subscriptions")]
    public async Task<ActionResult> CreateReportSubscription([FromBody] CreateReportSubscriptionCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Delete a report subscription
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("subscriptions/{subscriptionId}")]
    public async Task<ActionResult> DeleteReportSubscription(Guid subscriptionId)
    {
        var command = new DeleteReportSubscriptionCommand { SubscriptionId = subscriptionId };
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    #endregion

    #region Report Templates

    /// <summary>
    /// Get list of report templates
    /// </summary>
    /// <param name="request">Template filter parameters</param>
    /// <returns>List of templates</returns>
    [HttpGet("templates")]
    public async Task<ActionResult> GetReportTemplates([FromQuery] GetReportTemplatesQuery request)
    {
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    /// <summary>
    /// Create a new report template
    /// </summary>
    /// <param name="command">Template creation request</param>
    /// <returns>Created template details</returns>
    [HttpPost("templates")]
    public async Task<ActionResult> CreateReportTemplate([FromBody] CreateReportTemplateCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : Ok(response);
    }

    #endregion

    private string GetMimeType(Domain.Enums.ReportFormat format)
    {
        return format switch
        {
            Domain.Enums.ReportFormat.PDF => "application/pdf",
            Domain.Enums.ReportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Domain.Enums.ReportFormat.CSV => "text/csv",
            Domain.Enums.ReportFormat.JSON => "application/json",
            Domain.Enums.ReportFormat.HTML => "text/html",
            _ => "application/octet-stream"
        };
    }
}

