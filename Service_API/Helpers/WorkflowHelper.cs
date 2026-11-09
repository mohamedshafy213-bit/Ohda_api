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
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? PersonName { get; set; }
    public long? MilitaryNumber { get; set; }
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

    public static string EnrichApprovalTrailWithPersonNames(
        string? trailJson,
        Dictionary<long, string?> usersById,
        Dictionary<string, string?> usersByName)
    {
        if (string.IsNullOrWhiteSpace(trailJson))
            return trailJson ?? string.Empty;

        var list = ParseTrail(trailJson);
        if (list == null || list.Count == 0)
            return trailJson;

        bool modified = false;
        foreach (var step in list)
        {
            if (string.IsNullOrWhiteSpace(step.PersonName) || step.PersonName.Trim() == step.UserName.Trim())
            {
                if (step.UserId > 0 && usersById.TryGetValue(step.UserId, out var pNameById) && !string.IsNullOrWhiteSpace(pNameById))
                {
                    step.PersonName = pNameById;
                    modified = true;
                }
                else if (!string.IsNullOrWhiteSpace(step.UserName) && usersByName.TryGetValue(step.UserName, out var pNameByName) && !string.IsNullOrWhiteSpace(pNameByName))
                {
                    step.PersonName = pNameByName;
                    modified = true;
                }
            }
        }

        return modified ? JsonSerializer.Serialize(list) : trailJson;
    }
}
