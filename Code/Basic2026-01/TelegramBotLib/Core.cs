using static System.Console;

namespace TelegramBotLib
{
    public class Core
    {
        ToDoUser _toDoUser = new ToDoUser(string.Empty);
        List<ToDoItem> _toDoItems = new List<ToDoItem>();
        long _taskNumber = 0;
        long _taskCount = 0;
        string _userCommand = string.Empty;
        string _commandArgument = string.Empty;

        public static long maxNumber = 100;
        public static long maxTaskDiscriptionLength = 100;

        public void Start()
        {
            try
            {
                #region Добавить ограничение на максимальное количество задач.

                Write("Введите максимально допустимое количество задач: ");
                var inputMaxTaskNumber = ReadLine();

                try
                {
                    var maxTaskNumber = ParseAndValidateLong(inputMaxTaskNumber, BotConstants.MinTaskNumber, BotConstants.MaxTaskNumber);
                    maxNumber = maxTaskNumber;
                }
                catch (ArgumentException argumentException)
                {
                    WriteLine(argumentException.Message);
                    maxNumber = BotConstants.MaxTaskNumber;
                }

                #endregion

                #region Добавить ограничение на максимальную длину задачи.

                Write("Введите максимально допустимую длину задачи: ");
                var inputMaxTaskDiscriptionLength = ReadLine();

                try
                {
                    maxTaskDiscriptionLength = ParseAndValidateLong(inputMaxTaskDiscriptionLength, BotConstants.MinTaskDiscriptioLength, BotConstants.MaxTaskDiscriptionLength); ;
                }
                catch (ArgumentException argumentException)
                {
                    WriteLine(argumentException.Message);
                    maxTaskDiscriptionLength = BotConstants.MaxTaskDiscriptionLength;
                }

                #endregion

                ShowGreeting();

                while (true)
                {
                    try
                    {
                        GetUserCommandAndArgument();

                        switch (_userCommand)
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
                                if (!string.IsNullOrWhiteSpace(_toDoUser.TelegramUserName))
                                    CommandEcho(_commandArgument);
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
                            case BotConstants.CommandCompleteTask:
                                CommandCompleteTask(_commandArgument);
                                break;
                            case BotConstants.CommandShowAllTasks:
                                CommandShowAllTasks();
                                break;
                            default:
                                WriteLine("Неизвестная команда.");
                                CommandHelp();
                                break;
                        }
                    }
                    catch (TaskCountLimitException taskCountLimitException)
                    {
                        WriteLine(taskCountLimitException.Message);
                    }
                    catch (TaskLengthLimitException taskLengthLimitException)
                    {
                        WriteLine(taskLengthLimitException.Message);
                    }
                    catch (DuplicateTaskException duplicateTaskException)
                    {
                        WriteLine(duplicateTaskException.Message);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine("Произошла непредвиденная ошибка: ");
                WriteLine($"Type of exception: {e.GetType()}");
                WriteLine($"Message: {e.Message}");
                WriteLine($"StackTrace: {e.StackTrace}");
                WriteLine($"InnerException: {e.InnerException}");
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

        /// <summary>
        /// Получить команду от пользователя.
        /// </summary>
        void GetUserCommandAndArgument()
        {
            Write("Введите команду: ");
            _commandArgument = string.Empty;
            string commandFull = ReadLine();

            string[] arr = commandFull.Split(' ');
            if (arr.Length > 0)
            {
                _userCommand = arr[0].Trim();

                if (arr.Length > 1)
                {
                    for (int i = 1; i < arr.Length; i++)
                        _commandArgument += arr[i].Trim() + ' ';
                }
            }
        }

        #region Обработчики команд

        /// <summary>
        /// Получить имя пользователя.
        /// </summary>
        void CommandStart()
        {
            if (!string.IsNullOrWhiteSpace(_toDoUser.TelegramUserName))
            {
                WriteLine($"Привет, {_toDoUser.TelegramUserName}, я твой бот. Введи команду и получи результат.");
            }
            else
            {
                Write("Представьтесь: ");

                while (string.IsNullOrWhiteSpace(_toDoUser.TelegramUserName))
                {
                    _toDoUser.TelegramUserName = ReadLine();
                }
                WriteLine($"Рад тебя видеть, {_toDoUser.TelegramUserName}.");
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
            WriteLine($"{BotConstants.CommandShowTasks} - Вывести задачи в работе.");
            WriteLine($"{BotConstants.CommandRemoveTask} - Удалить задачу.");
            WriteLine($"{BotConstants.CommandCompleteTask} - Установить статус задачи на Завершена.");
            WriteLine($"{BotConstants.CommandShowAllTasks} - Вывести все задачи.");
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
            // Проверить на максимально допустимое кол-во задач.
            if (_taskCount == maxNumber)
                throw new TaskCountLimitException(maxNumber);

            Write(GetCasePhrase("Введите описание задачи: "));

            var taskDescription = ReadLine();
            ValidateString(taskDescription);

            // Проверить на максисально допустимую длину.
            if (!string.IsNullOrWhiteSpace(taskDescription) && taskDescription.Length > maxTaskDiscriptionLength)
                throw new TaskLengthLimitException(taskDescription.Length, maxTaskDiscriptionLength);

            // Проверить на дубликаты задач.
            var isExists = _toDoItems.Any(t => t.Name == taskDescription);
            if (isExists)
                throw new DuplicateTaskException(taskDescription);

            _toDoItems.Add(new ToDoItem(_toDoUser, taskDescription, ++_taskNumber));
            _taskCount++;
            WriteLine("Задача добавлена.");
        }

        /// <summary>
        /// Вывести задачи в работе.
        /// </summary>
        /// <returns>False - список задач пустой, иначе True.</returns>
        bool CommandShowTasks()
        {
            if (!_toDoItems.Any())
            {
                WriteLine(GetCasePhrase("Список задач пуст."));
                return false;
            }

            WriteLine(GetCasePhrase("Cписок задач:"));
            var tasks = _toDoItems.Where(t => t.State == ToDoItemState.Active);
            foreach (var task in tasks)
                WriteLine($"Задача: {task.Number:d6} - {task.Name} - {task.CreatedAt} - {task.Id}");

            return true;
        }

        void CommandRemoveTask()
        {
            if (!CommandShowTasks())
                return;

            Write("Введите номер задачи для удаления: ");

            var userInput = ReadLine()?.Trim();

            if (!long.TryParse(userInput, out var taskNumber))
            {
                WriteLine(string.Format(BotConstants.MessageNoTaskFoundByNumber, userInput, BotConstants.CommandRemoveTask));
                return;
            }

            var taskToRemove = _toDoItems.Where(t => t.Number == taskNumber).FirstOrDefault();

            if (taskToRemove != null)
            {
                _toDoItems.Remove(taskToRemove);
                WriteLine($"Задача с номером {taskNumber} удалена");
                _taskCount--;
            }
            else
                WriteLine(string.Format(BotConstants.MessageNoTaskFoundByNumber, userInput, BotConstants.CommandRemoveTask));
        }

        /// <summary>
        /// Установить статус задачи на Завершена.
        /// </summary>
        void CommandCompleteTask(string commandArgument)
        {
            var isCommandArgumentEmpty = string.IsNullOrWhiteSpace(commandArgument);
            if (!Guid.TryParse(commandArgument, out var taskGuid) || isCommandArgumentEmpty)
            {
                if (isCommandArgumentEmpty)
                    WriteLine($"Id задачи не указан.");
                else
                    WriteLine($"Id {commandArgument.Trim()} задачи некорректный.");

                return;
            }

            var task = _toDoItems.Where(t => Equals(t.Id, taskGuid)).FirstOrDefault();
            if (task == null)
            {
                WriteLine($"Задача с id = {taskGuid} не найдена.");
                return;
            }

            task.State = ToDoItemState.Completed;
            task.StateChangedAt = DateTime.Now;
            WriteLine($"Задача с Id {taskGuid} завершена.");
        }

        /// <summary>
        /// Отобразить все задачи.
        /// </summary>
        void CommandShowAllTasks()
        {
            if (!_toDoItems.Any())
                WriteLine(GetCasePhrase("Список задач пуст."));

            WriteLine(GetCasePhrase("Cписок задач:"));
            foreach (var task in _toDoItems)
                WriteLine($"({task.State}) - {task.Number:d6} - {task.Name} - {task.CreatedAt} - {task.Id}");
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

            if (!string.IsNullOrWhiteSpace(_toDoUser.TelegramUserName))
                return $"{_toDoUser.TelegramUserName}, {char.ToLower(firstChar)}{subPhrase}";

            return $"{char.ToUpper(firstChar)}{subPhrase}";
        }

        /// <summary>
        /// Получить из строки число (long) и проверить, что число входит в диапазон от min до max (включая границы).
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <param name="min">Минимальное значение.</param>
        /// <param name="max">Максимальное значение.</param>
        /// <returns>Число (long).</returns>
        /// <exception cref="ArgumentException"></exception>
        long ParseAndValidateLong(string? str, long min, long max)
        {
            ValidateString(str);
            long.TryParse(str, out var value);
            if (value < min || value > max)
                throw new ArgumentException($"Значение должно быть от {min} до {max}.");

            return value;
        }

        /// <summary>
        /// Проверить строку, что строка не пустая или равна null и не состоит только из символов-разделителей.
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <exception cref="ArgumentException"></exception>
        static void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка пустая или состоит только из других пробельных символов.");
        }

        #endregion
    }
}
