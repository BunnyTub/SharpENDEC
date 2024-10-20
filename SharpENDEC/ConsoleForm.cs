using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SharpENDEC
{
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();
        }

        private void ConsoleForm_Load(object sender, EventArgs e)
        {
            Console.SetOut(new TextBoxOutWriter(textBox1));
            Console.SetError(new TextBoxErrorWriter(textBox1));
        }

        public class TextBoxOutWriter : TextWriter
        {
            private readonly TextBox _textBox;
            private readonly StringBuilder _buffer;

            public TextBoxOutWriter(TextBox textBox)
            {
                _textBox = textBox;
                _buffer = new StringBuilder();
                _textBox.Multiline = true;
                _textBox.ScrollBars = ScrollBars.Vertical;
            }

            public override void Write(char value)
            {
                _buffer.Append(value);
                if (_buffer.Length >= 2)
                {
                    Flush();
                }
            }

            public override void Write(string value)
            {
                _buffer.Append(value);
                if (_buffer.Length >= 2)
                {
                    Flush();
                }
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Flush()
            {
                if (_buffer.Length > 0)
                {
                    _textBox.Invoke((MethodInvoker)delegate {
                        _textBox.AppendText(_buffer.ToString());
                        _textBox.ForeColor = Color.Lime;
                    });
                    _buffer.Clear();
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Flush();
                }
                base.Dispose(disposing);
            }
        }

        public class TextBoxInReader : TextReader
        {
            private TextBox _inputTextBox;
            private TextBox _outputTextBox;
            private string _inputBuffer;

            public TextBoxInReader(TextBox inputTextBox, TextBox outputTextBox)
            {
                _inputTextBox = inputTextBox;
                _outputTextBox = outputTextBox;

                // Capture Enter key press in the input TextBox
                _inputTextBox.KeyPress += new KeyPressEventHandler(OnEnterKeyPress);
            }

            private void OnEnterKeyPress(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true; // Prevent the beep sound

                    // Capture the input
                    _inputBuffer = _inputTextBox.Text;

                    // Clear input box and append to output box for feedback
                    _inputTextBox.Clear();
                    _outputTextBox.AppendText(_inputBuffer + Environment.NewLine);
                }
            }

            public override string ReadLine()
            {
                // Wait until the Enter key is pressed (simulate input waiting)
                while (string.IsNullOrEmpty(_inputBuffer))
                {
                    Application.DoEvents(); // Allows processing of other UI events
                }

                string result = _inputBuffer;
                _inputBuffer = null; // Clear the buffer after use

                return result;
            }

            public Encoding Encoding => Encoding.UTF8;
        }

        public class TextBoxErrorWriter : TextWriter
        {
            private readonly TextBox _textBox;
            private readonly StringBuilder _buffer;

            public TextBoxErrorWriter(TextBox textBox)
            {
                _textBox = textBox;
                _buffer = new StringBuilder();
                _textBox.Multiline = true;
                _textBox.ScrollBars = ScrollBars.Vertical;
            }

            public override void Write(char value)
            {
                _buffer.Append(value);
                if (_buffer.Length >= 0)
                {
                    Flush();
                }
            }

            public override void Write(string value)
            {
                _buffer.Append(value);
                if (_buffer.Length >= 0)
                {
                    Flush();
                }
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Flush()
            {
                if (_buffer.Length > 0)
                {
                    _textBox.Invoke((MethodInvoker)delegate {
                        _textBox.AppendText(_buffer.ToString());
                        _textBox.ForeColor = Color.Red;
                    });
                    _buffer.Clear();
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Flush();
                }
                base.Dispose(disposing);
            }
        }

        private void ConsoleForm_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
