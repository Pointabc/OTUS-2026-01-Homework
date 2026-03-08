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
            try
            {
                #region Добавить ограничение на максимальное количество задач.

                WriteLine("Введите максимально допустимое количество задач:");

                var inputMaxTaskNumber = ReadLine();
                ValidateString(inputMaxTaskNumber);
                var maxTaskNumber = ParseAndValidateLong(inputMaxTaskNumber, BotConstants.MinTaskNumber, BotConstants.MaxTaskNumber);

                if (maxTaskNumber < BotConstants.MinTaskNumber || maxTaskNumber > BotConstants.MaxTaskNumber)
                    throw new ArgumentException($"Максимально допустимое количество задач должно быть в диапазоне от {BotConstants.MinTaskNumber} до {BotConstants.MaxTaskNumber}.");

                Task.maxNumber = maxTaskNumber;

                #endregion

                #region Добавить ограничение на максимальную длину задачи.

                WriteLine("Введите максимально допустимую длину задачи:");
                var inputMaxTaskDiscriptionLength = ReadLine();
                ValidateString(inputMaxTaskDiscriptionLength);
                var maxTaskDiscriptionLength = ParseAndValidateLong(inputMaxTaskDiscriptionLength, BotConstants.MinTaskDiscriptioLength, BotConstants.MaxTaskDiscriptionLength);

                if (maxTaskDiscriptionLength < BotConstants.MinTaskDiscriptioLength || maxTaskDiscriptionLength > BotConstants.MaxTaskDiscriptionLength)
                    throw new ArgumentException($"Максимально допустимая длина имени задачи должно быть в диапазоне от {BotConstants.MinTaskDiscriptioLength} до {BotConstants.MinTaskDiscriptioLength}.");

                Task.maxTaskDiscriptionLength = maxTaskDiscriptionLength;

                #endregion

                ShowGreeting();

                while (true)
                {
                    Write("Введите команду: ");
                    string commandFull = ReadLine();
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
            catch (TaskCountLimitException taskCountLimitException)
            {
                // TODO_VS Не очень понятно, что значит "Попадание в catch не должно останавливать работу приложения".
                WriteLine(taskCountLimitException.Message);
            }
            catch (TaskLengthLimitException taskLengthLimitException)
            {
                // TODO_VS Не очень понятно, что значит "Попадание в catch не должно останавливать работу приложения".
                WriteLine(taskLengthLimitException.Message);
            }
            catch (DuplicateTaskException duplicateTaskException)
            {
                // TODO_VS Не очень понятно, что значит "Попадание в catch не должно останавливать работу приложения".
                WriteLine(duplicateTaskException.Message);
            }
            catch (ArgumentException argumentExeption)
            {
                WriteLine(argumentExeption.Message);
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
                    _userName = ReadLine();
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
            // Проверить на максимально допустимое кол-во задач.
            if (_taskNumber == Task.maxNumber)
                throw new TaskCountLimitException(Task.maxNumber);

            Write(GetCasePhrase("Введите описание задачи: "));

            var taskDescription = ReadLine();
            ValidateString(taskDescription);

            // Проверить на максисально допустимую длину.
            if (!string.IsNullOrWhiteSpace(taskDescription) && taskDescription.Length > Task.maxTaskDiscriptionLength)
                throw new TaskLengthLimitException(taskDescription.Length, Task.maxTaskDiscriptionLength);

            // Проверить на дубликаты задач.
            var isExists = _tasks.Any(t => t.Description == taskDescription);
            if (isExists)
                throw new DuplicateTaskException(taskDescription);

            _tasks.Add(new Task(taskDescription, ++_taskNumber));
            WriteLine("Задача добавлена.");
        }

        /// <summary>
        /// Отобразить задачи.
        /// </summary>
        /// <returns>False - список задач пустой, иначе True.</returns>
        bool CommandShowTasks()
        {
            if (!_tasks.Any())
            {
                WriteLine(GetCasePhrase("Список задач пуст."));
                return false;
            }

            WriteLine(GetCasePhrase("Cписок задач:"));
            foreach (var task in _tasks)
                WriteLine($"{task.Number:d6} {task.Description}");

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

            var taskToRemove = _tasks.Where(t => t.Number == taskNumber).FirstOrDefault();

            if (taskToRemove != null)
            {
                _tasks.Remove(taskToRemove);
                WriteLine($"Задача с номером {taskNumber} удалена");
            }
            else
                WriteLine(string.Format(BotConstants.MessageNoTaskFoundByNumber, userInput, BotConstants.CommandRemoveTask));
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
                throw new ArgumentException($"Значение числа должно быть от {min} до {max}.");

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
