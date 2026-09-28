using System;
using System.Collections.Generic;
using System.Text.Json;
using Entities.Models.Enums;

namespace Service_API.Helpers;

public class WorkflowStepRecord
{
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Created", "Approved", "Rejected", "Confirmed"
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public static class WorkflowHelper
{
    public static List<WorkflowStepRecord> ParseTrail(string? trailJson)
    {
        if (string.IsNullOrWhiteSpace(trailJson))
            return new List<WorkflowStepRecord>();

        try
        {
            return JsonSerializer.Deserialize<List<WorkflowStepRecord>>(trailJson) ?? new List<WorkflowStepRecord>();
        }
        catch
        {
            return new List<WorkflowStepRecord>();
        }
    }

    public static string AppendTrail(string? currentTrailJson, WorkflowStepRecord newRecord)
    {
        var list = ParseTrail(currentTrailJson);
        list.Add(newRecord);
        return JsonSerializer.Serialize(list);
    }
}
