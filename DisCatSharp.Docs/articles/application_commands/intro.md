---
uid: appcommands_intro
title: Application Commands Introduction
---

>[!NOTE]
> This article assumes you've recently read the article on *[writing your first bot](xref:basics_first_bot)*.

# Introduction to App Commands
Discord provides built-in commands called: *Application Commands*.<br/>
Be sure to install the `DisCatSharp.ApplicationCommands` package from NuGet before continuing.

At the moment it is possible to create such commands:
- Slash commands
- User context menu commands
- Message context menu commands

## Writing an Application Commands

### Creation of the first commands

>[!NOTE]
> In order for the bot to be able to create commands in the guild, it must be added to a guild with `applications.commands` scope.

Each command is a method with the attribute [SlashCommand](xref:DisCatSharp.ApplicationCommands.SlashCommandAttribute) or [ContextMenu](xref:DisCatSharp.ApplicationCommands.ContextMenuAttribute). They must be in classes that inherit from [ApplicationCommandsModule](xref:DisCatSharp.ApplicationCommands.ApplicationCommandsModule).
Also, the first argument to the method must be [InteractionContext](xref:DisCatSharp.ApplicationCommands.InteractionContext) or [ContextMenuContext](xref:DisCatSharp.ApplicationCommands.ContextMenuContext).

Simple slash command:
```cs
public class MyCommand : ApplicationCommandsModule
{
    [SlashCommand("my_command", "This is decription of the command.")]
    public async Task MySlashCommand(InteractionContext context)
    {

    }
}
```

Simple context menu command:
```cs
public class MySecondCommand : ApplicationCommandsModule
{
    [ContextMenu(ApplicationCommandType.User, "My Command")]
    public async Task MyContextMenuCommand(ContextMenuContext context)
    {

    }
}
```

Now let's add some actions to the commands, for example, send a reply:
```cs
await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
{
    Content = "Hello :3"
});
```

If the command will be executed for more than 3 seconds, we must response at the beginning of execution and edit it at the end.
```cs
await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

await Task.Delay(5000); // Simulating a long command execution.

await ctx.EditResponseAsync(new DiscordWebhookBuilder()
{
    Content = "Hello :3"
});
```

>[!NOTE]
> Note that you can make your commands static, but then you cannot use [Dependency Injection](xref:commands_dependency_injection) in them.


### Registration of commands

After writing the commands, we must register them. For this we need a [DiscordClient](xref:DisCatSharp.DiscordClient).

```cs
var appCommands = client.UseApplicationCommands();

appCommands.RegisterCommands<MyCommand>();
appCommands.RegisterCommands<MySecondCommand>();
```
Simple, isn't it? You can register global and guild commands.
Global commands will be available on all guilds of which the bot is a member. Guild commands will only appear in a specific guild.

>[!NOTE]
>Global commands are updated within an hour, so it is recommended to use guild commands for testing and development.

To register guild commands, it is enough to specify the Id of the guild as the first argument of the registration method.

```cs
var appCommands = client.UseApplicationCommands();

appCommands.RegisterCommands<MyCommand>(<guildId>);
appCommands.RegisterCommands<MySecondCommand>(<guildId>);
```

## Command Groups

Sometimes we may need to combine slash commands into groups.
In this case, we need to wrap our class with commands in another class and add the [SlashCommandGroup](xref:DisCatSharp.ApplicationCommands.SlashCommandGroupAttribute) attribute.

```cs
public class MyCommand : ApplicationCommandsModule
{
    [SlashCommandGroup("my_command", "This is decription of the command group.")]
    public class MyCommandGroup : ApplicationCommandsModule
    {
        [SlashCommand("first", "This is decription of the command.")]
        public async Task MySlashCommand(InteractionContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = "This is first subcommand."
            });
        }
        [SlashCommand("second", "This is decription of the command.")]
        public async Task MySecondCommand(InteractionContext context)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = "This is second subcommand."
            });
        }
    }
}
```

Commands will now be available via `/my_command first` and `/my_command second`.
Also, note that both classes must inherit [ApplicationCommandsModule](xref:DisCatSharp.ApplicationCommands.ApplicationCommandsModule).
