using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Core.Infrastructure;
using Xunit;

namespace StripeBilling.Tests.Integration;

public class PaymentFlowTests
{
    private BillingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BillingDbContext(options);
    }

    [Fact]
    public async Task FullPaymentFlow_CreatesTransactionAndInvoice()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Create customer
        var customer = new Customer
        {
            Id = customerId,
            TenantId = tenantId,
            Email = "test@example.com",
            Name = "Test Customer",
            StripeCustomerId = "cus_test123",
            CreatedAt = DateTime.UtcNow
        };
        context.Customers.Add(customer);

        // Create payment transaction
        var payment = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            Amount = 99.99m,
            Currency = "usd",
            Status = "succeeded",
            PaymentMethod = "card",
            StripePaymentIntentId = "pi_flow_test",
            StripeChargeId = "ch_flow_test",
            CreatedAt = DateTime.UtcNow
        };
        context.PaymentTransactions.Add(payment);

        // Create invoice
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            StripeInvoiceId = "in_flow_test",
            AmountDue = 99.99m,
            AmountPaid = 99.99m,
            Currency = "usd",
            Status = "paid",
            CreatedAt = DateTime.UtcNow
        };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        // Verify
        var savedCustomer = await context.Customers.FindAsync(customerId);
        var savedPayment = await context.PaymentTransactions.FirstAsync(p => p.StripePaymentIntentId == "pi_flow_test");
        var savedInvoice = await context.Invoices.FirstAsync(i => i.StripeInvoiceId == "in_flow_test");

        savedCustomer.Should().NotBeNull();
        savedPayment.Amount.Should().Be(99.99m);
        savedPayment.Status.Should().Be("succeeded");
        savedInvoice.AmountPaid.Should().Be(99.99m);
        savedInvoice.Status.Should().Be("paid");
    }

    [Fact]
    public async Task RefundFlow_UpdatesTransactionAndCreatesRefundRecord()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var payment = new PaymentTransaction
        {
            Id = transactionId,
            TenantId = tenantId,
            Amount = 100.00m,
            AmountRefunded = 0,
            Currency = "usd",
            Status = "succeeded",
            StripePaymentIntentId = "pi_refund_flow",
            CreatedAt = DateTime.UtcNow
        };
        context.PaymentTransactions.Add(payment);

        var refund = new Refund
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaymentTransactionId = transactionId,
            Amount = 25.00m,
            Currency = "usd",
            Reason = "requested_by_customer",
            Status = "succeeded",
            StripeRefundId = "re_flow_test",
            ProcessedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Refunds.Add(refund);

        // Update transaction
        payment.AmountRefunded = 25.00m;
        await context.SaveChangesAsync();

        var updatedPayment = await context.PaymentTransactions.FindAsync(transactionId);
        var savedRefund = await context.Refunds.FirstAsync(r => r.PaymentTransactionId == transactionId);

        updatedPayment!.AmountRefunded.Should().Be(25.00m);
        (updatedPayment.Amount - updatedPayment.AmountRefunded).Should().Be(75.00m);
        savedRefund.Status.Should().Be("succeeded");
    }
}
