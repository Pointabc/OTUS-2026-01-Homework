using static System.Console;

namespace TelegramBotLib
{
    public class Core
    {
        string _userName;

        public Core()
        {
            _userName = string.Empty;
        }

        public void Start()
        {
            ShowGreeting();

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
                        if (!string.IsNullOrWhiteSpace(_userName))
                            CommandEcho(commandArgument);
                        else
                            WriteLine("Для использования команды /echo нужно зарегистрироваться, с помощью команды /start.");
                        break;
                    default:
                        WriteLine("Неизвестная команда.");
                        CommandHelp();
                        break;
                }
            }
        }

        void ShowGreeting()
        {
            WriteLine("Привет, это твой Telegram бот - запускай команды и получай результат.");
            WriteLine("Основные команды: ");
            CommandHelp();
        }

        void CommandStart()
        {
            if (!string.IsNullOrWhiteSpace(_userName))
            {
                WriteLine($"Привет, {_userName}, я твой бот. Введи команду и получи результат.");
            }
            else
            {
                Write("Представьтесь: ");

                while (string.IsNullOrWhiteSpace(_userName))
                {
                    _userName = ReadLine()?.Trim();
                }
                WriteLine($"Рад тебя видеть, {_userName}.");
            }
        }

        void CommandHelp()
        {
            if (!string.IsNullOrWhiteSpace(_userName))
            {
                WriteLine($"{_userName}, ниже список команд.");
            }

            WriteLine($"{BotConstants.CommandStart} - Начать работать с ботом.");
            WriteLine($"{BotConstants.CommandHelp} - Вывести команды.");
            WriteLine($"{BotConstants.CommandInfo} - Вывести информацию о Telegram боте.");
            WriteLine($"{BotConstants.CommandEcho} - Вывести, то что ввел пользователь.");
            WriteLine($"{BotConstants.CommandExit} - Выход.");
        }

        void CommandInfo()
        {
            if (!string.IsNullOrWhiteSpace(_userName))
            {
                WriteLine($"{_userName}, ниже информация о программе.");
            }

            WriteLine($"Версия бота 0.0.1. Дата создания {BotConstants.CreatedDate}");
        }

        static void CommandEcho(string commandArgument)
        {
            WriteLine(commandArgument);
        }
    }
}
