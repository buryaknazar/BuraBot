using BuraBot.charts;
using BuraBot.ShopClasses;
using BuraBot.tools;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace BuraBot.SlashCommands
{
    public class AppSlashCommands : ApplicationCommandModule
    {
        // Add product
        [SlashCommand("AddProduct", "Adds product to your list")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task AddProduct(InteractionContext ctx,
            [Option("Name", "Enter the name of product")] string productName,

            [Option("Description", "Enter the description of product")] string productDescription,

            [Choice("PCEquipment", "PCEquipment")]
            [Choice("Sport", "Sport")]
            [Choice("Food", "Food")]
            [Option("Category", "Enter the product category")] string productCategory,

            [Option("Price", "Enter the product price")] long productPrice,

            [Option("Image", "Image of product")] DiscordAttachment picture
            )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Adding the product...")
                );

            var interactivity = Program.Client.GetInteractivity();
            var pollTime = TimeSpan.FromSeconds(10);

            DiscordEmoji[] emojiOptions =
            {
                    DiscordEmoji.FromName(Program.Client, ":white_check_mark:"),
                    DiscordEmoji.FromName(Program.Client, ":x:")
            };

            var CheckMessage = new DiscordEmbedBuilder()
            {
                Title = "Are you sure you want to add that product?:",
                Description = $"Name: {productName}\nDescription: {productDescription}\nCategory: {productCategory}\nPrice: {productPrice}\n{emojiOptions[0]} - Yes!" + $"\t{emojiOptions[1]} - No!",
                Color = DiscordColor.HotPink,
                ImageUrl = picture.Url
            };

            var sentPoll = await ctx.Channel.SendMessageAsync(embed: CheckMessage);

            foreach (var emoji in emojiOptions)
            {
                await sentPoll.CreateReactionAsync(emoji);
            }

            var totalReactions = await interactivity.CollectReactionsAsync(sentPoll, pollTime);

            int count1 = 0;
            int count2 = 0;

            foreach (var emoji in totalReactions)
            {
                if (emoji.Emoji == emojiOptions[0])
                    count1++;
                if (emoji.Emoji == emojiOptions[1])
                    count2++;
            }

            if(count1 > count2)
            {
                try
                {
                    Program.Products.AddProduct(new Product(productName, Tools.ToProductCategory(productCategory), Convert.ToInt32(productPrice), picture.Url, productDescription), ctx);

                    var embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = $"Name: {productName} |Description: {productDescription} |Category: {productCategory} |Price: {productPrice}",
                        Color = DiscordColor.Green,
                        Description = "Product was successful added!",
                        ImageUrl = picture.Url
                    };

                    await ctx.Channel.SendMessageAsync(embed: embedMessage);
                }
                catch
                {
                    var embedMessage = new DiscordEmbedBuilder()
                    {
                        Title = "Error, product was not added!",
                        Color = DiscordColor.Red,
                    };

                    await ctx.Channel.SendMessageAsync(embed: embedMessage);
                }
            }
            else
            {
                var embedMessage = new DiscordEmbedBuilder()
                {
                    Title = "Product was not added!",
                    Color = DiscordColor.Red,
                };

                await ctx.Channel.SendMessageAsync(embed: embedMessage);
            }

        }

        [SlashCommand("Profile", "Shows your user profile")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task ShowProfile(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            string username = ctx.User.Username;

            DiscordEmoji[] emoji =
            {
                    DiscordEmoji.FromName(Program.Client, ":desktop:"),
                    DiscordEmoji.FromName(Program.Client, ":dollar:"),
                    DiscordEmoji.FromName(Program.Client, ":skull:")
            };

            UserCharts UserInfo = new UserCharts();
            UserInfo.LoadUserInfo(ctx);

            var profileEmbed = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Turquoise)
                .WithTitle(username + $"'s Profile {emoji[0]}")
                .WithThumbnail(ctx.User.AvatarUrl)
                .AddField($"{emoji[1]} Total Cash Earned: ", UserInfo.totalCashEarned.ToString())
                .AddField($"{emoji[2]} Total Cash Losed: ", UserInfo.totalCashLosed.ToString())
                .AddField($"{emoji[1]} Net Income: ", UserInfo.GetNetIncome().ToString())
                );

            await ctx.Channel.SendMessageAsync(profileEmbed);
        }

        [SlashCommand("Help", "All info about commands")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task HelpCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );


            DiscordEmoji[] emoji =
            {
                    DiscordEmoji.FromName(Program.Client, ":sos:"),
            };

            var ShowButton = new DiscordButtonComponent(ButtonStyle.Success, "ShowButton", "Show commands");
            var EditButton = new DiscordButtonComponent(ButtonStyle.Success, "EditButton", "Edit commands");
            var OtherButton = new DiscordButtonComponent(ButtonStyle.Success, "OtherButton", "Other commands");
            var DeleteButton = new DiscordButtonComponent(ButtonStyle.Success, "DeleteButton", "Delete commands");

            var helpMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()

                .WithColor(DiscordColor.HotPink)
                .WithTitle($"{emoji[0]} Help Menu")
                .WithDescription("Please pick a button for more information on the commands")
                )
                .AddComponents(ShowButton, EditButton, DeleteButton, OtherButton);

            await ctx.Channel.SendMessageAsync(helpMessage);
        }


        // Show products
        [SlashCommand("ShowProductList", "Will print your all product list")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task ShowProductList(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your list: ")
            );

            try
            {
                Program.Products.LoadProducts(ctx);

                if (ListOfProducts.products.Count > 0)
                {
                    foreach (Product product in ListOfProducts.products)
                    {
                        var message = new DiscordEmbedBuilder
                        {
                            Title = $"Name: {product.Name} |Description: {product.Description} |Category: {product.Category} |Price: {product.Price}",
                            Color = DiscordColor.Gold,
                            ImageUrl = product.ImageUrl
                        };

                        await ctx.Channel.SendMessageAsync(embed: message);
                    }
                }
                else
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Title = $"There are no products to show you :(",
                        Color = DiscordColor.Red
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);
                }
            }
            catch
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "Error, can not load products from DB!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);
            }
        }

        // Delete product
        [SlashCommand("DeleteProduct", "Delete product from list")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task DeleteProduct(InteractionContext ctx, [Option("ProductID", "Enter product ID")] long productID)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Deletting the product...")
            );

            try
            {
                Program.Products.RemoveProduct(Convert.ToInt32(productID), ctx);

                var message = new DiscordEmbedBuilder
                {
                    Title = $"Product: #{productID} was successful deleted",
                    Color = DiscordColor.Green
                };

                await ctx.Channel.SendMessageAsync(embed: message);

            }
            catch
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "Error, product was not deleted!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);
            }
        }

        // Show products by name
        [SlashCommand("ShowProductsByName", "Will print your all products that have name you entered")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task ShowProductsByName(InteractionContext ctx, [Option("Name", "Enter name of products")] string product_name)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.LoadProducts(ctx);
            }
            catch
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "Error, can not load products from DB!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);

                return;
            }

            List<Product> productsToPrint = Program.Products.ReturnFilteredListByName(product_name);

            if(productsToPrint.Count > 0)
            {
                foreach (var product in productsToPrint)
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Title = $"#{product.ID} | Name: {product.Name} | Description: {product.Description} | Category: {product.Category} | Price: {product.Price}",
                        Color = DiscordColor.Gold,
                        ImageUrl = product.ImageUrl
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);
                }
            }
            else
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "There are no such products!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);
            }

        }

        // Show products by price
        [SlashCommand("ShowProductsByPrice", "Will print your all products that have price you entered")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task ShowProductsByPrice(InteractionContext ctx, [Option("Price", "Enter price of products")] long product_price)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.LoadProducts(ctx);
            }
            catch
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "Error, can not load products from DB!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);

                return;
            }

            List<Product> productsToPrint = Program.Products.ReturnFilteredListByPrice(product_price);

            if (productsToPrint.Count > 0)
            {
                foreach (var product in productsToPrint)
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Title = $"#{product.ID} | Name: {product.Name} | Description: {product.Description} | Category: {product.Category} | Price: {product.Price}",
                        Color = DiscordColor.Gold,
                        ImageUrl = product.ImageUrl
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);
                }
            }
            else
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "There are no such products!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);
            }

        }

        // Show products by category
        [SlashCommand("ShowProductsByCategory", "Will print your all products that have category you entered")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task ShowProductsByCategory(InteractionContext ctx,
            [Choice("PCEquipment", "PCEquipment")]
            [Choice("Sport", "Sport")]
            [Choice("Food", "Food")]
            [Option("Category", "Enter the product category")] string productCategory
            )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.LoadProducts(ctx);
            }
            catch
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "Error, can not load products from DB!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);

                return;
            }

            List<Product> productsToPrint = Program.Products.ReturnFilteredListByCategory(Tools.ToProductCategory(productCategory));

            if (productsToPrint.Count > 0)
            {
                foreach (var product in productsToPrint)
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Title = $"#{product.ID} | Name: {product.Name} | Description: {product.Description} | Category: {product.Category} | Price: {product.Price}",
                        Color = DiscordColor.Gold,
                        ImageUrl = product.ImageUrl
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);
                }
            }
            else
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "There are no such products!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);
            }

        }

        // Delete all products from list
        [SlashCommand("ClearList", "Will delete all products from your list")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task ClearList(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            Program.Products.LoadProducts(ctx);

            if(Program.Products.GetSize() > 0)
            {
                var interactivity = Program.Client.GetInteractivity();
                var pollTime = TimeSpan.FromSeconds(10);

                DiscordEmoji[] emojiOptions =
                {
                    DiscordEmoji.FromName(Program.Client, ":white_check_mark:"),
                    DiscordEmoji.FromName(Program.Client, ":x:")
                };

                var pollMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Lilac,
                    Title = "Are you sure want to delete all products?",
                    Description = $"{emojiOptions[0]} - Yes!\n" + $"{emojiOptions[1]} - No!"
                };

                var sentPoll = await ctx.Channel.SendMessageAsync(embed: pollMessage);

                foreach(var emoji in emojiOptions)
                {
                    await sentPoll.CreateReactionAsync(emoji);
                }

                var totalReactions = await interactivity.CollectReactionsAsync(sentPoll, pollTime);

                int count1 = 0;
                int count2 = 0;

                foreach(var emoji in totalReactions)
                {
                    if(emoji.Emoji == emojiOptions[0])
                        count1++;
                    if (emoji.Emoji == emojiOptions[1])
                        count2++;
                }

                if(count1 > count2)
                {
                    try
                    {
                        Program.Products.DeleteAllProducts(ctx);
                    }
                    catch
                    {
                        var ErrorMessage = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Red,
                            Title = "All products have not been deleted!",
                        };

                        await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                        return;
                    }
                    
                    var message = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Green,
                        Title = "All products have been deleted!",
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);
                }
                else if(count1 < count2)
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Green,
                        Title = "Your data will be saved!",
                    };

                    await ctx.Channel.SendMessageAsync(embed: message);
                }
            }
            else
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "Your list is clear!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: message);
            }
        }

        [SlashCommand("EditProduct", "You can edit any field of your product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task EditProduct(InteractionContext ctx, [Option("ID", "Enter product ID")] long productID,
            [Option("Name", "Enter the new name of product")] string productName,

            [Option("Description", "Enter the new description of product")] string productDescription,

            [Choice("PCEquipment", "PCEquipment")]
            [Choice("Sport", "Sport")]
            [Choice("Food", "Food")]
            [Option("Category", "Enter the new product category")] string productCategory,

            [Option("Price", "Enter the new product price")] long productPrice,

            [Option("Image", "New image of product")] DiscordAttachment picture
            )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.EditProduct(ctx, productID, productName, productDescription, Tools.ToProductCategory(productCategory), productPrice, picture.Url);
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Title = "Error, coudn't edit product!",
                    Color = DiscordColor.Red
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);

                return;
            }

            var message = new DiscordEmbedBuilder
            {
                Title = "Product's fields was changed!",
                Color = DiscordColor.Green
            };

            await ctx.Channel.SendMessageAsync(embed: message);
        }

        [SlashCommand("EditProductName", "Edits only name of product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task EditProductName(InteractionContext ctx, [Option("ProductID", "Enter product id to edit")] long productID, [Option("NewName", "Enter the new name")] string NewProductName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.EditProductName(ctx, Convert.ToInt32(productID), NewProductName);
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Error, can not connect to DB!"
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                return;
            }

            var GoodMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "The product's name was changed!"
            };

            await ctx.Channel.SendMessageAsync(embed: GoodMessage);
        }

        [SlashCommand("EditProductPrice", "Edits only price of product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task EditProductPrice(InteractionContext ctx, [Option("ProductID", "Enter product id to edit")] long productID, [Option("NewPrice", "Enter the new price")] long NewProductPrice)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.EditProductPrice(ctx, Convert.ToInt32(productID), NewProductPrice);
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Error, can not connect to DB!"
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                return;
            }

            var GoodMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "The product's price was changed!"
            };

            await ctx.Channel.SendMessageAsync(embed: GoodMessage);
        }

        [SlashCommand("EditProductImage", "Edits only image of product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task EditProductImage(InteractionContext ctx, [Option("ProductID", "Enter product id to edit")] long productID, [Option("NewImage", "Select the new image")] DiscordAttachment NewProductPicture)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.EditProductImage(ctx, Convert.ToInt32(productID), NewProductPicture.Url);
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Error, can not connect to DB!"
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                return;
            }

            var GoodMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "The product's image was changed!"
            };

            await ctx.Channel.SendMessageAsync(embed: GoodMessage);
        }

        [SlashCommand("EditProductCategory", "Edits only category of product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task EditProductCategory(InteractionContext ctx, [Option("ProductID", "Enter product id to edit")] long productID, [Option("NewCategory", "Enter the new category")] string NewProductCategory)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.EditProductCategory(ctx, Convert.ToInt32(productID), Tools.ToProductCategory(NewProductCategory));
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Error, can not connect to DB!"
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                return;
            }

            var GoodMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "The product's category was changed!"
            };

            await ctx.Channel.SendMessageAsync(embed: GoodMessage);
        }

        [SlashCommand("EditProductDescription", "Edits only description of product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task EditProductDescription(InteractionContext ctx, [Option("ProductID", "Enter product id to edit")] long productID, [Option("NewDescription", "Enter the new description")] string NewProductDescription)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.EditProductDescription(ctx, Convert.ToInt32(productID), NewProductDescription);
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Error, can not connect to DB!",
                    ImageUrl = ""
                    
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                return;
            }

            var GoodMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "The product's description was changed!"
            };

            await ctx.Channel.SendMessageAsync(embed: GoodMessage);
        }

        [SlashCommand("SellProduct", "Sell product")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task SellProduct(InteractionContext ctx, [Option("ProductID", "Enter the productID")] long productID)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Your request:")
            );

            try
            {
                Program.Products.SellProduct(ctx, Convert.ToInt32(productID));
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = $"Error, The product #{productID} was not sold!"
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
                return;
            }

            var GoodMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = $"The product #{productID} is sold!"
            };

            await ctx.Channel.SendMessageAsync(embed: GoodMessage);
        }

        [SlashCommand("GetChart", "Shows your total statistic")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [SlashRequireUserPermissions(Permissions.UseApplicationCommands)]
        public async Task GetChart(InteractionContext ctx)
        {
            DiscordEmoji[] emoji =
            {
                    DiscordEmoji.FromName(Program.Client, ":bar_chart:"),
                    DiscordEmoji.FromName(Program.Client, ":no_entry:")
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"{emoji[0]} Your chart:")
            );

            UserCharts chart = new UserCharts();

            try
            {
                var GoodMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.HotPink,
                    ImageUrl = chart.CreateChart(ctx)
                };

                await ctx.Channel.SendMessageAsync(embed: GoodMessage);
            }
            catch
            {
                var ErrorMessage = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = $"{emoji[1]} Error, Chart was not created!"
                };

                await ctx.Channel.SendMessageAsync(embed: ErrorMessage);
            }

        }
    }
}
