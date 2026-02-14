using BotEmulation;
using static System.Console;

var userName = string.Empty;

WriteLine("Привет, это твой Telegram бот - запускай команды и получай результат.");
WriteLine("Основные команды: ");
CommandHelp();

while (true)
{
    Write("Введите команду: ");
    string commandFull = ReadLine()?.Trim();
    var command = string.Empty;
    string commandArgument = string.Empty;

    string[] arr = commandFull.Split(' ');
    if (arr.Length > 0)
    {
        command = arr[0].Trim();

        if (arr.Length > 1)
        {
            for (int i = 1; i < arr.Length; i++)
                commandArgument += arr[i].Trim() + ' ';
        }
    }

    switch (command)
    {
        case BotConstants.CommandStart:
            CommandStart();
            break;
        case BotConstants.CommandHelp:
            CommandHelp();
            break;
        case BotConstants.CommandInfo:
            CommandInfo();
            break;
        case BotConstants.CommandExit:
            WriteLine("Работа с ботом завершена.");
            return;
        case BotConstants.CommandEcho:
            if (!string.IsNullOrEmpty(userName))
            {
                CommandEcho(commandArgument);
            }
            break;
        default:
            WriteLine("Неизвестная команда.");
            CommandHelp();
            break;
    }
}

void CommandStart()
{
    if (!string.IsNullOrEmpty(userName))
    {
        WriteLine($"Привет, {userName}, я твой бот. Введи команду и получи результат.");
    }
    else
    {
        Write("Представьтесь: ");

        while (string.IsNullOrEmpty(userName))
        {
            userName = ReadLine()?.Trim();
        }
        WriteLine($"Рад тебя видеть, {userName}.");
    }
}

void CommandHelp()
{
    if (!string.IsNullOrEmpty(userName))
    {
        WriteLine($"{userName}, ниже список команд.");
    }

    WriteLine($"{BotConstants.CommandStart} - Начать работать с ботом.");
    WriteLine($"{BotConstants.CommandHelp} - Вывести команды.");
    WriteLine($"{BotConstants.CommandInfo} - Вывести информацию о Telegram боте.");
    WriteLine($"{BotConstants.CommandEcho} - Вывести, то что ввел пользователь.");
    WriteLine($"{BotConstants.CommandExit} - Выход.");
}

void CommandInfo()
{
    if (!string.IsNullOrEmpty(userName))
    {
        WriteLine($"{userName}, ниже информация о программе.");
    }

    WriteLine($"Версия бота 0.0.1. Дата создания {BotConstants.CreatedDate}");
}

void CommandEcho(string commandArgument)
{
    WriteLine(commandArgument);
}