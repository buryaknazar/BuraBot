using BuraBot.commands;
using BuraBot.config;
using BuraBot.DBReader;
using BuraBot.ShopClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using System.Data.SqlClient;
using DSharpPlus.SlashCommands;
using BuraBot.SlashCommands;
using BuraBot.BuraBotClient;

namespace BuraBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        public static ListOfProducts Products { get; set; }

        public static DataBaseReader DB { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();

            Products = new ListOfProducts();
            DB = new DataBaseReader();
            await DB.ConnectDb();


            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += Client_Ready;
            Client.ComponentInteractionCreated += ButtonPressedResponce;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = Client.UseSlashCommands();

            Commands.CommandErrored += CommandErroredHandler;

            Commands.RegisterCommands<AppCommands>();
            slashCommandsConfig.RegisterCommands<AppSlashCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task ButtonPressedResponce(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            DiscordEmoji[] emoji =
            {
                    DiscordEmoji.FromName(Program.Client, ":printer:"),
                    DiscordEmoji.FromName(Program.Client, ":outbox_tray:"),
                    DiscordEmoji.FromName(Program.Client, ":secret:"),
                    DiscordEmoji.FromName(Program.Client, ":tools:")
            };

            switch (e.Interaction.Data.CustomId)
            {
                case "ShowButton":

                    string helpMessage1 = $"{emoji[0]} <Show> commands:" + "\n/ShowButtonList -> Will print your all product list" +
                        "\n/ShowProductsByName -> Will print your all products that have name you entered" +
                        "\n/ShowProductsByPrice -> Will print your all products that have price you entered" +
                        "\n/ShowProductsByCategory -> Will print your all products that have category you entered";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(helpMessage1));

                    break;

                case "EditButton":

                    string helpMessage2 = $"{emoji[3]} <Edit> commands:" + "\n/EditProduct -> You can edit any field of your product" +
                        "\n/EditProductName -> Edits only name of product" +
                        "\n/EditProductPrice -> Edits only price of product" +
                        "\n/EditProductImage -> Edits only image of product" +
                        "\n/EditProductCategory -> Edits only category of product" +
                        "\n/EditProductDescription -> Edits only description of product";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(helpMessage2));

                    break;

                case "DeleteButton":

                    string helpMessage3 = $"{emoji[1]} <Delete> commands:" + "\n/ClearList -> Will delete all products from your list" +
                        "\n/DeleteProduct -> Delete product from list";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(helpMessage3));

                    break;

                case "OtherButton":

                    string helpMessage4 = $"{emoji[2]} <Other> commands:" + "\n/AddProduct -> Adds product to your list" +
                        "\n/Profile -> Shows your user profile" +
                        "\n/SellProduct -> Just selling the product" +
                        "\n/GetChart -> Shows your total statistic on chart";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(helpMessage4));

                    break;


            }
        }

        private static async Task CommandErroredHandler(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if(e.Exception is ChecksFailedException exception )
            {
                string timeLeft = string.Empty;
                bool isCoolDownError = false;
                bool isRequireRoleError = false;

                foreach(var check in exception.FailedChecks)
                {
                    if(check is CooldownAttribute)
                    {
                        var coolDown = (CooldownAttribute)check;

                        timeLeft = coolDown.GetRemainingCooldown(e.Context).ToString(@"hh\:mm\:ss");
                        isCoolDownError = true;
                    }
                    else if(check is RequireRolesAttribute)
                    {
                        isRequireRoleError = true;
                    }
                }

                if (isCoolDownError)
                {
                    var coolDownMessage = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Title = "Please wait for the cooldown to end",
                        Description = $"Time: {timeLeft}"
                    };

                    await e.Context.Channel.SendMessageAsync(embed: coolDownMessage);
                }
                else if (isRequireRoleError)
                {
                    var RequireRoleMessage = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Title = "You don't have access to this command!",
                        Description = "Check whether you are registered, if not, then write the command (!Login)"
                    };

                    await e.Context.Channel.SendMessageAsync(embed: RequireRoleMessage);
                }

            }
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
