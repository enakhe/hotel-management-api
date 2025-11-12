namespace HotelManagement.Domain.Enums;

/// <summary>
/// Status of a subscription
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription created but awaiting first payment
    /// </summary>
    PendingPayment = 0,

    /// <summary>
    /// In trial period
    /// </summary>
    Trial = 1,

    /// <summary>
    /// Active and paid
    /// </summary>
    Active = 2,

    /// <summary>
    /// Payment failed, grace period
    /// </summary>
    PastDue = 3,

    /// <summary>
    /// Manually suspended by admin
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Cancelled by tenant
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Subscription period ended
    /// </summary>
    Expired = 6
}

/// <summary>
/// Status of an invoice
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// Invoice created but not finalized
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Invoice finalized, awaiting payment
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Invoice sent to customer
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Customer viewed invoice
    /// </summary>
    Viewed = 3,

    /// <summary>
    /// Partial payment received
    /// </summary>
    PartiallyPaid = 4,

    /// <summary>
    /// Fully paid
    /// </summary>
    Paid = 5,

    /// <summary>
    /// Past due date
    /// </summary>
    Overdue = 6,

    /// <summary>
    /// Invoice cancelled
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// Payment refunded
    /// </summary>
    Refunded = 8
}

/// <summary>
/// Type of invoice
/// </summary>
public enum InvoiceType
{
    /// <summary>
    /// First invoice with setup fee
    /// </summary>
    Initial = 0,

    /// <summary>
    /// Regular recurring invoice
    /// </summary>
    Recurring = 1,

    /// <summary>
    /// Plan upgrade charge
    /// </summary>
    Upgrade = 2,

    /// <summary>
    /// Plan downgrade with proration
    /// </summary>
    Downgrade = 3,

    /// <summary>
    /// Usage overage charges
    /// </summary>
    Overage = 4,

    /// <summary>
    /// One-time charge
    /// </summary>
    OneTime = 5,

    /// <summary>
    /// Credit note/refund
    /// </summary>
    Credit = 6
}

/// <summary>
/// Type of invoice line item
/// </summary>
public enum LineItemType
{
    /// <summary>
    /// Base subscription fee
    /// </summary>
    Subscription = 0,

    /// <summary>
    /// Module fee
    /// </summary>
    Module = 1,

    /// <summary>
    /// One-time setup fee
    /// </summary>
    SetupFee = 2,

    /// <summary>
    /// Email overage charge
    /// </summary>
    EmailOverage = 3,

    /// <summary>
    /// SMS overage charge
    /// </summary>
    SmsOverage = 4,

    /// <summary>
    /// Storage overage charge
    /// </summary>
    StorageOverage = 5,

    /// <summary>
    /// Discount applied
    /// </summary>
    Discount = 6,

    /// <summary>
    /// Tax line item
    /// </summary>
    Tax = 7,

    /// <summary>
    /// Credit applied
    /// </summary>
    Credit = 8,

    /// <summary>
    /// Refund line item
    /// </summary>
    Refund = 9
}

/// <summary>
/// Payment method used
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Bank transfer
    /// </summary>
    BankTransfer = 0,

    /// <summary>
    /// Credit card
    /// </summary>
    CreditCard = 1,

    /// <summary>
    /// Debit card
    /// </summary>
    DebitCard = 2,

    /// <summary>
    /// Mobile money
    /// </summary>
    MobileMoney = 3,

    /// <summary>
    /// Paystack payment
    /// </summary>
    Paystack = 4,

    /// <summary>
    /// Flutterwave payment
    /// </summary>
    Flutterwave = 5,

    /// <summary>
    /// PayPal
    /// </summary>
    Paypal = 6,

    /// <summary>
    /// Stripe
    /// </summary>
    Stripe = 7,

    /// <summary>
    /// Cash payment
    /// </summary>
    Cash = 8,

    /// <summary>
    /// Check payment
    /// </summary>
    Check = 9
}

/// <summary>
/// Status of a payment
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment initiated
    /// </summary>
    Initiated = 0,

    /// <summary>
    /// Awaiting confirmation
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Being processed by gateway
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Payment authorized (not yet captured)
    /// </summary>
    Authorized = 3,

    /// <summary>
    /// Payment captured
    /// </summary>
    Captured = 4,

    /// <summary>
    /// Payment completed successfully
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 6,

    /// <summary>
    /// Payment declined by bank
    /// </summary>
    Declined = 7,

    /// <summary>
    /// Payment cancelled
    /// </summary>
    Cancelled = 8,

    /// <summary>
    /// Payment refunded
    /// </summary>
    Refunded = 9,

    /// <summary>
    /// Payment partially refunded
    /// </summary>
    PartiallyRefunded = 10
}

/// <summary>
/// Payment gateway provider
/// </summary>
public enum PaymentGateway
{
    /// <summary>
    /// Paystack
    /// </summary>
    Paystack = 0,

    /// <summary>
    /// Flutterwave
    /// </summary>
    Flutterwave = 1,

    /// <summary>
    /// Stripe
    /// </summary>
    Stripe = 2,

    /// <summary>
    /// PayPal
    /// </summary>
    Paypal = 3,

    /// <summary>
    /// Manual/offline payment
    /// </summary>
    Manual = 4
}

/// <summary>
/// Onboarding status
/// </summary>
public enum OnboardingStatus
{
    /// <summary>
    /// Onboarding started
    /// </summary>
    Started = 0,

    /// <summary>
    /// Awaiting initial payment
    /// </summary>
    AwaitingPayment = 1,

    /// <summary>
    /// Payment received
    /// </summary>
    PaymentReceived = 2,

    /// <summary>
    /// Provisioning tenant resources
    /// </summary>
    Provisioning = 3,

    /// <summary>
    /// Onboarding completed
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Onboarding failed
    /// </summary>
    Failed = 5
}

/// <summary>
/// Onboarding step
/// </summary>
public enum OnboardingStep
{
    /// <summary>
    /// Initial registration
    /// </summary>
    Registration = 0,

    /// <summary>
    /// Plan selection
    /// </summary>
    PlanSelection = 1,

    /// <summary>
    /// Customization options
    /// </summary>
    Customization = 2,

    /// <summary>
    /// Payment setup
    /// </summary>
    PaymentSetup = 3,

    /// <summary>
    /// Invoice generation
    /// </summary>
    InvoiceGeneration = 4,

    /// <summary>
    /// Payment processing
    /// </summary>
    PaymentProcessing = 5,

    /// <summary>
    /// Tenant provisioning
    /// </summary>
    Provisioning = 6,

    /// <summary>
    /// Completed
    /// </summary>
    Completed = 7
}

/// <summary>
/// Plan pricing strategy
/// </summary>
public enum PlanPricingStrategy
{
    /// <summary>
    /// Fixed price (ignore module prices)
    /// </summary>
    Fixed = 0,

    /// <summary>
    /// Sum of all module prices
    /// </summary>
    ModuleSum = 1,

    /// <summary>
    /// Sum of modules plus percentage markup
    /// </summary>
    ModuleSumWithMarkup = 2
}


