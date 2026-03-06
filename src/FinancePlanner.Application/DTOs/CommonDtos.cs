using FinancePlanner.Application.Validation;

namespace FinancePlanner.Application.DTOs;

using FinancePlanner.Core.Enums;
using System.ComponentModel.DataAnnotations;

// Auth DTOs
public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

// User DTOs
public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

// Account DTOs
public class AccountDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateAccountRequest
{
    [Required(ErrorMessage = "Account name is required")]
    [MaxLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Initial balance is required")]
    [Range(-1000000000, 1000000000, ErrorMessage = "Initial balance must be between -1,000,000,000 and 1,000,000,000")]
    public decimal InitialBalance { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [MaxLength(3, ErrorMessage = "Currency code must be 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be a valid 3-letter code (e.g., USD, EUR)")]
    public string Currency { get; set; } = "USD";
}

public class UpdateAccountRequest
{
    [Required(ErrorMessage = "Account name is required")]
    [MaxLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
    public string AccountName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

// Transaction DTOs
public class TransactionDto
{
    public Guid TransactionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public int Frequency { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Color { get; set; }

    // Set on amend successors; used by the UI to filter history view.
    public Guid? PredecessorTransactionId { get; set; }
}

[ValidDateRange]
public class CreateTransactionRequest
{
    [Required(ErrorMessage = "Description is required")]
    [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(-1000000000, 1000000000, ErrorMessage = "Amount must be between -1,000,000,000 and 1,000,000,000")]
    public decimal Amount { get; set; }

    [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Frequency is required")]
    [Range(0, 6, ErrorMessage = "Frequency must be between 0 (Once) and 6 (BiMonthly)")]
    public int Frequency { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
    public string? Color { get; set; }
}

[ValidDateRange]
public class UpdateTransactionRequest
{
    [Required(ErrorMessage = "Description is required")]
    [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(-1000000000, 1000000000, ErrorMessage = "Amount must be between -1,000,000,000 and 1,000,000,000")]
    public decimal Amount { get; set; }

    [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Frequency is required")]
    [Range(0, 6, ErrorMessage = "Frequency must be between 0 (Once) and 6 (BiMonthly)")]
    public int Frequency { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public bool IsActive { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Payload for the Amend endpoint.
/// Describes what changes on a specific effective date — all other fields
/// (frequency, description, category, color) are inherited from the original
/// unless explicitly overridden here.
/// </summary>
public class AmendTransactionRequest
{
    /// <summary>
    /// The date from which the new values take effect.
    /// The original transaction will be end-dated to EffectiveDate minus one day.
    /// Must be strictly after the original transaction's StartDate.
    /// </summary>
    [Required(ErrorMessage = "Effective date is required")]
    public DateTimeOffset EffectiveDate { get; set; }

    /// <summary>New amount. Positive = income, negative = expense.</summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(-1000000000, 1000000000, ErrorMessage = "Amount must be between -1,000,000,000 and 1,000,000,000")]
    public decimal Amount { get; set; }

    // Optional overrides — omit to inherit from the original transaction
    [MaxLength(255)] public string? Description { get; set; }
    [MaxLength(100)] public string? Category { get; set; }
    [MaxLength(7)] public string? Color { get; set; }
}

// Projection DTOs
public class TransactionWithBalanceDto
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset StartDate { get; set; } 
    public string? Category { get; set; }

    public decimal RunningBalance { get; set; }

    public decimal BalanceChange { get; set; }

    public string Type => Amount >= 0 ? "income" : "expense";

    public decimal AbsoluteAmount => Math.Abs(Amount);
    public string? Color { get; set; }
}

public class EnhancedCashFlowProjection
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }

    public decimal StartingBalance { get; set; }

    public decimal EndingBalance { get; set; }

    public decimal NetChange { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public List<DailyBalanceSnapshot> DailySnapshots { get; set; } = new();

    public List<TransactionWithBalanceDto> Transactions { get; set; } = new();
}

public class DailyBalanceSnapshot
{
    public DateTime Date { get; set; }

    public decimal EndOfDayBalance { get; set; }

    public decimal StartOfDayBalance { get; set; }

    public decimal DayChange { get; set; }

    public int TransactionCount { get; set; }

    public decimal DayIncome { get; set; }

    public decimal DayExpenses { get; set; }

    public bool HasNegativeBalance { get; set; }

    public decimal LowestBalance { get; set; }
}

public class TransactionOccurrence
{
    public Guid TransactionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime OccurrenceDate { get; set; }
    public FrequencyType Frequency { get; set; }
    public string? Color { get; set; }

    // Display properties
    public string AmountDisplay => Amount.ToString("C");
    public string CategoryDisplay => string.IsNullOrEmpty(Category) ? "Uncategorized" : Category;
}

public class EnhancedMonthlyOverview
{
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid AccountId { get; set; }

    public decimal StartingBalance { get; set; }

    public decimal EndingBalance { get; set; }

    public decimal NetChange { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal AverageDailyBalance { get; set; }

    public decimal HighestBalance { get; set; }

    public decimal LowestBalance { get; set; }

    public int DaysWithNegativeBalance { get; set; }

    public List<DailyBalanceSnapshot> DailyBreakdown { get; set; } = new();

    public List<CategoryBreakdown> CategoryBreakdowns { get; set; } = new();
}

public class CategoryBreakdown
{
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "income" or "expense"
    public decimal Total { get; set; }
    public int TransactionCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

