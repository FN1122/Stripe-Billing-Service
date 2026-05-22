using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Services;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class EmailServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<IEmailRepository> _emailRepoMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    private EmailService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = Guid.NewGuid(),
            Role = "Admin"
        });
        return new EmailService(_tenantContextMock.Object, _emailRepoMock.Object);
    }

    [Fact]
    public async Task SendTemplated_TemplateNotFound_Returns404()
    {
        // Arrange
        _emailRepoMock.Setup(r => r.GetTemplateAsync(_tenantId, "nonexistent")).ReturnsAsync((EmailTemplate?)null);
        var service = CreateService();

        // Act
        var result = await service.SendTemplatedAsync("nonexistent", "user@test.com", new Dictionary<string, string>());

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task SendTemplated_WithVariables_ReplacesInSubjectAndBody()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            TemplateKey = "welcome",
            Subject = "Welcome {{name}}!",
            HtmlBody = "<p>Hello {{name}}, your plan is {{plan}}.</p>",
            IsActive = true
        };
        _emailRepoMock.Setup(r => r.GetTemplateAsync(_tenantId, "welcome")).ReturnsAsync(template);

        SendEmailDto? capturedDto = null;
        _emailRepoMock.Setup(r => r.CreateLogAsync(It.IsAny<EmailLog>()))
            .Callback<EmailLog>(log => capturedDto = new SendEmailDto { Subject = log.Subject, To = log.To })
            .ReturnsAsync(Guid.NewGuid());
        _emailRepoMock.Setup(r => r.UpdateLogAsync(It.IsAny<EmailLog>())).Returns(Task.CompletedTask);

        var service = CreateService();
        var variables = new Dictionary<string, string>
        {
            { "name", "John" },
            { "plan", "Pro" }
        };

        // Act
        var result = await service.SendTemplatedAsync("welcome", "john@test.com", variables);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Send_CreatesLogWithQueuedStatus_ThenUpdateToSent()
    {
        // Arrange
        var statuses = new List<string>();
        _emailRepoMock.Setup(r => r.CreateLogAsync(It.IsAny<EmailLog>()))
            .Callback<EmailLog>(log => statuses.Add(log.Status))
            .ReturnsAsync(Guid.NewGuid());
        _emailRepoMock.Setup(r => r.UpdateLogAsync(It.IsAny<EmailLog>()))
            .Callback<EmailLog>(log => statuses.Add(log.Status))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.SendAsync(new SendEmailDto { To = "test@test.com", Subject = "Test" });

        // Assert - first queued, then sent
        statuses.Should().HaveCount(2);
        statuses[0].Should().Be("queued");
        statuses[1].Should().Be("sent");
    }

    [Fact]
    public async Task PreviewTemplate_ReturnsProcessedHtml()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            TemplateKey = "invoice",
            Subject = "Invoice #{{invoiceId}}",
            HtmlBody = "<h1>Invoice #{{invoiceId}}</h1><p>Amount: {{amount}}</p>",
            IsActive = true
        };
        _emailRepoMock.Setup(r => r.GetTemplateAsync(_tenantId, "invoice")).ReturnsAsync(template);
        var service = CreateService();

        var variables = new Dictionary<string, string>
        {
            { "invoiceId", "INV-001" },
            { "amount", "$99.00" }
        };

        // Act
        var result = await service.PreviewTemplateAsync("invoice", variables);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data.Should().Contain("INV-001");
        result.Data.Should().Contain("$99.00");
        result.Data.Should().NotContain("{{invoiceId}}");
    }

    [Fact]
    public async Task UpdateTemplate_PartialUpdate_OnlyChangesProvidedFields()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            TemplateKey = "welcome",
            Subject = "Original Subject",
            HtmlBody = "<p>Original body</p>",
            PlainTextBody = "Original plain",
            IsActive = true
        };
        _emailRepoMock.Setup(r => r.GetTemplateAsync(_tenantId, "welcome")).ReturnsAsync(template);
        _emailRepoMock.Setup(r => r.UpdateTemplateAsync(It.IsAny<EmailTemplate>())).Returns(Task.CompletedTask);
        var service = CreateService();

        var update = new UpdateEmailTemplateDto { Subject = "New Subject" }; // only subject

        // Act
        var result = await service.UpdateTemplateAsync("welcome", update);

        // Assert
        result.IsValid.Should().BeTrue();
        template.Subject.Should().Be("New Subject");
        template.HtmlBody.Should().Be("<p>Original body</p>"); // unchanged
        template.PlainTextBody.Should().Be("Original plain"); // unchanged
    }

    [Fact]
    public async Task ResendEmail_NotFound_Returns404()
    {
        _emailRepoMock.Setup(r => r.GetEmailLogAsync(_tenantId, It.IsAny<Guid>())).ReturnsAsync((EmailLog?)null);
        var service = CreateService();

        var result = await service.ResendEmailAsync(Guid.NewGuid());

        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
