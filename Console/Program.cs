using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using static System.Console;
using YoutubeExplode;
using System.Collections.Generic; 
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.Enums;
using static System.Net.Mime.MediaTypeNames;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Cons
{
    class Program
    {
        public const string TOKEN = "5072968456:AAGnC7Ya2YC9A5jLG6vFIvoJWnGteOfHjGQ";
        static ITelegramBotClient Bot = new TelegramBotClient(TOKEN);
        public static int mainadmin = 907267780;
        public static long[] admin_id = { 2137186514, 5543791558 };
        public static int client = 0;
        public static YoutubeClient youtube = new YoutubeClient() ;
        public static async Task message_handler(Message message)
        {
            try
            {

                var video = youtube.Videos.GetAsync(message.Text);
                try
                {
                    await mess(message);
                }
                catch(Exception e)
                {
                    await Bot.SendTextMessageAsync(message.Chat, $"message_hadler error:\n{e.Message}\n{e.TargetSite}\n{e.HResult}");
                }
            }
            catch
            {
                try
                {


                    await search(message);
                }
                catch(Exception e)
                {
                    await Bot.SendTextMessageAsync(message.Chat, $"message_hadler error:\n{ e.Message}");
                }
            }
        }
        public static async Task mess(Message message)
        {
            string text = message.Text;
            var stream = await youtube.Videos.Streams.GetManifestAsync(text);
            var video = stream.GetMuxedStreams();
            var audio = stream.GetAudioOnlyStreams().OrderByDescending(i => i.Size).First();
            var phot = await youtube.Videos.GetAsync(text);
            var photo = phot.Thumbnails.OrderByDescending(i => Convert.ToInt32(i.Resolution.Area)).First().Url;

            var title = phot.Title;
            var author = phot.Author;
            var duration = phot.Duration;

            var str = video.OrderByDescending(i => i.Size);
            var st = new MuxedStreamInfo[10];
            int k = 1;
            string v_descriptions = "";
            v_descriptions += title + "\n" + "Author: " + author + "\n" + "Duration: " + duration;
            var pix = new InlineKeyboardButton[10];

            foreach (var i in str)
            {
                bool l = true;
                for (int j = 0; j < st.Length; j++)
                {
                    if (i.VideoQuality == st[j]?.VideoQuality)
                    {
                        l = false;
                    }
                }
                if (l)
                {
                    st[k] = i;
                    v_descriptions += $"\n{i.VideoQuality}:  {i.Size.MegaBytes.ToString("0.00")}MB";
                    pix[k] = InlineKeyboardButton.WithCallbackData(text: $"{i.VideoQuality}", callbackData: $"video {text} {i.VideoQuality}");
                    k ++;
                }
                
            }
            v_descriptions += "\nAudio: " + audio.Size.MegaBytes.ToString("0.00") + "MB";
            InlineKeyboardButton audio_f = InlineKeyboardButton.WithCallbackData(text: "audio", callbackData: $"audio {text} {audio.Size} ");
            InlineKeyboardMarkup result = new[] { pix.Where(i => i != null) , new[] { audio_f }  };

            await Bot.SendPhotoAsync(
                chatId: message.Chat,
                photo: photo,
                replyMarkup: result , 
                caption: v_descriptions);
  
        }
        public static async Task callback_handler(Message message , string text)
        {

            Console.WriteLine("kal: " + text);
            string[] url = text.Split(' ').ToArray();
            if (url.Length == 3)
            {

                try
                {

                    if (client >= admin_id.Length - 1)

                        client = 0;
                    else
                        client += 1;
                    try
                    {
                        await Bot.DeleteMessageAsync(chatId: message.Chat, messageId: message.MessageId);
                    }
                    catch (Exception e)
                    {
                        await Bot.SendTextMessageAsync(message.Chat, text: $"callback delete error {e}");

                    }
                    await Bot.SendTextMessageAsync(
                         chatId: admin_id[client],
                         text: $"{message.Chat.Id} {url[0]} {url[2]} {url[1]}");
                }
                catch (Exception e)
                {
                    await Bot.SendTextMessageAsync(message.Chat, text: $"message call quality error {e}");
                }   
            }
        }
        public static async Task search(Message message)
        {
 

            var you = new YoutubeClient();

            string text = message.Text;
            text = text.Replace(' ', '_');
            var result = you.Search.GetVideosAsync($"{text}");
            int q = 0;
            string url = " ";
            string thub_url = " ";
            string title = "";
            string author = "";
            string duration = "";

            try
            {


                await foreach (var i in result)
                {
                    Console.WriteLine(i + "   " + i.Title);
                    thub_url = i.Thumbnails.OrderByDescending(i => Convert.ToInt32(i.Resolution.Area)).First().Url;
                    url = i.Url;
                    title = i.Title;
                    author = i.Author.ToString();
                    duration = i.Duration.ToString();
                    break;
                }
            }
            catch(Exception e){
                await Bot.SendTextMessageAsync(message.Chat, $"mess foreach error:\n{e.Message}");
            }
            string v_descriptions = " ";
            v_descriptions = title + "\n" + "Author: " + author + "\n" + "Duration: " + duration;
            


            InlineKeyboardButton forward = InlineKeyboardButton.WithCallbackData(text: "--->", callbackData: $"1 {text} 0 forward");
            InlineKeyboardButton Download = InlineKeyboardButton.WithCallbackData(text: "Download", callbackData: $"{url}");
            InlineKeyboardMarkup kal = new[] { new[] { forward }, new[] { Download } };
            try
            {
                await Bot.SendPhotoAsync(
                    message.Chat,
                    photo: thub_url,
                    replyMarkup: kal,
                    caption: v_descriptions);
            }
            catch(Exception e)
            {
                await Bot.SendTextMessageAsync(message.Chat, $"message_hadler error:\n{e.Message}");
            }
       
        }
        public static async Task callback_4(Message message , string text)
        {
            try
            {


                Console.WriteLine("Callback_4 working");
                string[] s = text.Split(" ");
                string url = s[1];
                int d = Convert.ToInt32(s[2]);
                string point = s[3];
                if (point == "back")
                {
                    d -= 1;
                }
                else
                {
                    d += 1;
                }
                //var you = new YoutubeClient();
                var result = youtube.Search.GetVideosAsync($"{url}");
                int c = 0;
                string thub_url = " ", v_url = " ";
                string title = "";
                string author = "";
                string duration = "";
                await foreach (var i in result)
                {

                    if (c == d)
                    {
                        thub_url = i.Thumbnails.OrderByDescending(i => i.Resolution.Area).First().Url;
                        v_url = i.Url;
                        title = i.Title;
                        author = i.Author.ToString();
                        duration = i.Duration.ToString();

                        break;
                    }
                    c++;
                }

                string v_descriptions = "";
                v_descriptions += title + "\n" + "Author: " + author + "\n" + "Duration: " + duration;
                InlineKeyboardButton forward = InlineKeyboardButton.WithCallbackData(text: "--->", callbackData: $"1 {url} {d} forward");
                InlineKeyboardButton Download = InlineKeyboardButton.WithCallbackData(text: "Download", callbackData: $"{v_url}");
                InlineKeyboardMarkup kal;
                if (d > 0)
                {
                    InlineKeyboardButton back = InlineKeyboardButton.WithCallbackData(text: "<---", callbackData: $"1 {url} {d} back");
                    kal = new[] { new[] { back, forward }, new[] { Download } };
                }
                else
                {
                    kal = new[] { new[] { forward }, new[] { Download } };
                }
       
                InputMedia ker = new InputMedia(thub_url);
                InputMediaBase pho = new InputMediaPhoto(ker);
                pho.Caption = v_descriptions ;

                await Bot.EditMessageMediaAsync(
                    chatId: message.Chat,
                    messageId: message.MessageId,
                    media: pho,
                    replyMarkup: kal
                   );
             


            }
            catch(Exception e)
            {
                await Bot.SendTextMessageAsync(message.Chat, text: e.Message);
            }
        }
        public static async Task callback_download( Message message , string url )
        {
            try
            {
                //YoutubeClient you = new YoutubeClient();
                var stream = await youtube.Videos.GetAsync(url);
                var video = await youtube.Videos.Streams.GetManifestAsync(url);
                
                // Video descriptions 
                var title = stream.Title;
                var author = stream.Author;
                var duration = stream.Duration;

                var str = video.GetMuxedStreams().OrderByDescending(i => i.Size);
                var audios = video.GetAudioOnlyStreams().OrderByDescending(i => i.Size).First();
                var button = new InlineKeyboardButton[10];
                var st = new MuxedStreamInfo[10];
                string v_descriptions = "";
                v_descriptions += title + "\n" + "Author: " + author + "\n" + "Duration: " + duration;
                v_descriptions += "\nChoose one of them:";
                int k = 0;
                foreach (var i in str)
                {
                    bool l = true;
                    for (int j = 0; j < st.Length; j++)
                    {
                        if (i.VideoQuality == st[j]?.VideoQuality)
                        {
                            l = false;
                        }
                    }
                    if (l)
                    {
                        st[k] = i;
                        v_descriptions += $"\n{i.VideoQuality}:  {i.Size.MegaBytes.ToString("0.00")}MB";
                        button[k] = InlineKeyboardButton.WithCallbackData(text: $"{i.VideoQuality}", callbackData: $"video {url} {i.VideoQuality}");
                        k++;
                    }

                }
                InlineKeyboardButton audio_f = InlineKeyboardButton.WithCallbackData(text: "Audio", callbackData: $"audio {url} 100");
                InlineKeyboardMarkup kal = new[] { button.Where(i => i != null).ToArray(),new[] { audio_f } };
                await Bot.EditMessageCaptionAsync(
                    chatId:message.Chat,
                    messageId: message.MessageId,
                    caption: v_descriptions,
                    replyMarkup: kal);
                
            }
            catch ( Exception e)
            {
                await Bot.SendTextMessageAsync(message.Chat, text: e.Message);
            }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message?.Text?.ToLower() == "/start")
                {
                    await bot.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
                    return;
                }
                try
                {
                    string message_type = message.Type.ToString().ToLower();
                    if (message_type == "text")
                    {
                        await message_handler(message);
                    }
                    if (message_type == "video" || message_type == "audio")  
                    {
                        // await handler(message);
                    }
                        
                }
                catch (Exception e)
                {
                    await bot.SendTextMessageAsync(message.Chat, e.Message);
                }
                await bot.SendTextMessageAsync(message.Chat, "Привет-привет!!");
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var message = update.CallbackQuery.Message;
                var text = update.CallbackQuery.Data;
                string[] url = text.Split(' ').ToArray();
                if (url.Length == 3)
                {
                    await callback_handler(message, text);
                }
                if(url.Length == 4)
                {
                    await callback_4(message , text);
                }
                if(url.Length == 1)
                {
                    await callback_download( message , text);
                }
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + Bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            Bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}
