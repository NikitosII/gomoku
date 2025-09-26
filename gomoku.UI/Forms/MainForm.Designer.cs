using System.Windows.Controls;
using System.Windows.Input;

namespace gomoku.UI.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private PictureBox boardPictureBox;
        private Button newGameButton;
        private Label statusLabel;
        private ComboBox difficultyComboBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.boardPictureBox = new PictureBox();
            this.newGameButton = new Button();
            this.statusLabel = new Label();
            this.difficultyComboBox = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)this.boardPictureBox).BeginInit();
            this.SuspendLayout();

            // boardPictureBox
            this.boardPictureBox.BackColor = Color.BurlyWood;
            this.boardPictureBox.Location = new Point(20, 20);
            this.boardPictureBox.Name = "boardPictureBox";
            this.boardPictureBox.Size = new Size(600, 600);
            this.boardPictureBox.TabIndex = 0;
            this.boardPictureBox.TabStop = false;
            this.boardPictureBox.Paint += new PaintEventHandler(this.BoardPictureBox_Paint);
            this.boardPictureBox.MouseClick += new MouseEventHandler(this.BoardPictureBox_MouseClick);

            // newGameButton
            this.newGameButton.Location = new Point(640, 20);
            this.newGameButton.Name = "newGameButton";
            this.newGameButton.Size = new Size(120, 30);
            this.newGameButton.TabIndex = 1;
            this.newGameButton.Text = "New Game";
            this.newGameButton.UseVisualStyleBackColor = true;
            this.newGameButton.Click += new EventHandler(this.NewGameButton_Click);

            // difficultyComboBox
            this.difficultyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.difficultyComboBox.FormattingEnabled = true;
            this.difficultyComboBox.Items.AddRange(new object[] { "Easy", "Medium", "Hard" });
            this.difficultyComboBox.Location = new Point(640, 70);
            this.difficultyComboBox.Name = "difficultyComboBox";
            this.difficultyComboBox.Size = new Size(120, 23);
            this.difficultyComboBox.TabIndex = 2;
            this.difficultyComboBox.SelectedIndex = 1;

            // statusLabel
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new Point(640, 120);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(78, 15);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "Your turn (Black)";

            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 641);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.difficultyComboBox);
            this.Controls.Add(this.newGameButton);
            this.Controls.Add(this.boardPictureBox);
            this.Name = "MainForm";
            this.Text = "Gomoku - Five in a Row";
            ((System.ComponentModel.ISupportInitialize)this.boardPictureBox).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}
