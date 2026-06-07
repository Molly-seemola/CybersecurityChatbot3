using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    public partial class MainWindow : Window
    {
        private readonly ChatbotEngine _bot = new();
        private readonly NlpEngine _nlp = new();
        private readonly QuizManager _quiz = new();
        private readonly ActivityLog _log = new();
        private List<TaskItem> _tasks = new();

        // Track if we're waiting for a reminder response after adding a task
        private bool _awaitingReminderResponse = false;
        private TaskItem? _pendingTask = null;

        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.InitialiseDatabase();
            LoadTasksFromDb();
            AddBotMessage("👋 Welcome to CyberBot! What's your name?");
        }

        // ═══════════════════════════════════════════════════════════════
        //  NAVIGATION
        // ═══════════════════════════════════════════════════════════════
        private void ShowPanel(UIElement panel)
        {
            ChatPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Collapsed;
            LogPanel.Visibility = Visibility.Collapsed;
            panel.Visibility = Visibility.Visible;
        }

        private void BtnChat_Click(object s, RoutedEventArgs e) { ShowPanel(ChatPanel); _log.Add("Navigated to Chat"); RefreshLog(); }
        private void BtnTasks_Click(object s, RoutedEventArgs e) { ShowPanel(TasksPanel); _log.Add("Navigated to Task Manager"); RefreshLog(); RefreshTaskList(); }
        private void BtnQuiz_Click(object s, RoutedEventArgs e) { ShowPanel(QuizPanel); _log.Add("Navigated to Quiz"); RefreshLog(); }
        private void BtnLog_Click(object s, RoutedEventArgs e) { ShowPanel(LogPanel); RefreshLog(); }

        // ═══════════════════════════════════════════════════════════════
        //  CHAT
        // ═══════════════════════════════════════════════════════════════
        private void ChatInput_KeyDown(object s, KeyEventArgs e) { if (e.Key == Key.Enter) ProcessChat(); }
        private void BtnSend_Click(object s, RoutedEventArgs e) => ProcessChat();

        private void ProcessChat()
        {
            string input = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;
            AddUserMessage(input);
            ChatInput.Clear();

            // Step 1: capture name if not set
            if (string.IsNullOrEmpty(_bot.GetUserName()))
            {
                _bot.SetUserName(input);
                AddBotMessage($"Nice to meet you, {input}! 😊 Type 'help' to see what I can do.");
                _log.Add($"User set name: {input}");
                return;
            }

            // Step 2: if awaiting reminder confirmation
            if (_awaitingReminderResponse)
            {
                HandleReminderConfirmation(input);
                return;
            }

            // Step 3: sentiment check
            string sentiment = _bot.GetSentimentResponse(input);
            if (sentiment != null) { AddBotMessage(sentiment); return; }

            // Step 4: NLP intent parsing
            var parsed = _nlp.Parse(input);
            _log.Add($"Chat input: \"{input}\" → Intent: {parsed.Intent}");

            switch (parsed.Intent)
            {
                case NlpEngine.Intent.Greeting:
                    AddBotMessage(_bot.GetGreeting());
                    break;

                case NlpEngine.Intent.Farewell:
                    AddBotMessage("👋 Goodbye! Stay cyber-safe out there!");
                    break;

                case NlpEngine.Intent.Thanks:
                    AddBotMessage("You're welcome! 😊 Stay safe online.");
                    break;

                case NlpEngine.Intent.ShowHelp:
                    AddBotMessage(_bot.GetHelpText());
                    break;

                case NlpEngine.Intent.CyberTopic:
                    AddBotMessage(_bot.GetCyberResponse(parsed.ExtractedContent));
                    break;

                case NlpEngine.Intent.AddTask:
                    HandleAddTaskFromChat(parsed);
                    break;

                case NlpEngine.Intent.SetReminder:
                    HandleSetReminderFromChat(parsed);
                    break;

                case NlpEngine.Intent.ViewTasks:
                    ShowTasksSummaryInChat();
                    break;

                case NlpEngine.Intent.CompleteTask:
                    HandleCompleteTaskFromChat(parsed.ExtractedContent);
                    break;

                case NlpEngine.Intent.DeleteTask:
                    HandleDeleteTaskFromChat(parsed.ExtractedContent);
                    break;

                case NlpEngine.Intent.StartQuiz:
                    ShowPanel(QuizPanel);
                    AddBotMessage("Opening the quiz now! 📝");
                    _log.Add("Quiz opened via chat command");
                    break;

                case NlpEngine.Intent.ShowActivityLog:
                    string summary = _log.GetSummary();
                    AddBotMessage($"📋 Here's a summary of recent actions:\n\n{summary}");
                    break;

                default:
                    AddBotMessage("🤔 I didn't quite understand that. Try rephrasing or type 'help' to see available commands.");
                    break;
            }
        }

        // ─── Task via chat ───────────────────────────────────────────────────
        private void HandleAddTaskFromChat(NlpEngine.ParsedIntent parsed)
        {
            string title = string.IsNullOrWhiteSpace(parsed.ExtractedContent)
                ? "New Task" : CapitaliseFirst(parsed.ExtractedContent);

            _pendingTask = new TaskItem(title, "Added via chat");

            if (!string.IsNullOrEmpty(parsed.ReminderTime))
            {
                _pendingTask.ReminderInfo = parsed.ReminderTime;
                SaveTask(_pendingTask);
                AddBotMessage($"✅ Task added: '{title}' with reminder set for {parsed.ReminderTime}.");
                _log.Add($"Task added: '{title}' (Reminder: {parsed.ReminderTime})");
                _pendingTask = null;
            }
            else
            {
                // Ask if they want a reminder
                _awaitingReminderResponse = true;
                AddBotMessage($"✅ Task added: '{title}'. Would you like to set a reminder for this task? (e.g. 'yes, in 3 days' or 'no')");
                _log.Add($"Task added: '{title}' (no reminder set yet)");
                SaveTask(_pendingTask);
            }
        }

        private void HandleReminderConfirmation(string input)
        {
            string lower = input.ToLower();
            _awaitingReminderResponse = false;

            if (lower.Contains("no") || lower.Contains("skip") || lower.Contains("nope"))
            {
                AddBotMessage("👍 Got it! No reminder set.");
                _pendingTask = null;
                return;
            }

            // Extract reminder time from the confirmation
            var parsed = _nlp.Parse(input);
            string reminder = parsed.ReminderTime ?? input.Trim();

            if (_pendingTask != null)
            {
                // Update DB with reminder
                _pendingTask.ReminderInfo = reminder;
                // Reload and update last task
                _tasks = DatabaseHelper.GetAllTasks();
                if (_tasks.Count > 0)
                {
                    var last = _tasks[^1];
                    DatabaseHelper.DeleteTask(last.Id);
                    last.ReminderInfo = reminder;
                    DatabaseHelper.AddTask(last);
                }
                _log.Add($"Reminder set for '{_pendingTask.Title}': {reminder}");
                AddBotMessage($"⏰ Got it! I'll remind you {reminder}.");
                _pendingTask = null;
                LoadTasksFromDb();
                RefreshTaskList();
            }
        }

        private void HandleSetReminderFromChat(NlpEngine.ParsedIntent parsed)
        {
            string time = parsed.ReminderTime ?? parsed.ExtractedContent ?? "at an unspecified time";
            string content = parsed.ExtractedContent ?? "your task";
            AddBotMessage($"⏰ Reminder set for '{CapitaliseFirst(content)}' — {time}.");
            _log.Add($"Reminder set: '{content}' — {time}");

            // Add as task too
            var task = new TaskItem(CapitaliseFirst(content), "Reminder task", time);
            SaveTask(task);
        }

        private void ShowTasksSummaryInChat()
        {
            if (_tasks.Count == 0)
            {
                AddBotMessage("You have no tasks yet. Type 'add task - [task name]' to add one!");
                return;
            }
            var sb = new System.Text.StringBuilder("📋 Your tasks:\n\n");
            for (int i = 0; i < _tasks.Count; i++)
                sb.AppendLine($"{i + 1}. {_tasks[i]}");
            AddBotMessage(sb.ToString());
        }

        private void HandleCompleteTaskFromChat(string content)
        {
            if (int.TryParse(content.Trim(), out int num) && num >= 1 && num <= _tasks.Count)
            {
                var task = _tasks[num - 1];
                DatabaseHelper.CompleteTask(task.Id);
                _log.Add($"Task completed: '{task.Title}'");
                LoadTasksFromDb(); RefreshTaskList();
                AddBotMessage($"✔ Task '{task.Title}' marked as completed!");
            }
            else
                AddBotMessage("Please specify the task number. E.g. 'Complete task 1'. Type 'show tasks' to see the list.");
        }

        private void HandleDeleteTaskFromChat(string content)
        {
            if (int.TryParse(content.Trim(), out int num) && num >= 1 && num <= _tasks.Count)
            {
                var task = _tasks[num - 1];
                DatabaseHelper.DeleteTask(task.Id);
                _log.Add($"Task deleted: '{task.Title}'");
                LoadTasksFromDb(); RefreshTaskList();
                AddBotMessage($"🗑 Task '{task.Title}' deleted.");
            }
            else
                AddBotMessage("Please specify the task number. E.g. 'Delete task 2'. Type 'show tasks' to see the list.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  TASK MANAGER PANEL
        // ═══════════════════════════════════════════════════════════════
        private void BtnAddTask_Click(object s, RoutedEventArgs e)
        {
            string title = TxtTaskTitle.Text.Trim();
            string desc = TxtTaskDesc.Text.Trim();
            string rem = TxtReminder.Text.Trim();

            if (string.IsNullOrEmpty(title)) { MessageBox.Show("Please enter a task title."); return; }

            var task = new TaskItem(title, desc, string.IsNullOrEmpty(rem) ? null : rem);
            SaveTask(task);
            _log.Add($"Task added via UI: '{title}'" + (string.IsNullOrEmpty(rem) ? "" : $" (Reminder: {rem})"));

            TxtTaskTitle.Clear(); TxtTaskDesc.Clear(); TxtReminder.Clear();
            RefreshTaskList();
        }

        private void BtnCompleteTask_Click(object s, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskItem selected)
            {
                DatabaseHelper.CompleteTask(selected.Id);
                _log.Add($"Task completed via UI: '{selected.Title}'");
                LoadTasksFromDb(); RefreshTaskList();
            }
        }

        private void BtnDeleteTask_Click(object s, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskItem selected)
            {
                DatabaseHelper.DeleteTask(selected.Id);
                _log.Add($"Task deleted via UI: '{selected.Title}'");
                LoadTasksFromDb(); RefreshTaskList();
            }
        }

        private void SaveTask(TaskItem task)
        {
            int id = DatabaseHelper.AddTask(task);
            task.Id = id;
            LoadTasksFromDb();
            RefreshTaskList();
        }

        private void LoadTasksFromDb()
        {
            _tasks = DatabaseHelper.GetAllTasks();
        }

        private void RefreshTaskList()
        {
            TaskListView.Items.Clear();
            foreach (var t in _tasks)
                TaskListView.Items.Add(t);
        }

        // ═══════════════════════════════════════════════════════════════
        //  QUIZ
        // ═══════════════════════════════════════════════════════════════
        private void BtnStartQuiz_Click(object s, RoutedEventArgs e)
        {
            _quiz.Start();
            BtnStartQuiz.Visibility = Visibility.Collapsed;
            _log.Add("Quiz started");
            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            QuizOptionsPanel.Children.Clear();
            TxtQuizFeedback.Visibility = Visibility.Collapsed;

            var q = _quiz.GetCurrentQuestion();
            if (q == null)
            {
                string feedback = _quiz.GetFinalFeedback();
                TxtQuizQuestion.Text = $"🎉 Quiz Complete!\nScore: {_quiz.Score}/{_quiz.TotalQuestions}\n\n{feedback}";
                TxtQuizStatus.Text = "";
                _log.Add($"Quiz completed — Score: {_quiz.Score}/{_quiz.TotalQuestions}");
                BtnStartQuiz.Content = "🔄  Restart Quiz";
                BtnStartQuiz.Visibility = Visibility.Visible;
                return;
            }

            TxtQuizStatus.Text = $"Question {_quiz.CurrentIndex + 1} of {_quiz.TotalQuestions}  |  Score: {_quiz.Score}  |  Type: {(q.Type == QuestionType.TrueFalse ? "True/False" : "Multiple Choice")}";
            TxtQuizQuestion.Text = q.Question;

            for (int i = 0; i < q.Options.Count; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Content = $"{(q.Type == QuestionType.TrueFalse ? q.Options[i] : $"{(char)('A' + i)}.  {q.Options[i]}")}",
                    Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                    Foreground = Brushes.White,
                    FontSize = 13,
                    Height = 44,
                    Margin = new Thickness(0, 4, 0, 4),
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(14, 0, 0, 0)
                };
                btn.Template = MakeRoundedTemplate();
                btn.Click += (_, _) => AnswerSelected(idx);
                QuizOptionsPanel.Children.Add(btn);
            }
        }

        private void AnswerSelected(int idx)
        {
            // Disable all buttons
            foreach (var child in QuizOptionsPanel.Children)
                if (child is Button b) b.IsEnabled = false;

            var (correct, explanation) = _quiz.SubmitAnswer(idx);
            TxtQuizFeedback.Text = correct
                ? $"✅ Correct!\n{explanation}"
                : $"❌ Incorrect.\n{explanation}";
            TxtQuizFeedback.Foreground = correct
                ? new SolidColorBrush(Color.FromRgb(166, 227, 161))
                : new SolidColorBrush(Color.FromRgb(243, 139, 168));
            TxtQuizFeedback.Visibility = Visibility.Visible;
            _log.Add($"Quiz Q{_quiz.CurrentIndex}: {(correct ? "Correct" : "Incorrect")}");

            // Auto-advance after 2 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (_, _) => { timer.Stop(); ShowCurrentQuestion(); };
            timer.Start();
        }

        // ═══════════════════════════════════════════════════════════════
        //  ACTIVITY LOG
        // ═══════════════════════════════════════════════════════════════
        private void RefreshLog()
        {
            LogListBox.Items.Clear();
            foreach (var entry in _log.GetAll())
                LogListBox.Items.Add(entry);
        }

        private void BtnClearLog_Click(object s, RoutedEventArgs e)
        {
            _log.Clear();
            RefreshLog();
        }

        // ═══════════════════════════════════════════════════════════════
        //  CHAT UI HELPERS
        // ═══════════════════════════════════════════════════════════════
        private void AddUserMessage(string text)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                CornerRadius = new CornerRadius(12, 12, 2, 12),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(80, 4, 12, 4),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };
            ChatStack.Children.Add(border);
            ChatScroll.ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBlock
            {
                Text = "🤖",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 6, 0)
            });
            sp.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 520
            });

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 58)),
                CornerRadius = new CornerRadius(12, 12, 12, 2),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(12, 4, 80, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = sp
            };
            ChatStack.Children.Add(border);
            ChatScroll.ScrollToBottom();
        }

        // ═══════════════════════════════════════════════════════════════
        //  UTILITIES
        // ═══════════════════════════════════════════════════════════════
        private ControlTemplate MakeRoundedTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(ContentPresenter.MarginProperty, new Thickness(14, 0, 0, 0));
            factory.AppendChild(content);
            template.VisualTree = factory;
            return template;
        }

        private string CapitaliseFirst(string s)
            => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
    }
}