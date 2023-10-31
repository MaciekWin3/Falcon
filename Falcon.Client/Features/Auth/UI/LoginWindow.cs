using Falcon.Client.Features.Auth.Models;
using Terminal.Gui;

namespace Falcon.Client.Features.Auth.UI
{
    public class LoginWindow : Window
    {
        public Func<User, Task<string>> OnAuthorize { get; set; }
        public Action<string> OnLogin { get; set; }
        public Action OnExit { get; set; }
        public Action OnQuit { get; set; }

        public LoginWindow() : base("Login")
        {
            X = Pos.Center();
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Setup();
        }

        public MenuBar CreateMenuBar()
        {
            return new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("App", new MenuItem []
                {
                    new MenuItem("Quit", "Quit App", () => OnQuit?.Invoke(), null, null, Key.Q | Key.CtrlMask)
                }),
                new MenuBarItem("Help", new MenuItem[]
                {
                    new MenuItem("About", "About", () => MessageBox.Query(50, 5, "Hi!", "Application created by Maciej Winnik", "Ok"), null, null, Key.Q | Key.CtrlMask)
                })
            });
        }

        public void Setup()
        {
            var nameLabel = new Label(0, 1, "Nickname");
            var nameText = new TextField("")
            {
                X = Pos.Left(nameLabel),
                Y = Pos.Top(nameLabel) + 1,
                Width = Dim.Fill(),
            };

            Add(nameLabel);
            Add(nameText);

            var passwordLabel = new Label("Password")
            {
                X = Pos.Left(nameText),
                Y = Pos.Top(nameText) + 1,
                Width = Dim.Fill()
            };

            var passwordText = new TextField("")
            {
                X = Pos.Left(passwordLabel),
                Y = Pos.Top(passwordLabel) + 1,
                Width = Dim.Fill(),
                Secret = true
            };

            Add(passwordLabel);
            Add(passwordText);

            var loginButton = new Button("Login", true)
            {
                Y = Pos.Top(passwordText) + 2,
                X = Pos.Center() - 15
            };

            var exitButton = new Button("Exit")
            {
                Y = Pos.Top(loginButton),
                X = Pos.Center() + 5
            };

            Add(loginButton);
            Add(exitButton);

            var progressBar = new ProgressBar()
            {
                Y = 12,
                X = Pos.Center(),
                Width = 20
            };
            bool Timer(MainLoop caller)
            {
                progressBar.Pulse();
                return true;
            }
            loginButton.Clicked += async () =>
            {
                if (nameText.Text.ToString().TrimStart().Length == 0)
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Name cannot be empty.", "Ok");
                    return;
                }

                if (string.IsNullOrEmpty(passwordText.Text.ToString()))
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Invalid username or password", "Ok");
                    return;
                }

                Add(progressBar);
                var x = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(300), Timer);
                var user = new User(username: nameText.Text.ToString(), password: passwordText.Text.ToString());

                var token = await OnAuthorize.Invoke(user);

                Application.MainLoop.RemoveTimeout(x);
                Remove(progressBar);
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Invalid credentials", "Ok");
                }
                else
                {
                    OnLogin?.Invoke(token);
                }
            };

            exitButton.Clicked += () =>
            {
                OnExit?.Invoke();
            };
        }
    }
}