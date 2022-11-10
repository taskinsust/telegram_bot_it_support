
using Newtonsoft.Json;
using System.Threading;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.SCL.Auth;
using Telegram.Bot.SCL.Auth.Configurations;
using Telegram.Bot.SCL.Auth.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ITelegramAuthentication _telegramAuthentication;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ITelegramBotClient botClient, ITelegramAuthentication telegramAuthentication, ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _telegramAuthentication = telegramAuthentication;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        await UserRegPlusCredentialCheck(_botClient, message);

        var action = messageText.Split(' ')[0] switch
        {
            //"/inline_keyboard" => SendInlineKeyboard(_botClient, message, cancellationToken),
            //"/keyboard" => SendReplyKeyboard(_botClient, message, cancellationToken),
            //"/remove" => RemoveKeyboard(_botClient, message, cancellationToken),
            //"/photo" => SendFile(_botClient, message, cancellationToken),
            //"/request" => RequestContactAndLocation(_botClient, message, cancellationToken),
            //"/inline_mode" => StartInlineQuery(_botClient, message, cancellationToken),
            //"/throw" => FailingHandler(_botClient, message, cancellationToken),
            //_ => Usage(_botClient, message, cancellationToken)

            "/Menu" => SupportList(_botClient, message, cancellationToken),
            "/Email" => EmailSupport(_botClient, message),
            "/Network" => NetworkSupport(_botClient, message),
            "/Hardware" => HardwareSupport(_botClient, message),
            "/Support" => Support(_botClient, message),
            "/RequestEmail" => RequestEmailRequest(_botClient, message),
            "/RequestContact" => RequestContact(_botClient, message),
            "/throw" => FailingHandler(_botClient, message, cancellationToken),
            _ => Usage(_botClient, message, cancellationToken)
        };
        //Message sentMessage = await action;
        //_logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            // Simulate longer running task
            await Task.Delay(500, cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Removing keyboard",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendFile(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id,
                ChatAction.UploadPhoto,
                cancellationToken: cancellationToken);

            const string filePath = @"Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "Nice Picture",
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup RequestReplyKeyboard = new(
                new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Who or Where are you?",
                replyMarkup: RequestReplyKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            const string usage = "Usage:\n" +
                                "/Menu - Support List";
            //"/Email - email support\n" +
            //"/Network    - network support\n" +
            //"/Hardware   - hardware support\n" +
            //"/Support    - other support\n" +
            //"/RequestEmail   - send email address\n" +
            //"/RequestContact - send contact number";

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> StartInlineQuery(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Press the button to start Inline Query",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static Task<Message> FailingHandler(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
    }

    private async Task UserRegPlusCredentialCheck(ITelegramBotClient botClient, Message message)
    {
        if (message.Contact is not null)
        {
            //update contact information based on telegram id
            var updateResult = _telegramAuthentication.UpdateUserCredential(message.From.FirstName, message.From.LastName, message.From.Username, (long)message.Contact.UserId, message.Contact.PhoneNumber);
            var responseResult = JsonConvert.DeserializeObject<ResponseResult>(updateResult);
            if (responseResult.Message == ErrorMessages.WAITING_FOR_EMAIL_REQUEST)
                await RequestEmailRequest(_botClient, message);
            return;
        }

        var result = _telegramAuthentication.CheckUserCredential(message.From.IsBot, message.From.FirstName, message.From.LastName, message.From.Username, message.From.Id);
        var response = JsonConvert.DeserializeObject<ResponseResult>(result);
        if (!String.IsNullOrEmpty(message.Text) &&
            (message.Text.ToLower().Contains("@summitcommunications.net") ||
            message.Text.ToLower().Contains("@rsl-service.com") ||
            message.Text.ToLower().Contains("@summit-towers.net") ||
            message.Text.ToLower().Contains("@summit-centre.com")))
        {
            //Update User Profile Email Address
            _telegramAuthentication.UpdateUserProfileEmail(message.From.IsBot, message.From.FirstName, message.From.LastName, message.From.Username, message.From.Id, message.Text);

            //Update Identity User Email
            _telegramAuthentication.UpdateEmail(message.From.IsBot, message.From.FirstName, message.From.LastName, message.From.Username, message.From.Id, message.Text);
            result = _telegramAuthentication.CheckUserCredential(message.From.IsBot, message.From.FirstName, message.From.LastName, message.From.Username, message.From.Id);
            response = JsonConvert.DeserializeObject<ResponseResult>(result);
        }
        if (!response.IsSuccess)
        {
            if (response.Message.Equals(ErrorMessages.FIRST_TIME_USER_REQUEST))
            {
                await RequestContact(_botClient, message);
                return;
            }

            else if (response.Message.Equals(ErrorMessages.WAITING_FOR_EMAIL_REQUEST))
            {
                await RequestEmailRequest(_botClient, message);
                return;
            }
            await SendMessage(_botClient, message, response.Message);
            return;
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        try
        {
            User fromObj = callbackQuery.From;
            var message = callbackQuery.Message;
            var messageText = callbackQuery.Data;
            var action = messageText.Split(' ')[0] switch
            {
                "/Email" => EmailSupport(_botClient, message),
                "/Network" => NetworkSupport(_botClient, message),
                "/Hardware" => HardwareSupport(_botClient, message),
                "/Support" => Support(_botClient, message),
                _ => InsertTicket(_botClient, callbackQuery, cancellationToken)
            };
        }
        catch (Exception e)
        {
        }
    }

    private static async Task InsertTicket(ITelegramBotClient _botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        //Save this response to database
        Task<HttpResponseMessage> responseMessage = new TicketAPI().Insert(new TicketEntity()
        {
            Title = callbackQuery.Data,
            TelegramUserId = callbackQuery.From.Id.ToString()
        });

        await _botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Thank you for your ticket regarding " + callbackQuery.Data + ". A support IT engineer will contact you very soon",
                showAlert: true
                );
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,

            isPersonal: true,
            cacheTime: 0,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chatId: chosenInlineResult.From.Id,
            text: $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }

    #endregion

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    static async Task RequestEmailRequest(ITelegramBotClient _botClient, Message message)
    {
        try
        {
            var forceReplyMarkup = new ForceReplyMarkup() { Selective = true };
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "please type your official/SComm email address. example: yourname@summitcommunications.net.",
                replyMarkup: forceReplyMarkup
            );
        }
        catch (Exception e)
        {

        }
    }

    static async Task RequestContact(ITelegramBotClient _botClient, Message message)
    {
        try
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
           {
                      KeyboardButton.WithRequestContact("Contact")
                });

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Please do not type anything in this phase, just tab contact button to share your phone number",
                replyMarkup: RequestReplyKeyboard
            );
        }
        catch (Exception e)
        {

        }
    }

    static async Task SendMessage(ITelegramBotClient _botClient, Message message, string response)
    {
        try
        {
            await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: response,
                    replyMarkup: new ReplyKeyboardRemove()
                );
        }
        catch (Exception e)
        {
        }
    }

    private static async Task EmailSupport(ITelegramBotClient _botClient, Message message)
    {
        try
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
           {
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Outlook Configure/reconfigure","Outlook Configure/reconfigure"),
                                                     },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Mail Archive Issue","Mail Archive Issue"),
                                                      },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Outlook Operational Problem","Outlook Operational Problem"),
                             },

                        });

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "please choose your email support category",
                replyMarkup: inlineKeyboard
            ); ;
        }
        catch { }
    }
    private static async Task NetworkSupport(ITelegramBotClient _botClient, Message message)
    {
        try
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
           {
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("LAN/WiFi Connection Problem","LAN/WiFi Connection Problem"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("No Internet/ Slow Internet","No Internet/ Slow Internet"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("AC - Access permission add/remove","AC - Access permission add/remove"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("CC Camera issue","CC Camera issue"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("VPN Access/Reset/troubleshoot","VPN Access/Reset/troubleshoot"),
                             }
                        });

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "please choose your network support category",
                replyMarkup: inlineKeyboard
            ); ;
        }
        catch { }
    }
    private static async Task HardwareSupport(ITelegramBotClient _botClient, Message message)
    {
        try
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
           {
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Hardware Problem/Troubleshoot","Hardware Problem/Troubleshoot"),
                                                     },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Toner/Cartridge Request","Toner/Cartridge Request"),
                                                      },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Damage/Broken/Lost Issue","Damage/Broken/Lost Issue"),
                             },
                              new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Support laptop Arrange","Support laptop Arrange"),
                             },
                        });

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "please choose your hardware support category",
                replyMarkup: inlineKeyboard
            ); ;
        }
        catch { }
    }
    private static async Task Support(ITelegramBotClient _botClient, Message message)
    {
        try
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
           {
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("MS office install/troubleshoot","MS office install/troubleshoot"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Regular Operational software install/Troubleshoot","Regular Operational software install/Troubleshoot"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Printer/Scanner/IP install/Troubleshoot","Printer/Scanner/IP install/Troubleshoot"),
                             },
                            new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Any Kind of IT System Login/privilege Problem","Any Kind of IT System Login/privilege Problem"),
                             },
                             new []
                             {
                                 InlineKeyboardButton.WithCallbackData("Common Drive Connection/troubleshoot","Common Drive Connection/troubleshoot"),
                             },
                    });

            await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "please choose your support category",
            replyMarkup: inlineKeyboard
            );
        }
        catch { }
    }

    private static async Task<Message> SupportList(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
                      chatId: message.Chat.Id,
                      chatAction: ChatAction.Typing,
                      cancellationToken: cancellationToken);

        // Simulate longer running task
        await Task.Delay(500, cancellationToken);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Email", "/Email"),
                        InlineKeyboardButton.WithCallbackData("Network", "/Network"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Hardware", "/Hardware"),
                        InlineKeyboardButton.WithCallbackData("Support", "/Support"),
                    },
            });

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Which kinds of support are you seeking?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}
