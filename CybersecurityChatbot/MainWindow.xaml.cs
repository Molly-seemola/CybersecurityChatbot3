using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Main WPF window — the GUI entry point for the entire application.
    /// Combines all features from Parts 1, 2, and 3 in a single cohesive interface.
    ///
    /// Part 1: ASCII art, basic chatbot responses.
    /// Part 2: GUI, keyword recognition, random responses, sentiment, memory/recall, conversation flow.
    /// Part 3 Task 1: Task Manager with MySQL database integration.
    /// Part 3 Task 2: Cybersecurity Quiz mini-game.
    /// Part 3 Task 3: NLP simulation via NlpEngine.
    /// Part 3 Task 4: Activity Log feature.
    ///
    /// Rubric target (Combining Parts — 9-10 marks):
    /// Cohesive integration providing seamless, professional user experience across all features.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Core engine instances ────────────────────────────────────────────
        private readonly ChatbotEngine _bot = new(); // Part 1 & 2: responses, memory, sentiment
        private readonly NlpEngine _nlp = new(); // Part 3 Task 3: intent detection
        private readonly QuizManager _quiz = new(); // Part 3 Task 2: quiz mini-game
        private readonly ActivityLog _log = new(); // Part 3 Task 4: activity tracking
        private List<TaskItem> _tasks = new(); // In-memory task list (synced with DB)

        // ── Reminder conversation state ──────────────────────────────────────
        // Tracks whether the bot is waiting for the user to confirm a reminder
        // after adding a task via chat (Part 3 Task 1 multi-turn conversation).
        private bool _awaitingReminder = false;
        private TaskItem _pendingTask = null;

        /// <summary>
        /// Initialises the main window, sets up the database,
        /// loads existing tasks, and displays the Part 1 ASCII art welcome.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Part 3 Task 1: Create DB and Tasks table if they don't exist
            DatabaseHelper.InitialiseDatabase();

            // Load any previously saved tasks from MySQL
            LoadTasksFromDb();

            // Part 1: Display ASCII art banner in monospace font
            AddBotMessage(ChatbotEngine.AsciiArt, isCode: true);

            // Welcome message — asks for name to enable Part 2 memory/recall
            AddBotMessage("👋 Welcome to CyberBot — your Cybersecurity Awareness Assistant!\n\nWhat's your name?");
        }

        // ════════════════════════════════════════════════════════════════
        //  NAVIGATION — switches between the 4 main panels
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hides all panels and shows only the specified one.
        /// Ensures a clean single-panel view at all times.
        /// </summary>
        private void ShowPanel(UIElement panel)
        {
            ChatPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Collapsed;
            LogPanel.Visibility = Visibility.Collapsed;
            panel.Visibility = Visibility.Visible;
        }

        // Navigation button click handlers — each logs the navigation action
        private void BtnChat_Click(object s, RoutedEventArgs e)
        { ShowPanel(ChatPanel); _log.Add("Navigated to Chat"); RefreshLog(); }

        private void BtnTasks_Click(object s, RoutedEventArgs e)
        { ShowPanel(TasksPanel); _log.Add("Navigated to Task Manager"); RefreshLog(); RefreshTaskList(); }

        private void BtnQuiz_Click(object s, RoutedEventArgs e)
        { ShowPanel(QuizPanel); _log.Add("Navigated to Quiz"); RefreshLog(); }

        private void BtnLog_Click(object s, RoutedEventArgs e)
        { ShowPanel(LogPanel); RefreshLog(); }

        // ════════════════════════════════════════════════════════════════
        //  CHAT — main conversation handler
        // ════════════════════════════════════════════════════════════════

        /// <summary>Sends a chat message when Enter key is pressed.</summary>
        private void ChatInput_KeyDown(object s, KeyEventArgs e)
        { if (e.Key == Key.Enter) ProcessChat(); }

        /// <summary>Sends a chat message when the Send button is clicked.</summary>
        private void BtnSend_Click(object s, RoutedEventArgs e) => ProcessChat();

        /// <summary>
        /// Central chat processing method — handles all user input.
        /// Processes input through a layered pipeline:
        /// 1. Name capture (Part 2: memory)
        /// 2. Reminder confirmation (Part 3 Task 1: multi-turn flow)
        /// 3. Sentiment detection (Part 2)
        /// 4. NLP intent parsing (Part 3 Task 3)
        /// 5. Intent-based response dispatch
        /// </summary>
        private void ProcessChat()
        {
            string input = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return; // Ignore empty input

            // Display user message in chat UI
            AddUserMessage(input);
            ChatInput.Clear();

            // ── Step 1: Capture user name (Part 2 memory/recall) ────────────
            // First message is always the user's name — stored for personalisation
            if (string.IsNullOrEmpty(_bot.GetUserName()))
            {
                _bot.SetUserName(input);
                AddBotMessage($"Nice to meet you, {input}! 😊\n\nI'm here to help you stay safe online. Type 'help' to see everything I can do.");
                _log.Add($"User introduced themselves as: {input}");
                return;
            }

            // ── Step 2: Reminder confirmation (Part 3 Task 1 multi-turn) ────
            // If bot is waiting for reminder confirmation after adding a task
            if (_awaitingReminder) { HandleReminderConfirmation(input); return; }

            // ── Step 3: Sentiment detection (Part 2) ────────────────────────
            // Check for emotional keywords before NLP — sentiment takes priority
            string sentiment = _bot.GetSentimentResponse(input);
            if (sentiment != null)
            {
                AddBotMessage(sentiment);
                _log.Add($"Sentiment response triggered by: \"{input}\"");
                return;
            }

            // ── Step 4: NLP intent parsing (Part 3 Task 3) ──────────────────
            // Parse the user's input to detect intent using keyword matching
            var parsed = _nlp.Parse(input);
            _log.Add($"Chat: \"{input}\" → Detected intent: {parsed.Intent}");

            // ── Step 5: Dispatch based on detected intent ────────────────────
            switch (parsed.Intent)
            {
                // Social intents
                case NlpEngine.Intent.Greeting:
                    // Part 2: personalised greeting using remembered name
                    AddBotMessage(_bot.GetGreeting());
                    break;

                case NlpEngine.Intent.Farewell:
                    AddBotMessage($"👋 Goodbye, {_bot.GetUserName()}! Stay cyber-safe out there. Remember: think before you click!");
                    break;

                case NlpEngine.Intent.Thanks:
                    AddBotMessage($"You're welcome, {_bot.GetUserName()}! 😊 Staying informed is the best defence online.");
                    break;

                case NlpEngine.Intent.ShowHelp:
                    // Part 2: Help text includes memory-based favourite topic suggestion
                    AddBotMessage(_bot.GetHelpText());
                    break;

                // Part 2: Random response for a cybersecurity topic
                case NlpEngine.Intent.CyberTopic:
                    AddBotMessage(_bot.GetCyberResponse(parsed.ExtractedContent));
                    _log.Add($"Cyber topic discussed: {parsed.ExtractedContent}");
                    break;

                // Part 2: Follow-up conversation flow — "tell me more"
                case NlpEngine.Intent.FollowUp:
                    AddBotMessage(_bot.GetFollowUpResponse());
                    _log.Add("Follow-up response provided");
                    break;

                // Part 3 Task 1 + 3: Add task via natural language
                case NlpEngine.Intent.AddTask:
                    HandleAddTaskFromChat(parsed);
                    break;

                // Part 3 Task 3: Set reminder via natural language
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

                // Part 3 Task 2: Open quiz via chat command
                case NlpEngine.Intent.StartQuiz:
                    ShowPanel(QuizPanel);
                    AddBotMessage("Opening the quiz now! 📝 Good luck!");
                    _log.Add("Quiz opened via chat command");
                    break;

                // Part 3 Task 4: Show activity log via chat command
                case NlpEngine.Intent.ShowActivityLog:
                    AddBotMessage($"📋 Here's a summary of recent actions:\n\n{_log.GetSummary()}");
                    break;

                default:
                    // Part 2: Personalised fallback using memory (favourite topic)
                    string fav = _bot.GetFavouriteTopic();
                    string suggestion = string.IsNullOrEmpty(fav)
                        ? "Type 'help' to see all available commands."
                        : $"Maybe I can help with '{fav}'? Or type 'help' to see all options.";
                    AddBotMessage($"🤔 I didn't quite understand that. Could you rephrase?\n{suggestion}");
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  TASK CHAT HANDLERS (Part 3 Task 1 + Task 3)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Handles "add task" intents detected via NLP.
        /// If a reminder time was extracted, saves immediately.
        /// Otherwise asks if the user wants a reminder (multi-turn conversation).
        /// </summary>
        private void HandleAddTaskFromChat(NlpEngine.ParsedIntent parsed)
        {
            // Use extracted content as title, fallback to generic name
            string title = string.IsNullOrWhiteSpace(parsed.ExtractedContent)
                ? "New Cybersecurity Task" : Cap(parsed.ExtractedContent);

            _pendingTask = new TaskItem(title, "Added via chat");

            if (!string.IsNullOrEmpty(parsed.ReminderTime))
            {
                // Reminder was specified inline — save immediately with reminder
                _pendingTask.ReminderInfo = parsed.ReminderTime;
                SaveTask(_pendingTask);
                AddBotMessage($"✅ Task added: '{title}'\n⏰ Reminder set for {parsed.ReminderTime}.");
                _log.Add($"Task added: '{title}' (Reminder: {parsed.ReminderTime})");
                _pendingTask = null;
            }
            else
            {
                // No reminder specified — save task first, then ask about reminder
                SaveTask(_pendingTask);
                _awaitingReminder = true; // Flag to intercept next message
                AddBotMessage($"✅ Task added: '{title}'.\n\nWould you like to set a reminder for this task? (e.g. 'Yes, in 3 days' or 'No')");
                _log.Add($"Task added: '{title}' — awaiting reminder confirmation");
            }
        }

        /// <summary>
        /// Handles the user's response to the reminder confirmation prompt.
        /// Part 3 Task 1: Supports "remind me in X days" conversation as shown in rubric example.
        /// </summary>
        private void HandleReminderConfirmation(string input)
        {
            _awaitingReminder = false; // Reset state flag
            string lower = input.ToLower();

            // User declined reminder
            if (lower.Contains("no") || lower.Contains("skip") || lower.Contains("nope"))
            {
                AddBotMessage("👍 No problem! Your task has been saved without a reminder.");
                _pendingTask = null;
                return;
            }

            // User confirmed — extract the reminder time using NLP
            var parsed = _nlp.Parse(input);
            string reminder = parsed.ReminderTime ?? input.Trim();

            if (_pendingTask != null)
            {
                // Update the last saved task in the DB with the reminder
                _tasks = DatabaseHelper.GetAllTasks();
                if (_tasks.Count > 0)
                {
                    var last = _tasks[^1]; // Most recently added task
                    DatabaseHelper.DeleteTask(last.Id);
                    last.ReminderInfo = reminder;
                    DatabaseHelper.AddTask(last);
                }
                _log.Add($"Reminder set for '{_pendingTask.Title}': {reminder}");
                AddBotMessage($"⏰ Got it! I'll remind you {reminder} about '{_pendingTask.Title}'.");
                _pendingTask = null;
                LoadTasksFromDb();
                RefreshTaskList();
            }
        }

        /// <summary>
        /// Handles "set reminder" intents — creates a task with a reminder attached.
        /// </summary>
        private void HandleSetReminderFromChat(NlpEngine.ParsedIntent parsed)
        {
            string time = parsed.ReminderTime ?? parsed.ExtractedContent ?? "at an unspecified time";
            string content = Cap(parsed.ExtractedContent ?? "your task");
            var task = new TaskItem(content, "Reminder via chat", time);
            SaveTask(task);
            AddBotMessage($"⏰ Reminder set for '{content}' — {time}.");
            _log.Add($"Reminder set: '{content}' — {time}");
        }

        /// <summary>Displays a numbered list of all current tasks in the chat.</summary>
        private void ShowTasksSummaryInChat()
        {
            if (_tasks.Count == 0)
            {
                AddBotMessage("You have no tasks yet! Type 'add task - [task name]' to get started.");
                return;
            }
            var sb = new System.Text.StringBuilder("📋 Your current tasks:\n\n");
            for (int i = 0; i < _tasks.Count; i++)
                sb.AppendLine($"{i + 1}. {_tasks[i]}");
            AddBotMessage(sb.ToString());
        }

        /// <summary>
        /// Marks a task as complete by number (e.g. "complete task 1").
        /// Syncs the change to MySQL and refreshes the UI.
        /// </summary>
        private void HandleCompleteTaskFromChat(string content)
        {
            if (int.TryParse(content.Trim(), out int num) && num >= 1 && num <= _tasks.Count)
            {
                var t = _tasks[num - 1];
                DatabaseHelper.CompleteTask(t.Id); // UPDATE in DB
                _log.Add($"Task completed: '{t.Title}'");
                LoadTasksFromDb();
                RefreshTaskList();
                AddBotMessage($"✔ Task '{t.Title}' marked as completed! Great work! 🎉");
            }
            else
                AddBotMessage("Please include the task number — e.g. 'Complete task 1'. Type 'show tasks' to see your list.");
        }

        /// <summary>
        /// Deletes a task by number (e.g. "delete task 2").
        /// Syncs the deletion to MySQL and refreshes the UI.
        /// </summary>
        private void HandleDeleteTaskFromChat(string content)
        {
            if (int.TryParse(content.Trim(), out int num) && num >= 1 && num <= _tasks.Count)
            {
                var t = _tasks[num - 1];
                DatabaseHelper.DeleteTask(t.Id); // DELETE from DB
                _log.Add($"Task deleted: '{t.Title}'");
                LoadTasksFromDb();
                RefreshTaskList();
                AddBotMessage($"🗑 Task '{t.Title}' has been deleted.");
            }
            else
                AddBotMessage("Please include the task number — e.g. 'Delete task 2'. Type 'show tasks' to see your list.");
        }

        // ════════════════════════════════════════════════════════════════
        //  TASK MANAGER PANEL (Part 3 Task 1)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Adds a new task from the Task Manager form (GUI button).
        /// Validates that a title was entered before saving to the DB.
        /// </summary>
        private void BtnAddTask_Click(object s, RoutedEventArgs e)
        {
            string title = TxtTaskTitle.Text.Trim();
            string desc = TxtTaskDesc.Text.Trim();
            string rem = TxtReminder.Text.Trim();

            // Validate required field
            if (string.IsNullOrEmpty(title))
            { MessageBox.Show("Please enter a task title.", "CyberBot"); return; }

            var task = new TaskItem(title, desc, string.IsNullOrEmpty(rem) ? null : rem);
            SaveTask(task); // Saves to DB and refreshes list

            _log.Add($"Task added via UI: '{title}'" +
                     (string.IsNullOrEmpty(rem) ? "" : $" (Reminder: {rem})"));

            // Clear form fields after successful add
            TxtTaskTitle.Clear();
            TxtTaskDesc.Clear();
            TxtReminder.Clear();
        }

        /// <summary>Marks the selected task as complete from the Task Manager UI.</summary>
        private void BtnCompleteTask_Click(object s, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskItem t)
            {
                DatabaseHelper.CompleteTask(t.Id);
                _log.Add($"Task completed via UI: '{t.Title}'");
                LoadTasksFromDb();
                RefreshTaskList();
            }
            else MessageBox.Show("Please select a task first.", "CyberBot");
        }

        /// <summary>Deletes the selected task from the Task Manager UI and DB.</summary>
        private void BtnDeleteTask_Click(object s, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is TaskItem t)
            {
                DatabaseHelper.DeleteTask(t.Id);
                _log.Add($"Task deleted via UI: '{t.Title}'");
                LoadTasksFromDb();
                RefreshTaskList();
            }
            else MessageBox.Show("Please select a task first.", "CyberBot");
        }

        /// <summary>
        /// Saves a task to MySQL and reloads the in-memory list and UI.
        /// Central save method used by both chat and UI task handlers.
        /// </summary>
        private void SaveTask(TaskItem task)
        {
            int id = DatabaseHelper.AddTask(task); // INSERT into DB
            task.Id = id;                            // Update in-memory object with DB Id
            LoadTasksFromDb();
            RefreshTaskList();
        }

        /// <summary>Reloads the task list from MySQL into the in-memory _tasks list.</summary>
        private void LoadTasksFromDb() => _tasks = DatabaseHelper.GetAllTasks();

        /// <summary>Refreshes the TaskListView control to reflect the current _tasks list.</summary>
        private void RefreshTaskList()
        {
            TaskListView.Items.Clear();
            foreach (var t in _tasks) TaskListView.Items.Add(t);
        }

        // ════════════════════════════════════════════════════════════════
        //  QUIZ PANEL (Part 3 Task 2)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Starts a new quiz session when the Start/Restart button is clicked.
        /// Resets score and question index, then shows the first question.
        /// </summary>
        private void BtnStartQuiz_Click(object s, RoutedEventArgs e)
        {
            _quiz.Start();
            BtnStartQuiz.Visibility = Visibility.Collapsed; // Hide start button during quiz
            _log.Add("Quiz started");
            ShowCurrentQuestion();
        }

        /// <summary>
        /// Renders the current quiz question and answer option buttons.
        /// Shows the question type (MC or T/F) and question number in the status bar.
        /// Displays final score and feedback when all questions are answered.
        /// </summary>
        private void ShowCurrentQuestion()
        {
            // Clear previous question UI
            QuizOptionsPanel.Children.Clear();
            TxtQuizFeedback.Visibility = Visibility.Collapsed;

            var q = _quiz.GetCurrentQuestion();

            // All questions answered — show results
            if (q == null)
            {
                string fb = _quiz.GetFinalFeedback(); // Score-based feedback message
                TxtQuizQuestion.Text =
                    $"🎉 Quiz Complete!\n\nScore: {_quiz.Score} / {_quiz.TotalQuestions}\n\n{fb}";
                TxtQuizStatus.Text = "";
                _log.Add($"Quiz completed — Score: {_quiz.Score}/{_quiz.TotalQuestions}");

                // Show restart button
                BtnStartQuiz.Content = "🔄  Restart Quiz";
                BtnStartQuiz.Visibility = Visibility.Visible;
                return;
            }

            // Show question type and progress
            string type = q.Type == QuestionType.TrueFalse ? "True/False" : "Multiple Choice";
            TxtQuizStatus.Text = $"Question {_quiz.CurrentIndex + 1} of {_quiz.TotalQuestions}  |  Score: {_quiz.Score}  |  {type}";
            TxtQuizQuestion.Text = q.Question;

            // Dynamically generate answer option buttons
            for (int i = 0; i < q.Options.Count; i++)
            {
                int idx = i; // Capture loop variable for click handler closure

                // Label: "True"/"False" for T/F, "A. ..."/"B. ..." for MC
                string label = q.Type == QuestionType.TrueFalse
                    ? q.Options[i]
                    : $"{(char)('A' + i)}.  {q.Options[i]}";

                var btn = new Button
                {
                    Content = label,
                    Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                    Foreground = Brushes.White,
                    FontSize = 13,
                    Height = 44,
                    Margin = new Thickness(0, 4, 0, 4),
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(14, 0, 0, 0),
                    Template = RoundedTemplate() // Apply rounded corner style
                };

                // Attach click handler with captured index
                btn.Click += (_, _) => AnswerSelected(idx);
                QuizOptionsPanel.Children.Add(btn);
            }
        }

        /// <summary>
        /// Processes the user's answer selection.
        /// Disables all buttons to prevent double-answering,
        /// shows coloured feedback (green = correct, red = incorrect),
        /// then automatically advances to the next question after 2.5 seconds.
        /// </summary>
        private void AnswerSelected(int idx)
        {
            // Disable all option buttons immediately to prevent re-clicking
            foreach (var child in QuizOptionsPanel.Children)
                if (child is Button b) b.IsEnabled = false;

            // Submit answer and get result
            var (correct, explanation) = _quiz.SubmitAnswer(idx);

            // Show feedback text with appropriate colour
            TxtQuizFeedback.Text = correct
                ? $"✅ Correct!\n{explanation}"
                : $"❌ Incorrect.\n{explanation}";
            TxtQuizFeedback.Foreground = correct
                ? new SolidColorBrush(Color.FromRgb(166, 227, 161)) // Green
                : new SolidColorBrush(Color.FromRgb(243, 139, 168)); // Red
            TxtQuizFeedback.Visibility = Visibility.Visible;

            _log.Add($"Quiz Q{_quiz.CurrentIndex}: {(correct ? "Correct" : "Incorrect")}");

            // Auto-advance to next question after 2.5 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            { Interval = TimeSpan.FromSeconds(2.5) };
            timer.Tick += (_, _) => { timer.Stop(); ShowCurrentQuestion(); };
            timer.Start();
        }

        // ════════════════════════════════════════════════════════════════
        //  ACTIVITY LOG PANEL (Part 3 Task 4)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Refreshes the Activity Log ListBox with all current log entries.
        /// Part 3 Task 4: Displays all actions with timestamps in the Log panel.
        /// Called whenever the log panel is opened or an entry is cleared.
        /// </summary>
        private void RefreshLog()
        {
            LogListBox.Items.Clear();
            foreach (var entry in _log.GetAll()) // GetAll() = "show more" full history
                LogListBox.Items.Add(entry);
        }

        /// <summary>Clears all activity log entries and refreshes the display.</summary>
        private void BtnClearLog_Click(object s, RoutedEventArgs e)
        { _log.Clear(); RefreshLog(); }

        // ════════════════════════════════════════════════════════════════
        //  CHAT UI HELPERS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates and adds a user message bubble to the chat (right-aligned, dark purple).
        /// </summary>
        private void AddUserMessage(string text)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(49, 50, 68)),
                CornerRadius = new CornerRadius(12, 12, 2, 12), // Rounded except bottom-right
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(80, 4, 12, 4),
                HorizontalAlignment = HorizontalAlignment.Right,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap
                }
            };
            ChatStack.Children.Add(border);
            ChatScroll.ScrollToBottom(); // Always scroll to newest message
        }

        /// <summary>
        /// Creates and adds a bot message bubble to the chat (left-aligned, dark blue).
        /// Supports a monospace code style for ASCII art display (Part 1).
        /// </summary>
        /// <param name="text">The message text to display.</param>
        /// <param name="isCode">If true, uses Consolas font for ASCII art rendering.</param>
        private void AddBotMessage(string text, bool isCode = false)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(205, 214, 244)),
                // Part 1: Use Consolas for ASCII art, Segoe UI for normal messages
                FontSize = isCode ? 11 : 13,
                FontFamily = isCode ? new FontFamily("Consolas") : new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 550
            };

            // Add robot emoji icon for normal messages (not for ASCII art)
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            if (!isCode)
                sp.Children.Add(new TextBlock
                {
                    Text = "🤖",
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 2, 8, 0)
                });
            sp.Children.Add(tb);

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 58)),
                CornerRadius = new CornerRadius(12, 12, 12, 2), // Rounded except bottom-left
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(12, 4, 80, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = sp
            };
            ChatStack.Children.Add(border);
            ChatScroll.ScrollToBottom();
        }

        // ════════════════════════════════════════════════════════════════
        //  UTILITIES
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Generates a reusable rounded corner ControlTemplate for quiz buttons.
        /// Created programmatically since quiz buttons are generated dynamically.
        /// </summary>
        private ControlTemplate RoundedTemplate()
        {
            var t = new ControlTemplate(typeof(Button));
            var fef = new FrameworkElementFactory(typeof(Border));

            // Bind background to the button's Background property
            fef.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            fef.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));

            // Content presenter with left-aligned padding
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            cp.SetValue(ContentPresenter.MarginProperty, new Thickness(14, 0, 0, 0));
            fef.AppendChild(cp);
            t.VisualTree = fef;
            return t;
        }

        /// <summary>
        /// Capitalises the first letter of a string.
        /// Used to format extracted task titles from user input.
        /// </summary>
        private string Cap(string s)
            => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
    }
}