using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using BuraBot.tools;
using BuraBot.ShopClasses;
using BuraBot.tools;
using System.Data.SqlClient;
using DSharpPlus;

namespace BuraBot.commands
{
    public class AppCommands : BaseCommandModule
    {
        [Command("Login")]
        [Cooldown(2, 200, CooldownBucketType.User)]
        public async Task Login(CommandContext ctx)
        {
            if(Tools.CheckIfUserAlreadyLogged(ctx) == false)
            {
                int UserID = 0;

                string sqlCommand = $"INSERT INTO Card Values('{ctx.User.Username}','')";
                string sqlCommand2 = $"SELECT id FROM Card WHERE USERKEY like '{ctx.User.Username}'";

                try
                {
                    SqlCommand cmd = new SqlCommand(sqlCommand, Program.DB.connection);
                    cmd.ExecuteNonQuery();
                    
                    SqlCommand cmd2 = new SqlCommand(sqlCommand2, Program.DB.connection);

                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserID = Convert.ToInt32(reader["id"]);
                        }
                    }

                    string sqlCommand3 = $"INSERT INTO UserInfo (username, userID) Values('{ctx.User.Username}', {UserID})";
                    SqlCommand cmd3 = new SqlCommand(sqlCommand3, Program.DB.connection);
                    cmd3.ExecuteNonQuery();
                }
                catch
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Title = "Error, logging user!",
                        Color = DiscordColor.Red
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);

                    return;
                }

                var Successfulmessage = new DiscordEmbedBuilder
                {
                    Title = $"User: {ctx.User.Username} has successfully registered!",
                    Color = DiscordColor.Green
                };

                await ctx.Channel.SendMessageAsync(embed: Successfulmessage);

                var role = ctx.Guild.GetRole(1159650918548119582);

                await ctx.Member.GrantRoleAsync(role);


            }
            else
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Title = "You are already logged!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
            }
        }


            

        [Command("test")]
        //[RequireRoles(RoleCheckMode.All, "User")]
        public async Task test(CommandContext ctx)
        {
            var message = new DiscordEmbedBuilder
            {
                Title = "Test triggered",
                Color = DiscordColor.Red
                //ImageUrl = chart.CreateChart(ctx)
            };

            await ctx.Channel.SendMessageAsync(embed: message);
        }



        [Command("WorkWithList")]
        public async Task WorkWithListButtons(CommandContext ctx)
        {
            DiscordButtonComponent AddProductButton = new DiscordButtonComponent(ButtonStyle.Success, "#addProductButtonID", "Add Product", false, new DiscordComponentEmoji(854260064906117121));
            DiscordButtonComponent DeleteProductButton = new DiscordButtonComponent(ButtonStyle.Danger, "#deleteProductButtonID", "Delete Product"/*, false, new DiscordComponentEmoji(":x:")*/);
            DiscordButtonComponent ShowProductButton = new DiscordButtonComponent(ButtonStyle.Primary, "#showListButtonID", "Show Product List"/*, false, new DiscordComponentEmoji(":hotsprings:")*/);

            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithTitle("Working with list")
                .WithDescription("Please select a button:")
                )
                .AddComponents(AddProductButton, DeleteProductButton, ShowProductButton);

            await ctx.Channel.SendMessageAsync(message);
        }
    }
}
