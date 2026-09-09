using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;

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
                // Unique session marker stored strictly inside Users.DeliveryAddress
                var sessionPrefix = $"WA:{incomingId}|";

                // 1. Look for active registration draft in Users table
                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.DeliveryAddress != null && u.DeliveryAddress.StartsWith(sessionPrefix));

                // 2. If no active draft, check for completed user by Phone
                if (user == null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == incomingId && u.IsProfileComplete);
                }

                // =========================
                // 1. CANCEL / RESET
                // =========================
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

                // =========================
                // 2. START REGISTRATION
                // =========================
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

                    // Clean up any stale incomplete drafts for this WhatsApp sender
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
                        DeliveryAddress = $"{sessionPrefix}STEP_NAME", // Track session state in Users table
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

                // =========================
                // 3. PROFILE UPDATE (AUTHENTICATED)
                // =========================
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

                // If user is not currently in an active draft session
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
                    // Silent on regular conversation chat
                    return;
                }

                // Extract current step from DeliveryAddress
                var step = user.DeliveryAddress.Substring(sessionPrefix.Length);

                // --- AUTHENTICATION CHALLENGE ---
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
                        Message = "✅ *Password verified!*\n\nLet's update your profile:\n\n📍 What is your **Full Street / Village Address**?"
                    });
                    return;
                }

                // --- 1. FULL NAME ---
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

                // --- 2. 10-DIGIT MOBILE NUMBER (WITH DUPLICATE & ZOMBIE DRAFT DETECTION) ---
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

                    // Check for repeated digits like 9999999999
                    if (new string(sanitizedPhone[0], 10) == sanitizedPhone)
                    {
                        await SendMessageAsync(new WhatsAppSendDto
                        {
                            To = rawRecipient,
                            Message = "⚠️ Please enter a real mobile number, not a repeating sequence:"
                        });
                        return;
                    }

                    // Check if another user row already exists with this phone number
                    var existingOtherUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id != user.Id && u.Phone == sanitizedPhone);

                    if (existingOtherUser != null)
                    {
                        // Case A: A completed active user already has this phone -> WARN!
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
                            // Case B: Old abandoned incomplete draft -> remove it to avoid zombie records!
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

                // --- 3. ROLE ---
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

                // --- 4. EMAIL ---
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

                // --- 5. PASSWORD ---
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

                // --- 6. PREFERRED LANGUAGE ---
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
                        Message = "📍 *Location Details*\n\nWhat is your **Street / Village / Landmark Address**?\n_(e.g. Near Bus Stand, Rampur)_"
                    });
                    return;
                }

                // --- 7. ADDRESS / LOCATION ---
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

                // --- 8. DISTRICT ---
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

                // --- 9. STATE ---
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

                // --- 10. REGION ---
                if (step == "STEP_REGION")
                {
                    // Saved cleanly to user.Region in the Users table
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

                // --- 11. PINCODE ---
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
                    else // Buyer
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

                // --- FARMER 1: PRIMARY CROPS ---
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

                // --- FARMER 2: BANK ACCOUNT ---
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

                // --- FARMER 3: IFSC CODE ---
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

                // --- FARMER 4: ACCOUNT HOLDER NAME ---
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

                // --- FARMER 5: UPI ID & COMPLETE ---
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
                    user.DeliveryAddress = null; // Clear session prefix completely
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

                // --- BUYER 1: BUSINESS NAME ---
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

                // --- BUYER 2: DELIVERY ADDRESS ---
                if (step == "STEP_BUYER_DELIVERY_ADDRESS")
                {
                    var deliv = message.Equals("skip", StringComparison.OrdinalIgnoreCase) ? user.Location : message;
                    user.Address = Truncate(deliv, 200); // Temporary storage
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

                // --- BUYER 3: GST NUMBER & COMPLETE ---
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
                    user.DeliveryAddress = user.Address; // Move actual address to DeliveryAddress
                    user.Address = null;
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

        // =========================
        // HELPER UTILITIES
        // =========================

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

        public async Task SendMessageAsync(WhatsAppSendDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("http://localhost:4000/send", request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp Send Error] {ex.Message}");
            }
        }
    }
}