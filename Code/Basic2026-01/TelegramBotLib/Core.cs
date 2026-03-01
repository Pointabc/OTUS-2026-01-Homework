using static System.Console;

namespace TelegramBotLib
{
    public class Core
    {
        string _userName = string.Empty;
        List<Task> _tasks = new List<Task>();
        long _taskNumber = 0;

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
                    case BotConstants.CommandAddTask:
                        CommandAddTask();
                        break;
                    case BotConstants.CommandShowTasks:
                        CommandShowTasks();
                        break;
                    case BotConstants.CommandRemoveTask:
                        CommandRemoveTask();
                        break;
                    default:
                        WriteLine("Неизвестная команда.");
                        CommandHelp();
                        break;
                }
            }
        }

        /// <summary>
        /// Вывести приветствие.
        /// </summary>
        void ShowGreeting()
        {
            WriteLine("Привет, это твой Telegram бот - запускай команды и получай результат.");
            WriteLine("Основные команды: ");
            CommandHelp();
        }

        #region Обработчики команд

        /// <summary>
        /// Получить имя пользователя.
        /// </summary>
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

        /// <summary>
        /// Вывести список команд.
        /// </summary>
        void CommandHelp()
        {
            WriteLine(GetCasePhrase("Ниже список команд."));
            WriteLine($"{BotConstants.CommandStart} - Начать работать с ботом.");
            WriteLine($"{BotConstants.CommandHelp} - Вывести команды.");
            WriteLine($"{BotConstants.CommandInfo} - Вывести информацию о Telegram боте.");
            WriteLine($"{BotConstants.CommandEcho} - Вывести, то что ввел пользователь.");
            WriteLine($"{BotConstants.CommandAddTask} - Добавить задчу.");
            WriteLine($"{BotConstants.CommandShowTasks} - Показать задачи.");
            WriteLine($"{BotConstants.CommandRemoveTask} - Удалить задачу.");
            WriteLine($"{BotConstants.CommandExit} - Выход.");
        }

        /// <summary>
        /// Вывести информацию о боте.
        /// </summary>
        void CommandInfo()
        {
            WriteLine(GetCasePhrase("Информация о программе."));
            WriteLine($"Версия бота 0.0.1. Дата создания {BotConstants.CreatedDate}");
        }

        /// <summary>
        /// Вывести ввод пользователя.
        /// </summary>
        /// <param name="commandArgument"></param>
        static void CommandEcho(string commandArgument)
        {
            if (string.IsNullOrWhiteSpace(commandArgument))
            {
                WriteLine("Пустая строка.");
                return;
            }

            WriteLine(commandArgument);
        }

        /// <summary>
        /// Добавить задачу.
        /// </summary>
        void CommandAddTask()
        {
            Write(GetCasePhrase("Введите описание задачи: "));

            var taskDescription = ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(taskDescription))
            {
                _tasks.Add(new Task(taskDescription, ++_taskNumber));
                WriteLine("Задача добавлена.");
            }
        }

        /// <summary>
        /// Отобразить задачи.
        /// </summary>
        void CommandShowTasks()
        {
            if (!_tasks.Any())
            {
                WriteLine(GetCasePhrase("Список задач пуст."));
                return;
            }

            WriteLine(GetCasePhrase("Cписок задач:"));
            foreach (var task in _tasks)
                WriteLine($"{task.Number:d6} {task.Description}");
        }

        void CommandRemoveTask()
        {
            if (!_tasks.Any())
            {
                WriteLine(GetCasePhrase("Список задач пустой."));
                return;
            }

            CommandShowTasks();
            Write("Введите номер задачи для удаления: ");

            var userInput = ReadLine()?.Trim();

            if (!long.TryParse(userInput, out var taskNumber))
            {
                WriteLine($"Задача с номером '{userInput}' не найдена. Используйте команду {BotConstants.CommandRemoveTask} и введите существующий номер задачи.");
                return;
            }

            var taskToRemove = _tasks.Where(t => t.Number == taskNumber).FirstOrDefault();
            
            if (taskToRemove != null)
                _tasks.Remove(taskToRemove);

            WriteLine($"Задача с номером {taskNumber} удалена");
        }

        #endregion

        #region Дополнительные методы.

        /// <summary>
        /// Получить фразу на обращение, в зависимости от заполненности имени пользователя.
        /// </summary>
        /// <returns></returns>
        string GetCasePhrase(string phrase)
        {
            var subPhrase = phrase.Substring(1);
            var firstChar = phrase[0];

            if (!string.IsNullOrWhiteSpace(_userName))
                return $"{_userName}, {char.ToLower(firstChar)}{subPhrase}";

            return $"{char.ToUpper(firstChar)}{subPhrase}";
        }

        #endregion
    }
}
