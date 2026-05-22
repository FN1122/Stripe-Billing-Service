using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Newtonsoft.Json;
using Stripe;
using Invoice = Stripe.Invoice;
using Subscription = Stripe.Subscription;

namespace Core.Services
{
    public class StripeWebhookHandler : IStripeWebhookHandler
    {
        private readonly IWebhookEventInboundRepository _eventRepo;
        private readonly IPaymentTransactionRepository _transactionRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IWebhookDispatchService _webhookDispatch;
        private readonly ITenantContextProvider _tenantContextProvider;

        public StripeWebhookHandler(IWebhookEventInboundRepository eventRepo, IPaymentTransactionRepository transactionRepo, ICustomerRepository customerRepo, ISubscriptionRepository subscriptionRepo, IInvoiceRepository invoiceRepo, IWebhookDispatchService webhookDispatch, ITenantContextProvider tenantContextProvider)
        {
            _eventRepo = eventRepo;
            _transactionRepo = transactionRepo;
            _customerRepo = customerRepo;
            _subscriptionRepo = subscriptionRepo;
            _invoiceRepo = invoiceRepo;
            _webhookDispatch = webhookDispatch;
            _tenantContextProvider = tenantContextProvider;
        }

        public async Task<GatewayResponseWrapper<bool>> ProcessAsync(Event stripeEvent, Guid tenantId)
        {
            var response = new GatewayResponseWrapper<bool>();
            try
            {
                var existingEvent = await _eventRepo.GetByStripeEventIdAsync(tenantId, stripeEvent.Id);
                if (existingEvent != null) { response.SetSuccess(true, "Event already processed (duplicate)."); return response; }

                var inboundEvent = new WebhookEventInbound { TenantId = tenantId, StripeEventId = stripeEvent.Id, EventType = stripeEvent.Type, Data = JsonConvert.SerializeObject(stripeEvent.Data), Status = "processed", ProcessedAt = DateTime.UtcNow };
                await _eventRepo.CreateAsync(inboundEvent);

                switch (stripeEvent.Type)
                {
                    case "charge.succeeded": await HandleChargeSucceeded(stripeEvent, tenantId); break;
                    case "charge.failed": await HandleChargeFailed(stripeEvent, tenantId); break;
                    case "charge.refunded": await HandleChargeRefunded(stripeEvent, tenantId); break;
                    case "charge.dispute.created": await HandleDisputeCreated(stripeEvent, tenantId); break;
                    case "payment_intent.succeeded": await HandlePaymentIntentSucceeded(stripeEvent, tenantId); break;
                    case "payment_intent.payment_failed": await HandlePaymentIntentFailed(stripeEvent, tenantId); break;
                    case "invoice.created": await HandleInvoiceCreated(stripeEvent, tenantId); break;
                    case "invoice.finalized": await HandleInvoiceFinalized(stripeEvent, tenantId); break;
                    case "invoice.payment_succeeded": await HandleInvoicePaymentSucceeded(stripeEvent, tenantId); break;
                    case "invoice.payment_failed": await HandleInvoicePaymentFailed(stripeEvent, tenantId); break;
                    case "customer.subscription.created": await HandleSubscriptionCreated(stripeEvent, tenantId); break;
                    case "customer.subscription.updated": await HandleSubscriptionUpdated(stripeEvent, tenantId); break;
                    case "customer.subscription.deleted": await HandleSubscriptionDeleted(stripeEvent, tenantId); break;
                    case "customer.subscription.trial_will_end": await HandleSubscriptionTrialWillEnd(stripeEvent, tenantId); break;
                    case "customer.created": await HandleCustomerCreated(stripeEvent, tenantId); break;
                    case "customer.updated": await HandleCustomerUpdated(stripeEvent, tenantId); break;
                    default: inboundEvent.Status = "ignored"; await _eventRepo.UpdateAsync(inboundEvent); break;
                }
                response.SetSuccess(true, "Webhook processed successfully.");
            }
            catch (Exception ex) { response.SetError($"Error processing webhook: {ex.Message}"); }
            return response;
        }

        private async Task HandleChargeSucceeded(Event stripeEvent, Guid tenantId)
        {
            try { var charge = stripeEvent.Data.Object as Charge; if (charge == null) return;
                var transaction = await _transactionRepo.GetByStripePaymentIntentIdAsync(tenantId, charge.PaymentIntentId);
                if (transaction != null) { transaction.Status = "succeeded"; transaction.StripeChargeId = charge.Id; transaction.PaymentMethod = charge.PaymentMethodDetails?.Type; transaction.PaymentMethodLast4 = charge.PaymentMethodDetails?.Card?.Last4; transaction.PaymentMethodBrand = charge.PaymentMethodDetails?.Card?.Brand; transaction.ReceiptUrl = charge.ReceiptUrl; await _transactionRepo.UpdateAsync(transaction);
                    await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.PaymentSucceeded, new { transactionId = transaction.Id, amount = transaction.Amount, stripeChargeId = charge.Id }); }
            } catch { }
        }

