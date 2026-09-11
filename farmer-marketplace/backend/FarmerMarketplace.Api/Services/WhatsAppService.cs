// backend/FarmerMarketplace.Api/Services/WhatsAppService.cs
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FarmerMarketplace.Api.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private const string GatewayUrl = "http://localhost:4000/send";

        // Regex Validators
        private static readonly Regex IndianMobileRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);
        private static readonly Regex PincodeRegex = new(@"^[1-9][0-9]{5}$", RegexOptions.Compiled);
        private static readonly Regex IfscRegex = new(@"^[A-Z]{4}0[A-Z0-9]{6}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex UpiRegex = new(@"^[a-zA-Z0-9.\-_]{2,256}@[a-zA-Z]{2,64}$", RegexOptions.Compiled);
        private static readonly Regex GstRegex = new(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public WhatsAppService(
            HttpClient httpClient,
            AppDbContext context,
            PasswordHasher passwordHasher)
        {
            _httpClient = httpClient;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // =========================================================================
        // 1. ORDER & PAYMENT NOTIFICATIONS (STATUS, PAYMENT & DELIVERY ALERTS)
        // =========================================================================

        public async Task NotifyOrderStatusChangeAsync(Order order, OrderStatus newStatus)
        {
            if (order.Buyer == null || string.IsNullOrWhiteSpace(order.Buyer.Phone))
                return;

            var statusEmoji = newStatus switch
            {
                OrderStatus.Confirmed => "✅",
                OrderStatus.InTransit => "🚚",
                OrderStatus.Delivered => "🎉",
                OrderStatus.Cancelled => "❌",
                _ => "ℹ️"
            };

            var orderCode = order.Id.ToString()[..8].ToUpper();
            var sb = new StringBuilder();
            sb.AppendLine("🔔 *Order Status Update - FarmerMarketplace*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"Namaste, *{order.Buyer.Name}*! 🙏");
            sb.AppendLine($"Your order *#{orderCode}* status is now:");
            sb.AppendLine($"{statusEmoji} *{newStatus}*\n");

            if (newStatus == OrderStatus.Confirmed)
            {
                sb.AppendLine("Farmers have confirmed your order and started packing your produce.");
            }
            else if (newStatus == OrderStatus.InTransit)
            {
                sb.AppendLine("Your fresh produce is out for delivery! 🚛");
                if (order.VehicleNumber.HasValue)
                    sb.AppendLine($"• *Vehicle Route #:* {order.VehicleNumber}");
                if (order.EstimatedArrival.HasValue)
                    sb.AppendLine($"• *Estimated Arrival:* {order.EstimatedArrival.Value:dd MMM, hh:mm tt} UTC");
            }
            else if (newStatus == OrderStatus.Delivered)
            {
                sb.AppendLine("Your order has arrived. Enjoy your farm-fresh harvest! 🥗");
            }
            else if (newStatus == OrderStatus.Cancelled)
            {
                sb.AppendLine("Your order has been cancelled.");
            }

            sb.AppendLine("\nThank you for choosing FarmerMarketplace!");

            await SendMessageAsync(order.Buyer.Phone, sb.ToString());
        }

        public async Task NotifyDeliveryDispatchedAsync(Order order, string? vehicleNumber, DateTime? estimatedArrival)
        {
            if (order.Buyer == null || string.IsNullOrWhiteSpace(order.Buyer.Phone))
                return;

            var orderCode = order.Id.ToString()[..8].ToUpper();
            var sb = new StringBuilder();
            sb.AppendLine("🚛 *Your Order is Out for Delivery!*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"Namaste, *{order.Buyer.Name}*! 🙏");
            sb.AppendLine($"Your fresh farm order *#{orderCode}* has been dispatched on our delivery vehicle.");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(vehicleNumber))
                sb.AppendLine($"• *Vehicle:* {vehicleNumber}");
            if (estimatedArrival.HasValue)
                sb.AppendLine($"• *Estimated Arrival:* {estimatedArrival.Value:dd MMM, hh:mm tt} UTC");
            if (!string.IsNullOrWhiteSpace(order.DeliveryAddress))
                sb.AppendLine($"• *Destination:* {order.DeliveryAddress}");
            sb.AppendLine();
            sb.AppendLine("Please ensure someone is available at the delivery location to receive the fresh harvest.");
            sb.AppendLine("\n🌾 *FarmerMarketplace Logistics*");

            await SendMessageAsync(order.Buyer.Phone, sb.ToString());
        }

        public async Task NotifyDeliveryDispatchedAsync(Order order, int? vehicleNumber, DateTime? estimatedArrival)
        {
            await NotifyDeliveryDispatchedAsync(order, vehicleNumber?.ToString(), estimatedArrival);
        }

        public async Task NotifyPaymentCapturedAsync(Order order, decimal amount, string paymentId)
        {
            if (order.Buyer == null || string.IsNullOrWhiteSpace(order.Buyer.Phone))
                return;

            var orderCode = order.Id.ToString()[..8].ToUpper();
            var sb = new StringBuilder();
            sb.AppendLine("💳 *Payment Received! - FarmerMarketplace*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"Namaste, *{order.Buyer.Name}*! 🙏");
            sb.AppendLine($"We have received your payment of *₹{amount:N2}* for Order *#{orderCode}*.");
            sb.AppendLine($"• *Payment ID:* {paymentId}");
            sb.AppendLine("• *Status:* Confirmed ✅");
            sb.AppendLine($"• *Fulfillment:* {order.DeliveryType}\n");
            sb.AppendLine("Farmers have been notified to prepare your fresh produce for dispatch! 🌾");

            await SendMessageAsync(order.Buyer.Phone, sb.ToString());
        }

        // =========================================================================
        // 2. INCOMING WEBHOOK HANDLER (CHATBOT QUESTIONNAIRE & PROFILE STATE MACHINE)
        // =========================================================================

        public async Task ReceiveMessageAsync(WhatsAppIncomingDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Message))
                return;

            var rawRecipient = request.PhoneNumber.Trim();
            var incomingId = CleanSenderId(rawRecipient);
            var message = request.Message.Trim();

            Console.WriteLine($"[WhatsApp] {request.ContactName} ({incomingId}): {message}");

            try
            {
                var sessionPrefix = $"WA:{incomingId}|";

                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.DeliveryAddress != null && u.DeliveryAddress.StartsWith(sessionPrefix));

                if (user == null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == incomingId && u.IsProfileComplete);
                }

                if (message.Equals("cancel", StringComparison.OrdinalIgnoreCase) ||
                    message.Equals("reset", StringComparison.OrdinalIgnoreCase))
                {
                    if (user != null && !user.IsProfileComplete)
                    {
                        _context.Users.Remove(user);
                        await _context.SaveChangesAsync();
                    }

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "🛑 Registration cancelled. Type *register* whenever you are ready to start again!"
                    });
                    return;
                }

                if (message.Equals("register", StringComparison.OrdinalIgnoreCase))
                {
                    if (user != null && user.IsProfileComplete)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = $"Welcome back, *{user.Name}*! Your account is already registered with mobile number *{user.Phone}*.\n\nType *profile* to update your details."
                        });
                        return;
                    }

                    var oldDrafts = await _context.Users
                        .Where(u => !u.IsProfileComplete && u.DeliveryAddress != null && u.DeliveryAddress.StartsWith(sessionPrefix))
                        .ToListAsync();

                    if (oldDrafts.Any())
                    {
                        _context.Users.RemoveRange(oldDrafts);
                        await _context.SaveChangesAsync();
                    }

                    user = new User
                    {
                        Phone = Truncate(incomingId, 15),
                        Name = string.Empty,
                        Role = UserRole.Farmer,
                        PasswordHash = string.Empty,
                        IsProfileComplete = false,
                        PreferredLanguage = "en",
                        DeliveryAddress = $"{sessionPrefix}STEP_NAME",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(user);

                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "🌾 *Welcome to FarmerMarketplace!*\n\nWhat is your **Full Name**?\n_(Type *cancel* at any time to abort)_"
                    });
                    return;
                }

                if (message.Equals("profile", StringComparison.OrdinalIgnoreCase))
                {
                    if (user == null || !user.IsProfileComplete)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "You do not have a completed account yet! Type *register* to create one."
                        });
                        return;
                    }

                    user.DeliveryAddress = $"{sessionPrefix}AUTH_CHALLENGE";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "🔒 *Security Verification*\n\nPlease enter your account password to authorize updating your profile:"
                    });
                    return;
                }

                if (user == null || string.IsNullOrEmpty(user.DeliveryAddress) || !user.DeliveryAddress.StartsWith(sessionPrefix))
                {
                    if (IsGreeting(message))
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "🌾 *Welcome to FarmerMarketplace!*\n\n• Type *register* to create a new account.\n• Type *profile* to update your existing profile."
                        });
                    }
                    return;
                }

                var step = user.DeliveryAddress.Substring(sessionPrefix.Length);

                if (step == "AUTH_CHALLENGE")
                {
                    var isAuthorized = _passwordHasher.VerifyPassword(message, user.PasswordHash);
                    if (!isAuthorized)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "❌ Incorrect password. Access denied.\n\nPlease enter your correct password, or type *cancel*:"
                        });
                        return;
                    }

                    user.DeliveryAddress = $"{sessionPrefix}STEP_ADDRESS";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "✅ *Password verified!*\n\nLet's update your profile:\n\n📍 What is your **Full Street / Location Address**?"
                    });
                    return;
                }

                if (step == "STEP_NAME")
                {
                    if (message.Length < 2 || message.Length > 100)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Please enter a valid name (between 2 and 100 characters):"
                        });
                        return;
                    }

                    user.Name = Truncate(message, 100);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_PHONE";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = $"Nice to meet you, *{user.Name}*!\n\n📱 What is your **10-digit Mobile Number**?\n_(e.g. 9876543210 - this will be your account login)_"
                    });
                    return;
                }

                if (step == "STEP_PHONE")
                {
                    var sanitizedPhone = ExtractTenDigitPhone(message);

                    if (string.IsNullOrEmpty(sanitizedPhone) || !IndianMobileRegex.IsMatch(sanitizedPhone))
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Invalid mobile number. Please enter a valid **10-digit Indian mobile number** starting with 6, 7, 8, or 9 (e.g. 9876543210):"
                        });
                        return;
                    }

                    if (sanitizedPhone.Distinct().Count() == 1)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Please enter a real mobile number, not a repeating sequence:"
                        });
                        return;
                    }

                    var existingOtherUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id != user.Id && u.Phone == sanitizedPhone);

                    if (existingOtherUser != null)
                    {
                        if (existingOtherUser.IsProfileComplete)
                        {
                            await SendMessageAsync(new WhatsAppSendDto
                            {
                                To = rawRecipient,
                                Message = $"⚠️ An active account with mobile number *{sanitizedPhone}* already exists!\n\nPlease enter a different mobile number, or type *cancel*:"
                            });
                            return;
                        }
                        else
                        {
                            _context.Users.Remove(existingOtherUser);
                            await _context.SaveChangesAsync();
                        }
                    }

                    user.Phone = sanitizedPhone;
                    user.DeliveryAddress = $"{sessionPrefix}STEP_ROLE";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = $"Mobile number *{user.Phone}* recorded! ✅\n\nSelect your role:\n*1.* Farmer 🚜\n*2.* FPO Admin 🏢\n*3.* Buyer 🛒\n\nReply with *1*, *2*, or *3*:"
                    });
                    return;
                }

                if (step == "STEP_ROLE")
                {
                    var lower = message.ToLower();
                    if (lower == "1" || lower.Contains("farmer") || lower.Contains("kisan"))
                    {
                        user.Role = UserRole.Farmer;
                    }
                    else if (lower == "2" || lower.Contains("fpo") || lower.Contains("admin"))
                    {
                        user.Role = UserRole.FpoAdmin;
                    }
                    else if (lower == "3" || lower.Contains("buyer") || lower.Contains("vyapari"))
                    {
                        user.Role = UserRole.Buyer;
                    }
                    else
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Please reply with:\n*1* for Farmer 🚜\n*2* for FPO Admin 🏢\n*3* for Buyer 🛒"
                        });
                        return;
                    }

                    user.DeliveryAddress = $"{sessionPrefix}STEP_EMAIL";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your **Email address**?\n\nType *skip* if you do not want to provide an email."
                    });
                    return;
                }

                if (step == "STEP_EMAIL")
                {
                    if (message.Equals("skip", StringComparison.OrdinalIgnoreCase))
                    {
                        user.Email = null;
                    }
                    else
                    {
                        if (!EmailRegex.IsMatch(message) || message.Length > 100)
                        {
                            await SendMessageAsync(new WhatsAppSendDto
                            {
                                To = rawRecipient,
                                Message = "⚠️ Please enter a valid email address (e.g. name@example.com), or type *skip*:"
                            });
                            return;
                        }

                        var emailLower = message.ToLower();
                        var emailTaken = await _context.Users.AnyAsync(u => u.Id != user.Id && u.Email != null && u.Email.ToLower() == emailLower);
                        if (emailTaken)
                        {
                            await SendMessageAsync(new WhatsAppSendDto
                            {
                                To = rawRecipient,
                                Message = "⚠️ This email is already registered. Enter a different email or type *skip*:"
                            });
                            return;
                        }

                        user.Email = emailLower;
                    }

                    user.DeliveryAddress = $"{sessionPrefix}STEP_PASSWORD";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "Please choose a **Password** for your web login:\n_(Must be at least 6 characters long)_"
                    });
                    return;
                }

                if (step == "STEP_PASSWORD")
                {
                    if (message.Length < 6)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Password must be at least 6 characters long.\n\nPlease enter your password again:"
                        });
                        return;
                    }

                    user.PasswordHash = _passwordHasher.HashPassword(message);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_LANGUAGE";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your preferred language?\n\n*en* - English\n*hi* - हिन्दी\n*bn* - বাংলা\n*mr* - मराठी\n\nType *skip* for English."
                    });
                    return;
                }

                if (step == "STEP_LANGUAGE")
                {
                    var lang = "en";
                    var lower = message.ToLower();

                    if (lower.Contains("hi")) lang = "hi";
                    else if (lower.Contains("bn") || lower.Contains("bang")) lang = "bn";
                    else if (lower.Contains("mr")) lang = "mr";

                    user.PreferredLanguage = Truncate(lang, 10);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_ADDRESS";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "📍 *Location Details*\n\nWhat is your **Street / Location / Landmark Address**?\n_(e.g. Near Bus Stand, Rampur)_"
                    });
                    return;
                }

                if (step == "STEP_ADDRESS")
                {
                    user.Location = message.Equals("skip", StringComparison.OrdinalIgnoreCase) ? null : Truncate(message, 200);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_DISTRICT";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your **District**? (e.g. Pune, Jaipur, Kolkata)"
                    });
                    return;
                }

                if (step == "STEP_DISTRICT")
                {
                    user.District = Truncate(message, 100);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_STATE";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your **State**?\n_(e.g. Punjab, Maharashtra, West Bengal, Uttar Pradesh)_"
                    });
                    return;
                }

                if (step == "STEP_STATE")
                {
                    user.State = Truncate(message, 100);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_REGION";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your **Region / Zone** (e.g. North, South, Malwa)?\n_(Type *skip* to pass)_"
                    });
                    return;
                }

                if (step == "STEP_REGION")
                {
                    user.Region = message.Equals("skip", StringComparison.OrdinalIgnoreCase) ? null : Truncate(message, 200);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_PINCODE";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your **6-digit Pincode**? (e.g. 700006)"
                    });
                    return;
                }

                if (step == "STEP_PINCODE")
                {
                    var cleanPin = new string(message.Where(char.IsDigit).ToArray());

                    if (!PincodeRegex.IsMatch(cleanPin))
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Invalid Pincode. Please enter a valid **6-digit Pincode** (e.g. 700006):"
                        });
                        return;
                    }

                    user.Pincode = cleanPin;

                    if (user.Role == UserRole.Farmer || user.Role == UserRole.FpoAdmin)
                    {
                        user.DeliveryAddress = $"{sessionPrefix}STEP_CROPS";
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "🌾 What are your **Primary Crops**?\n_(e.g. Wheat, Tomato, Potato - comma separated, or type *skip*)_"
                        });
                    }
                    else
                    {
                        user.DeliveryAddress = $"{sessionPrefix}STEP_BUSINESS_NAME";
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "🏢 What is your **Business or Company Name**?\n_(Type *skip* if none)_"
                        });
                    }
                    return;
                }

                // ==========================================
                // FARMER / FPO ADMIN FLOW
                // ==========================================

                if (step == "STEP_CROPS")
                {
                    user.PrimaryCrops = message.Equals("skip", StringComparison.OrdinalIgnoreCase) ? null : Truncate(message, 500);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_BANK_ACCOUNT";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "🏦 *Banking Details*\n\nWhat is your **Bank Account Number**?\n_(Type *skip* to add later)_"
                    });
                    return;
                }

                if (step == "STEP_BANK_ACCOUNT")
                {
                    if (message.Equals("skip", StringComparison.OrdinalIgnoreCase))
                    {
                        user.BankAccountNumber = null;
                        user.DeliveryAddress = $"{sessionPrefix}STEP_UPI";
                        user.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "💳 What is your **UPI ID** for instant payouts?\n_(e.g. 9876543210@upi, or type *skip*)_"
                        });
                        return;
                    }

                    var digitsOnly = new string(message.Where(char.IsDigit).ToArray());
                    if (digitsOnly.Length < 8 || digitsOnly.Length > 20)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Invalid account number. Enter 8 to 20 digits, or type *skip*:"
                        });
                        return;
                    }

                    user.BankAccountNumber = digitsOnly;
                    user.DeliveryAddress = $"{sessionPrefix}STEP_BANK_IFSC";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is your bank **IFSC Code**? (e.g. SBIN0001234)"
                    });
                    return;
                }

                if (step == "STEP_BANK_IFSC")
                {
                    var cleanIfsc = message.Replace(" ", "").ToUpper();

                    if (!IfscRegex.IsMatch(cleanIfsc))
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Invalid IFSC Code. Example: SBIN0001234, HDFC0000123:"
                        });
                        return;
                    }

                    user.BankIfsc = cleanIfsc;
                    user.DeliveryAddress = $"{sessionPrefix}STEP_ACCOUNT_HOLDER";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "What is the **Account Holder Name** on the bank account?"
                    });
                    return;
                }

                if (step == "STEP_ACCOUNT_HOLDER")
                {
                    user.AccountHolderName = Truncate(message, 100);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_UPI";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "💳 What is your **UPI ID** for instant payouts?\n_(e.g. 9876543210@upi, or type *skip*)_"
                    });
                    return;
                }

                if (step == "STEP_UPI")
                {
                    if (!message.Equals("skip", StringComparison.OrdinalIgnoreCase))
                    {
                        var cleanUpi = message.Trim().ToLower();
                        if (!UpiRegex.IsMatch(cleanUpi))
                        {
                            await SendMessageAsync(new WhatsAppSendDto
                            {
                                To = rawRecipient,
                                Message = "⚠️ Invalid UPI ID. Example: 9876543210@upi or name@okhdfcbank. Re-enter or type *skip*:"
                            });
                            return;
                        }
                        user.UpiId = Truncate(cleanUpi, 50);
                    }

                    user.IsProfileComplete = true;
                    user.DeliveryAddress = null;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message =
                            "🌟 *Registration & Profile Complete!*\n\n" +
                            "Your account is officially active on FarmerMarketplace:\n\n" +
                            $"• *Name:* {user.Name}\n" +
                            $"• *Mobile Number:* {user.Phone} 📱\n" +
                            $"• *Role:* {user.Role} 🚜\n" +
                            $"• *Location:* {user.District}, {user.State} ({user.Pincode})\n" +
                            (string.IsNullOrWhiteSpace(user.PrimaryCrops) ? "" : $"• *Crops:* {user.PrimaryCrops}\n") +
                            (string.IsNullOrWhiteSpace(user.BankIfsc) ? "" : $"• *IFSC:* {user.BankIfsc}\n") +
                            (string.IsNullOrWhiteSpace(user.UpiId) ? "" : $"• *UPI:* {user.UpiId}\n") +
                            "\n🎉 You can now log in on the website using your mobile number and password!"
                    });
                    return;
                }

                // ==========================================
                // BUYER FLOW
                // ==========================================

                if (step == "STEP_BUSINESS_NAME")
                {
                    user.BusinessName = message.Equals("skip", StringComparison.OrdinalIgnoreCase) ? null : Truncate(message, 150);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_BUYER_DELIVERY_ADDRESS";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "📦 What is your **Delivery Address** for orders?\n_(Type *skip* if same as location address)_"
                    });
                    return;
                }

                if (step == "STEP_BUYER_DELIVERY_ADDRESS")
                {
                    var deliv = message.Equals("skip", StringComparison.OrdinalIgnoreCase) ? user.Location : message;
                    user.Address = Truncate(deliv, 200);
                    user.DeliveryAddress = $"{sessionPrefix}STEP_GST_NUMBER";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message = "🧾 What is your **15-digit GST Number**?\n_(Type *skip* if not registered)_"
                    });
                    return;
                }

                if (step == "STEP_GST_NUMBER")
                {
                    if (!message.Equals("skip", StringComparison.OrdinalIgnoreCase))
                    {
                        var cleanGst = message.Replace(" ", "").ToUpper();
                        if (!GstRegex.IsMatch(cleanGst))
                        {
                            await SendMessageAsync(new WhatsAppSendDto
                            {
                                To = rawRecipient,
                                Message = "⚠️ Invalid GST format. Please enter a valid 15-character GSTIN or type *skip*:"
                            });
                            return;
                        }
                        user.GstNumber = cleanGst;
                    }

                    user.IsProfileComplete = true;
                    user.DeliveryAddress = user.Address;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await SendMessageAsync(new WhatsAppSendDto
                    {
                        To = rawRecipient,
                        Message =
                            "🌟 *Registration & Profile Complete!*\n\n" +
                            "Your Buyer account is active on FarmerMarketplace:\n\n" +
                            $"• *Name:* {user.Name}\n" +
                            $"• *Mobile Number:* {user.Phone} 📱\n" +
                            $"• *Role:* Buyer 🛒\n" +
                            $"• *Location:* {user.District}, {user.State}\n" +
                            (string.IsNullOrWhiteSpace(user.BusinessName) ? "" : $"• *Business:* {user.BusinessName}\n") +
                            (string.IsNullOrWhiteSpace(user.GstNumber) ? "" : $"• *GST:* {user.GstNumber}\n") +
                            "\n🎉 You can now log in on the website using your mobile number and password!"
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"[WhatsApp Error Exception] {errorMsg}");

                await SendMessageAsync(new WhatsAppSendDto
                {
                    To = rawRecipient,
                    Message = $"⚠️ There was an issue processing your input: {errorMsg}\n\nPlease reply with your answer again, or type *cancel* to restart."
                });
            }
        }

        // =========================================================================
        // 3. DISPATCH & FORMATTING UTILITIES
        // =========================================================================

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
                return false;

            try
            {
                var cleanDigits = new string(phoneNumber.Where(char.IsDigit).ToArray());
                if (cleanDigits.Length == 10) 
                    cleanDigits = "91" + cleanDigits;

                var recipientJid = cleanDigits.Contains("@") ? cleanDigits : $"{cleanDigits}@s.whatsapp.net";

                await SendMessageAsync(new WhatsAppSendDto
                {
                    To = recipientJid,
                    Message = message
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp Gateway Error] Failed to send message to {phoneNumber}: {ex.Message}");
                return false;
            }
        }

        public async Task SendMessageAsync(WhatsAppSendDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(GatewayUrl, request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp Send Error] {ex.Message}");
            }
        }

        private static bool IsGreeting(string msg)
        {
            var lower = msg.ToLower();
            return lower == "hi" || lower == "hello" || lower == "hey" || lower == "help" || lower == "namaste";
        }

        private static string ExtractTenDigitPhone(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var digits = new string(input.Where(char.IsDigit).ToArray());

            if (digits.Length == 12 && digits.StartsWith("91"))
            {
                digits = digits.Substring(2);
            }
            else if (digits.Length == 11 && digits.StartsWith("0"))
            {
                digits = digits.Substring(1);
            }

            return digits.Length == 10 ? digits : string.Empty;
        }

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string CleanSenderId(string rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone)) return string.Empty;
            return rawPhone.Split('@')[0].Trim();
        }
    }
}