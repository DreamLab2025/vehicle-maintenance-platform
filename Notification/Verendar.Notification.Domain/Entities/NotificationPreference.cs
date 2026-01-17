using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

namespace Verendar.Notification.Domain.Entities;

[Index(nameof(UserId), IsUnique = true)]
public class NotificationPreference : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberVerified { get; set; } = false;

    [MaxLength(256)]
    public string? Email { get; set; }
    public bool EmailVerified { get; set; } = false;

    public bool InAppEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;

    public bool SmsForHighPriorityOnly { get; set; } = true;
}