        private async Task HandleChargeFailed(Event stripeEvent, Guid tenantId)
        {
            try { var charge = stripeEvent.Data.Object as Charge; if (charge == null) return;
                var transaction = await _transactionRepo.GetByStripePaymentIntentIdAsync(tenantId, charge.PaymentIntentId);
                if (transaction != null) { transaction.Status = "failed"; transaction.FailureReason = charge.FailureMessage; await _transactionRepo.UpdateAsync(transaction);
                    await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.PaymentFailed, new { transactionId = transaction.Id, reason = charge.FailureMessage }); }
            } catch { }
        }

        private async Task HandleChargeRefunded(Event stripeEvent, Guid tenantId)
        {
            try { var charge = stripeEvent.Data.Object as Charge; if (charge == null || charge.AmountRefunded == 0) return;
                var transaction = await _transactionRepo.GetByStripeChargeIdAsync(tenantId, charge.Id);
                if (transaction != null) { transaction.AmountRefunded = (decimal)charge.AmountRefunded / 100; if (transaction.AmountRefunded >= transaction.Amount) transaction.Status = "refunded"; await _transactionRepo.UpdateAsync(transaction);
                    await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.RefundProcessed, new { transactionId = transaction.Id, amount = transaction.AmountRefunded }); }
            } catch { }
        }

        private async Task HandleDisputeCreated(Event stripeEvent, Guid tenantId)
        {
            try { var dispute = stripeEvent.Data.Object as Dispute; if (dispute == null) return;
                await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.DisputeCreated, new { stripeDisputeId = dispute.Id, amount = dispute.Amount, reason = dispute.Reason });
            } catch { }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent, Guid tenantId)
        {
            try { var intent = stripeEvent.Data.Object as PaymentIntent; if (intent == null) return;
                var transaction = await _transactionRepo.GetByStripePaymentIntentIdAsync(tenantId, intent.Id);
                if (transaction != null) { transaction.Status = "succeeded"; await _transactionRepo.UpdateAsync(transaction); }
            } catch { }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent, Guid tenantId)
        {
            try { var intent = stripeEvent.Data.Object as PaymentIntent; if (intent == null) return;
                var transaction = await _transactionRepo.GetByStripePaymentIntentIdAsync(tenantId, intent.Id);
                if (transaction != null) { transaction.Status = "failed"; transaction.FailureReason = intent.LastPaymentError?.Message; await _transactionRepo.UpdateAsync(transaction); }
            } catch { }
        }

        private async Task HandleInvoiceCreated(Event stripeEvent, Guid tenantId)
        {
            try { var invoice = stripeEvent.Data.Object as Invoice; if (invoice == null) return;
                var existing = await _invoiceRepo.GetByStripeInvoiceIdAsync(invoice.Id); if (existing != null) return;
                var customer = await _customerRepo.GetByStripeCustomerIdAsync(tenantId, invoice.CustomerId);
                var newInvoice = new Core.Infrastructure.Invoice { TenantId = tenantId, CustomerId = customer?.Id ?? Guid.Empty, StripeInvoiceId = invoice.Id, InvoiceNumber = invoice.Number, Subtotal = (decimal)invoice.Subtotal / 100, Tax = (decimal)(invoice.Tax ?? 0L) / 100, Total = (decimal)invoice.Total / 100, AmountPaid = (decimal)invoice.AmountPaid / 100, AmountDue = (decimal)invoice.AmountDue / 100, Currency = invoice.Currency, Status = "draft", HostedInvoiceUrl = invoice.HostedInvoiceUrl, InvoicePdfUrl = invoice.InvoicePdf };
                await _invoiceRepo.CreateAsync(newInvoice);
            } catch { }
        }

        private async Task HandleInvoiceFinalized(Event stripeEvent, Guid tenantId)
        {
            try { var invoice = stripeEvent.Data.Object as Invoice; if (invoice == null) return;
                var existing = await _invoiceRepo.GetByStripeInvoiceIdAsync(invoice.Id);
                if (existing != null) { existing.Status = "open"; existing.DueDate = invoice.DueDate; await _invoiceRepo.UpdateAsync(existing); }
            } catch { }
        }

        private async Task HandleInvoicePaymentSucceeded(Event stripeEvent, Guid tenantId)
        {
            try { var invoice = stripeEvent.Data.Object as Invoice; if (invoice == null) return;
                var existing = await _invoiceRepo.GetByStripeInvoiceIdAsync(invoice.Id);
                if (existing != null) { existing.Status = "paid"; existing.PaidAt = DateTime.UtcNow; existing.AmountPaid = (decimal)invoice.AmountPaid / 100; await _invoiceRepo.UpdateAsync(existing);
                    await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.InvoicePaid, new { invoiceId = existing.Id, stripeInvoiceId = invoice.Id, amount = existing.Total }); }
            } catch { }
        }

        private async Task HandleInvoicePaymentFailed(Event stripeEvent, Guid tenantId)
        {
            try { var invoice = stripeEvent.Data.Object as Invoice; if (invoice == null) return;
                var existing = await _invoiceRepo.GetByStripeInvoiceIdAsync(invoice.Id);
                if (existing != null) { existing.Status = "open"; await _invoiceRepo.UpdateAsync(existing);
                    await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.InvoicePaymentFailed, new { invoiceId = existing.Id, stripeInvoiceId = invoice.Id }); }
            } catch { }
        }

        private async Task HandleSubscriptionCreated(Event stripeEvent, Guid tenantId)
        {
            try { var subscription = stripeEvent.Data.Object as Subscription; if (subscription == null) return;
                var customer = await _customerRepo.GetByStripeCustomerIdAsync(tenantId, subscription.CustomerId); if (customer == null) return;
                var existing = await _subscriptionRepo.GetByStripeSubscriptionIdAsync(subscription.Id); if (existing != null) return;
                var newSub = new Core.Infrastructure.Subscription { TenantId = tenantId, CustomerId = customer.Id, StripeSubscriptionId = subscription.Id, Status = subscription.Status, Quantity = (int)(subscription.Items?.Data?.FirstOrDefault()?.Quantity ?? 1L), CurrentPeriodStart = subscription.CurrentPeriodStart, CurrentPeriodEnd = subscription.CurrentPeriodEnd, TrialEnd = subscription.TrialEnd };
                await _subscriptionRepo.CreateAsync(newSub);
                await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.SubscriptionActivated, new { subscriptionId = newSub.Id, stripeSubscriptionId = subscription.Id });
            } catch { }
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent, Guid tenantId)
        {
            try { var subscription = stripeEvent.Data.Object as Subscription; if (subscription == null) return;
                var existing = await _subscriptionRepo.GetByStripeSubscriptionIdAndTenantAsync(tenantId, subscription.Id); if (existing == null) return;
                existing.Status = subscription.Status; existing.Quantity = (int)(subscription.Items?.Data?.FirstOrDefault()?.Quantity ?? (long)existing.Quantity); existing.CurrentPeriodStart = subscription.CurrentPeriodStart; existing.CurrentPeriodEnd = subscription.CurrentPeriodEnd; existing.TrialEnd = subscription.TrialEnd; existing.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepo.UpdateAsync(existing);
                await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.SubscriptionUpdated, new { subscriptionId = existing.Id, status = subscription.Status });
            } catch { }
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent, Guid tenantId)
        {
            try { var subscription = stripeEvent.Data.Object as Subscription; if (subscription == null) return;
                var existing = await _subscriptionRepo.GetByStripeSubscriptionIdAndTenantAsync(tenantId, subscription.Id); if (existing == null) return;
                existing.Status = "canceled"; existing.CancelledAt = DateTime.UtcNow; existing.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepo.UpdateAsync(existing);
                await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.SubscriptionCancelled, new { subscriptionId = existing.Id, stripeSubscriptionId = subscription.Id });
            } catch { }
        }

        private async Task HandleSubscriptionTrialWillEnd(Event stripeEvent, Guid tenantId)
        {
            try { var subscription = stripeEvent.Data.Object as Subscription; if (subscription == null) return;
                var existing = await _subscriptionRepo.GetByStripeSubscriptionIdAndTenantAsync(tenantId, subscription.Id); if (existing == null) return;
                await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.SubscriptionTrialEnding, new { subscriptionId = existing.Id, trialEndDate = existing.TrialEnd });
            } catch { }
        }

        private async Task HandleCustomerCreated(Event stripeEvent, Guid tenantId)
        {
            try { var stripeCustomer = stripeEvent.Data.Object as Stripe.Customer; if (stripeCustomer == null) return;
                await _webhookDispatch.EnqueueAsync(tenantId, Constants.WebhookEvents.Outbound.CustomerCreated, new { stripeCustomerId = stripeCustomer.Id, email = stripeCustomer.Email, name = stripeCustomer.Name });
            } catch { }
        }

        private async Task HandleCustomerUpdated(Event stripeEvent, Guid tenantId)
        {
            try { var stripeCustomer = stripeEvent.Data.Object as Stripe.Customer; if (stripeCustomer == null) return;
                var customer = await _customerRepo.GetByStripeCustomerIdAsync(tenantId, stripeCustomer.Id);
                if (customer != null) { customer.Email = stripeCustomer.Email ?? customer.Email; customer.Name = stripeCustomer.Name ?? customer.Name; customer.Phone = stripeCustomer.Phone ?? customer.Phone; customer.UpdatedAt = DateTime.UtcNow; await _customerRepo.UpdateAsync(customer); }
            } catch { }
        }
    }
}
