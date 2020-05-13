using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DipPrakt
{
    class Program
    {
        public Program()
        {

            jiraLog = JiraLogin();

        }
        static Atlassian.Jira.Jira jiraLog;
        /// <summary>
        /// Метод для подключения к Jira
        /// </summary>
        static Atlassian.Jira.Jira JiraLogin()
        {
            string login;
            string APItoken;
            string URL;
            bool errLogFlag = false;
            Atlassian.Jira.Jira jiraLog = null;
            Console.WriteLine("Для продолжения работы необходимо войти в систему, 1 если использовать тестовые данные, любое другое значение - войти со своим логином");

            if (Console.ReadLine() == "1")
            {
                login = "alexey.kim@itqc.ru";
                APItoken = "0LGTwkc7Nf9o946UImIW15A5";
                URL = "https://itqctest1.atlassian.net/";
            }
            else
            {
                do
                {
                    Console.Write("Введите логин: ");
                    login = Console.ReadLine();

                    Console.Write("Введите API-токен: ");
                    APItoken = Console.ReadLine();

                    Console.Write("Введите ссылку: ");
                    URL = Console.ReadLine();

                    errLogFlag = false;
                    try
                    {
                        jiraLog = Atlassian.Jira.Jira.CreateRestClient(new Atlassian.Jira.Remote.JiraRestClient(URL, login, APItoken));
                        var URLserv = jiraLog.ServerInfo.GetServerInfoAsync().Result.BaseUrl;
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка входа. Проверьте правильность введенных данных.");
                        errLogFlag = true;
                    }
                } while (errLogFlag);
            }
            return jiraLog;
        }

        /// <summary>
        /// Метод для экспорта CSV
        /// </summary>
        static void CSVWork(Dictionary<string, List<string>> filt)
        {
            var issues = IssuesFilter(filt);
            string[] parMas;
            List<IssueWork> issueList = new List<IssueWork>();
            string puth;
            bool errFlag = false;
            foreach (var c in issues)
            {
                parMas = IssuesCheckNull(c);

                IssueWork IssWork = new IssueWork(parMas[0], parMas[1], parMas[2], parMas[3], parMas[4], parMas[5], parMas[6], parMas[7], parMas[8], parMas[9], parMas[10]);

                issueList.Add(IssWork);
            }

            StringBuilder csv = new StringBuilder();
            foreach (var s in issueList)
            {
                csv.AppendLine(Regex.Replace(s.Summary, @"\s+", " ") + ";" +

                    Regex.Replace(s.Key, @"\s+", " ") + ";" +
                    Regex.Replace(s.Priority, @"\s+", " ") + ";" +
                    Regex.Replace(s.Status, @"\s+", " ") + ";" +
                    Regex.Replace(s.Type, @"\s+", " ") + ";" +
                    Regex.Replace(s.Created, @"\s+", " ") + ";" +
                    Regex.Replace(s.Environment, @"\s+", " ") + ";" +
                    Regex.Replace(s.Project, @"\s+", " ") + ";" +
                    Regex.Replace(s.AssigneeUser, @"\s+", " ") + ";" +
                    Regex.Replace(s.ReporterUser, @"\s+", " ") + ";" +
                    Regex.Replace(s.Description, @"\s+", " "));
            }

            do
            {
                errFlag = false;
                Console.Write("Введите путь к файлу: ");
                puth = Console.ReadLine().Trim() + ".csv";
                try
                {
                    File.WriteAllText(puth, csv.ToString(), Encoding.GetEncoding(1251));
                }
                catch
                {
                    Console.WriteLine("Путь не верен, попробуйте ещё раз");
                    errFlag = true;
                }
            } while (errFlag);
        }
        /// <summary>
        /// 
        /// Метод для экспорта в JSON
        /// </summary>
        static void JsonWork(Dictionary<string, List<string>> filt)
        {
            bool errFlag = false;
            var issues = IssuesFilter(filt);//вызов метода фильрации
            string[] parMas;//объявление массива, куда будут записываться значения из Jira
            string puth;//путь к файлу
            List<IssueWork> issueList = new List<IssueWork>();//Создания списка, в который будут записываться объекты класса
            foreach (var c in issues)
            {
                parMas = IssuesCheckNull(c);//Вызов метода проверки на null
                IssueWork IssWork = new IssueWork(parMas[0], parMas[1], parMas[2], parMas[3], parMas[4], parMas[5], parMas[6], parMas[7], parMas[8], parMas[9], parMas[10]);
                issueList.Add(IssWork);//Добавление объекта в список
            }
            do
            {
                errFlag = false;
                Console.Write("Введите путь к файлу: ");
                puth = Console.ReadLine().Trim() + ".json";
                try
                {
                    File.WriteAllText(puth, JsonConvert.SerializeObject(issueList));
                }
                catch
                {
                    Console.WriteLine("Путь не верен, попробуйте ещё раз");
                    errFlag = true;
                }
            } while (errFlag);
        }
        /// <summary>
        /// Метод для фильтрации задач
        /// </summary>
        static List<Atlassian.Jira.Issue> IssuesFilter(Dictionary<string, List<string>> filt)
        {
            var issues = jiraLog.Issues.Queryable.ToList();
            List<Atlassian.Jira.Issue> issuesList = new List<Atlassian.Jira.Issue>();
            if (filt.ContainsKey("summary"))
            {
                issuesList.AddRange(
                    issues.Where(c =>//если условие выполняется - нужная задача записывается список
                    {
                        if (filt["summary"].Where(t => t == c.Summary).Count() > 0 && !issuesList.Contains(c))//Возвращает все задачи, где проект в Jira равен пользовательскому и задача не записана в список
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("key"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["key"].Where(t => t == c.Key.ToString()).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("priority"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["priority"].Where(t => t == c.Priority.ToString()).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();

            }

            if (filt.ContainsKey("status"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["status"].Where(t => t == c.Status.ToString()).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("type"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["type"].Where(t => t == c.Type.ToString()).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("created"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["created"].Where(t => t == c.Created.ToString()).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("environment"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["environment"].Where(t => t == c.Environment.ToString()).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("project"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["project"].Where(t => t == c.Project).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("assigneeuser"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["assigneeuser"].Where(t => t == c.AssigneeUser.DisplayName).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            if (filt.ContainsKey("reporteruser"))
            {
                issuesList.AddRange(
                    issues.Where(c =>
                    {
                        if (filt["reporteruser"].Where(t => t == c.ReporterUser.DisplayName).Count() > 0 && !issuesList.Contains(c))
                            return true;
                        else return false;
                    }).ToList());
                issues.Clear();
                issues.AddRange(issuesList);
                issuesList.Clear();
            }

            issuesList.AddRange(issues);
            return issuesList;
        }
        /// <summary>
        /// Метод для проверки на Null значений из Jira
        /// </summary>
        static string[] IssuesCheckNull(Atlassian.Jira.Issue c)
        {
            string[] parMas = new string[11];//создание массива, в котором будут храниться значения параметров из Jira
            if (c.Summary != null)
            {
                parMas[0] = c.Summary;
            }
            else parMas[0] = "Аннотация не задана";

            if (c.Key != null)
            {
                parMas[1] = c.Key.ToString();
            }
            else parMas[1] = "Ключ не задан";

            if (c.Priority != null)
            {
                parMas[2] = c.Priority.ToString();
            }
            else parMas[2] = "Приоритет не задан";

            if (c.Status != null)
            {
                parMas[3] = c.Status.ToString();
            }
            else parMas[3] = "Статус не задан";

            if (c.Type != null)
            {
                parMas[4] = c.Type.ToString();
            }
            else parMas[4] = "Тип не задан";

            if (c.Created != null)
            {
                parMas[5] = c.Created.ToString();
            }
            else parMas[5] = "Время создания не задано";

            if (c.Environment != null)
            {
                parMas[6] = c.Environment;
            }
            else parMas[6] = "Окружение не задано";

            if (c.Project != null)
            {
                parMas[7] = c.Project;
            }
            else parMas[7] = "Принадлежность к проекту не задана";

            if (c.AssigneeUser != null)
                parMas[8] = c.AssigneeUser.DisplayName;
            else
                parMas[8] = "Исполнитель не задан";

            if (c.ReporterUser != null)
            {
                parMas[9] = c.ReporterUser.DisplayName;
            }
            else parMas[9] = "Создатель не задан";

            if (c.Description != null)
            {
                parMas[10] = c.Description;
            }
            else parMas[10] = "Описание не задано";

            return parMas;
        }
        /// <summary>
        /// Метод для взаимодействия с пользователем
        /// </summary>
        static void UserWork()
        {
            bool errFlag = false;
            int parCount = 0;
            Dictionary<string, List<string>> filt = new Dictionary<string, List<string>>();//объявление словаря с ключами тип string и значением типа List<string>
            string[] availablePar = { "1) summary", "2) key", "3) priority", "4) status", "5) type", "6) created", "7) environment", "8) project", "9) assigneeuser", "10) reporteruser", "0) Получить результат" };//убрал описание
            Program Start = new Program();

            Console.WriteLine("Список доступных параметров: ");

            foreach (var s in availablePar)
                Console.WriteLine(s);

            do
            {
                Console.WriteLine("Выберите параметры для фильтрации: ");
                int id = Convert.ToInt32(Console.ReadLine());//выбор параметров
                if (id != 0)//проверка на существование нулевого элемента
                {
                    var parName = availablePar[id - 1].Remove(0, 3);
                    string value = "";
                    while (value != "0")
                    {
                        Console.Write("Введите значение фильтра " + parName + " (0 - возврат к выбору параметров): ");
                        value = Console.ReadLine();
                        if (value == "0")
                            break;
                        Console.WriteLine();
                        if (!filt.ContainsKey(parName))//проверка на существование ключа
                        {
                            filt.Add(parName, new List<string> { value });
                        }
                        else
                        {
                            filt[parName].Add(value);
                        }
                    }
                    parCount++;
                }
                else
                    break;
            } while (parCount < 10);//проверка ввода
            Console.WriteLine("Ввод окончен");

            do
            {
                errFlag = false;
                Console.Write("В какой документ Вы хотите вывести информацию (1 - JSON, 2 - CSV, 0 - Выход): ");
                int change = 0;
                try
                {
                    change = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Поддерживается только цифровой ввод");
                    errFlag = true;
                    continue;
                }
                if (change.ToString().Contains("1") || change.ToString().Contains("2"))
                {
                    if (change.ToString().Contains("1"))
                        JsonWork(filt);
                    if (change.ToString().Contains("2"))
                        CSVWork(filt);
                }
                else
                {
                    if (change.ToString().Contains("0"))
                    {
                        Console.WriteLine("Остановка работы");
                        break;
                    }

                    Console.WriteLine("Вы не выбрали документ, попробуйте снова.");

                    errFlag = true;
                }
            } while (errFlag);

        }
        static void Main(string[] args)
        {
            string anew;
            UserWork();
            do
            {
                Console.Write("Запустить программу заново? 1 - Да, Другое значение - Нет: ");
                anew = Console.ReadLine().Trim();

                if (anew == "1")
                {
                    Console.WriteLine();
                    UserWork();
                }

            } while (anew == "1");
            Console.WriteLine("Остановка программы");
            Console.ReadKey();
        }
    }

    public class IssueWork
    {
        public IssueWork(string summary, string key, string priority, string status, string type, string created, string environment, string project, string assigneeUser, string reporterUser, string description)
        {
            Summary = summary;
            Key = key;
            Priority = priority;
            Status = status;
            Type = type;
            Created = created;
            Environment = environment;
            Project = project;
            AssigneeUser = assigneeUser;
            ReporterUser = reporterUser;
            Description = description;
        }
        public string Summary { get; set; }
        public string Key { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Created { get; set; }
        public string Environment { get; set; }
        public string Project { get; set; }
        public string ReporterUser { get; set; }
        public string AssigneeUser { get; set; }
        public string Description { get; set; }


    }
}