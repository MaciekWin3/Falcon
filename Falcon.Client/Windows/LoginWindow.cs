using Terminal.Gui;

namespace Falcon.Client.Windows
{
    public class LoginWindow : Window
    {
        public Func<(string name, string password), Task<string>> OnAuthorize { get; set; }
        public Action<string> OnLogin { get; set; }
        public Action OnExit { get; set; }

        public LoginWindow() : base("Login")
        {
            X = Pos.Center();
            Y = Pos.Center();
            Width = Dim.Percent(100);
            Height = Dim.Percent(100);
            Setup();
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
            loginButton.Clicked += async () =>
            {
                if (nameText.Text.ToString().TrimStart().Length == 0)
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Name cannot be empty.", "Ok");
                    return;
                }

                if (string.IsNullOrEmpty(passwordText.Text.ToString()))
                {
                    MessageBox.ErrorQuery(24, 8, "Error", "Invalid credentials", "Ok");
                    return;
                }

                var token = await OnAuthorize.Invoke((name: nameText.Text.ToString(), password: passwordText.Text.ToString()));
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